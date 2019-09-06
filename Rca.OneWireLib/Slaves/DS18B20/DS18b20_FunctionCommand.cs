using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib.Slaves
{
    public partial class DS18B20
    {
        class FunctionCommand
        {
            public const byte ConvertT = 0x44;
            public const byte WriteScratchpad = 0x4E;
            public const byte ReadScratchpad = 0xBE;
            public const byte CopyScratchpad = 0x48;
            public const byte RecallE = 0xB8;
            public const byte ReadPowerSupply = 0xB4;
        }
    }
}

