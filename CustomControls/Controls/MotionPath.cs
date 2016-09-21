using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

namespace LayoutPath
{
    [ContentProperty(Name = "MotionContent")]
    public class MotionPath : ContentControl
    {
        //used to initialize content child when it is set
        public FrameworkElement MotionContent
        {
            get { return _motionContent; }
            set
            {
                ContentControl wrapper = new ContentControl();
                wrapper.Content = value;

                _motionContent = wrapper;
                if (_motionContent == null)
                    return;

                _motionContent.RenderTransform = new CompositeTransform();
                if (DesignMode.DesignModeEnabled)
                {
                    _motionContent.Loaded += delegate
                     {
                         FillContent();
                     };
                    FillContent();
                }
                else
                {
                    FillContent();
                }
            }
        }

        #region private members
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Viewbox _wrapViewBox = new Viewbox();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Grid containerGrid = new Grid();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Storyboard _storyboard = new Storyboard();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Line _previewLine;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FrameworkElement _motionContent;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _autoScale;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Point _lineRelativeEnd = new Point(double.NaN, double.NaN);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Point _lineAbsoluteStart = new Point(double.NaN, double.NaN);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Point _lineAbsoluteEnd = new Point(double.NaN, double.NaN);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _showMotionPath;
        #endregion

        #region constructor
        public MotionPath()
        {
            _previewLine = new Line();
            _previewLine.RenderTransform = new CompositeTransform();
            _previewLine.IsHitTestVisible = false;
            _previewLine.StrokeThickness = 1;
            _previewLine.Stroke = new SolidColorBrush(Colors.White);
            _previewLine.Opacity = 0.25;
            _previewLine.Visibility = ShowMotionPath ? Visibility.Visible : Visibility.Collapsed;

            //prevent preview line from filling up space by specifying a negative margin equal to its size
            _previewLine.SizeChanged += delegate (object sender, SizeChangedEventArgs args)
            {
                if (args.PreviousSize.Width == 0 && args.PreviousSize.Height == 0 || Math.Abs(-_previewLine.Margin.Right - args.PreviousSize.Width) > 0 || Math.Abs(-_previewLine.Margin.Bottom - args.PreviousSize.Height) > 0)
                    _previewLine.Margin = new Thickness(0, 0, -_previewLine.ActualWidth, -_previewLine.ActualHeight);
            };

            Loaded += delegate
             {
                 //find Current point related to parent
                 var ttv = TransformToVisual((UIElement)Parent);
                 CurrentPoint = ttv.TransformPoint(new Point(0, 0));

                 if (ShowMotionPath)
                     CalculateLineMovement();
             };
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

        public bool AutoScale
        {
            get { return _autoScale; }
            set
            {
                _autoScale = value;
                FillContent();
            }
        }

        public bool AutoRewind { get; set; }

        #region CurrentPoint dp
        public static readonly DependencyProperty CurrentPointProperty = DependencyProperty.Register(
            "CurrentPoint", typeof(Point), typeof(MotionPath), new PropertyMetadata(default(Point)));

        public Point CurrentPoint
        {
            get { return (Point)GetValue(CurrentPointProperty); }
            private set { SetValue(CurrentPointProperty, value); }
        }
        #endregion

        #region State dp
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(AnimationState), typeof(MotionPath), new PropertyMetadata(default(AnimationState), StatePropertyChangedCallback));

        private static void StatePropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null)
                return;

