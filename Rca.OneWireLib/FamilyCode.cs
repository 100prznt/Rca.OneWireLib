using Rca.OneWireLib.Slaves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib
{
    public enum FamilyCode : byte
    {
        /// <summary>
        /// 1-Wire® slave to I²C master bridge
        /// </summary>
        [Slave(typeof(DS28E17))]
        DS28E17 = 0x19,
        /// <summary>
        /// Digital thermometer
        /// </summary>
        [Slave(typeof(DS18B20))]
        DS18B20 = 0x28,
        /// <summary>
        /// 20Kb 1-Wire EEPROM
        /// </summary>
        [Slave(typeof(DS28EC20))]
        DS28EC20 = 0x43,
    }
}
