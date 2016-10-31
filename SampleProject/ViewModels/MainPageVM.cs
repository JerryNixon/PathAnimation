﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SampleProject.Models;
using System.Windows.Input;
using SampleProject.Helpers;
using SampleProject.Views;
using SampleProject.Views.LayoutPathSamples;

namespace SampleProject.ViewModels
{
    public class MainPageVM
    {

        public List<TileModel> LayoutPathOptions { get; set; } = new List<TileModel>()
        {
            new TileModel()
            {
                Title = "Properties controller",
                Description = "Manipulate all properties of LayoutPath control.",
                ThumbnailUri = new Uri("ms-appx:///Assets/Thumbs/lp1.png", UriKind.RelativeOrAbsolute),
                Command = new DelegateCommand(delegate { App.Frame.Navigate(typeof(LayoutPathControllerPage)); })
            },
            new TileModel()
            {
                Title = "Attached properties",
                Description = "Manipulate all attached properties of LayoutPath control.",
                ThumbnailUri = new Uri("ms-appx:///Assets/Thumbs/lp2.png", UriKind.RelativeOrAbsolute),
                Command = new DelegateCommand(delegate { App.Frame.Navigate(typeof(AttachedPropertiesLayoutPathSample)); })
            },
            new TileModel()
            {
                Title = "Alignment and rotation",
                Description = "Specify alignment and rotation properties to achieve a circle effect. Use ItemsPadding to move each children to specific location.",
                ThumbnailUri = new Uri("ms-appx:///Assets/Thumbs/lp3.png", UriKind.RelativeOrAbsolute),
                Command = new DelegateCommand(delegate { App.Frame.Navigate(typeof(CirclePathTextSample)); })
            },
            new TileModel()
            {
                Title = "Text along sample",
                Description = "An example showing how you can animate any text along path.",
                ThumbnailUri = new Uri("ms-appx:///Assets/Thumbs/lp4.png", UriKind.RelativeOrAbsolute),
                Command = new DelegateCommand(delegate { App.Frame.Navigate(typeof(TextAlongPathPage)); })
            },
            new TileModel()
            {
                Title = "Racing car",
                Description = "Data bind a path geometry to multiple paths. Use smoothness to control child motion.",
                ThumbnailUri = new Uri("ms-appx:///Assets/Thumbs/lp5.png", UriKind.RelativeOrAbsolute),
                Command = new DelegateCommand(delegate { App.Frame.Navigate(typeof(RacingLayoutPathPage)); })
            },
        };

    }
}
