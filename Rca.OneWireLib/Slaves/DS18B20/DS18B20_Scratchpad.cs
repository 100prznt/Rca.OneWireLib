using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib.Slaves
{
    public partial class DS18B20
    {
        enum Scratchpad
        {
            TemperatureLSB = 0,
            TemperatureMSB = 1,
            ThRegisterOrUserByte1 = 2,
            TlRegisterOrUserByte2 = 3,
            ConfigurationRegister = 4,
            Reserved = 5,
            Reserved2 = 6,
            Reserved3 = 7,
            CRC = 8
        }
    }
}
