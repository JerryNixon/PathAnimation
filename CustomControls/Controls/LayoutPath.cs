﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Security.Cryptography.Core;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using CustomControls.Enums;
using CustomControls.ExtendedSegments;

namespace CustomControls.Controls
{
    /// <summary>
    /// A control used to animate children along a path
    /// </summary>
    [ContentProperty(Name = "Children")]
    public partial class LayoutPath : ContentControl
    {
        #region private variables

        /// <summary>
        /// Contains all items that are animated along path
        /// </summary>
        private Grid CHILDREN;

        /// <summary>
        /// Top level container for this control
        /// </summary>
        private Viewbox VIEW_BOX;

        /// <summary>
        /// The path object, generated by specified <see cref="Path"/> property
        /// </summary>
        private Path PATH;

        private readonly ObservableCollection<object> _children = new ObservableCollection<object>();

        #endregion

        #region public properties

        /// <summary>
        /// Children that are animated along <see cref="Path"/>
        /// </summary>
        public IList<object> Children => _children;

        /// <summary>
        /// The extended geometry, mainly used for getting point at fraction length
        /// </summary>
        public ExtendedPathGeometry ExtendedGeometry { get; private set; }

        #endregion

        #region initialization

        protected override void OnApplyTemplate()
        {
            VIEW_BOX = GetTemplateChild(nameof(VIEW_BOX)) as Viewbox;
            PATH = GetTemplateChild(nameof(PATH)) as Path;
            CHILDREN = GetTemplateChild(nameof(CHILDREN)) as Grid;

            PATH.SetBinding(Windows.UI.Xaml.Shapes.Path.DataProperty, new Binding()
            {
                Path = new PropertyPath("Path"),
                Source = this
            });

            VIEW_BOX.SetBinding(Viewbox.StretchProperty, new Binding()
            {
                Path = new PropertyPath("Stretch"),
                Source = this
            });

            PATH.Opacity = PathVisibility == Visibility.Visible ? 0.5 : 0;

            if (ExtendedGeometry != null)
                PATH.Margin = new Thickness(-ExtendedGeometry.PathOffset.X, -ExtendedGeometry.PathOffset.Y, 0, 0);

            foreach (var child in _children)
                CHILDREN.Children.Add(new LayoutPathChildWrapper(child as FrameworkElement, ChildAlignment, MoveVertically, FlipItems));

            //TODO: _children.Clear does not invoke this event.
            _children.CollectionChanged += ChildrenOnCollectionChanged;

            base.OnApplyTemplate();
        }

        /// <summary>
        /// Used for providing better designer support, when adding or removing items
        /// </summary>
        private readonly object _collectionChangedLocker = new object();
        private async void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            lock (_collectionChangedLocker)
            {
                if (args.NewItems != null)
                {
                    foreach (var child in args.NewItems)
                    {
                        var wrapper =
                            CHILDREN.Children.FirstOrDefault(x => ((LayoutPathChildWrapper)x).Content == child);
                        if (wrapper == null)
                            CHILDREN.Children.Insert(args.NewStartingIndex,
                                new LayoutPathChildWrapper(child as FrameworkElement, ChildAlignment, MoveVertically,
                                    FlipItems));
                    }
                }

                if (args.OldItems != null)
                {
                    foreach (var child in args.OldItems)
                    {
                        var wrapper = CHILDREN.Children.FirstOrDefault(x => ((LayoutPathChildWrapper)x).Content == child);
                        if (wrapper != null)
                        {
                            CHILDREN.Children.Remove(wrapper);
                            ((LayoutPathChildWrapper)wrapper).Content = null;
                        }
                    }
                }
            }

            //Force UI to render elements. If not, width and height are giving 0 values. 
            //Dimensions are needed for correctly aligning items.
            await Task.Delay(1);

            TransformToProgress(PathProgress);
        }

        public LayoutPath()
        {
            DefaultStyleKey = typeof(LayoutPath);

            Loaded += delegate
            {
                TransformToProgress(PathProgress);
            };
        }

