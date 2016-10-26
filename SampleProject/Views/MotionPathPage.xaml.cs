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
using CustomControls.Converters;
using CustomControls.Extensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleProject.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MotionPathPage : Page
    {
        public MotionPathPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            TestMotion.Start();
        }

        private void ButtonBase_pause(object sender, RoutedEventArgs e)
        {
            TestMotion.Pause();
        }

        private void ButtonReset_OnClick(object sender, RoutedEventArgs e)
        {
            TestMotion.Reset();
        }

        private void RangeBase_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (TestMotion != null && e.OldValue != 0)
                TestMotion.Duration = TimeSpan.FromSeconds(e.NewValue);
        }

        private void ToggleSine_Changed(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            if (cb.IsChecked == true)
            {
                TestMotion.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
            }
            else
            {
                TestMotion.EasingFunction = null;
            }
        }

        private void Back_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void quickSetPath(object sender, RoutedEventArgs e)
        {
            TestMotion.Path = new StringToPathGeometryConverter().Convert("M 10,100 C 10,300 300,-200 300,100");
        }

        private void quickRelativeEnd(object sender, RoutedEventArgs e)
        {
            TestMotion.Path = null;
            TestMotion.LineAbsoluteEnd = new Point(double.NaN, double.NaN);
            TestMotion.LineAbsoluteStart = new Point(double.NaN, double.NaN);
            TestMotion.LineRelativeEnd = new Point(100, 100);
        }

        private void quickAbsEnd(object sender, RoutedEventArgs e)
        {
            TestMotion.Path = null;
            TestMotion.LineAbsoluteEnd = new Point(20, 30);
            TestMotion.LineAbsoluteStart = new Point(double.NaN, double.NaN);
            TestMotion.LineRelativeEnd = new Point(double.NaN, double.NaN);
        }

        private void quickAbsStartRelEnd(object sender, RoutedEventArgs e)
        {
            TestMotion.Path = null;
            TestMotion.LineAbsoluteEnd = new Point(double.NaN, double.NaN);
            TestMotion.LineAbsoluteStart = new Point(50, 50);
            TestMotion.LineRelativeEnd = new Point(10, 150);
        }

        private void quickAbsStartAbsEnd(object sender, RoutedEventArgs e)
        {
            TestMotion.Path = null;
            TestMotion.LineAbsoluteEnd = new Point(300, 20);
            TestMotion.LineAbsoluteStart = new Point(50, 50);
            TestMotion.LineRelativeEnd = new Point(double.NaN, double.NaN);
        }
    }
}
