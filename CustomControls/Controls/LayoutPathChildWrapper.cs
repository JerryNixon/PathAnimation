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
    public partial class LayoutPathChildWrapper : ContentControl
    {
        #region private members
        private ContentControl ALIGNMENT { get; set; }

        private CompositeTransform Transform { get; set; }
        #endregion

        #region internal members
        internal double ProgressDistance { get; private set; } = double.NaN;

        internal double RawProgress;
        #endregion

        #region initialization
        protected override void OnApplyTemplate()
        {
            ALIGNMENT = GetTemplateChild(nameof(ALIGNMENT)) as ContentControl;
        }

        public LayoutPathChildWrapper(FrameworkElement child, ChildAlignment alingment, Orientations orientation)
        {
            Content = child;
            DefaultStyleKey = typeof(LayoutPathChildWrapper);
            RenderTransform = Transform = new CompositeTransform();
            Loaded += delegate
            {
                UpdateAlignment(alingment, orientation);
            };
        }
        #endregion
        
        #region propety changed callbacks

        private static void ProgressPropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var s = ((LayoutPathChildWrapper)o);
            double oldV = (double)e.OldValue;
            double newV = (double)e.NewValue;

            if (!double.IsNaN(newV))
            {
                s.ProgressDistance = Math.Abs(newV - oldV);
                if (s.ProgressDistance > 50)
                {
                    //occurs when we transfer from end to beginning (e.g. 99 to 1: In that case Progress distance will give a value of 98 while the actual must be 2).
                    if (oldV > 50)
                        oldV = oldV - 100;
                    else
                        newV = newV - 100;
                    s.ProgressDistance = Math.Abs(newV - oldV);
                }
            }
        }

        private static void TranslateXPropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutPathChildWrapper)o).Transform.TranslateX = (double)e.NewValue;
        }

        private static void TranslateYPropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutPathChildWrapper)o).Transform.TranslateY = (double)e.NewValue;
        }

        private static void RotationPropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutPathChildWrapper)o).Transform.Rotation = (double)e.NewValue;
        }

        #endregion

        #region internal methods
        internal void UpdateAlignment(ChildAlignment alignment, Orientations orientation)
        {
            if (ALIGNMENT == null)
                return;

            var translate = ALIGNMENT.RenderTransform as TranslateTransform;
            if (translate == null)
                return;

            if (alignment == ChildAlignment.Center)
            {
                translate.X = translate.Y = 0;
            }
            else
            {
                int flipValue = 1;
                if (orientation == Orientations.ToPathReversed || orientation == Orientations.VerticalReversed)
                    flipValue = -1;
                var child = (FrameworkElement)Content;
                if (orientation == Orientations.ToPath || orientation == Orientations.ToPathReversed)
                {
                    translate.X = 0;
                    if (alignment == ChildAlignment.Outer)
                    {
                        translate.Y = child.ActualHeight * flipValue * -1 / 2.0;
                    }
                    else
                    {
                        translate.Y = child.ActualHeight * flipValue / 2.0;
                    }
                }
                else
                {
                    translate.Y = 0;
                    if (alignment == ChildAlignment.Outer)
                    {
                        translate.X = child.ActualWidth * flipValue * -1 / 2.0;
                    }
                    else
                    {
                        translate.X = child.ActualWidth * flipValue / 2.0;
                    }
                }

            }
        }

        internal void SetTransformCenter(double x, double y)
        {
            Transform.CenterX = x;
            Transform.CenterY = y;
        }
        #endregion
    }
}
