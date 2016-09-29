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
using Windows.UI.Xaml.Shapes;
using CustomControls.Enums;
using CustomControls.ExtendedSegments;

namespace CustomControls.Controls
{
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
        public IList<UIElement> Children => _children;
        private readonly ObservableCollection<UIElement> _children = new ObservableCollection<UIElement>();

        public ExtendedPathGeometry ExtendedGeometry { get; private set; }

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
                            var wrapper =
                                CHILDREN.Children.FirstOrDefault(x => ((LayoutPathChildWrapper)x).Content == child);
                            if (wrapper != null)
                                CHILDREN.Children.Remove(wrapper);
                        }
                    }
                }
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
                ((LayoutPath)o).TransformToProgress(((LayoutPath)o).Progress);
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


            ItemsPaddingProperty = DependencyProperty.Register(nameof(ItemsPadding), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double), (o, e) => transformToProgress(o, e)));
            OrientToPathProperty = DependencyProperty.Register(nameof(OrientToPath), typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool), (o, e) => transformToProgress(o, e)));
            StackAtStartProperty = DependencyProperty.Register("StackAtStart", typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool), (o, e) => transformToProgress(o, e)));
            StackAtEndProperty = DependencyProperty.Register("StackAtEnd", typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool), (o, e) => transformToProgress(o, e)));

        }

        #endregion

        #region dependency properties

        /// <summary>
        /// Set the distance from the start, where <see cref="Children"/> will be transformed (value in Percent 0-100)
        /// </summary>
        public double Progress { get { return (double)GetValue(ProgressProperty); } set { SetValue(ProgressProperty, value); } }
        public static readonly DependencyProperty ProgressProperty;

        /// <summary>
        /// Describes how content is resized to fill its allocated space 
        /// </summary>
        public Stretch Stretch { get { return (Stretch)GetValue(StretchProperty); } set { SetValue(StretchProperty, value); } }
        public static readonly DependencyProperty StretchProperty;

        /// <summary>
        /// Sets the visibility of the <see cref="Path"/>
        /// </summary>
        public bool PathVisible { get { return (bool)GetValue(PathVisibleProperty); } set { SetValue(PathVisibleProperty, value); } }
        public static readonly DependencyProperty PathVisibleProperty;

        /// <summary>
        /// Set the geometry that it will be used for the translation of <see cref="Children"/>
        /// </summary>
        public Geometry Path { get { return (Geometry)GetValue(PathProperty); } set { SetValue(PathProperty, value); } }
        public static readonly DependencyProperty PathProperty;

        /// <summary>
        /// Set true if you want <see cref="Children"/> to rotate when moving along <see cref="Path"/>
        /// </summary>
        public bool OrientToPath { get { return (bool)GetValue(OrientToPathProperty); } set { SetValue(OrientToPathProperty, value); } }
        public static readonly DependencyProperty OrientToPathProperty;

        /// <summary>
        /// Sets the distance in percent of <see cref="PathLength"/> that items will have along <see cref="Path"/>
        /// </summary>
        public double ItemsPadding { get { return (double)GetValue(ItemsPaddingProperty); } set { SetValue(ItemsPaddingProperty, value); } }
        public static readonly DependencyProperty ItemsPaddingProperty;


        /// <summary>
        /// Gets the <see cref="Point"/> at the perimeter of <see cref="Path"/> on current <see cref="Progress"/>
        /// </summary>
        public Point CurrentPosition { get { return (Point)GetValue(CurrentPositionProperty); } private set { SetValue(CurrentPositionProperty, value); } }
        public static readonly DependencyProperty CurrentPositionProperty;

        /// <summary>
        /// Gets the length distance for <see cref="CurrentPosition"/>
        /// </summary>
        public double CurrentLength { get { return (double)GetValue(CurrentLengthProperty); } private set { SetValue(CurrentLengthProperty, value); } }
        public static readonly DependencyProperty CurrentLengthProperty;

        public bool MoveVertically { get { return (bool)GetValue(MoveVerticallyProperty); } set { SetValue(MoveVerticallyProperty, value); } }
        public static readonly DependencyProperty MoveVerticallyProperty;

        public ChildAlignment ChildAlignment { get { return (ChildAlignment)GetValue(ChildAlignmentProperty); } set { SetValue(ChildAlignmentProperty, value); } }
        public static readonly DependencyProperty ChildAlignmentProperty;

        public double CurrentRotation { get { return (double)GetValue(CurrentRotationProperty); } private set { SetValue(CurrentRotationProperty, value); } }
        public static readonly DependencyProperty CurrentRotationProperty;

        public bool FlipItems { get { return (bool)GetValue(FlipItemsProperty); } set { SetValue(FlipItemsProperty, value); } }
        public static readonly DependencyProperty FlipItemsProperty;

        public bool StackAtStart { get { return (bool)GetValue(StackAtStartProperty); } set { SetValue(StackAtStartProperty, value); } }
        public static readonly DependencyProperty StackAtStartProperty;

        public bool StackAtEnd { get { return (bool)GetValue(StackAtEndProperty); } set { SetValue(StackAtEndProperty, value); } }
        public static readonly DependencyProperty StackAtEndProperty;

        public int SmoothRotation { get { return (int)GetValue(SmoothRotationProperty); } set { SetValue(SmoothRotationProperty, value); } }
        public static readonly DependencyProperty SmoothRotationProperty;

        public int SmoothTranslation { get { return (int)GetValue(SmoothTranslationProperty); } set { SetValue(SmoothTranslationProperty, value); } }
        public static readonly DependencyProperty SmoothTranslationProperty;

        #endregion

        #region methods

        private bool transformedOnce = false;
        private double previousProgres = 0;
        private double progressDistanceAVG = 1;
        private object progressLocker = new object();
        private void TransformToProgress(double progress)
        {
            if (ExtendedGeometry == null || CHILDREN == null)
                return;



            var progDist = Math.Abs(progress - previousProgres);
            if (progDist > 50 || progDist == 0)
                progDist = progressDistanceAVG;

            var children = CHILDREN.Children.ToArray();

            for (int i = 0; i < children.Count(); i++)
            {
                double childPercent = progress - (i * ItemsPadding);

                if (childPercent < 0)
                {
                    if (StackAtStart)
                        childPercent = 0;
                    else
                        childPercent = 100 + childPercent;
                }
                else if (childPercent > 100)
                {
                    if (StackAtEnd)
                        childPercent = 100;
                }

                Point childPoint;
                double rotationTheta;
                ExtendedGeometry.GetPointAtFractionLength(childPercent, out childPoint, out rotationTheta);

                if (i == 0)
                    CurrentPosition = childPoint;

                var wrapper = (LayoutPathChildWrapper)children[i];
                var wrappedChild = wrapper.Content as FrameworkElement;
                var childWidth = wrappedChild.ActualWidth;
                var childHeight = wrappedChild.ActualHeight;

                CompositeTransform wrapperTransform = (CompositeTransform)wrapper.RenderTransform;

                if (!OrientToPath)
                    rotationTheta = 0;

                if (MoveVertically)
                    rotationTheta += 90;

                if (FlipItems)
                    rotationTheta += 180;

                rotationTheta = rotationTheta % 360;



                if (transformedOnce && SmoothRotation > 0 && progDist > 0)
                {
                    var degreesDistance = Math.Max(rotationTheta, wrapperTransform.Rotation) - Math.Min(rotationTheta, wrapperTransform.Rotation);
                    while (degreesDistance > 180)
                    {
                        if (rotationTheta > wrapperTransform.Rotation)
                            wrapperTransform.Rotation = wrapperTransform.Rotation + 360;
                        else
                            wrapperTransform.Rotation = wrapperTransform.Rotation - 360;
                        degreesDistance = Math.Max(rotationTheta, wrapperTransform.Rotation) - Math.Min(rotationTheta, wrapperTransform.Rotation);
                    }
                    wrapperTransform.Rotation = (wrapperTransform.Rotation * SmoothRotation * 0.2 + rotationTheta * progDist) / (SmoothRotation * 0.2 + progDist);
                }
                else
                {
                    wrapperTransform.Rotation = rotationTheta;
                }

                double translateX = childPoint.X - ExtendedGeometry.PathOffset.X;
                double translateY = childPoint.Y - ExtendedGeometry.PathOffset.Y;

                //center align child
                translateX -= childWidth / 2.0;
                translateY -= childHeight / 2.0;
                wrapperTransform.CenterX = childWidth / 2.0;
                wrapperTransform.CenterY = childHeight / 2.0;

                if (transformedOnce && SmoothTranslation > 0 && progDist > 0)
                {
                    wrapperTransform.TranslateX = (wrapperTransform.TranslateX * SmoothTranslation * 0.2 / progDist + translateX) / (SmoothTranslation * 0.2 / progDist + 1);
                    wrapperTransform.TranslateY = (wrapperTransform.TranslateY * SmoothTranslation * 0.2 / progDist + translateY) / (SmoothTranslation * 0.2 / progDist + 1);
                }
                else
                {
                    wrapperTransform.TranslateX = translateX;
                    wrapperTransform.TranslateY = translateY;
                }

                if (i == 0)
                    CurrentRotation = rotationTheta;
            }

            if (!children.Any())
            {
                var tmp = ExtendedGeometry.GetPointAtFractionLength(progress);
                CurrentPosition = tmp.Item1;
                CurrentRotation = tmp.Item2;
            }

            CurrentLength = ExtendedGeometry.PathLength * (progress / 100.0);
            transformedOnce = true;
            previousProgres = progress;
            progressDistanceAVG = (progressDistanceAVG + progDist) / 2.0;
        }

        #endregion

    }
}
