using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Hurricane.Extensions.Controls
{
    public class SliderWhichMakesMoves : Slider
    {
        private Thumb _thumb;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_thumb != null)
            {
                _thumb.MouseEnter -= thumb_MouseEnter;
            }

            _thumb = (GetTemplateChild("PART_Track") as Track).Thumb;

            if (_thumb != null)
            {
                _thumb.MouseEnter += thumb_MouseEnter;
            }
        }

        private void thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // the left button is pressed on mouse enter

                // so the thumb must have been moved under the mouse

                // in response to a click on the track.

                // Generate a MouseLeftButtonDown event.

                MouseButtonEventArgs args = new MouseButtonEventArgs(
                    e.MouseDevice, e.Timestamp, MouseButton.Left) {RoutedEvent = MouseLeftButtonDownEvent};
                (sender as Thumb).RaiseEvent(args);
            }
        }
    }
}
