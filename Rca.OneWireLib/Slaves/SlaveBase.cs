using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rca.OneWireLib.Masters;

namespace Rca.OneWireLib.Slaves
{
    public abstract class SlaveBase : IOneWireDevice
    {
        #region IOneWireDevice
        public DS2482_100 Master { get; set; }

        public byte[] OneWireAddress { get; set; }

        #endregion IOneWireDevice

        #region Properties
        public string OneWireAddressString => BitConverter.ToString(OneWireAddress);

        #endregion Properties

        #region Services
        public abstract void Initialize();



        #endregion Services

        #region Internal services

        protected void ResetOneWireAndMatchDeviceRomAddress()
        {
            Master.OneWireReset();

            Master.OneWireWriteByte(RomCommand.Match);

            foreach (var item in OneWireAddress)
            {
                Master.OneWireWriteByte(item);
            }
        }
        #endregion Internal services
    }
}
