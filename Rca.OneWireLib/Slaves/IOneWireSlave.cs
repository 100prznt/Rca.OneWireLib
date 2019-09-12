using Rca.OneWireLib.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib.Slaves
{
    internal interface IOneWireSlave
    {
        IOneWireMaster Master { get; set; }

        //byte FamilyCode { get; }

        /// <summary>
        /// Ist das auch die UID
        /// </summary>
        byte[] OneWireAddress { get; set; }


        //void Initialize(DS2482_100 ic, byte[] OneWireAddress);
    }
}
