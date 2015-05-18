using System;
using Android.Animation;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using DK.Ostebaronen.Droid.Views;

namespace Sample
{
    [Activity(Label = "Animated Path sample", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private AnimatedPathView _view;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            _view = FindViewById<AnimatedPathView>(Resource.Id.animated_path);
            //_view.ViewTreeObserver.PreDraw += ViewTreeObserverOnPreDraw;

            _view.Click += ViewOnClick;
        }

        //private void ViewTreeObserverOnPreDraw(object sender, ViewTreeObserver.PreDrawEventArgs preDrawEventArgs) 
        //{
        //    _view.ViewTreeObserver.PreDraw -= ViewTreeObserverOnPreDraw;

        //    _view.SetPath(_view.CreatePathFromSvgString("M 300,110 C 300,200 200,250 150,300 C 110,240 10,190 10,110 C 10,30 130,10 150,100 C 150,10 300,30 300,110 z"));
        //    _view.ScalePathBy(2,2); // 2x size 
        //}


        private void ViewOnClick(object sender, EventArgs eventArgs)
        {
            var anim = ObjectAnimator.OfFloat(_view, "percentage", 0.0f, 1.0f);
            anim.SetDuration(2000);
            anim.SetInterpolator(new LinearInterpolator());
            anim.Start();
        }
    }
}

