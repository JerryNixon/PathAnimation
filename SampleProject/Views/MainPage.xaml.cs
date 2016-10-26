using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using SampleProject.Views;
using SampleProject.Views.CascadingAnimationSamples;
using SampleProject.Views.LayoutPathSamples;

namespace SampleProject
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }


        private void LayoutPath_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LayoutPathPage));
        }

        private void MotionPath_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MotionPathPage));
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CirclePathTextSample));
        }

        private void textAlongPath_click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(TextAlongPathPage));
        }

        private void racingPath_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(RacingLayoutPathPage));
        }


        private void cascadingAnimationRealtimeExample(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CascadingAnimationControllerPage));
        }

        private void cascadingAnimationCachedExample(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CascadingAnimationCachedExamplePage));
        }

        private void attachedProperties_click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AttachedPropertiesLayoutPathSample));
        }
    }
}
