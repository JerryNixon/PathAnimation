using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SampleProject.Views;

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
    }
}
