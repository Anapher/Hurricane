using System.Windows;

namespace Hurricane.Utilities
{
    public class Cursor
    {
        public static Point Position
        {
            get
            {
                var position = System.Windows.Forms.Cursor.Position;
                return new Point(position.X, position.Y);
            }
        }
    }
}