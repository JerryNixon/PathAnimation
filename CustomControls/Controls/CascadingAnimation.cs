using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using CustomControls.ExtendedSegments;
using CustomControls.Extensions;

namespace CustomControls.Controls
{
    [ContentProperty(Name = "Children")]
    public class CascadingAnimation : ContentControl
    {
        public event Action Completed;

        private CascadingAnimation _startAfter;

        private CascadingAnimation _nextAnimation;

        public static readonly DependencyProperty StartAfterProperty = DependencyProperty.Register(
            "StartAfter", typeof(CascadingAnimation), typeof(CascadingAnimation), new PropertyMetadata(default(CascadingAnimation),
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    var sender = (CascadingAnimation)o;
                    var whenCompleted = (CascadingAnimation)e.NewValue;

                    if (sender._startAfter != null)
                    {
                        sender._startAfter.Completed -= sender.AutoStart;
                    }
                    sender._startAfter = whenCompleted;
                    sender._startAfter.Completed += sender.AutoStart;
                    whenCompleted._nextAnimation = sender;
                }));

        public CascadingAnimation StartAfter
        {
            get { return (CascadingAnimation)GetValue(StartAfterProperty); }
            set { SetValue(StartAfterProperty, value); }
        }

        private void AutoStart()
        {
            PlayAnimation();
        }

        public IList<UIElement> Children => _children;

        Storyboard _storyboard = new Storyboard();
        List<UIElement> _children = new List<UIElement>();
        List<UIElement> _childrenToAnimate = new List<UIElement>();
        private Geometry _cascadingText;

        /// <summary>
        /// The path that is shown when specifying CascadingText
        /// </summary>
        public Path PreviewPath { get; private set; }

        /// <summary>
        /// Setting the path text to animate. Will be overridden if children are also specified.
        /// </summary>
        public Geometry CascadingText
        {
            get { return _cascadingText; }
            set
            {
                _cascadingText = value;
                Path p = new Path();
                p.Data = value;
                p.Fill = new SolidColorBrush(Colors.White);
                p.IsHitTestVisible = false;
                p.HorizontalAlignment = HorizontalAlignment.Left;
                p.VerticalAlignment = VerticalAlignment.Top;
                PreviewPath = p;
                Content = p;
            }
        }

