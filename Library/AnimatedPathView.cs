using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Java.Interop;

namespace DK.Ostebaronen.Droid.Views
{
    [Register("dk.ostebaronen.droid.views.AnimatedPathView")]
    public class AnimatedPathView : View
    {
        private Paint _paint;

        private float _pathLength;
        private float _progress;
        private Color _strokeColor;
        private float _strokeWidth;

        protected AnimatedPathView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public AnimatedPathView(Context context)
            : this(context, null)
        {
        }

        public AnimatedPathView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public AnimatedPathView(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.AnimatedPathView);
            _strokeColor = a.GetColor(Resource.Styleable.AnimatedPathView_apvStrokeColor,
                Color.Argb(0xff, 0x00, 0xff, 0x00));
            _strokeWidth = a.GetFloat(Resource.Styleable.AnimatedPathView_apvStrokeWidth, 8.0f);

            Init();
        }

        /// <summary>
        ///     Sets or gets the stroke width of the Path drawn.
        /// </summary>
        public float StrokeWidth
        {
            get { return _strokeWidth; }
            set
            {
                _strokeWidth = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Sets or gets the stroke color of the Path drawn.
        /// </summary>
        public Color StrokeColor
        {
            get { return _strokeColor; }
            set
            {
                _strokeColor = value;
                Invalidate();
            }
        }

        public Path Path { get; private set; }

        /// <summary>
        ///     Set the percentage of the Path animated.
        /// </summary>
        public float Percentage
        {
            get { return _progress; }
            set
            {
                if (value < 0.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException("value");

                _progress = value;
                Invalidate();
            }
        }

        private void Init()
        {
            _paint = new Paint(PaintFlags.AntiAlias) {Color = _strokeColor, StrokeWidth = _strokeWidth};
            _paint.SetStyle(Paint.Style.Stroke);

            SetPath(new Path());
        }

        public void SetPath(Path p)
        {
            Path = p;
            var measure = new PathMeasure(Path, false);
            _pathLength = measure.Length;
        }

        /// <summary>
        ///     Creates a path, by starting with moving to first point then calling
        ///     LineTo for the rest of the elements. This means that it will become
        ///     a path with no breaks. Use the other override to create a more
        ///     sophisticated path
        /// </summary>
        /// <param name="path"></param>
        public void SetPath(IList<Tuple<float, float>> path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (!path.Any())
                throw new ArgumentOutOfRangeException("path");

            var p = new Path();
            var first = path.First();
            p.MoveTo(first.Item1, first.Item2);

            foreach (var item in path.Skip(1))
                p.LineTo(item.Item1, item.Item2);

            SetPath(p);
        }

        [Export("setPercentage")] // for animation
        [Obsolete("Use property instead. This is only for Object Animators")]
        public void SetPercentage(float percentage)
        {
            Percentage = percentage;
        }

        /// <summary>
        ///     Scale the Path by factor N, where N is the number of times each coordinate is
        ///     multiplied.
        /// </summary>
        /// <param name="x">Multiplicity of X coordinates</param>
        /// <param name="y">Multiplicity of Y coordinates</param>
        public void ScalePathBy(float x, float y)
        {
            var m = new Matrix();
            m.PostScale(x, y);
            Path.Transform(m);

            var measure = new PathMeasure(Path, false);
            _pathLength = measure.Length;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            var effect = new DashPathEffect(new[] {_pathLength, _pathLength}, (_pathLength - _pathLength * _progress));
            _paint.SetPathEffect(effect);

            canvas.Save();
            canvas.Translate(PaddingLeft, PaddingTop);
            canvas.DrawPath(Path, _paint);
            canvas.Restore();
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            var widthSize = MeasureSpec.GetSize(widthMeasureSpec);
            var heightSize = MeasureSpec.GetSize(heightMeasureSpec);
            var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

            if (widthMode == MeasureSpecMode.AtMost)
                throw new InvalidOperationException("AnimatedPathView cannot have a wrap_content property");

            if (heightMode == MeasureSpecMode.AtMost)
                throw new InvalidOperationException("AnimatedPathView cannot have a wrap_content property");

            SetMeasuredDimension(widthSize, heightSize);
        }

        /// <summary>
        ///     Create Path from very simple SVG Paths, supports M, L, C and z
        /// </summary>
        /// <param name="data">SVG Path string</param>
        /// <returns>Path</returns>
        public Path CreatePathFromSvgString(string data)
        {
            var p = new Path();
            try
            {
                var matches = Regex.Matches(data, @"[MmZzLlHhVvCcSsQqTtAa]|[-+]?[0-9]*\.?[0-9]+(?:[eE][-+]?[0-9]+)?");
                var i = 0;
                while (i < matches.Count)
                {
                    var token = matches[i++].Value;
                    if (token == "M")
                    {
                        var x = float.Parse(matches[i++].Value);
                        var y = float.Parse(matches[i++].Value);
                        p.MoveTo(x, y);
                    }
                    else if (token == "L")
                    {
                        var x = float.Parse(matches[i++].Value);
                        var y = float.Parse(matches[i++].Value);
                        p.LineTo(x, y);
                    }
                    else if (token == "C")
                    {
                        var x1 = float.Parse(matches[i++].Value);
                        var y1 = float.Parse(matches[i++].Value);
                        var x2 = float.Parse(matches[i++].Value);
                        var y2 = float.Parse(matches[i++].Value);
                        var x3 = float.Parse(matches[i++].Value);
                        var y3 = float.Parse(matches[i++].Value);
                        p.CubicTo(x1, y1, x2, y2, x3, y3);
                    }
                    else if (token == "z")
                    {
                        p.Close();
                    }
                    else
                    {
                        throw new IndexOutOfRangeException("unknown command [" + token + "]");
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                throw new Exception("bad data ", e);
            }

            return p;
        }
    }
}