using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class LayoutPathPage : Page
    {
        public LayoutPathPage()
        {
            this.InitializeComponent();
            childAlignmentCB.ItemsSource = Enum.GetValues(typeof(ChildAlignment));
            stretchCB.ItemsSource = Enum.GetValues(typeof(Stretch));
            Loaded += delegate (object sender, RoutedEventArgs args)
            {
                childAlignmentCB.SelectedIndex = 1;
                PathStoryboard.Begin();
            };
        }

        private void Back_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
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

        private void StartPauseAnimation(object sender, RoutedEventArgs e)
        {
            if (PathStoryboard.GetCurrentState() != ClockState.Active)
                PathStoryboard.Begin();
            else
                PathStoryboard.Stop();
        }
    }
}
