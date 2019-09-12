using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace Rca.OneWireLib.Masters
{
    public interface IOneWireMaster
    {
        byte[] RomNo { get; set; }

        int ChannelCount { get; }

        bool IsInitialized { get; }

        void Init(I2cDevice i2cDevice);

        bool OneWireSelectChannel(int channel);

        bool OneWireReset();

        /// <summary>
        /// Find the 'first' devices on the 1-Wire bus
        /// </summary>
        /// <returns>true: device found, ROM number in ROM_NO buffer; false: no device present</returns>
        bool OneWireFirst();

        /// <summary>
        /// Find the 'next' devices on the 1-Wire bus
        /// </summary>
        /// <returns>true: device found, ROM number in ROM_NO buffer; false: device not found, end of search</returns>
        bool OneWireNext();

        void OneWireWriteByte(byte byte_value);

        void EnableStrongPullup();

        byte OneWireReadByte();

        bool OneWireReadBit();
    }
}
