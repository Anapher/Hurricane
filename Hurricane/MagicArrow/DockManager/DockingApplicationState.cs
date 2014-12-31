using System;
using System.Windows;

namespace Hurricane.MagicArrow.DockManager
{
    [Serializable]
    public class DockingApplicationState
    {
        public double Height { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }

        public DockingSide CurrentSide { get; set; }
        public WindowState WindowState { get; set; }

        public DockingApplicationState()
        {
            this.WindowState = WindowState.Normal;
        }
    }

    public enum DockingSide { Left, Right, None }
}