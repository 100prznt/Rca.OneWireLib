using Rca.OneWireLib.Slaves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib
{
    internal class SlaveContainer
    {
        public IOneWireSlave Slave { get; set; }

        public SlaveInfo Info { get; set; }

        public SlaveContainer(IOneWireSlave slave, int channel)
        {
            Slave = slave;
            Info = new SlaveInfo(slave.OneWireAddress, channel);
        }
    }
}
