using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SampleProject.Views;
using SampleProject.Views.LayoutPathSamples;

namespace SampleProject
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
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
    }
}
