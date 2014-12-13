using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Utilities.Native
{
    public struct CopyDataStruct : IDisposable
    {
        public IntPtr dwData;
        public int cbData;
        public IntPtr lpData;

        public void Dispose()
        {
            if (this.lpData != IntPtr.Zero)
            {
                UnsafeNativeMethods.LocalFree(this.lpData);
                this.lpData = IntPtr.Zero;
            }
        }
    }
}
