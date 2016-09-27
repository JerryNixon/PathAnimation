using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using CustomControls.Enums;

namespace CustomControls.Controls
{
    public class LayoutPathChildWrapper : ContentControl
    {
        public ContentControl ALINGMENT { get; set; }

        protected override void OnApplyTemplate()
        {
            ALINGMENT = GetTemplateChild(nameof(ALINGMENT)) as ContentControl;
        }

        public LayoutPathChildWrapper(FrameworkElement child, ChildAlignment alingment, bool moveVertically, bool flip)
        {
            Content = child;
            DefaultStyleKey = typeof(LayoutPathChildWrapper);
            RenderTransform = new CompositeTransform();
            Loaded += delegate (object sender, RoutedEventArgs args)
            {
                UpdateAlingment(alingment, moveVertically, flip);
            };
        }

        public void UpdateAlingment(ChildAlignment alingment, bool moveVertically, bool flip)
        {
            var translate = (TranslateTransform)ALINGMENT.RenderTransform;
            if (alingment == ChildAlignment.Center)
            {
                translate.X = translate.Y = 0;
            }
            else
            {
                var child = (FrameworkElement)Content;
                if (!moveVertically)
                {
                    translate.X = 0;
                    if (alingment == ChildAlignment.Outer && !flip)
                    {
                        translate.Y = -child.ActualHeight / 2.0;
                    }
                    else
                    {
                        translate.Y = +child.ActualHeight / 2.0;
                    }
                }
                else
                {
                    translate.Y = 0;
                    if (alingment == ChildAlignment.Outer && !flip)
                    {
                        translate.X = -child.ActualWidth / 2.0;
                    }
                    else
                    {
                        translate.X = +child.ActualWidth / 2.0;
                    }
                }

            }
        }

    }
}
