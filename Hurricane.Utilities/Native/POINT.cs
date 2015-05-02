using System.Runtime.InteropServices;

namespace Hurricane.Utilities.Native
{
    /// <summary>
    /// The Point structure defines the X- and Y- coordinates of a point. 
    /// </summary>
    /// <remarks>
    /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/gdi/rectangl_0tiq.asp
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        /// <summary>
        /// Specifies the X-coordinate of the point. 
        /// </summary>
        public int X;
        /// <summary>
        /// Specifies the Y-coordinate of the point. 
        /// </summary>
        public int Y;

        public override bool Equals(object obj)
        {
            if (obj is POINT)
            {
                var point = (POINT)obj;

                return point.X == X && point.Y == Y;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public static bool operator ==(POINT a, POINT b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(POINT a, POINT b)
        {
            return !(a == b);
        }
    }
}
