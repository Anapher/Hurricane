using System;
using System.Windows;

namespace Hurricane.Model.Skin
{
    [Serializable]
    public class WindowSettings
    {
        public double Height { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }

        public DockingSide CurrentSide { get; set; }
        public WindowState WindowState { get; set; }
    }

    public enum DockingSide { None, Left, Right }
}