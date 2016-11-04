﻿using System;
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
        private ContentControl ALIGNMENT { get; set; }

        private CompositeTransform Transform { get; set; }

        internal double ProgressDistance { get; private set; } = double.NaN;

        static LayoutPathChildWrapper()
        {
            ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(LayoutPathChildWrapper), new PropertyMetadata(default(double),
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
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
                }));
            RotationProperty = DependencyProperty.Register("Rotation", typeof(double), typeof(LayoutPathChildWrapper), new PropertyMetadata(default(double),
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    ((LayoutPathChildWrapper)o).Transform.Rotation = (double)e.NewValue;
                }));
            TranslateXProperty = DependencyProperty.Register("TranslateX", typeof(double), typeof(LayoutPathChildWrapper), new PropertyMetadata(default(double),
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    ((LayoutPathChildWrapper)o).Transform.TranslateX = (double)e.NewValue;
                }));
            TranslateYProperty = DependencyProperty.Register("TranslateY", typeof(double), typeof(LayoutPathChildWrapper), new PropertyMetadata(default(double),
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    ((LayoutPathChildWrapper)o).Transform.TranslateY = (double)e.NewValue;
                }));
        }

        #region dependency properties

        public double Progress { get { return (double)GetValue(ProgressProperty); } internal set { SetValue(ProgressProperty, value); } }
        public static readonly DependencyProperty ProgressProperty;

        public double Rotation { get { return (double)GetValue(RotationProperty); } internal set { SetValue(RotationProperty, value); } }
        public static readonly DependencyProperty RotationProperty;

        public double TranslateX { get { return (double)GetValue(TranslateXProperty); } internal set { SetValue(TranslateXProperty, value); } }
        public static readonly DependencyProperty TranslateXProperty;

        public double TranslateY { get { return (double)GetValue(TranslateYProperty); } internal set { SetValue(TranslateYProperty, value); } }
        public static readonly DependencyProperty TranslateYProperty;

        #endregion

        protected override void OnApplyTemplate()
        {
            ALIGNMENT = GetTemplateChild(nameof(ALIGNMENT)) as ContentControl;
        }

        public LayoutPathChildWrapper(FrameworkElement child, ChildAlignment alingment, Orientations orientation)
        {
            Content = child;
            DefaultStyleKey = typeof(LayoutPathChildWrapper);
            RenderTransform = Transform = new CompositeTransform();
            Loaded += delegate (object sender, RoutedEventArgs args)
            {
                UpdateAlingment(alingment, orientation);
            };
        }

        internal void UpdateAlingment(ChildAlignment alingment, Orientations orientation)
        {
            if (ALIGNMENT == null)
                return;

            var translate = (TranslateTransform)ALIGNMENT.RenderTransform;
            if (alingment == ChildAlignment.Center)
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
                    if (alingment == ChildAlignment.Outer)
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
                    if (alingment == ChildAlignment.Outer)
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
    }
}
