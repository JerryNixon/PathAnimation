﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using CustomControls.ExtendedSegments;

namespace CustomControls.Controls
{
    [ContentProperty(Name = "Children")]
    public class LayoutPath : ContentControl
    {
        #region variables

        /// <summary>
        /// Contains information about potential blank space on the left and top of our path
        /// </summary>
        private Point _pathOffset = new Point(double.MaxValue, double.MaxValue);

        /// <summary>
        /// A grid containing all items, including path
        /// </summary>
        private readonly Grid _containerGrid = new Grid();

        /// <summary>
        /// contains all segments added by initial analysis
        /// </summary>
        private List<ExtendedSegmentBase> _allSegments;

        /// <summary>
        /// Top level container for this control
        /// </summary>
        private readonly Viewbox _viewbox = new Viewbox();

        /// <summary>
        /// The path object, generated by specified <see cref="Path"/> property
        /// </summary>
        private Path _pathObject;

        /// <summary>
        /// Children that will be animated along <see cref="Path"/>
        /// </summary>
        public IList<UIElement> Children => _children;
        private readonly ObservableCollection<UIElement> _children = new ObservableCollection<UIElement>();

        #endregion

        #region constructors

        public LayoutPath()
        {
            _viewbox.Child = _containerGrid;
            Content = _viewbox;

            _children.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs args)
            {

                if (args.NewItems != null)
                {
                    foreach (var child in args.NewItems)
                    {
                        ContentControl wrapper = new ContentControl();
                        if (child != Path)
                            wrapper.Content = child;
                        wrapper.HorizontalAlignment = HorizontalAlignment.Left;
                        wrapper.VerticalAlignment = VerticalAlignment.Top;
                        wrapper.RenderTransform = new CompositeTransform();

                        if (child != Path)
                            //+1 because on index 0 we have the Path
                            _containerGrid.Children.Insert(args.NewStartingIndex + 1, wrapper);
                        else
                            _containerGrid.Children.Add(_pathObject);
                    }
                }

                if (args.OldItems != null)
                {
                    foreach (var child in args.OldItems)
                    {
                        var wrapper = _containerGrid.Children.Where(x => x is ContentControl).FirstOrDefault(x => ((ContentControl)x).Content == child);
                        if (wrapper != null)
                            _containerGrid.Children.Remove(wrapper);
                    }
                }

                TransformToProgress(Progress);
            };

            Loaded += delegate
            {
                AnalyzeSegments();
                TransformToProgress(Progress);
            };

            _containerGrid.Loaded += delegate
            {
                TransformToProgress(Progress);
            };
        }

