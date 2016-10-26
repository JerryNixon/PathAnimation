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
using Windows.UI.Xaml.Navigation;
using CustomControls;
using CustomControls.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleProject.Views.CascadingAnimationSamples
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CascadingAnimationControllerPage : Page
    {
        CascadingAnimation x;
        public CascadingAnimationControllerPage()
        {
            this.InitializeComponent();
            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                x = ct1;
                await x.InitialiseAsync();
                loadingGrid.Visibility = Visibility.Collapsed;
                x.PlayAnimation();
            };
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            loadingGrid.Visibility = Visibility.Visible;
            await x.ResetAsync();
            loadingGrid.Visibility = Visibility.Collapsed;
            x.PlayAnimation();
        }

        private async void back_OnClick(object sender, RoutedEventArgs e)
        {
            await x.ResetAsync();
            this.Frame.GoBack();
        }
    }
}
