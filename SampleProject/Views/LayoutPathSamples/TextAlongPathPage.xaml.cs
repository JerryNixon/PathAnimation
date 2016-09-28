﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class TextAlongPathPage : Page
    {
        public TextAlongPathPage()
        {
            this.InitializeComponent();

            string text = "Hello world! This is a text animation sample!";

            var chars = text.ToCharArray().Reverse();

            layoutPath.Children.Clear();
            startKeyFrame.Value = 100 + text.Length * layoutPath.ItemsPadding;
            Storyboard1.RepeatBehavior = RepeatBehavior.Forever;

            foreach (char c in chars)
            {
                TextBlock textBlock = new TextBlock() { Text = c + "" };
                layoutPath.Children.Add(textBlock);
            }

            Storyboard1.Begin();

            
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}