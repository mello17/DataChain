using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataChain.DataLayer
{
    public enum UserRole : byte
    {

        Admin = 1,
        Writer = 2,
        Reader = 3,
        Unset = 4
    }
}

