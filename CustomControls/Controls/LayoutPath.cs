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
    public class LayoutPath : ContentControl
    {
        #region variables
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

        /// <summary>
        /// Children that are animated along <see cref="Path"/>
        /// </summary>
        public IList<object> Children => _children;
        private readonly ObservableCollection<object> _children = new ObservableCollection<object>();

        /// <summary>
        /// The extended geometry, mainly used for getting point at fraction length
        /// </summary>
        public ExtendedPathGeometry ExtendedGeometry { get; private set; }

        /// <summary>
        /// Used for providing better designer support, when adding or removing items
        /// </summary>
        private readonly object _collectionChangedLocker = new object();

        #endregion

        #region constructors

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

            PATH.Opacity = PathVisible ? 0.5 : 0;

            if (ExtendedGeometry != null)
                PATH.Margin = new Thickness(-ExtendedGeometry.PathOffset.X, -ExtendedGeometry.PathOffset.Y, 0, 0);


            foreach (var child in _children)
            {
                CHILDREN.Children.Add(new LayoutPathChildWrapper(child as FrameworkElement, ChildAlignment, MoveVertically, FlipItems));
            }

            //TODO: _children.Clear does not invoke this event.
            _children.CollectionChanged += async delegate (object sender, NotifyCollectionChangedEventArgs args)
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

                TransformToProgress(Progress);
            };

            base.OnApplyTemplate();
        }

        public LayoutPath()
        {
            DefaultStyleKey = typeof(LayoutPath);

            Loaded += delegate
            {
                TransformToProgress(Progress);
            };
        }

        /// <summary>
        /// Initializes dependency properties
        /// </summary>
        static LayoutPath()
        {
            StretchProperty = DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(LayoutPath), new PropertyMetadata(Stretch.None));
            CurrentLengthProperty = DependencyProperty.Register(nameof(CurrentLength), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double)));
            CurrentRotationProperty = DependencyProperty.Register(nameof(CurrentRotation), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double)));
            CurrentPositionProperty = DependencyProperty.Register(nameof(CurrentPosition), typeof(Point), typeof(LayoutPath), new PropertyMetadata(default(Point)));
            SmoothRotationProperty = DependencyProperty.Register("SmoothRotation", typeof(int), typeof(LayoutPath), new PropertyMetadata(default(int)));
            SmoothTranslationProperty = DependencyProperty.Register("SmoothTranslation", typeof(int), typeof(LayoutPath), new PropertyMetadata(default(int)));

            ProgressProperty = DependencyProperty.Register(nameof(Progress), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double),
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    ((LayoutPath)o).TransformToProgress((double)e.NewValue);
                }));

            PathVisibleProperty = DependencyProperty.Register(nameof(PathVisible), typeof(bool), typeof(LayoutPath), new PropertyMetadata(true,
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    var path = ((LayoutPath)o).PATH;
                    //we don't collapse path because we need it's space for stretching control.
                    if (path != null)
                        path.Opacity = (bool)e.NewValue ? 0.5 : 0;
                }));

            PathProperty = DependencyProperty.Register(nameof(Path), typeof(Geometry), typeof(LayoutPath), new PropertyMetadata(default(Geometry),
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs args)
                {
                    var data = (Geometry)args.NewValue;
                    var sen = ((LayoutPath)o);
                    sen.ExtendedGeometry = new ExtendedPathGeometry(data as PathGeometry);
                    if (sen.PATH != null)
                        sen.PATH.Margin = new Thickness(-sen.ExtendedGeometry.PathOffset.X, -sen.ExtendedGeometry.PathOffset.Y, 0, 0);
                    sen.TransformToProgress(((LayoutPath)o).Progress);
                }));

            ChildAlignmentProperty = DependencyProperty.Register(nameof(ChildAlignment), typeof(ChildAlignment), typeof(LayoutPath), new PropertyMetadata(ChildAlignment.Center,
               delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
               {
                   var sender = ((LayoutPath)o);
                   if (sender.CHILDREN != null)
                   {
                       foreach (LayoutPathChildWrapper child in sender.CHILDREN.Children)
                       {
                           child.UpdateAlingment((Enums.ChildAlignment)e.NewValue, sender.MoveVertically, sender.FlipItems);
                       }
                   }
               }));

            Action<DependencyObject, DependencyPropertyChangedEventArgs> transformToProgress = delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
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

            };

            MoveVerticallyProperty = DependencyProperty.Register(nameof(MoveVertically), typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool),
                 delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                 {
                     var sender = ((LayoutPath)o);
                     if (sender.CHILDREN != null)
                     {
                         foreach (LayoutPathChildWrapper child in sender.CHILDREN.Children)
                         {
                             child.UpdateAlingment(sender.ChildAlignment, sender.MoveVertically, sender.FlipItems);
                         }
                     }
                     transformToProgress(o, e);
                 }));

            FlipItemsProperty = DependencyProperty.Register("FlipItems", typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool),
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    var sender = ((LayoutPath)o);
                    if (sender.CHILDREN != null)
                    {
                        foreach (LayoutPathChildWrapper child in sender.CHILDREN.Children)
                        {
                            child.UpdateAlingment(sender.ChildAlignment, sender.MoveVertically, sender.FlipItems);
                        }
                    }
                    transformToProgress(o, e);
                }));

            ChildProgressProperty = DependencyProperty.RegisterAttached("ChildProgress", typeof(double), typeof(LayoutPath), new PropertyMetadata(double.NaN,
            delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
            {
                transformToProgress(o, e);
            }));


            ItemsPaddingProperty = DependencyProperty.Register(nameof(ItemsPadding), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double), (o, e) => transformToProgress(o, e)));
            OrientToPathProperty = DependencyProperty.Register(nameof(OrientToPath), typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool), (o, e) => transformToProgress(o, e)));
            StackAtStartProperty = DependencyProperty.Register("StackAtStart", typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool), (o, e) => transformToProgress(o, e)));
            StackAtEndProperty = DependencyProperty.Register("StackAtEnd", typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool), (o, e) => transformToProgress(o, e)));
            ChildEasingFunctionProperty = DependencyProperty.Register("EasingFunctionBase", typeof(EasingFunctionBase), typeof(LayoutPath), new PropertyMetadata(default(EasingFunctionBase), (o, e) => transformToProgress(o, e)));
        }

        #endregion

        #region dependency properties

        /// <summary>
        /// Set the distance from start, where <see cref="Children"/> will be transformed (value in Percent 0-100)
        /// </summary>
        public double Progress { get { return (double)GetValue(ProgressProperty); } set { SetValue(ProgressProperty, value); } }
        public static readonly DependencyProperty ProgressProperty;

        /// <summary>
        /// Describes how content is resized to fill its allocated space 
        /// </summary>
        public Stretch Stretch { get { return (Stretch)GetValue(StretchProperty); } set { SetValue(StretchProperty, value); } }
        public static readonly DependencyProperty StretchProperty;

        /// <summary>
        /// Sets the visibility of <see cref="Path"/>
        /// </summary>
        public bool PathVisible { get { return (bool)GetValue(PathVisibleProperty); } set { SetValue(PathVisibleProperty, value); } }
        public static readonly DependencyProperty PathVisibleProperty;

        /// <summary>
        /// Sets the geometry that will be used for translating <see cref="Children"/>
        /// </summary>
        public Geometry Path { get { return (Geometry)GetValue(PathProperty); } set { SetValue(PathProperty, value); } }
        public static readonly DependencyProperty PathProperty;

        /// <summary>
        /// Set true if you want <see cref="Children"/> to rotate when moving along <see cref="Path"/>
        /// </summary>
        public bool OrientToPath { get { return (bool)GetValue(OrientToPathProperty); } set { SetValue(OrientToPathProperty, value); } }
        public static readonly DependencyProperty OrientToPathProperty;

        /// <summary>
        /// Sets the distance that <see cref="Children"/> will keep between each other (in percent of total length).
        /// 
        /// Example: setting ItemsPadding to 20 and progress being to 50, first element will be at progress=50, second at progress=30, third at progress=10 etc..
        /// </summary>
        public double ItemsPadding { get { return (double)GetValue(ItemsPaddingProperty); } set { SetValue(ItemsPaddingProperty, value); } }
        public static readonly DependencyProperty ItemsPaddingProperty;


        /// <summary>
        /// Gets the <see cref="Point"/> at fraction length of <see cref="Path"/> on current <see cref="Progress"/>
        /// Smoothness does not affect CurrentPosition
        /// </summary>
        public Point CurrentPosition { get { return (Point)GetValue(CurrentPositionProperty); } private set { SetValue(CurrentPositionProperty, value); } }
        public static readonly DependencyProperty CurrentPositionProperty;

        /// <summary>
        ///  Gets the degrees at fraction length of <see cref="Path"/> on current <see cref="Progress"/>
        ///  Smoothness does not affect CurrentRotation
        /// </summary>
        public double CurrentRotation { get { return (double)GetValue(CurrentRotationProperty); } private set { SetValue(CurrentRotationProperty, value); } }
        public static readonly DependencyProperty CurrentRotationProperty;

        /// <summary>
        /// Gets the length distance for <see cref="CurrentPosition"/>
        /// </summary>
        public double CurrentLength { get { return (double)GetValue(CurrentLengthProperty); } private set { SetValue(CurrentLengthProperty, value); } }
        public static readonly DependencyProperty CurrentLengthProperty;

        /// <summary>
        /// Set true to rotate children by 90 degrees, 
        /// </summary>
        public bool MoveVertically { get { return (bool)GetValue(MoveVerticallyProperty); } set { SetValue(MoveVerticallyProperty, value); } }
        public static readonly DependencyProperty MoveVerticallyProperty;

        /// <summary>
        /// Sets the position of items along path
        /// </summary>
        public ChildAlignment ChildAlignment { get { return (ChildAlignment)GetValue(ChildAlignmentProperty); } set { SetValue(ChildAlignmentProperty, value); } }
        public static readonly DependencyProperty ChildAlignmentProperty;

        /// <summary>
        /// Set true to rotate items by 180 degrees
        /// </summary>
        public bool FlipItems { get { return (bool)GetValue(FlipItemsProperty); } set { SetValue(FlipItemsProperty, value); } }
        public static readonly DependencyProperty FlipItemsProperty;

        /// <summary>
        /// Sets child progress to 0 if it is lower than 0. 
        /// This results items to be stacked at the beginning of path if <see cref="ItemsPadding"/> is specified and progress values are near 0. 
        /// </summary>
        public bool StackAtStart { get { return (bool)GetValue(StackAtStartProperty); } set { SetValue(StackAtStartProperty, value); } }
        public static readonly DependencyProperty StackAtStartProperty;

        /// <summary>
        /// Sets child progress to 100 if it is greater than 100. 
        /// This results items to be stacked at the end of path for progress values greater than 100. 
        /// </summary>
        public bool StackAtEnd { get { return (bool)GetValue(StackAtEndProperty); } set { SetValue(StackAtEndProperty, value); } }
        public static readonly DependencyProperty StackAtEndProperty;

        /// <summary>
        /// Smooths children rotation.
        /// </summary>
        public int SmoothRotation { get { return (int)GetValue(SmoothRotationProperty); } set { SetValue(SmoothRotationProperty, value); } }
        public static readonly DependencyProperty SmoothRotationProperty;

        /// <summary>
        /// Smooths children translation
        /// </summary>
        public int SmoothTranslation { get { return (int)GetValue(SmoothTranslationProperty); } set { SetValue(SmoothTranslationProperty, value); } }
        public static readonly DependencyProperty SmoothTranslationProperty;

        /// <summary>
        /// Sets the easing function each children will have when moving along path.
        /// </summary>
        public EasingFunctionBase ChildEasingFunction { get { return (EasingFunctionBase)GetValue(ChildEasingFunctionProperty); } set { SetValue(ChildEasingFunctionProperty, value); } }
        public static readonly DependencyProperty ChildEasingFunctionProperty;

        #endregion

        #region attached properties

        public static readonly DependencyProperty IsMovableProperty = DependencyProperty.RegisterAttached("IsMovable", typeof(Boolean), typeof(LayoutPath), new PropertyMetadata(true));

        public static void SetIsMovable(UIElement element, Boolean value)
        {
            element.SetValue(IsMovableProperty, value);
        }
        public static Boolean GetIsMovable(UIElement element)
        {
            return (Boolean)element.GetValue(IsMovableProperty);
        }


        public static readonly DependencyProperty ChildProgressProperty;

        public static void SetChildProgress(UIElement element, double value)
        {
            element.SetValue(ChildProgressProperty, value);
        }
        public static double GetChildProgress(UIElement element)
        {
            return (double)element.GetValue(ChildProgressProperty);
        }

        public static readonly DependencyProperty ProgressOffsetProperty = DependencyProperty.RegisterAttached("ProgressOffset", typeof(double), typeof(LayoutPath), new PropertyMetadata(0.0));

        public static void SetProgressOffset(UIElement element, double value)
        {
            element.SetValue(ProgressOffsetProperty, value);
        }
        public static double GetProgressOffset(UIElement element)
        {
            return (double)element.GetValue(ProgressOffsetProperty);
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

                var childProgress = GetChildProgress((UIElement)wrapper.Content);
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
                if (StackAtStart)
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
                if (StackAtEnd)
                {
                    progress = 100;
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

            //try smooth rotation
            if (!double.IsNaN(wrapper.ProgressDistance) && SmoothRotation > 0 && wrapper.ProgressDistance > 0)
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
                wrapper.Rotation = wrapper.Rotation = (rotation * SmoothRotation * 0.2 + rotationTheta * wrapper.ProgressDistance) / (SmoothRotation * 0.2 + wrapper.ProgressDistance);
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

            if (!double.IsNaN(wrapper.ProgressDistance) && SmoothTranslation > 0 && wrapper.ProgressDistance > 0)
            {
                wrapper.TranslateX = (wrapper.TranslateX * SmoothTranslation * 0.2 / wrapper.ProgressDistance + translateX) / (SmoothTranslation * 0.2 / wrapper.ProgressDistance + 1);
                wrapper.TranslateY = (wrapper.TranslateY * SmoothTranslation * 0.2 / wrapper.ProgressDistance + translateY) / (SmoothTranslation * 0.2 / wrapper.ProgressDistance + 1);
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
