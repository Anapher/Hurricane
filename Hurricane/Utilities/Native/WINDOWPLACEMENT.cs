using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace Hurricane.Utilities.Native
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }
}
