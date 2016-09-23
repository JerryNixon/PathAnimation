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
using LayoutPath.Enums;

namespace CustomControls.Controls
{
    public class MotionPath : ContentControl
    {
        #region private members
        private Border CONTENT_WRAPPER;
        private LineGeometry LINE_GEOMETRY;
        private Grid LAYOUT_ROOT;
        //private Viewbox VIEW_BOX;
        private Path PATH;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Storyboard _storyboard = new Storyboard();
        #endregion

        protected override void OnApplyTemplate()
        {
            CONTENT_WRAPPER = GetTemplateChild(nameof(CONTENT_WRAPPER)) as Border;
            LINE_GEOMETRY = GetTemplateChild(nameof(LINE_GEOMETRY)) as LineGeometry;
            LAYOUT_ROOT = GetTemplateChild(nameof(LAYOUT_ROOT)) as Grid;
            //VIEW_BOX = GetTemplateChild(nameof(VIEW_BOX)) as Viewbox;
            PATH = GetTemplateChild(nameof(PATH)) as Path;

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

            //StretchProperty = DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(MotionPath), new PropertyMetadata(Stretch.None,
            //    delegate (DependencyObject o, DependencyPropertyChangedEventArgs e)
            //    {
            //        ((MotionPath)o).VIEW_BOX.Stretch = (Stretch)e.NewValue;
            //    }));

            LineAbsoluteStartProperty = DependencyProperty.Register("LineAbsoluteStart", typeof(Point), typeof(MotionPath),
                new PropertyMetadata(new Point(double.NaN, double.NaN), RefreshCalculations));

            LineAbsoluteEndProperty = DependencyProperty.Register("LineAbsoluteEnd", typeof(Point), typeof(MotionPath),
                new PropertyMetadata(new Point(double.NaN, double.NaN), RefreshCalculations));

            LineRelativeEndProperty = DependencyProperty.Register("LineRelativeEnd", typeof(Point), typeof(MotionPath),
                  new PropertyMetadata(new Point(double.NaN, double.NaN), RefreshCalculations));

            PathVisibilityProperty = DependencyProperty.Register("PathVisibility", typeof(Visibility), typeof(MotionPath), new PropertyMetadata(default(Visibility)));
        }

        private static void RefreshCalculations(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var s = (MotionPath)o;
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

        //public Stretch Stretch { get { return VIEW_BOX.Stretch; } set { SetValue(StretchProperty, value); } }
        //public static readonly DependencyProperty StretchProperty;

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

            StartLinearMovement();
        }

        public void Reset()
        {
            //forcing line to re-calculated
            CalculateLineMovement();
            if (State == AnimationState.Ready)
            {
                return;
            }

            _storyboard.Stop();
            State = AnimationState.Ready;
        }

        public async void Pause()
        {
            await PauseAsync();
        }

        public async Task PauseAsync()
        {
            if (State == AnimationState.Running || State == AnimationState.Rewinding)
            {
                _storyboard.Pause();

                //Can't identify when storyboard is actually paused because
                //clock state is giving always active.

                //while (true)
                //{
                //    var state = _storyboard.GetCurrentState();
                //    if (state != ClockState.Active)
                //        break;
                //    await Task.Delay(10);
                //}

                await Task.Delay(50);
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

            var child = (UIElement)((ContentPresenter)CONTENT_WRAPPER.Child).Content;
            var cont = CONTENT_WRAPPER;
            var transform = cont.TransformToVisual(child);
            Point screenCoords = transform.TransformPoint(new Point(0, 0));
            var childX = -screenCoords.X;
            var childY = -screenCoords.Y;

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
            var el = (FrameworkElement)((ContentPresenter)CONTENT_WRAPPER.Child).Content;
            LAYOUT_ROOT.MinWidth = (LINE_GEOMETRY.StartPoint.X > LINE_GEOMETRY.EndPoint.X
                                       ? LINE_GEOMETRY.StartPoint.X
                                       : LINE_GEOMETRY.EndPoint.X) + el.ActualWidth;
            LAYOUT_ROOT.MinHeight = (LINE_GEOMETRY.StartPoint.Y > LINE_GEOMETRY.EndPoint.Y
                                       ? LINE_GEOMETRY.StartPoint.Y
                                       : LINE_GEOMETRY.EndPoint.Y) + el.ActualHeight;
        }

        private void StartLinearMovement()
        {
            CalculateLineMovement();
            var p = LINE_GEOMETRY;
            //var transform = (CompositeTransform)p.RenderTransform;
            DoubleAnimationUsingKeyFrames xAn = new DoubleAnimationUsingKeyFrames();
            DoubleAnimationUsingKeyFrames yAn = new DoubleAnimationUsingKeyFrames();
            PointAnimationUsingKeyFrames pAn = new PointAnimationUsingKeyFrames();
            ObjectAnimationUsingKeyFrames stateAn = new ObjectAnimationUsingKeyFrames();

            var child = (UIElement)((ContentPresenter)CONTENT_WRAPPER.Child).Content;
            var cont = CONTENT_WRAPPER;
            var transform = cont.TransformToVisual(child);
            Point screenCoords = transform.TransformPoint(new Point(0, 0));
            var TranslateX = -screenCoords.X;
            var TranslateY = -screenCoords.Y;

            stateAn.KeyFrames.Add(new DiscreteObjectKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = AnimationState.Running
            });

            //Point animation
            pAn.KeyFrames.Add(new EasingPointKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = new Point(p.StartPoint.X - TranslateX, p.StartPoint.Y - TranslateY)
            });
            pAn.KeyFrames.Add(new EasingPointKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(Duration),
                Value = new Point(p.EndPoint.X - TranslateX, p.EndPoint.Y - TranslateY)
            });

