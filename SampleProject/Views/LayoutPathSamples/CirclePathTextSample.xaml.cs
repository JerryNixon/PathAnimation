using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleProject.Views.LayoutPathSamples
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CirclePathTextSample : Page
    {
        Storyboard storyboard = new Storyboard();

        public CirclePathTextSample()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            DoubleAnimationUsingKeyFrames an = new DoubleAnimationUsingKeyFrames();
            an.EnableDependentAnimation = true;


            double lineDuration = 3;
            double transitionDuration = 1;


            an.KeyFrames.Add(new DiscreteDoubleKeyFrame()
            {
                Value = 100,
                KeyTime = TimeSpan.Zero
            });

            for (int i = 0; i < layoutPath.Children.Count; i++)
            {
                var currentTime = i * lineDuration;

                //wait to read
                an.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    Value = 100 + i * layoutPath.ChildrenPadding,
                    KeyTime = TimeSpan.FromSeconds(currentTime + lineDuration),

                });


                //go to next
                an.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    Value = 100 + (i) * layoutPath.ChildrenPadding,
                    KeyTime = TimeSpan.FromSeconds(currentTime + transitionDuration),
                    EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut }
                });


            }

            an.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                Value = 200,
                KeyTime = TimeSpan.FromSeconds(layoutPath.Children.Count * lineDuration + transitionDuration),
                EasingFunction = new ExponentialEase()
            });

            an.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                Value = 200,
                KeyTime = TimeSpan.FromSeconds(layoutPath.Children.Count * lineDuration + transitionDuration + lineDuration),
                EasingFunction = new ExponentialEase()
            });

            Storyboard.SetTargetProperty(an, "(LayoutPath.PathProgress)");
            Storyboard.SetTarget(an, layoutPath);
            storyboard.Children.Add(an);

            storyboard.RepeatBehavior = RepeatBehavior.Forever;
            storyboard.Begin();
        }

       
    }
}
