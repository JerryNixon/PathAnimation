using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using System;
using Windows.UI.Xaml.Markup;

namespace CustomControls.Controls
{
    public sealed class PathAnimationWrapper : ContentControl
    {
        public PathAnimationWrapper()
        {
            DefaultStyleKey = typeof(PathAnimationWrapper);
            RenderTransform = new CompositeTransform();
        }

        Viewbox PART_VIEWBOX;
        protected override void OnApplyTemplate()
        {
            PART_VIEWBOX = GetTemplateChild(nameof(PART_VIEWBOX)) as Viewbox;
        }

        public double Progress { get { return (double)GetValue(ProgressProperty); } set { SetValue(ProgressProperty, value); } }
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(double), typeof(PathAnimationWrapper), new PropertyMetadata(0, ProgressChanged));
        private static void ProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var point = (d as PathAnimationWrapper).CurrentPosition =
                GetPointAtFractionLength(d.GetValue(PathProperty) as PathGeometry, (double)e.NewValue);
            var transform = (d as ContentControl).RenderTransform as CompositeTransform;
            transform.TranslateX = point.X;
            transform.TranslateY = point.Y;
        }

        public Point CurrentPosition { get { return (Point)GetValue(CurrentPositionProperty); } set { SetValue(CurrentPositionProperty, value); } }
        public static readonly DependencyProperty CurrentPositionProperty =
            DependencyProperty.Register(nameof(CurrentPosition), typeof(Point), typeof(PathAnimationWrapper), new PropertyMetadata(null));

        public PathGeometry Path { get { return (PathGeometry)GetValue(PathProperty); } set { SetValue(PathProperty, value); } }
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(nameof(Path), typeof(PathGeometry), typeof(PathAnimationWrapper), new PropertyMetadata(null));

        public Stretch Stretch { get { return PART_VIEWBOX.Stretch; } set { SetValue(StretchProperty, value); } }
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(PathAnimationWrapper), new PropertyMetadata(Stretch.None, StretchChanged));
        private static void StretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PathAnimationWrapper).PART_VIEWBOX.Stretch = (Stretch)e.NewValue;
        }

        static Point GetPointAtFractionLength(PathGeometry path, double progress)
        {
            // TODO: calculate
            return default(Point);
        }
    }
}
