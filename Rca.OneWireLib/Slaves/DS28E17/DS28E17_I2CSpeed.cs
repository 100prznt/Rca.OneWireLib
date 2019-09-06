using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib.Slaves
{
    public partial class DS28E17
    {
        /// <summary>
        /// I²C Speed
        /// </summary>
        public enum I2CSpeed : byte
        {
            I2C_Speed100kHz = 0b00000000,
            I2C_Speed400kHz = 0b00000001,
            I2C_Speed900kHz = 0b00000010
        }
    }
}
