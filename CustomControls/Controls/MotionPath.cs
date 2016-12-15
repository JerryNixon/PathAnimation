using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using CustomControls.Converters;
using CustomControls.ExtendedSegments;
using CustomControls.Extensions;
using LayoutPath.Enums;

namespace CustomControls.Controls
{
    public class MotionPath : ContentControl
    {
        #region private members
        private Border CONTENT_WRAPPER;
        private Border VIEWBOX_WARNING;
        private LineGeometry LINE_GEOMETRY;
        private Grid POINT_GRID;
        private Path PATH;
        private LayoutPath LAYOUT_PATH;
        private ContentPresenter CONTENT_PRESENTER;

        private ExtendedLineSegment _movementLine;
        private CompositeTransform _contentTransform;
        private Point _initialContentPoint;
        private TimeSpan _playingDuration;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Storyboard _storyboard = new Storyboard();

        #endregion

        public new double Width
        {
            get { return base.Width; }
            set
            {
                base.Width = value;
                ValidateWidthHeight();
            }
        }

        public new double Height
        {
            get { return base.Height; }
            set
            {
                base.Height = value;
                ValidateWidthHeight();
            }
        }

        private void ValidateWidthHeight()
        {
            if (VIEWBOX_WARNING == null)
                return;

            bool found = false;
            if (double.IsNaN(Width) || double.IsNaN(Height))
            {
                var parent = VisualTreeHelper.GetParent(this);

                while (parent != null)
                {
                    if (parent is Viewbox)
                    {
                        found = true;
                        break;
                    }
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }
            VIEWBOX_WARNING.Visibility = found ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnApplyTemplate()
        {
            CONTENT_WRAPPER = GetTemplateChild(nameof(CONTENT_WRAPPER)) as Border;
            VIEWBOX_WARNING = GetTemplateChild(nameof(VIEWBOX_WARNING)) as Border;
            LINE_GEOMETRY = GetTemplateChild(nameof(LINE_GEOMETRY)) as LineGeometry;
            POINT_GRID = GetTemplateChild(nameof(POINT_GRID)) as Grid;
            PATH = GetTemplateChild(nameof(PATH)) as Path;
            LAYOUT_PATH = GetTemplateChild(nameof(LAYOUT_PATH)) as LayoutPath;
            CONTENT_PRESENTER = GetTemplateChild(nameof(CONTENT_PRESENTER)) as ContentPresenter;

            _contentTransform = (CompositeTransform)CONTENT_WRAPPER.RenderTransform;

            PATH.SetBinding(VisibilityProperty, new Binding()
            {
                Path = new PropertyPath("PathVisibility"),
                Source = this
            });

            LAYOUT_PATH.SetBinding(LayoutPath.PathVisibilityProperty, new Binding()
            {
                Path = new PropertyPath("PathVisibility"),
                Source = this
            });

            LAYOUT_PATH.SetBinding(LayoutPath.ItemOrientationProperty, new Binding()
            {
                Path = new PropertyPath("OrientToPath"),
                Source = this,
                Converter = new BoolToOrientationConverter()
            });

            ValidateWidthHeight();

            UpdatePathData(Path);

            base.OnApplyTemplate();
        }

        #region constructors

        public MotionPath()
        {
            DefaultStyleKey = typeof(MotionPath);
            _storyboard.Completed += delegate (object sender, object o)
            {
                State = AnimationState.Complete;
            };

            Loaded += delegate
            {
                CalculateLineMovement();
            };
        }

        static MotionPath()
        {
            CurrentPointProperty = DependencyProperty.Register("CurrentPoint", typeof(Point), typeof(MotionPath), new PropertyMetadata(new Point(double.NaN, double.NaN)));

            AutoRewindProperty = DependencyProperty.Register("AutoRewind", typeof(bool), typeof(MotionPath), new PropertyMetadata(false));

            DurationProperty = DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(MotionPath), new PropertyMetadata(TimeSpan.FromSeconds(1)));

            StateProperty = DependencyProperty.Register("State", typeof(AnimationState), typeof(MotionPath), new PropertyMetadata(AnimationState.Ready,
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    var sender = (MotionPath)o;
                    var val = (AnimationState)e.NewValue;

                    sender.StateChanged?.Invoke(sender, val);
                    if (val == AnimationState.Complete)
                        sender.Completed?.Invoke(sender);
                }));

            ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(MotionPath), new PropertyMetadata(0.0,
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
                {
                    ((MotionPath)o).ProgressChanged((double)e.NewValue);
                }));