        #endregion

        #region dependency properties callbacks

        private static void FlipItemsChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var sender = ((LayoutPath)o);
            if (sender.CHILDREN != null)
            {
                foreach (LayoutPathChildWrapper child in sender.CHILDREN.Children)
                {
                    child.UpdateAlingment(sender.ChildAlignment, sender.MoveVertically, sender.FlipItems);
                }
            }
            TransformToProgress(o, e);
        }

        private static void MoveVerticallyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var sender = ((LayoutPath)o);
            if (sender.CHILDREN != null)
            {
                foreach (LayoutPathChildWrapper child in sender.CHILDREN.Children)
                {
                    child.UpdateAlingment(sender.ChildAlignment, sender.MoveVertically, sender.FlipItems);
                }
            }
            TransformToProgress(o, e);
        }

        private static void ChildAlingmentChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var sender = ((LayoutPath)o);
            if (sender.CHILDREN != null)
            {
                foreach (LayoutPathChildWrapper child in sender.CHILDREN.Children)
                {
                    child.UpdateAlingment((Enums.ChildAlignment)e.NewValue, sender.MoveVertically, sender.FlipItems);
                }
            }
        }

        private static void PathChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var data = (Geometry)e.NewValue;
            var sen = ((LayoutPath)o);
            sen.ExtendedGeometry = new ExtendedPathGeometry(data as PathGeometry);
            if (sen.PATH != null)
                sen.PATH.Margin = new Thickness(-sen.ExtendedGeometry.PathOffset.X, -sen.ExtendedGeometry.PathOffset.Y, 0, 0);
            sen.TransformToProgress(((LayoutPath)o).PathProgress);
        }

        private static void PathVisibleChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var path = ((LayoutPath)o).PATH;
            //we don't collapse path because we need it's space for stretching control.
            if (path != null)
                path.Opacity = (Visibility)e.NewValue == Visibility.Visible ? 0.5 : 0;
        }

        private static void ProgressChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutPath)o).TransformToProgress((double)e.NewValue);
        }

        private static void TransformToProgress(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            LayoutPathChildWrapper wrapper = null;
            while (true)
            {
                o = VisualTreeHelper.GetParent(o);
                if (o is LayoutPathChildWrapper)
                    wrapper = (LayoutPathChildWrapper)o;

                if (o is LayoutPath)
                {
                    if (wrapper != null)
                        ((LayoutPath)o).MoveChild(wrapper, (double)e.NewValue);
                    return;
                }
                if (o == null)
                    return;
            }
        }

        #endregion

        #region methods

        private void TransformToProgress(double progress)
        {
            if (ExtendedGeometry == null || CHILDREN == null)
                return;

            var children = CHILDREN.Children.ToArray();

            for (int i = 0; i < children.Count(); i++)
            {
                double childPercent = progress - (i * ItemsPadding);

                var wrapper = (LayoutPathChildWrapper)children[i];
                if (!GetIsMovable((UIElement)wrapper.Content))
                    continue;

                var childProgress = GetProgress((UIElement)wrapper.Content);
                if (!double.IsNaN(childProgress))
                    childPercent = childProgress;

                var childOffset = GetProgressOffset((UIElement)wrapper.Content);
                childPercent += childOffset;

                MoveChild(wrapper, childPercent);
            }

            var tmp = ExtendedGeometry.GetPointAtFractionLength(progress);
            CurrentPosition = tmp.Item1;
            CurrentRotation = tmp.Item2;

            CurrentLength = ExtendedGeometry.PathLength * (progress / 100.0);
        }

        private void MoveChild(LayoutPathChildWrapper wrapper, double childPercent)
        {
            ApplyStackFilters(ref childPercent);

            if (ChildEasingFunction != null)
                childPercent = ChildEasingFunction.Ease(childPercent / 100.0) * 100;

            Point childPoint;
            double rotationTheta;
            ExtendedGeometry.GetPointAtFractionLength(childPercent, out childPoint, out rotationTheta);

            wrapper.Progress = childPercent;

            Rotate(wrapper, rotationTheta);
            Translate(wrapper, childPoint);
        }

        private void ApplyStackFilters(ref double progress)
        {
            if (progress < 0)
            {
                if (StartBehavior == Behaviors.Stack)
                {
                    progress = 0;
                }
                else
                {
                    //transfer to range 0-100.
                    //Examples
                    // -2 + 100 = 98 exit
                    // -102 + 100 = -2 + 100 = 98 exit
                    while (progress < 0)
                        progress = 100 + progress;
                }
            }
            else if (progress > 100)
            {
                if (EndBehavior == Behaviors.Stack)
                {
                    //avoid going to beginning of the path
                    progress = 99.9999;
                }
                else
                {
                    //transfer to range 0-100
                    progress = progress % 100;
                }
            }
        }

        private void Rotate(LayoutPathChildWrapper wrapper, double rotationTheta)
        {
            if (!OrientToPath)
                rotationTheta = 0;

            if (MoveVertically)
                rotationTheta += 90;

            if (FlipItems)
                rotationTheta += 180;

            rotationTheta = rotationTheta % 360;

            var rotationSmoothing = RotationSmoothingDefault;
            var childRotationSmoothing = GetRotationSmoothing((UIElement)wrapper.Content);
            if (!double.IsNaN(childRotationSmoothing))
                rotationSmoothing = childRotationSmoothing;

            //try smooth rotation
            if (!double.IsNaN(wrapper.ProgressDistance) && rotationSmoothing > 0 && wrapper.ProgressDistance > 0)
            {
                var degreesDistance = Math.Max(rotationTheta, wrapper.Rotation) - Math.Min(rotationTheta, wrapper.Rotation);
                var rotation = wrapper.Rotation;
                while (degreesDistance > 180)
                {
                    if (rotationTheta > rotation)
                        rotation = rotation + 360;
                    else
                        rotation = rotation - 360;
                    degreesDistance = Math.Max(rotationTheta, rotation) - Math.Min(rotationTheta, rotation);
                }
                wrapper.Rotation = wrapper.Rotation = (rotation * rotationSmoothing * 0.2 + rotationTheta * wrapper.ProgressDistance) / (rotationSmoothing * 0.2 + wrapper.ProgressDistance);
            }
            else
            {
                wrapper.Rotation = wrapper.Rotation = rotationTheta;
            }
        }

        private void Translate(LayoutPathChildWrapper wrapper, Point childPoint)
        {
            var wrappedChild = wrapper.Content as FrameworkElement;
            var childWidth = wrappedChild.ActualWidth;
            var childHeight = wrappedChild.ActualHeight;

            double translateX = childPoint.X - ExtendedGeometry.PathOffset.X;
            double translateY = childPoint.Y - ExtendedGeometry.PathOffset.Y;

            //center align child
            translateX -= childWidth / 2.0;
            translateY -= childHeight / 2.0;

            wrapper.SetTransformCenter(childWidth / 2.0, childHeight / 2.0);

            var translationSmoothing = TranslationSmoothingDefault;
            var childTranslationSmoothing = GetTranslationSmoothing((UIElement)wrapper.Content);
            if (!double.IsNaN(childTranslationSmoothing))
                translationSmoothing = childTranslationSmoothing;

            if (!double.IsNaN(wrapper.ProgressDistance) && translationSmoothing > 0 && wrapper.ProgressDistance > 0)
            {
                wrapper.TranslateX = (wrapper.TranslateX * translationSmoothing * 0.2 / wrapper.ProgressDistance + translateX) / (translationSmoothing * 0.2 / wrapper.ProgressDistance + 1);
                wrapper.TranslateY = (wrapper.TranslateY * translationSmoothing * 0.2 / wrapper.ProgressDistance + translateY) / (translationSmoothing * 0.2 / wrapper.ProgressDistance + 1);
            }
            else
            {
                wrapper.TranslateX = translateX;
                wrapper.TranslateY = translateY;
            }
        }

        #endregion
    }
}
