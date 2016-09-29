using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.Core;
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
    public sealed partial class RacingLayoutPathPage : Page
    {

        private double progress = 0;
        private double progressChange = 0.5;
        private Timer timer;

        public RacingLayoutPathPage()
        {
            this.InitializeComponent();


            Loaded += delegate (object sender, RoutedEventArgs args)
            {
                layoutPath.SmoothRotation = 25;
                mapPath.SmoothTranslation = layoutPath.SmoothTranslation = 28;
                timer = new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000 / 60.0));
            };

        }

        private void Callback(object state)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, delegate
            {
                progress = progress % 100;
                layoutPath.Progress = progress;
                progress = progress + progressChange;
            });
        }

        private void speedUp(object sender, RoutedEventArgs e)
        {
            if (progressChange > 1)
                return;
            progressChange = progressChange + 0.05;
        }

        private void speedDown(object sender, RoutedEventArgs e)
        {
            if (progressChange < 0.3)
                return;

            progressChange = progressChange - 0.05;
        }

        private void Back_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
