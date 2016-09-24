using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using CustomControls.ExtendedSegments;
using LayoutPath.Enums;

namespace CustomControls.Controls
{
    public class MotionPath : ContentControl
    {
        #region private members
        private Border CONTENT_WRAPPER;
        private LineGeometry LINE_GEOMETRY;
        private Grid LAYOUT_ROOT;
        private Path PATH;

        private ExtendedLineSegment _movementLine;
        private CompositeTransform _contentTransform;
        private Point _initialContentPoint = new Point();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Storyboard _storyboard = new Storyboard();
        #endregion

        protected override void OnApplyTemplate()
        {
            CONTENT_WRAPPER = GetTemplateChild(nameof(CONTENT_WRAPPER)) as Border;
            LINE_GEOMETRY = GetTemplateChild(nameof(LINE_GEOMETRY)) as LineGeometry;
            LAYOUT_ROOT = GetTemplateChild(nameof(LAYOUT_ROOT)) as Grid;
            PATH = GetTemplateChild(nameof(PATH)) as Path;

            _contentTransform = (CompositeTransform)CONTENT_WRAPPER.RenderTransform;

            PATH.SetBinding(VisibilityProperty, new Binding()
            {
                Path = new PropertyPath("PathVisibility"),
                Source = this
            });

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

            LineAbsoluteStartProperty = DependencyProperty.Register("LineAbsoluteStart", typeof(Point), typeof(MotionPath),
                new PropertyMetadata(new Point(double.NaN, double.NaN), RefreshCalculations));

            LineAbsoluteEndProperty = DependencyProperty.Register("LineAbsoluteEnd", typeof(Point), typeof(MotionPath),
                new PropertyMetadata(new Point(double.NaN, double.NaN), RefreshCalculations));

            LineRelativeEndProperty = DependencyProperty.Register("LineRelativeEnd", typeof(Point), typeof(MotionPath),
                  new PropertyMetadata(new Point(double.NaN, double.NaN), RefreshCalculations));

            PathVisibilityProperty = DependencyProperty.Register("PathVisibility", typeof(Visibility), typeof(MotionPath), new PropertyMetadata(default(Visibility)));

            CurrentTimeProperty = DependencyProperty.Register("CurrentTime", typeof(TimeSpan), typeof(MotionPath), new PropertyMetadata(default(TimeSpan)));
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

            CalculateLineMovement();
            _initialContentPoint = GetChildAbsolutePoint();
            _movementLine = new ExtendedLineSegment(new LineSegment() { Point = LINE_GEOMETRY.EndPoint }, LINE_GEOMETRY.StartPoint);
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
            an.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = Duration,
                Value = AutoRewind ? 200 : 100
            });

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
            }
            CalculateLineMovement();
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

            if (!HasValue(LineAbsoluteEnd) && !HasValue(LineRelativeEnd))
                return;

            var tmp = GetChildAbsolutePoint();
            var childX = tmp.X;
            var childY = tmp.Y;

            if (HasValue(LineAbsoluteStart))
            {
                LINE_GEOMETRY.StartPoint = new Point(LineAbsoluteStart.X, LineAbsoluteStart.Y);

                if (HasValue(LineAbsoluteEnd))
                {
                    LINE_GEOMETRY.EndPoint = new Point(LineAbsoluteEnd.X, LineAbsoluteEnd.Y);
                }
                else if (HasValue(LineRelativeEnd))
                {
                    LINE_GEOMETRY.EndPoint = new Point(LineAbsoluteStart.X + LineRelativeEnd.X, LineAbsoluteStart.Y + LineRelativeEnd.Y);
                }
            }
            else if (HasValue(LineAbsoluteEnd))
            {
                LINE_GEOMETRY.StartPoint = new Point(childX, childY);
                LINE_GEOMETRY.EndPoint = new Point(LineAbsoluteEnd.X, LineAbsoluteEnd.Y);
            }
            else if (HasValue(LineRelativeEnd))
            {
                LINE_GEOMETRY.StartPoint = new Point(childX, childY);
                LINE_GEOMETRY.EndPoint = new Point(LineRelativeEnd.X + childX, LineRelativeEnd.Y + childY);
            }

            //set minimum values for stretching efficiently control
            //var el = (FrameworkElement)((ContentPresenter)CONTENT_WRAPPER.Child).Content;
            //LAYOUT_ROOT.MinWidth = (LINE_GEOMETRY.StartPoint.X > LINE_GEOMETRY.EndPoint.X
            //                           ? LINE_GEOMETRY.StartPoint.X
            //                           : LINE_GEOMETRY.EndPoint.X) + el.ActualWidth;
            //LAYOUT_ROOT.MinHeight = (LINE_GEOMETRY.StartPoint.Y > LINE_GEOMETRY.EndPoint.Y
            //                           ? LINE_GEOMETRY.StartPoint.Y
            //                           : LINE_GEOMETRY.EndPoint.Y) + el.ActualHeight;

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

            var p = _movementLine.GetPointAt(progress / 100.0);
            _contentTransform.TranslateX = p.X - _initialContentPoint.X;
            _contentTransform.TranslateY = p.Y - _initialContentPoint.Y;

            CurrentPoint = new Point(p.X, p.Y);

            CurrentTime = _storyboard.GetCurrentTime();
        }

        private Point GetChildAbsolutePoint()
        {
            var child = (UIElement)((ContentPresenter)CONTENT_WRAPPER.Child).Content;
            var cont = CONTENT_WRAPPER;
            var transform = cont.TransformToVisual(child);
            Point screenCoords = transform.TransformPoint(new Point(0, 0));
            var childX = -screenCoords.X;
            var childY = -screenCoords.Y;
            return new Point(childX, childY);
        }

        //because Point? is buggy when setting point to XAML, double.Nan is used for finding out which points are set
        private bool HasValue(Point p)
        {
            return !(double.IsNaN(p.X) && double.IsNaN(p.Y));
        }
        #endregion
    }
}
