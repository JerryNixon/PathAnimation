using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SampleProject
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            MyAnimationStoryboard.Begin();
        }
    }
}
