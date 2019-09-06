using Rca.OneWireLib.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib
{
    interface IOneWireDevice
    {
        DS2482_100 Master { get; }

        byte[] OneWireAddress { get; }

        //void Initialize(DS2482_100 ic, byte[] OneWireAddress);
    }
}
