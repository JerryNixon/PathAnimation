using System;
using Windows.UI.Xaml;

namespace CustomControls.Controls
{
    public partial class LayoutPathChildWrapper
    {
        static LayoutPathChildWrapper()
        {
            ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(LayoutPathChildWrapper),
                new PropertyMetadata(default(double), ProgressPropertyChangedCallback));
            RotationProperty = DependencyProperty.Register("Rotation", typeof(double), typeof(LayoutPathChildWrapper),
                new PropertyMetadata(default(double), RotationPropertyChangedCallback));
            TranslateXProperty = DependencyProperty.Register("TranslateX", typeof(double), typeof(LayoutPathChildWrapper),
                new PropertyMetadata(default(double), TranslateXPropertyChangedCallback));
            TranslateYProperty = DependencyProperty.Register("TranslateY", typeof(double), typeof(LayoutPathChildWrapper),
                new PropertyMetadata(default(double), TranslateYPropertyChangedCallback));
        }




        #region dependency properties

        public double Progress { get { return (double)GetValue(ProgressProperty); } internal set { SetValue(ProgressProperty, value); } }
        public static readonly DependencyProperty ProgressProperty;

        public double Rotation { get { return (double)GetValue(RotationProperty); } internal set { SetValue(RotationProperty, value); } }
        public static readonly DependencyProperty RotationProperty;

        public double TranslateX { get { return (double)GetValue(TranslateXProperty); } internal set { SetValue(TranslateXProperty, value); } }
        public static readonly DependencyProperty TranslateXProperty;

        public double TranslateY { get { return (double)GetValue(TranslateYProperty); } internal set { SetValue(TranslateYProperty, value); } }
        public static readonly DependencyProperty TranslateYProperty;

        #endregion

    }
}