        public async Task InitialiseAsync(bool initialiseQueue = true)
        {
            await Task.Run(async delegate
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
                {
                    if (!_children.Any())
                        AnalyzeSegments();
                    else
                        InitializeChildren();
                });

            });
            if (initialiseQueue && _nextAnimation != null)//cascade initialization
                await _nextAnimation.InitialiseAsync();
        }

        public async Task ResetAsync(bool initialiseQueue = true)
        {
            await Task.Run(async delegate
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
                {
                    foreach (var child in _children)
                    {
                        child.Opacity = FromOpacity;
                    }
                    _storyboard.Stop();
                });

            });
            if (initialiseQueue && _nextAnimation != null)//cascade initialization
                await _nextAnimation.ResetAsync();
        }

        public TimeSpan NextLetterOn { get; set; }
        public TimeSpan DelayExecution { get; set; }

        public double FromOpacity { get; set; }

        public TimeSpan FromOpacityDuration { get; set; }

        public double FromScale { get; set; } = 1;
        public TimeSpan FromScaleDuration { get; set; }

        public Point FromLetterOffset { get; set; }
        public TimeSpan FromLetterOffsetDuration { get; set; }

        public CascadingAnimation()
        {
            FromOpacityDuration = TimeSpan.FromSeconds(0.1);
            NextLetterOn = TimeSpan.FromSeconds(0.05);
            FromLetterOffsetDuration = TimeSpan.FromSeconds(0);
            DelayExecution = TimeSpan.Zero;
            FromLetterOffset = new Point(0, 0);

            _storyboard.Completed += (sender, args) => Completed?.Invoke();

            Loaded += delegate (object sender, RoutedEventArgs args)
            {
                if (DesignMode.DesignModeEnabled)
                {
                    if (_children.Any())
                    {
                        Grid container = new Grid();
                        foreach (var child in _children)
                            container.Children.Add(child);
                        Content = container;
                    }
                }
            };

            CacheMode = new BitmapCache();
            UseLayoutRounding = false;
        }

        private void AnalyzeSegments()
        {
            List<KeyValuePair<Matrix, PathFigure>> ranges = new List<KeyValuePair<Matrix, PathFigure>>();
            var pg = (PathGeometry)this.CascadingText;
            double TotalLength = 0;
            if (pg == null)
                return;

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

                Matrix range = new Matrix(double.MaxValue, double.MinValue, double.MaxValue, double.MinValue, 0, 0);
                foreach (var segment in figureSegments)
                {
                    for (double i = 0; i < 1; i = i + 0.01)
                    {
                        var point = segment.GetPointAt(i);
                        if (point.X <= range.M11)
                            range.M11 = point.X;
                        if (point.X >= range.M12)
                            range.M12 = point.X;

                        if (point.Y <= range.M21)
                            range.M21 = point.Y;
                        if (point.Y >= range.M22)
                            range.M22 = point.Y;
                    }
                }
                ranges.Add(new KeyValuePair<Matrix, PathFigure>(range, figure));
            }

            Grid container = new Grid();
            while (ranges.Any())
            {
                Path path = new Path() { Fill = this.Foreground };
                PathGeometry geometry = new PathGeometry();
                var pair = ranges.First();
                var items = ranges.Where(x => (x.Key.M11 >= pair.Key.M11 && x.Key.M12 <= pair.Key.M12 || x.Key.M11 <= pair.Key.M11 && x.Key.M12 >= pair.Key.M12) &&
                (x.Key.M21 >= pair.Key.M21 && x.Key.M22 <= pair.Key.M22 || x.Key.M21 <= pair.Key.M21 && x.Key.M22 >= pair.Key.M22));

                var min = int.MaxValue;
                var max = 0;
                foreach (var item in items)
                {
                    var index = ranges.IndexOf(item);
                    if (index < min)
                        min = index;
                    if (index > max)
                        max = index;
                }

                foreach (var item in ranges.Skip(min).Take(max + 1).ToList())
                {
                    ranges.Remove(item);
                    geometry.Figures.Add(new PathFigure() { Segments = item.Value.Segments, StartPoint = item.Value.StartPoint, IsClosed = item.Value.IsClosed, IsFilled = item.Value.IsFilled });
                }
                path.Data = geometry;
                path.RenderTransform = new CompositeTransform()
                {
                    CenterX = (geometry.Bounds.Right + geometry.Bounds.Left) / 2.0,
                    CenterY = (geometry.Bounds.Bottom + geometry.Bounds.Top) / 2.0
                };
                _children.Add(path);
                path.Opacity = FromOpacity;
                container.Children.Add(path);
                _childrenToAnimate.Add(path);
            }

            Content = container;
        }

        private void InitializeChildren()
        {
            Grid container = new Grid();
            foreach (FrameworkElement child in _children)
            {
                container.Children.Add(child);
                if (child is Panel)
                {
                    AnalyzePanelChildren(child as Panel);
                }
                else
                {
                    child.Opacity = FromOpacity;
                    child.RenderTransform = new CompositeTransform()
                    {
                        CenterX = child.ActualWidth == 0 ? child.Width / 2.0 : child.ActualWidth / 2.0,
                        CenterY = child.ActualHeight == 0 ? child.Height / 2.0 : child.ActualHeight / 2.0
                    };
                    _childrenToAnimate.Add(child);
                }
            }
            Content = container;
        }



        private void AnalyzePanelChildren(Panel p)
        {
            foreach (FrameworkElement child in p.Children)
            {
                if (child is Panel)
                {
                    AnalyzePanelChildren(child as Panel);
                }
                else
                {
                    child.Opacity = FromOpacity;
                    child.RenderTransform = new CompositeTransform()
                    {
                        CenterX = child.ActualWidth == 0 ? child.Width / 2.0 : child.ActualWidth / 2.0,
                        CenterY = child.ActualHeight == 0 ? child.Height / 2.0 : child.ActualHeight / 2.0
                    };
                    _childrenToAnimate.Add(child);
                }
            }
        }

        public void PlayAnimation()
        {
            _storyboard.Stop();
            _storyboard.Children.Clear();
            for (int index = 0; index < _childrenToAnimate.Count; index++)
            {
                var letterPath = _childrenToAnimate[index];

                AddAnimationToStoryboard(index, letterPath, FromOpacity, FromOpacityDuration, "(UIElement.Opacity)");
                if (FromLetterOffsetDuration.TotalMilliseconds > 0)
                {
                    AddAnimationToStoryboard(index, letterPath, FromLetterOffset.Y, FromLetterOffsetDuration,
                        "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
                    AddAnimationToStoryboard(index, letterPath, FromLetterOffset.X, FromLetterOffsetDuration,
                        "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                }
                if (FromScaleDuration.TotalMilliseconds > 0)
                {
                    AddAnimationToStoryboard(index, letterPath, FromScale, FromScaleDuration,
                        "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
                    AddAnimationToStoryboard(index, letterPath, FromScale, FromScaleDuration,
                        "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
                }

            }
            _storyboard.Begin();
        }

        private void AddAnimationToStoryboard(int index, UIElement element, dynamic value, TimeSpan duration, string target)
        {
            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            animation.EnableDependentAnimation = true;
            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame()
            {
                KeyTime = TimeSpan.Zero,
                Value = value
            });

            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame()
            {
                KeyTime = TimeSpan.FromTicks(DelayExecution.Ticks + (index) * NextLetterOn.Ticks),
                Value = value
            });

            animation.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = TimeSpan.FromTicks(DelayExecution.Ticks + (index) * NextLetterOn.Ticks + duration.Ticks),
                Value = 1
            });

            Storyboard.SetTargetProperty(animation, target);
            Storyboard.SetTarget(animation, element);
            _storyboard.Children.Add(animation);
        }

        public string ToXamlDeclaration()
        {
            string color = "{ThemeResource ApplicationForegroundThemeBrush}";
            if (this.Foreground != null)
            {
                if (Foreground is SolidColorBrush)
                {
                    color = ((SolidColorBrush)Foreground).Color.ToString();
                }
            }

            string xaml = "";
            foreach (Path path in _childrenToAnimate.Where(x => x is Path))
            {
                xaml += $"<Path Fill=\"{color}\" Data=\"{path.Data.ToPathGeometryString()}\"/>" + Environment.NewLine;
            }

            return xaml;
        }
    }
}