            PathProperty = DependencyProperty.Register(nameof(Path), typeof(Geometry), typeof(MotionPath), new PropertyMetadata(default(Geometry),
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs args)
                {
                    var sender = (MotionPath)o;
                    var data = (Geometry)args.NewValue;
                    sender.UpdatePathData(data);
                }));

            LineAbsoluteStartProperty = DependencyProperty.Register("LineAbsoluteStart", typeof(Point), typeof(MotionPath),
                new PropertyMetadata(new Point(double.NaN, double.NaN), RefreshCalculations));

            LineAbsoluteEndProperty = DependencyProperty.Register("LineAbsoluteEnd", typeof(Point), typeof(MotionPath),
                new PropertyMetadata(new Point(double.NaN, double.NaN), RefreshCalculations));

            LineRelativeEndProperty = DependencyProperty.Register("LineRelativeEnd", typeof(Point), typeof(MotionPath),
                  new PropertyMetadata(new Point(double.NaN, double.NaN), RefreshCalculations));

            PathVisibilityProperty = DependencyProperty.Register("PathVisibility", typeof(Visibility), typeof(MotionPath), new PropertyMetadata(default(Visibility)));

            CurrentTimeProperty = DependencyProperty.Register("CurrentTime", typeof(TimeSpan), typeof(MotionPath), new PropertyMetadata(default(TimeSpan)));

            OrientToPathProperty = DependencyProperty.Register("OrientToPath", typeof(bool), typeof(MotionPath), new PropertyMetadata(default(bool)));

            EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(EasingFunctionBase), typeof(MotionPath), new PropertyMetadata(default(EasingFunctionBase)));
        }

        private static void RefreshCalculations(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var s = (MotionPath)o;
            if (s.State == AnimationState.Running || s.State == AnimationState.Rewinding)
                return;

            s.CalculateLineMovement();
        }

        #endregion

        #region events
        public delegate void CancelEventHandler(object sender, CancelEventArgs args);
        public delegate void StateChangedEventHandler(object sender, AnimationState state);
        public delegate void EventHandler(object sender);
        /// <summary>
        /// Raised when state is ready and we are starting a new animation
        /// </summary>
        public event CancelEventHandler Starting;
        /// <summary>
        /// Raising when a new animation starts
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// Raising when an animation completes
        /// </summary>
        public event EventHandler Completed;
        public event StateChangedEventHandler StateChanged;
        #endregion

        #region properties

        public bool AutoRewind { get { return (bool)GetValue(AutoRewindProperty); } set { SetValue(AutoRewindProperty, value); } }
        public static readonly DependencyProperty AutoRewindProperty;

        public Point CurrentPoint { get { return (Point)GetValue(CurrentPointProperty); } private set { SetValue(CurrentPointProperty, value); } }
        public static readonly DependencyProperty CurrentPointProperty;

        public AnimationState State { get { return (AnimationState)GetValue(StateProperty); } private set { SetValue(StateProperty, value); } }
        public static readonly DependencyProperty StateProperty;

        public TimeSpan Duration { get { return (TimeSpan)GetValue(DurationProperty); } set { SetValue(DurationProperty, value); } }
        public static readonly DependencyProperty DurationProperty;

        public Point LineAbsoluteStart { get { return (Point)GetValue(LineAbsoluteStartProperty); } set { SetValue(LineAbsoluteStartProperty, value); } }
        public static readonly DependencyProperty LineAbsoluteStartProperty;

        public Point LineAbsoluteEnd { get { return (Point)GetValue(LineAbsoluteEndProperty); } set { SetValue(LineAbsoluteEndProperty, value); } }
        public static readonly DependencyProperty LineAbsoluteEndProperty;

        public Point LineRelativeEnd { get { return (Point)GetValue(LineRelativeEndProperty); } set { SetValue(LineRelativeEndProperty, value); } }
        public static readonly DependencyProperty LineRelativeEndProperty;

        public Visibility PathVisibility { get { return (Visibility)GetValue(PathVisibilityProperty); } set { SetValue(PathVisibilityProperty, value); } }
        public static readonly DependencyProperty PathVisibilityProperty;

        public double Progress { get { return (double)GetValue(ProgressProperty); } private set { SetValue(ProgressProperty, value); } }
        public static readonly DependencyProperty ProgressProperty;

        public TimeSpan CurrentTime { get { return (TimeSpan)GetValue(CurrentTimeProperty); } private set { SetValue(CurrentTimeProperty, value); } }
        public static readonly DependencyProperty CurrentTimeProperty;

        public Geometry Path { get { return (Geometry)GetValue(PathProperty); } set { SetValue(PathProperty, value); } }
        public static readonly DependencyProperty PathProperty;

        public bool OrientToPath { get { return (bool)GetValue(OrientToPathProperty); } set { SetValue(OrientToPathProperty, value); } }
        public static readonly DependencyProperty OrientToPathProperty;

        public EasingFunctionBase EasingFunction { get { return (EasingFunctionBase)GetValue(EasingFunctionProperty); } set { SetValue(EasingFunctionProperty, value); } }
        public static readonly DependencyProperty EasingFunctionProperty;

        #endregion

        #region public methods

        public void Start()
        {
            TryStart();
            Started?.Invoke(this);
        }

        public async Task StartAsync()
        {
            await Task.Run(delegate
            {
                TryStart();
            });

            while (_storyboard.GetCurrentState() != ClockState.Active)
            {
                await Task.Delay(10);
            }
            Started?.Invoke(this);
        }

        private void TryStart()
        {
            if (State == AnimationState.Running || State == AnimationState.Rewinding)
                return;

            if (State == AnimationState.Paused)
            {
                _storyboard.Resume();
                return;
            }

            var cancelArgs = new CancelEventArgs();
            Starting?.Invoke(this, cancelArgs);
            if (cancelArgs.Cancel)
                return;
            _playingDuration = Duration;
            StartProgressAnimation();
        }

        private void StartProgressAnimation()
        {
            _storyboard.Stop();
            _storyboard.Children.Clear();

            DoubleAnimationUsingKeyFrames an = new DoubleAnimationUsingKeyFrames();
            an.EnableDependentAnimation = true;
            an.KeyFrames.Add(new DiscreteDoubleKeyFrame()
            {
                KeyTime = TimeSpan.Zero,
                Value = 0
            });

            if (!AutoRewind)
            {
                an.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = Duration,
                    Value = 100,
                    EasingFunction = EasingFunction
                });
            }
            else
            {
                an.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = TimeSpan.FromTicks(Duration.Ticks / 2),
                    Value = 100,
                    EasingFunction = EasingFunction
                });

                an.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = Duration,
                    Value = 200,
                    EasingFunction = EasingFunction
                });
            }

            if (Path == null || true)
            {
                CalculateLineMovement();
                _initialContentPoint = GetChildAbsolutePoint();
                _movementLine = new ExtendedLineSegment(new LineSegment() { Point = LINE_GEOMETRY.EndPoint }, LINE_GEOMETRY.StartPoint);
                _contentTransform.CenterX = _initialContentPoint.X;
                _contentTransform.CenterY = _initialContentPoint.Y;

            }
            Storyboard.SetTargetProperty(an, "(MotionPath.Progress)");
            Storyboard.SetTarget(an, this);

            _storyboard.Children.Add(an);
            _storyboard.Begin();
        }

        public void Reset()
        {
            if (State != AnimationState.Ready)
            {
                _storyboard.Stop();
                State = AnimationState.Ready;
                _contentTransform.TranslateY = 0;
                _contentTransform.TranslateX = 0;
                _contentTransform.Rotation = 0;
            }
            if (Path == null)
                CalculateLineMovement();
        }

        public void RewindNow()
        {
            if (AutoRewind)
            {
                var time = _storyboard.GetCurrentTime();
                if (State == AnimationState.Running || State == AnimationState.Rewinding)
                {
                    _storyboard.Seek(_playingDuration - time);
                    State = State == AnimationState.Running ? AnimationState.Rewinding : AnimationState.Running;
                }
            }
        }


        public void Pause()
        {
            if (State == AnimationState.Running || State == AnimationState.Rewinding)
            {
                _storyboard.Pause();

                State = AnimationState.Paused;
            }
        }

        #endregion

        #region private methods

        private void CalculateLineMovement()
        {
            if (LINE_GEOMETRY == null)
                return;

            if (LineAbsoluteEnd.IsNan() && LineRelativeEnd.IsNan())
                return;

            var tmp = GetChildAbsolutePoint();
            var childX = tmp.X;
            var childY = tmp.Y;

            if (!LineAbsoluteStart.IsNan())
            {
                LINE_GEOMETRY.StartPoint = new Point(LineAbsoluteStart.X, LineAbsoluteStart.Y);

                if (!LineAbsoluteEnd.IsNan())
                {
                    LINE_GEOMETRY.EndPoint = new Point(LineAbsoluteEnd.X, LineAbsoluteEnd.Y);
                }
                else if (!LineRelativeEnd.IsNan())
                {
                    LINE_GEOMETRY.EndPoint = new Point(LineAbsoluteStart.X + LineRelativeEnd.X, LineAbsoluteStart.Y + LineRelativeEnd.Y);
                }
            }
            else if (!LineAbsoluteEnd.IsNan())
            {
                LINE_GEOMETRY.StartPoint = new Point(childX, childY);
                LINE_GEOMETRY.EndPoint = new Point(LineAbsoluteEnd.X, LineAbsoluteEnd.Y);
            }
            else if (!LineRelativeEnd.IsNan())
            {
                LINE_GEOMETRY.StartPoint = new Point(childX, childY);
                LINE_GEOMETRY.EndPoint = new Point(LineRelativeEnd.X + childX, LineRelativeEnd.Y + childY);
            }

            CurrentPoint = new Point(childX, childY);
        }

        private void ProgressChanged(double progress)
        {
            if (progress <= 100 && State != AnimationState.Running)
                State = AnimationState.Running;
            else if (progress > 100 && progress < 200 && State != AnimationState.Rewinding)
                State = AnimationState.Rewinding;

            if (progress > 100)
                progress = 200 - progress;

            if (Path == null)
            {
                if (_movementLine == null)
                    return;
                var p = _movementLine.GetPointAt(progress / 100.0);
                _contentTransform.TranslateX = p.X - _initialContentPoint.X;
                _contentTransform.TranslateY = p.Y - _initialContentPoint.Y;
                if (OrientToPath)
                {
                    _contentTransform.Rotation = _movementLine.GetOrientedDegreesAt(progress / 100.0);
                }
                CurrentPoint = new Point(p.X, p.Y);
            }
            else
            {
                if (progress >= 100)
                    progress = 99.9999999999;//avoid going to beginning of path
                LAYOUT_PATH.PathProgress = progress;
                CurrentPoint = LAYOUT_PATH.CurrentPosition;
            }

            CurrentTime = _storyboard.GetCurrentTime();
        }

        private Point GetChildAbsolutePoint()
        {
            var child = (UIElement)CONTENT_PRESENTER.Content;
            var cont = CONTENT_WRAPPER;
            var transform = cont.TransformToVisual(child);
            Point screenCoords = transform.TransformPoint(new Point(0, 0));
            var childX = -screenCoords.X;
            var childY = -screenCoords.Y;
            return new Point(childX, childY);
        }

        private void UpdatePathData(Geometry data)
        {
            if (PATH == null)
                return;

            if (data != null)
            {
                CONTENT_WRAPPER.Child = null;

                LAYOUT_PATH.Path = data;
                POINT_GRID.Visibility = Visibility.Collapsed;
                LAYOUT_PATH.Visibility = Visibility.Visible;
                LAYOUT_PATH.Children.Add(CONTENT_PRESENTER);
            }
            else
            {
                if (LAYOUT_PATH.Children.Any())
                    LAYOUT_PATH.Children.RemoveAt(0);
                POINT_GRID.Visibility = Visibility.Visible;
                LAYOUT_PATH.Visibility = Visibility.Collapsed;
                CONTENT_WRAPPER.Child = CONTENT_PRESENTER;

            }
        }


        #endregion
    }
}