            var sender = (MotionPath)obj;
            sender.StateChanged?.Invoke(sender, (AnimationState)args.NewValue);
            if ((AnimationState)args.NewValue == AnimationState.Complete)
                sender.Completed?.Invoke(sender);
        }

        public AnimationState State
        {
            get { return (AnimationState)GetValue(StateProperty); }
            private set { SetValue(StateProperty, value); }
        }
        #endregion

        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(3);

        public Point LineAbsoluteStart
        {
            get { return _lineAbsoluteStart; }
            set
            {
                _lineAbsoluteStart = value;
                if (ShowMotionPath)
                    CalculateLineMovement();
            }
        }

        public Point LineAbsoluteEnd
        {
            get { return _lineAbsoluteEnd; }
            set
            {
                _lineAbsoluteEnd = value;
                if (ShowMotionPath)
                    CalculateLineMovement();
            }
        }

        public Point LineRelativeEnd
        {
            get { return _lineRelativeEnd; }
            set
            {
                _lineRelativeEnd = value;
                if (ShowMotionPath)
                    CalculateLineMovement();
            }
        }

        public bool ShowMotionPath
        {
            get { return _showMotionPath; }
            set
            {
                _showMotionPath = value;
                if (value)
                    CalculateLineMovement();
                _previewLine.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

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

        private void FillContent()
        {
            Content = null;
            _wrapViewBox.Child = null;
            containerGrid.Children.Clear();

            if (MotionContent != null)
            {
                containerGrid.Children.Add(MotionContent);
            }

            if (AutoScale)
            {
                _wrapViewBox.Child = containerGrid;
                Content = _wrapViewBox;
            }
            else
            {
                Content = containerGrid;
            }

            if (DesignMode.DesignModeEnabled || ShowMotionPath)
            {
                if (MotionContent == null)
                    return;
                //used for generating line in design mode
                CalculateLineMovement();
            }

#if DEBUG
            containerGrid.Children.Add(_previewLine);
#endif
        }

        private Line CalculateLineMovement()
        {
            Point begin = new Point(0, 0);

            if (!HasValue(LineAbsoluteEnd) && !HasValue(LineRelativeEnd))
                return _previewLine;

            _previewLine.X1 = 0;
            _previewLine.Y1 = 0;
            _previewLine.X2 = 0;
            _previewLine.Y2 = 0;


            if (HasValue(LineAbsoluteStart) && Parent != null)
            {
                begin.X = LineAbsoluteStart.X;
                begin.Y = LineAbsoluteStart.Y;

                var ttv = TransformToVisual((UIElement)Parent);
                Point screenCoords = ttv.TransformPoint(new Point(0, 0));
                _previewLine.RenderTransform = new CompositeTransform()
                {
                    TranslateX = -screenCoords.X + LineAbsoluteStart.X,
                    TranslateY = -screenCoords.Y + LineAbsoluteStart.Y,
                };

                if (HasValue(LineAbsoluteEnd))
                {
                    _previewLine.X2 = LineAbsoluteEnd.X - begin.X;
                    _previewLine.Y2 = LineAbsoluteEnd.Y - begin.Y;
                }
                else if (HasValue(LineRelativeEnd))
                {
                    _previewLine.X2 = LineRelativeEnd.X;
                    _previewLine.Y2 = LineRelativeEnd.Y;
                }
            }
            else if (HasValue(LineAbsoluteEnd) && Parent != null)
            {
                var ttv = TransformToVisual((UIElement)Parent);
                Point screenCoords = ttv.TransformPoint(new Point(0, 0));

                _previewLine.X2 = -screenCoords.X + LineAbsoluteEnd.X;
                _previewLine.Y2 = -screenCoords.Y + LineAbsoluteEnd.Y;

            }
            else if (HasValue(LineRelativeEnd))
            {
                _previewLine.X2 = LineRelativeEnd.X;
                _previewLine.Y2 = LineRelativeEnd.Y;
            }



            return _previewLine;
        }

        private void StartLinearMovement()
        {
            var p = CalculateLineMovement();
            var transform = (CompositeTransform)p.RenderTransform;
            DoubleAnimationUsingKeyFrames xAn = new DoubleAnimationUsingKeyFrames();
            DoubleAnimationUsingKeyFrames yAn = new DoubleAnimationUsingKeyFrames();
            PointAnimationUsingKeyFrames pAn = new PointAnimationUsingKeyFrames();
            ObjectAnimationUsingKeyFrames stateAn = new ObjectAnimationUsingKeyFrames();

            stateAn.KeyFrames.Add(new DiscreteObjectKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = AnimationState.Running
            });

            //Point animation

            //for linear movement, we can pre-calculate the currnet point positions of our movement
            //instead of getting position for each value change.
            var ttv = TransformToVisual((UIElement)Parent);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));

            pAn.KeyFrames.Add(new EasingPointKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = new Point(screenCoords.X + p.X1, screenCoords.Y + p.Y1)
            });
            pAn.KeyFrames.Add(new EasingPointKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(Duration),
                Value = new Point(screenCoords.X + p.X2, screenCoords.Y + p.Y2)
            });

            //x animation
            xAn.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = p.X1 + transform.TranslateX
            });
            xAn.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(Duration),
                Value = p.X2 + transform.TranslateX
            });

            //y animation
            yAn.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = p.Y1 + transform.TranslateY
            });
            yAn.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(Duration),
                Value = p.Y2 + transform.TranslateY
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
                    Value = new Point(screenCoords.X + p.X1, screenCoords.Y + p.Y1)
                });

                xAn.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = TimeSpan.FromTicks(Duration.Ticks * 2),
                    Value = p.X1 + transform.TranslateX
                });

                yAn.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = TimeSpan.FromTicks(Duration.Ticks * 2),
                    Value = p.Y1 + transform.TranslateY
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
            Storyboard.SetTarget(xAn, MotionContent);
            Storyboard.SetTarget(yAn, MotionContent);

            _storyboard.Stop();

            _storyboard.Children.Clear();

            _storyboard.Children.Add(stateAn);
            _storyboard.Children.Add(pAn);
            _storyboard.Children.Add(xAn);
            _storyboard.Children.Add(yAn);

            _storyboard.Begin();
        }

        //because Point? is causing trouble to when setting point to xaml, double.Nan is used for finding out which points are set
        private bool HasValue(Point p)
        {

            return !(double.IsNaN(p.X) && double.IsNaN(p.Y));
        }
    }
}
