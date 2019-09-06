using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib.Slaves
{
    public partial class DS18B20
    {
        class Scratchpad
        {
            public const int TemperatureLSB = 0;

            public const int TemperatureMSB = 1;

            public const int ThRegisterOrUserByte1 = 2;

            public const int TlRegisterOrUserByte2 = 3;

            public const int ConfigurationRegister = 4;

            public const int Reserved = 5;

            public const int Reserved2 = 6;

            public const int Reserved3 = 7;

            public const int CRC = 8;
        }
    }
}
