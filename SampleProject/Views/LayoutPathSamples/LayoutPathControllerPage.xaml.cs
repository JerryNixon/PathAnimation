using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using CustomControls.Enums;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleProject.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LayoutPathControllerPage : Page
    {
        public LayoutPathControllerPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            ChildAlignmentCb.ItemsSource = Enum.GetValues(typeof(ChildAlignment));
            StretchCb.ItemsSource = Enum.GetValues(typeof(Stretch));
            ItemOrientationCb.ItemsSource = Enum.GetValues(typeof(Orientations));
            StartBehaviorCb.ItemsSource = Enum.GetValues(typeof(Behaviors));
            EndBehaviorCb.ItemsSource = Enum.GetValues(typeof(Behaviors));
            Loaded += delegate (object sender, RoutedEventArgs args)
            {
                ChildAlignmentCb.SelectedIndex = 1;
                ItemOrientationCb.SelectedIndex = 1;
                StartBehaviorCb.SelectedIndex = 0;
                EndBehaviorCb.SelectedIndex = 0;
                PathStoryboard.Begin();
            };
        }



        private void Add_OnClick(object sender, RoutedEventArgs e)
        {
            LayoutPath1.Children.Insert(0, new Rectangle() { Fill = new SolidColorBrush(Colors.DarkOrange), Width = 20, Height = 20, RadiusX = 20, RadiusY = 20 });
        }

        private void RemoveFirst_OnClick(object sender, RoutedEventArgs e)
        {
            if (LayoutPath1.Children.Any())
                LayoutPath1.Children.RemoveAt(0);
        }

        private bool paused = false;
        private void StartPauseAnimation(object sender, RoutedEventArgs e)
        {
            if (paused)
                PathStoryboard.Resume();
            else
                PathStoryboard.Pause();
            paused = !paused;
        }

        private void ToggleSine_Changed(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            if (cb.IsChecked == true)
            {
                LayoutPath1.ChildrenEasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
            }
            else
            {
                LayoutPath1.ChildrenEasingFunction = null;
            }
        }

        private void RefreshOnCLick(object sender, RoutedEventArgs e)
        {
            LayoutPath1.ResetSmoothingAndRefresh();
        }
    }
}
