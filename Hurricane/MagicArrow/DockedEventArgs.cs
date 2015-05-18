using System;
using Hurricane.Model.Skin;

namespace Hurricane.MagicArrow
{
    class DockedEventArgs : EventArgs
    {
        public DockingSide DockingSide { get; }

        public DockedEventArgs(DockingSide side)
        {
            DockingSide = side;
        }
    }
}
