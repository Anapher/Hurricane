using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.MagicArrow.DockManager
{
   [Serializable] public class DockingApplicationState
    {
        public double Height { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }

        public DockingSide CurrentSide { get; set; }
    }

   public enum DockingSide { Left, Right, None }
}
