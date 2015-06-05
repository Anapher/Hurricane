using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Hurricane.Controls
{
    class AnimatedSlider : Slider
    {
        private Thumb _thumb;
        private bool _isDownOnSlider;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_thumb != null)
            {
                _thumb.MouseEnter -= thumb_MouseEnter;
                _thumb.LostMouseCapture -= thumb_LostMouseCapture;
            }

            _thumb = ((Track) GetTemplateChild("PART_Track"))?.Thumb;

            if (_thumb != null)
            {
                _thumb.MouseEnter += thumb_MouseEnter;
                _thumb.LostMouseCapture += thumb_LostMouseCapture;
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            _isDownOnSlider = true;
        }

        private void thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _isDownOnSlider)
            {
                MouseButtonEventArgs args = new MouseButtonEventArgs(
                    e.MouseDevice, e.Timestamp, MouseButton.Left) {RoutedEvent = MouseLeftButtonDownEvent};
                ((Thumb) sender).RaiseEvent(args);
            }
        }

        private void thumb_LostMouseCapture(object sender, EventArgs e)
        {
            _isDownOnSlider = false;
        }
    }
}
