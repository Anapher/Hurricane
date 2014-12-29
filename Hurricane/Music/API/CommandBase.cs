using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music.API
{
    abstract class CommandBase
    {
        public string Token { get; protected set; }
    }
}
