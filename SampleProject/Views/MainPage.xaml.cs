using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SampleProject.ViewModels;
using SampleProject.Views.CascadingAnimationSamples;
using SampleProject.Views.LayoutPathSamples;

namespace SampleProject.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPageVM _vm = new MainPageVM();
        public MainPage()
        {
            DataContext = _vm;
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
        
     
      
        private void cascadingAnimationRealtimeExample(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CascadingAnimationControllerPage));
        }

        private void cascadingAnimationCachedExample(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CascadingAnimationCachedExamplePage));
        }
    }
}
