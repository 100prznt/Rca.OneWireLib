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
        /// 8-bit long device commands
        /// </summary>
        class DeviceCommand
        {
            /// <summary>
            /// This is used to address and write 1–255 bytes to an I2C slave in one transaction. 
            /// </summary>
            public const byte WriteDataWithStop = 0x4B;
            /// <summary>
            /// Addresses and writes 1–255 bytes to an I2C slave without completing the transaction with
            /// a stop. This command allows writing large amounts of data at one time when used in
            /// conjunction with the Write Data Only or Write Data Only with Stop Device commands. 
            /// </summary>
            public const byte WriteDataNoStop = 0x5A;
            /// <summary>
            /// Used when a start and I2C address have previously been issued with a Write Data No Stop
            /// Device command. This command writes 1–255 bytes to an I2C slave without completing the
            /// transaction with a stop and enables writing large amounts of data at one time when used
            /// with the Write Data Only or a last Write Data Only with Stop Device commands. 
            /// </summary>
            public const byte WriteDataOnly = 0x69;
            /// <summary>
            /// Used when a start and I2C address have previously been issued with a Write Data No Stop
            /// Device command. This command writes 1–255 bytes to an I2C slave completing the transaction
            /// with a stop.
            /// </summary>
            public const byte WriteDataOnlyWithStop = 0x78;
            /// <summary>
            /// This is used to address and read 1–255 bytes from an I2C slave in one transaction. 
            /// </summary>
            public const byte ReadDataWithStop = 0x87;
            /// <summary>
            /// This is used to first address and write 1–255 bytes to an I2C slave.
            /// Secondly, it addresses and reads 1–255 bytes from an I2C slave and issues a stop. 
            /// </summary>
            public const byte WriteReadDataWithStop = 0x2D;
            /// <summary>
            /// This is used to write the settings of the I2C speed bits per the formatting of the
            /// Configuration register. After selecting the device by a ROM function command, send
            /// this Device command followed by the desired byte setting for the Configuration register.
            /// </summary>
            public const byte WriteConfiguration = 0xD2;
            /// <summary>
            /// This is used to read the settings of the I2C speed bits from the Configuration register.
            /// After addressing the device by a ROM function command, this Device command, followed by
            /// a read data byte returns the setting of the Configuration register.
            /// </summary>
            public const byte ReadConfiguration = 0xE1;
            /// <summary>
            /// In addition to the SLEEP_N pin, the Enable Sleep Mode command puts the device into a low
            /// current mode. All 1-Wire communication is ignored until woken up. Immediately after the
            /// command, the device monitors the WAKEUP input pin and exits sleep mode on a rising edge. 
            /// </summary>
            public const byte EnableSleepMode = 0x1E;
            /// <summary>
            /// The command reads the device revision. Send this 1-Wire Device command and read data byte,
            /// after first sending a 1-Wire ROM function command that addresses the device. The read data
            /// byte contains the major revision in the upper nibble of the bytes and the minor revision in
            /// the lower nibble of the byte.
            /// </summary>
            public const byte ReadDeviceRevison = 0xC3;
        }
    }
}
