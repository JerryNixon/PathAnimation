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
            SmoothRotationProperty = DependencyProperty.Register("SmoothRotation", typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double)));
            SmoothTranslationProperty = DependencyProperty.Register("SmoothTranslation", typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double)));

            //properties with callbacks
            ProgressProperty = DependencyProperty.Register(nameof(Progress), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double), ProgressChangedCallback));
            PathVisibleProperty = DependencyProperty.Register(nameof(PathVisible), typeof(bool), typeof(LayoutPath), new PropertyMetadata(true, PathVisibleChangedCallback));
            PathProperty = DependencyProperty.Register(nameof(Path), typeof(Geometry), typeof(LayoutPath), new PropertyMetadata(default(Geometry), PathChangedCallback));
            ChildAlignmentProperty = DependencyProperty.Register(nameof(ChildAlignment), typeof(ChildAlignment), typeof(LayoutPath), new PropertyMetadata(ChildAlignment.Center, ChildAlingmentChangedCallback));
            MoveVerticallyProperty = DependencyProperty.Register(nameof(MoveVertically), typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool), MoveVerticallyChangedCallback));
            FlipItemsProperty = DependencyProperty.Register("FlipItems", typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool), FlipItemsChangedCallback));
            ItemsPaddingProperty = DependencyProperty.Register(nameof(ItemsPadding), typeof(double), typeof(LayoutPath), new PropertyMetadata(default(double), TransformToProgress));
            OrientToPathProperty = DependencyProperty.Register(nameof(OrientToPath), typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool), TransformToProgress));
            StackAtStartProperty = DependencyProperty.Register("StackAtStart", typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool), TransformToProgress));
            StackAtEndProperty = DependencyProperty.Register("StackAtEnd", typeof(bool), typeof(LayoutPath), new PropertyMetadata(default(bool), TransformToProgress));
            ChildEasingFunctionProperty = DependencyProperty.Register("EasingFunctionBase", typeof(EasingFunctionBase), typeof(LayoutPath), new PropertyMetadata(default(EasingFunctionBase), TransformToProgress));

            //Attached properties
            ChildProgressProperty = DependencyProperty.RegisterAttached("ChildProgress", typeof(double), typeof(LayoutPath), new PropertyMetadata(double.NaN, TransformToProgress));
            IsMovableProperty = DependencyProperty.RegisterAttached("IsMovable", typeof(Boolean), typeof(LayoutPath), new PropertyMetadata(true));
            ProgressOffsetProperty = DependencyProperty.RegisterAttached("ProgressOffset", typeof(double), typeof(LayoutPath), new PropertyMetadata(0.0, TransformToProgress));
            ChildSmoothRotationProperty = DependencyProperty.RegisterAttached("ChildSmoothRotation", typeof(double), typeof(LayoutPath), new PropertyMetadata(double.NaN));
            ChildSmoothTranslationProperty = DependencyProperty.RegisterAttached("ChildSmoothTranslation", typeof(double), typeof(LayoutPath), new PropertyMetadata(double.NaN));
        }

      
        #region attached properties


        public static readonly DependencyProperty IsMovableProperty;
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


        public static readonly DependencyProperty ProgressOffsetProperty;
        public static void SetProgressOffset(UIElement element, double value)
        {
            element.SetValue(ProgressOffsetProperty, value);
        }
        public static double GetProgressOffset(UIElement element)
        {
            return (double)element.GetValue(ProgressOffsetProperty);
        }


        public static readonly DependencyProperty ChildSmoothRotationProperty;
        public static void SetChildSmoothRotation(UIElement element, double value)
        {
            element.SetValue(ChildSmoothRotationProperty, value);
        }
        public static double GetChildSmoothRotation(UIElement element)
        {
            return (double)element.GetValue(ChildSmoothRotationProperty);
        }


        public static readonly DependencyProperty ChildSmoothTranslationProperty;
        public static void SetChildSmoothTranslation(UIElement element, double value)
        {
            element.SetValue(ChildSmoothTranslationProperty, value);
        }
        public static double GetChildSmoothTranslation(UIElement element)
        {
            return (double)element.GetValue(ChildSmoothTranslationProperty);
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
        public double SmoothRotation { get { return (double)GetValue(SmoothRotationProperty); } set { SetValue(SmoothRotationProperty, value); } }
        public static readonly DependencyProperty SmoothRotationProperty;

        /// <summary>
        /// Smooths children translation
        /// </summary>
        public double SmoothTranslation { get { return (double)GetValue(SmoothTranslationProperty); } set { SetValue(SmoothTranslationProperty, value); } }
        public static readonly DependencyProperty SmoothTranslationProperty;

        /// <summary>
        /// Sets the easing function each children will have when moving along path.
        /// </summary>
        public EasingFunctionBase ChildEasingFunction { get { return (EasingFunctionBase)GetValue(ChildEasingFunctionProperty); } set { SetValue(ChildEasingFunctionProperty, value); } }
        public static readonly DependencyProperty ChildEasingFunctionProperty;

        #endregion
    }
}
