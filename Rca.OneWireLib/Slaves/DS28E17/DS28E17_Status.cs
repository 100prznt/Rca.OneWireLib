using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib.Slaves
{
    public partial class DS28E17
    {
        /// <summary>
        /// Any of the Device commands that interact with the I2C bus return a Status byte that
        /// describes success or not of the I2C transaction.
        /// </summary>
        [DebuggerDisplay("{DebuggerDisplay,nq}")]
        public class Status
        {
            /// <summary>
            /// Indicates the received 1-Wire packet data does not match the corresponding CRC16.
            /// When set, the device responds with an invalid Write Status byte of FFh since no I2C
            /// communication was initiated.
            /// false: Valid CRC16
            /// true: Invalid CRC16
            /// </summary>
            public bool Crc16Invalid { get; private set; }

            /// <summary>
            /// If this bit is set, only the I2C address was sent, but no write data was transmitted
            /// do to the byte not being acknowledged by the I2C slave device.The device responds with
            /// an invalid Write Status byte of FFh.
            /// false: I2C slave device acknowledged the I2C address
            /// true: I2C slave device does not acknowledge the I2C address
            /// </summary>
            public bool AddressNotAcknowledged { get; private set; }

            /// <summary>
            /// false = Start
            /// true = Invalid start
            /// </summary>
            public bool StartInvalid { get; private set; }

            /// <summary>
            /// Parse the status byte
            /// </summary>
            /// <param name="status">status byte</param>
            /// <returns>DS28E17 status</returns>
            public static Status Parse(byte status)
            {
                return new Status()
                {
                    Crc16Invalid =           (status & 0b00000001) != 0,
                    AddressNotAcknowledged = (status & 0b00000010) != 0,
                    StartInvalid =           (status & 0b00001000) != 0,
                };
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private string DebuggerDisplay => Crc16Invalid || AddressNotAcknowledged || StartInvalid ? "Error" : "Success";
            
        }
    }
}
