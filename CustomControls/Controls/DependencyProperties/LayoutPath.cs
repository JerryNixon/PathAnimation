using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using CustomControls.Enums;
using CustomControls.ExtendedSegments;

namespace CustomControls.Controls
{
    public partial class LayoutPath
    {
        /// <summary>
        /// Initializes dependency properties
        /// </summary>
        static LayoutPath()
        {
            StretchProperty = DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(LayoutPath), new PropertyMetadata(Stretch.None));
            CurrentLengthProperty = DependencyProperty.Register(nameof(CurrentLength), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double)));
            CurrentRotationProperty = DependencyProperty.Register(nameof(CurrentRotation), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double)));
            CurrentPositionProperty = DependencyProperty.Register(nameof(CurrentPosition), typeof(Point), typeof(LayoutPath), new PropertyMetadata(default(Point)));

            PathProperty = DependencyProperty.Register(nameof(Path), typeof(Geometry), typeof(LayoutPath), new PropertyMetadata(default(Geometry), PathChangedCallback));

            ChildAlignmentProperty = DependencyProperty.Register(nameof(ChildAlignment), typeof(ChildAlignment), typeof(LayoutPath), new PropertyMetadata(ChildAlignment.Center, ChildAlignmentChangedCallback));
            ChildEasingFunctionProperty = DependencyProperty.Register(nameof(EasingFunctionBase), typeof(EasingFunctionBase), typeof(LayoutPath), new PropertyMetadata(default(EasingFunctionBase), TransformToProgress));
            ChildrenPaddingProperty = DependencyProperty.Register(nameof(ChildrenPadding), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double), TransformToProgress));

            PathVisibilityProperty = DependencyProperty.Register(nameof(PathVisibility), typeof(Visibility), typeof(LayoutPath), new PropertyMetadata(Visibility.Visible, PathVisibleChangedCallback));

            StartBehaviorProperty = DependencyProperty.Register(nameof(StartBehavior), typeof(Behaviors), typeof(LayoutPath), new PropertyMetadata(Behaviors.Default, TransformToProgress));
            EndBehaviorProperty = DependencyProperty.Register(nameof(EndBehavior), typeof(Behaviors), typeof(LayoutPath), new PropertyMetadata(Behaviors.Default, TransformToProgress));

            ItemOrientationProperty = DependencyProperty.Register(nameof(ItemOrientation), typeof(Orientations), typeof(LayoutPath), new PropertyMetadata(Orientations.ToPath, OrientationChangedCallback));

            //properties that can be overridden
            PathProgressProperty = DependencyProperty.Register(nameof(PathProgress), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double), ProgressChangedCallback));
            TranslationSmoothingDefaultProperty = DependencyProperty.Register(nameof(TranslationSmoothingDefault), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double)));
            RotationSmoothingDefaultProperty = DependencyProperty.Register(nameof(RotationSmoothingDefault), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double)));

            //Attached properties
            ProgressProperty = DependencyProperty.RegisterAttached("Progress", typeof(double), typeof(LayoutPath), new PropertyMetadata(double.NaN, AttachedTransformToProgress));
            TranslationSmoothingProperty = DependencyProperty.RegisterAttached("TranslationSmoothing", typeof(double), typeof(LayoutPath), new PropertyMetadata(double.NaN));
            RotationSmoothingProperty = DependencyProperty.RegisterAttached("RotationSmoothing", typeof(double), typeof(LayoutPath), new PropertyMetadata(double.NaN));

            IsMovableProperty = DependencyProperty.RegisterAttached("IsMovable", typeof(Boolean), typeof(LayoutPath), new PropertyMetadata(true));
            ProgressOffsetProperty = DependencyProperty.RegisterAttached("ProgressOffset", typeof(double), typeof(LayoutPath), new PropertyMetadata(0.0, AttachedTransformToProgress));
        }


        #region attached properties


        public static readonly DependencyProperty IsMovableProperty;
        public static void SetIsMovable(UIElement element, bool value)
        {
            element.SetValue(IsMovableProperty, value);
        }
        public static bool GetIsMovable(UIElement element)
        {
            return (bool)element.GetValue(IsMovableProperty);
        }


        public static readonly DependencyProperty ProgressProperty;
        public static void SetProgress(UIElement element, double value)
        {
            element.SetValue(ProgressProperty, value);
        }
        public static double GetProgress(UIElement element)
        {
            return (double)element.GetValue(ProgressProperty);
        }


        public static readonly DependencyProperty ProgressOffsetProperty;
        public static void SetProgressOffset(UIElement element, double value)
        {
            element.SetValue(ProgressOffsetProperty, value);
        }
        public static double GetProgressOffset(UIElement element)
        {
            return (double)element.GetValue(ProgressOffsetProperty);
        }


        public static readonly DependencyProperty RotationSmoothingProperty;
        public static void SetRotationSmoothing(UIElement element, double value)
        {
            element.SetValue(RotationSmoothingProperty, value);
        }
        public static double GetRotationSmoothing(UIElement element)
        {
            return (double)element.GetValue(RotationSmoothingProperty);
        }


        public static readonly DependencyProperty TranslationSmoothingProperty;
        public static void SetTranslationSmoothing(UIElement element, double value)
        {
            element.SetValue(TranslationSmoothingProperty, value);
        }
        public static double GetTranslationSmoothing(UIElement element)
        {
            return (double)element.GetValue(TranslationSmoothingProperty);
        }


        #endregion


        #region dependency properties

        /// <summary>
        /// Set the distance from start, where <see cref="Children"/> will be transformed (value in Percent 0-100)
        /// </summary>
        public double PathProgress { get { return (double)GetValue(PathProgressProperty); } set { SetValue(PathProgressProperty, value); } }
        public static readonly DependencyProperty PathProgressProperty;

        /// <summary>
        /// Describes how content is resized to fill its allocated space 
        /// </summary>
        public Stretch Stretch { get { return (Stretch)GetValue(StretchProperty); } set { SetValue(StretchProperty, value); } }
        public static readonly DependencyProperty StretchProperty;

        /// <summary>
        /// Sets the visibility of <see cref="Path"/>
        /// </summary>
        public Visibility PathVisibility { get { return (Visibility)GetValue(PathVisibilityProperty); } set { SetValue(PathVisibilityProperty, value); } }
        public static readonly DependencyProperty PathVisibilityProperty;

        /// <summary>
        /// Sets the geometry that will be used for translating <see cref="Children"/>
        /// </summary>
        public Geometry Path { get { return (Geometry)GetValue(PathProperty); } set { SetValue(PathProperty, value); } }
        public static readonly DependencyProperty PathProperty;

        /// <summary>
        /// Specify orientation of <see cref="Children"/> when moving along <see cref="Path"/>
        /// </summary>
        public Orientations ItemOrientation { get { return (Orientations)GetValue(ItemOrientationProperty); } set { SetValue(ItemOrientationProperty, value); } }
        public static readonly DependencyProperty ItemOrientationProperty;

        /// <summary>
        /// Sets the distance that <see cref="Children"/> will keep between each other (in percent of total length).
        /// 
        /// Example: setting ItemsPadding to 20 and progress being to 50, first element will be at progress=50, second at progress=30, third at progress=10 etc..
        /// </summary>
        public double ChildrenPadding { get { return (double)GetValue(ChildrenPaddingProperty); } set { SetValue(ChildrenPaddingProperty, value); } }
        public static readonly DependencyProperty ChildrenPaddingProperty;

        /// <summary>
        /// Gets the <see cref="Point"/> at fraction length of <see cref="Path"/> on current <see cref="PathProgress"/>
        /// Smoothness does not affect CurrentPosition
        /// </summary>
        public Point CurrentPosition { get { return (Point)GetValue(CurrentPositionProperty); } private set { SetValue(CurrentPositionProperty, value); } }
        public static readonly DependencyProperty CurrentPositionProperty;

        /// <summary>
        ///  Gets the degrees at fraction length of <see cref="Path"/> on current <see cref="PathProgress"/>
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
        /// Sets the position of items along path
        /// </summary>
        public ChildAlignment ChildAlignment { get { return (ChildAlignment)GetValue(ChildAlignmentProperty); } set { SetValue(ChildAlignmentProperty, value); } }
        public static readonly DependencyProperty ChildAlignmentProperty;
        
        /// <summary>
        /// Sets child progress to 0 if it is lower than 0. 
        /// This results items to be stacked at the beginning of path if <see cref="ChildrenPadding"/> is specified and progress values are near 0. 
        /// </summary>
        public Behaviors StartBehavior { get { return (Behaviors)GetValue(StartBehaviorProperty); } set { SetValue(StartBehaviorProperty, value); } }
        public static readonly DependencyProperty StartBehaviorProperty;

        /// <summary>
        /// Sets child progress to 100 if it is greater than 100. 
        /// This results items to be stacked at the end of path for progress values greater than 100. 
        /// </summary>
        public Behaviors EndBehavior { get { return (Behaviors)GetValue(EndBehaviorProperty); } set { SetValue(EndBehaviorProperty, value); } }
        public static readonly DependencyProperty EndBehaviorProperty;

        /// <summary>
        /// Smooths children rotation.
        /// </summary>
        public double TranslationSmoothingDefault { get { return (double)GetValue(TranslationSmoothingDefaultProperty); } set { SetValue(TranslationSmoothingDefaultProperty, value); } }
        public static readonly DependencyProperty TranslationSmoothingDefaultProperty;

        /// <summary>
        /// Smooths children translation
        /// </summary>
        public double RotationSmoothingDefault { get { return (double)GetValue(RotationSmoothingDefaultProperty); } set { SetValue(RotationSmoothingDefaultProperty, value); } }
        public static readonly DependencyProperty RotationSmoothingDefaultProperty;

        /// <summary>
        /// Sets the easing function each children will have when moving along path.
        /// </summary>
        public EasingFunctionBase ChildEasingFunction { get { return (EasingFunctionBase)GetValue(ChildEasingFunctionProperty); } set { SetValue(ChildEasingFunctionProperty, value); } }
        public static readonly DependencyProperty ChildEasingFunctionProperty;

        #endregion
    }
}
