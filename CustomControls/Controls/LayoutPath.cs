using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using CustomControls.ExtendedSegments;

namespace CustomControls.Controls
{
    [ContentProperty(Name = "Children")]
    public class LayoutPath : ContentControl, INotifyPropertyChanged
    {
        #region private variables
        private readonly Grid _containerGrid = new Grid();
        private List<ExtendedSegmentBase> _allSegments;
        private readonly Viewbox _viewbox = new Viewbox();

        /// <summary>
        /// used to indetify possible blank space on the left and top of our path
        /// </summary>
        private Point _pathOffset = new Point(double.MaxValue, double.MaxValue);
        #endregion

        #region private property members
        private Geometry _pathData;
        private bool _pathVisible = true;
        private bool _orientToPath = true;
        private bool _stretch;
        private double _totalLength;
        private double _stepDistance;
        private readonly ObservableCollection<UIElement> _children = new ObservableCollection<UIElement>();
        private Point _currentPoint;
        #endregion

        public IList<UIElement> Children => _children;

        public LayoutPath() : base()
        {
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
                             _containerGrid.Children.Add(Path);
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

                 GoToPercent(Percent);
             };

            Loaded += delegate (object sender, RoutedEventArgs args)
            {
                AnalyzeSegments();
                UpdateStretch();
            };

            _containerGrid.Loaded+= delegate(object sender, RoutedEventArgs args)
            {
                GoToPercent(Percent);
            };
        }

        #region Percent Dependency Property
        public static readonly DependencyProperty PercentProperty = DependencyProperty.Register("Percent", typeof(Double), typeof(LayoutPath), new PropertyMetadata(0.0, PercentPropertyChangedCallback));

        public Double Percent
        {
            get { return (Double)GetValue(PercentProperty); }
            set { SetValue(PercentProperty, value); }
        }

        private static void PercentPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var uc = (LayoutPath)dependencyObject;
            uc.GoToPercent((double)dependencyPropertyChangedEventArgs.NewValue);
        }
        #endregion

        #region properties

        public bool PathVisible
        {
            get { return _pathVisible; }
            set
            {
                if (value.Equals(_pathVisible))
                    return;
                _pathVisible = value;
                OnPropertyChanged();
                if (Path != null)
                    Path.Opacity = PathVisible ? 0.2 : 0;
            }
        }

        public bool Stretch
        {
            get { return _stretch; }
            set
            {
                if (value.Equals(_stretch))
                    return;
                _stretch = value;
                OnPropertyChanged();
                UpdateStretch();
            }
        }

        /// <summary>
        /// Current Point of current progress
        /// </summary>
        public Point CurrentPoint
        {
            get { return _currentPoint; }
            private set
            {
                if (value.Equals(_currentPoint))
                    return;
                _currentPoint = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The generated path object. You can access it programmatically only after setting PathData
        /// </summary>
        public Path Path { get; private set; }

        /// <summary>
        /// The path that is used for animating objects
        /// </summary>
        public Geometry PathData
        {
            get { return _pathData; }
            set
            {
                if (this.Children.Contains(Path))
                    this.Children.Remove(Path);

                _pathData = value;
                Path p = new Path();
                p.Data = value;
                p.Fill = new SolidColorBrush(Colors.Gray);
                p.Opacity = PathVisible ? 0.5 : 0;
                p.IsHitTestVisible = false;
                p.HorizontalAlignment = HorizontalAlignment.Left;
                p.VerticalAlignment = VerticalAlignment.Top;
                Path = p;

                _containerGrid.Children.Insert(0, p);
            }
        }

        public bool OrientToPath
        {
            get { return _orientToPath; }
            set
            {
                if(value.Equals(_orientToPath))
                    return;
                _orientToPath = value;
                GoToPercent(Percent);
            }
        }

        /// <summary>
        /// The distance between children in percent
        /// </summary>
        public double StepDistance
        {
            get { return _stepDistance; }
            set
            {
                if (value.Equals(_stepDistance))
                    return;
                _stepDistance = value;
                OnPropertyChanged();
                GoToPercent(Percent);
            }
        }

        /// <summary>
        /// The total length of the path's shape
        /// </summary>
        public double TotalLength
        {
            get { return _totalLength; }
            private set
            {
                if (value.Equals(_totalLength))
                    return;
                _totalLength = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region methods
        private void UpdateStretch()
        {
            Content = null;
            _viewbox.Child = null;
            if (Stretch)
            {
                _viewbox.Child = _containerGrid;
                Content = _viewbox;
            }
            else
            {
                Content = _containerGrid;
            }
        }

        private void AnalyzeSegments()
        {
            _allSegments = new List<ExtendedSegmentBase>();
            var pg = (PathGeometry)this.PathData;
            TotalLength = 0;

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
                    t.DistanceFromStart = TotalLength += t.SegmentLength;
                }

                _allSegments.AddRange(figureSegments);
            }

            for (int i = 0; i < _allSegments.Count; i++)
            {
                _allSegments[i].EndsAtPercent = _allSegments[i].DistanceFromStart / TotalLength;
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

            Path.Margin = new Thickness(-_pathOffset.X, -_pathOffset.Y, 0, 0);



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

        private void GoToPercent(double perc)
        {
            if (_allSegments == null)
                return;
            var children = _containerGrid.Children.Where(x => x != Path).ToArray();

            for (int i = 0; i < children.Count(); i++)
            {
                double childPercent = perc - (i * StepDistance);
                Point childPoint;
                double rotationTheta;
                GetPointAtFractionLength(childPercent, out childPoint, out rotationTheta);

                if (i == 0)
                    CurrentPoint = childPoint;

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
                GetPointAtFractionLength(perc, out childPoint, out rotationTheta);
                CurrentPoint = childPoint;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