            //x animation
            xAn.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = p.StartPoint.X - TranslateX
            });
            xAn.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(Duration),
                Value = p.EndPoint.X - TranslateX
            });

            //y animation
            yAn.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = p.StartPoint.Y - TranslateY
            });
            yAn.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(Duration),
                Value = p.EndPoint.Y - TranslateY
            });

            if (AutoRewind)
            {
                pAn.Duration = TimeSpan.FromTicks(Duration.Ticks * 2);
                xAn.Duration = TimeSpan.FromTicks(Duration.Ticks * 2);
                yAn.Duration = TimeSpan.FromTicks(Duration.Ticks * 2);
                stateAn.Duration = TimeSpan.FromTicks(Duration.Ticks * 2);

                pAn.KeyFrames.Add(new EasingPointKeyFrame()
                {
                    KeyTime = TimeSpan.FromTicks(Duration.Ticks * 2),
                    Value = new Point(p.StartPoint.X - TranslateX, p.StartPoint.Y - TranslateY)
                });

                xAn.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = TimeSpan.FromTicks(Duration.Ticks * 2),
                    Value = p.StartPoint.X - TranslateX
                });

                yAn.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = TimeSpan.FromTicks(Duration.Ticks * 2),
                    Value = p.StartPoint.Y - TranslateY
                });

                stateAn.KeyFrames.Add(new DiscreteObjectKeyFrame()
                {
                    KeyTime = TimeSpan.FromTicks(Duration.Ticks),
                    Value = AnimationState.Rewinding
                });
                stateAn.KeyFrames.Add(new DiscreteObjectKeyFrame()
                {
                    KeyTime = TimeSpan.FromTicks(Duration.Ticks * 2),
                    Value = AnimationState.Complete
                });
            }
            else
            {
                stateAn.KeyFrames.Add(new DiscreteObjectKeyFrame()
                {
                    KeyTime = KeyTime.FromTimeSpan(Duration),
                    Value = AnimationState.Complete,
                });

                pAn.Duration = Duration;
                xAn.Duration = Duration;
                yAn.Duration = Duration;
                stateAn.Duration = Duration;
            }


            stateAn.EnableDependentAnimation = true;
            pAn.EnableDependentAnimation = true;
            xAn.EnableDependentAnimation = true;
            yAn.EnableDependentAnimation = true;

            Storyboard.SetTargetProperty(stateAn, "(MotionPath.State)");
            Storyboard.SetTargetProperty(pAn, "(MotionPath.CurrentPoint)");
            Storyboard.SetTargetProperty(xAn, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
            Storyboard.SetTargetProperty(yAn, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");

            Storyboard.SetTarget(stateAn, this);
            Storyboard.SetTarget(pAn, this);
            Storyboard.SetTarget(xAn, CONTENT_WRAPPER);
            Storyboard.SetTarget(yAn, CONTENT_WRAPPER);

            _storyboard.Stop();

            _storyboard.Children.Clear();

            _storyboard.Children.Add(stateAn);
            _storyboard.Children.Add(pAn);
            _storyboard.Children.Add(xAn);
            _storyboard.Children.Add(yAn);

            _storyboard.Begin();
        }

        //because Point? is buggy when setting point to XAML, double.Nan is used for finding out which points are set
        private bool HasValue(Point p)
        {
            return !(double.IsNaN(p.X) && double.IsNaN(p.Y));
        }
        #endregion
    }
}