        static LayoutPath()
        {
            StretchProperty = DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(LayoutPath), new PropertyMetadata(Stretch.None,
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    ((LayoutPath)o)._viewbox.Stretch = (Stretch)e.NewValue;
                }));

            ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(LayoutPath), new PropertyMetadata(0,
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    ((LayoutPath)o).TransformToProgress((double)e.NewValue);
                }));

            PathVisibleProperty = DependencyProperty.Register("PathVisible", typeof(bool), typeof(LayoutPath), new PropertyMetadata(true,
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    ((LayoutPath)o)._pathObject.Opacity = (bool)e.NewValue ? 0.5 : 0;
                }));

            PathProperty = DependencyProperty.Register(nameof(Path), typeof(Geometry), typeof(LayoutPath), new PropertyMetadata(null,
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    ((LayoutPath)o).UpdateGeometryData((Geometry)e.NewValue);
                }));

            CurrentPositionProperty = DependencyProperty.Register(nameof(CurrentPosition), typeof(Point), typeof(LayoutPath), new PropertyMetadata(false,
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    ((LayoutPath)o).TransformToProgress(((LayoutPath)o).Progress);
                }));

            ItemsPaddingProperty = DependencyProperty.Register("ItemsPadding", typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double),
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    ((LayoutPath)o).TransformToProgress(((LayoutPath)o).Progress);
                }));

            OrientToPathProperty = DependencyProperty.Register("OrientToPath", typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool),
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    ((LayoutPath)o).TransformToProgress(((LayoutPath)o).Progress);
                }));

            PathLengthProperty = DependencyProperty.Register("PathLength", typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double)));

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
        public Stretch Stretch { get { return _viewbox.Stretch; } set { SetValue(StretchProperty, value); } }
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
        /// Total circumferential length of <see cref="Path"/>
        /// </summary>
        public double PathLength { get { return (double)GetValue(PathLengthProperty); } private set { SetValue(PathLengthProperty, value); } }
        public static readonly DependencyProperty PathLengthProperty;

        /// <summary>
        /// Gets the <see cref="Point"/> at the perimeter of <see cref="Path"/> on current <see cref="Progress"/>
        /// </summary>
        public Point CurrentPosition { get { return (Point)GetValue(CurrentPositionProperty); } private set { SetValue(CurrentPositionProperty, value); } }
        public static readonly DependencyProperty CurrentPositionProperty;

        #endregion

        #region methods

        private void UpdateGeometryData(Geometry data)
        {
            if (Children.Contains(_pathObject))
                Children.Remove(_pathObject);

            Path p = new Path();
            p.Data = data;
            p.Fill = new SolidColorBrush(Colors.White);
            p.Opacity = PathVisible ? 0.5 : 0;
            p.IsHitTestVisible = false;
            p.HorizontalAlignment = HorizontalAlignment.Left;
            p.VerticalAlignment = VerticalAlignment.Top;
            _pathObject = p;

            _containerGrid.Children.Insert(0, p);
        }

        private void AnalyzeSegments()
        {
            _allSegments = new List<ExtendedSegmentBase>();
            var pg = (PathGeometry)Path;
            double pathLength = 0;

            foreach (var figure in pg.Figures)
            {
                var figureSegments = new List<ExtendedSegmentBase>();
                if (figure.IsClosed)
                    figure.Segments.Add(new LineSegment() { Point = figure.StartPoint });//close path by adding again startPoint
                for (int i = 0; i < figure.Segments.Count; i++)
                {
                    Point startPoint;
                    //determine start point of segment
                    startPoint = (i == 0 ? figure.StartPoint : figureSegments[i - 1].EndPoint);

                    if (figure.Segments[i] is LineSegment)
                        figureSegments.Add(new ExtendedLineSegment(figure.Segments[i], startPoint));
                    else if (figure.Segments[i] is BezierSegment)
                        figureSegments.Add(new ExtendedBezierSegment(figure.Segments[i], startPoint));
                    else if (figure.Segments[i] is QuadraticBezierSegment)
                        figureSegments.Add(new ExtendedQuadraticBezierSegment(figure.Segments[i], startPoint));
                    else if (figure.Segments[i] is ArcSegment)
                        figureSegments.Add(new ExtendedArcSegment(figure.Segments[i], startPoint));
                    else if (figure.Segments[i] is PolyLineSegment || figure.Segments[i] is PolyBezierSegment || figure.Segments[i] is PolyQuadraticBezierSegment)
                        figureSegments.Add(new ExtendedPolySegmentBase(figure.Segments[i], startPoint));
                }

                foreach (ExtendedSegmentBase t in figureSegments)
                {
                    t.DistanceFromStart = pathLength += t.SegmentLength;
                }

                _allSegments.AddRange(figureSegments);
            }

            for (int i = 0; i < _allSegments.Count; i++)
            {
                _allSegments[i].EndsAtPercent = _allSegments[i].DistanceFromStart / pathLength;
            }
            for (int i = 1; i < _allSegments.Count; i++)
            {
                _allSegments[i].StartsAtPercent = _allSegments[i - 1].EndsAtPercent;
            }

            foreach (var segment in _allSegments)
            {
                for (double i = 0; i < 1; i = i + 0.01)
                {
                    var point = segment.GetPointAt(i);
                    if (point.X < _pathOffset.X)
                        _pathOffset.X = point.X;
                    if (point.Y < _pathOffset.Y)
                        _pathOffset.Y = point.Y;
                }
            }

            _pathObject.Margin = new Thickness(-_pathOffset.X, -_pathOffset.Y, 0, 0);

            PathLength = pathLength;

        }

        private void GetPointAtFractionLength(double progress, out Point point, out double rotationTheta)
        {
            if (progress < 0)
                progress = 0;
            progress = (progress % 100) / 100.0;//make sure that 0 <= percent <= 1
                                                //get segment that falls on this percent
            var segment = _allSegments.First(c => c.EndsAtPercent >= progress);

            //find range of segment
            double range = segment.EndsAtPercent - segment.StartsAtPercent;
            progress = progress - segment.StartsAtPercent; //tranfer to 0
            progress = progress / range;//convert to local percent

            point = segment.GetPointAt(progress);//get point at percent for segment
            rotationTheta = segment.GetDegreesAt(progress);
        }

        private void TransformToProgress(double progress)
        {
            if (_allSegments == null)
                return;
            var children = _containerGrid.Children.Where(x => x != _pathObject).ToArray();

            for (int i = 0; i < children.Count(); i++)
            {
                double childPercent = progress - (i * ItemsPadding);
                Point childPoint;
                double rotationTheta;
                GetPointAtFractionLength(childPercent, out childPoint, out rotationTheta);

                if (i == 0)
                    CurrentPosition = childPoint;

                var child = (ContentControl)children[i];
                CompositeTransform transform = (CompositeTransform)child.RenderTransform;
                transform.TranslateX = childPoint.X - ((FrameworkElement)child.Content).ActualWidth / 2.0 - _pathOffset.X;
                transform.TranslateY = childPoint.Y - ((FrameworkElement)child.Content).ActualHeight / 2.0 - _pathOffset.Y;
                transform.CenterX = child.RenderSize.Width / 2.0;
                transform.CenterY = child.RenderSize.Height / 2.0;

                if (OrientToPath)
                {
                    transform.Rotation = rotationTheta;
                }
                else
                {
                    transform.Rotation = 0;
                }
            }

            if (!children.Any())
            {
                Point childPoint;
                double rotationTheta;
                GetPointAtFractionLength(progress, out childPoint, out rotationTheta);
                CurrentPosition = childPoint;
            }
        }

        #endregion

    }
}
