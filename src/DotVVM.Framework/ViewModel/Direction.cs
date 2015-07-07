using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.ViewModel
{
    [Flags]
    public enum Direction
    {
        None = 0,
        ServerToClient = 1,
        ClientToServer = 2,
        Both = 3
    }
}