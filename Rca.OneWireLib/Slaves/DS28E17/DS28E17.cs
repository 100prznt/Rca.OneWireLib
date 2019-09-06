using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rca.OneWireLib.Helpers;
using Rca.OneWireLib.Masters;

namespace Rca.OneWireLib.Slaves
{
    /// <summary>
    /// The DS28E17 is a 1-Wire® slave to I²C master bridge device that interfaces directly to I²C slaves.
    /// </summary>
    public partial class DS28E17 : SlaveBase
    {
        #region Services
        public override void Initialize()
        {
            var rev = ReadRevision();
            SetConfiguration(DS28E17.I2CSpeed.I2C_Speed100kHz);
            var conf = ReadConfiguration();

            //TestWrite();

            var statusWrite = WriteBytes(0x44, new byte[2] { 0x24, 0x16 });
            var statusRead = ReadBytes(0x44, 6, out var buffer);
        }

        /// <summary>
        /// The command reads the device revision. 
        /// </summary>
        /// <returns>The read data byte contains the major revision in the upper nibble of the bytes and
        /// the minor revision in the lower nibble of the byte.</returns>
        public byte ReadRevision()
        {
            ResetOneWireAndMatchDeviceRomAddress();

            Master.EnableStrongPullup();

            Master.OneWireWriteByte((byte)DS28E17.DeviceCommand.ReadDeviceRevison);

            return Master.OneWireReadByte();
        }

        /// <summary>
        /// This is used to write the settings of the I2C speed bits per the formatting of the
        /// Configuration register.
        /// </summary>
        /// <param name="speed">I²C Speed</param>
        public void SetConfiguration(DS28E17.I2CSpeed speed)
        {
            ResetOneWireAndMatchDeviceRomAddress();

            Master.EnableStrongPullup();

            Master.OneWireWriteByte((byte)DS28E17.DeviceCommand.WriteConfiguration);
            Master.OneWireWriteByte((byte)speed);
        }

        /// <summary>
        /// This is used to read the settings of the I2C speed bits from the Configuration register.
        /// </summary>
        /// <returns>The setting of the Configuration register.</returns>
        public byte ReadConfiguration()
        {
            ResetOneWireAndMatchDeviceRomAddress();

            Master.EnableStrongPullup();

            Master.OneWireWriteByte((byte)DS28E17.DeviceCommand.ReadConfiguration);

            return Master.OneWireReadByte();
        }

        /// <summary>
        /// Performs a write operation on the I2C bus.
        /// </summary>
        /// <param name="i2cDeviceAddress">Address of the I2C slave.</param>
        /// <param name="buffer">Bytes to write.</param>
        /// <returns>Status of the write operation.</returns>
        public DS28E17.Status WriteBytes(byte i2cDeviceAddress, byte[] buffer)
        {
            ResetOneWireAndMatchDeviceRomAddress();
            Master.EnableStrongPullup();

            //byte deviceCommand = (byte)DS28E17_Commands.WriteDataWithStop;
            //byte i2cSlaveAddress = 0x44; //0x44 SHT85 address
            var bufferReverse = buffer.Reverse().ToArray();

            var cmd = new List<byte>
            {
                (byte)DS28E17.DeviceCommand.WriteDataWithStop,
                (byte)(i2cDeviceAddress << 1), //7 bit for address, bit[0] = 0 (i2c write)
                (byte)bufferReverse.Length
            };
            cmd.AddRange(buffer);

            var crc16 = ~CRC16.Compute(cmd.ToArray()); //CRC must be inverted

            foreach (byte b in cmd)
            {
                Master.OneWireWriteByte(b);
                SpinWait.SpinUntil(() => false, 10);
            }

            Master.OneWireWriteByte((byte)(crc16 & 0xFF)); //least significant byte of 16 bit CRC
            Master.OneWireWriteByte((byte)(crc16 >> 8));   //most significant byte of 16 bit CRC

            SpinWait.SpinUntil(() => false, 10);

            var status = DS28E17.Status.Parse(Master.OneWireReadByte());
            var writeStatus = Master.OneWireReadByte();

            return status;
        }

        /// <summary>
        /// Performs a read operation on the I2C bus.
        /// </summary>
        /// <param name="i2cDeviceAddress">Address of the I2C slave.</param>
        /// <param name="count">Number of bytes to be read.</param>
        /// <param name="buffer">Bytes read from the I2C bus.</param>
        /// <returns>Status of the read operation.</returns>
        public DS28E17.Status ReadBytes(byte i2cDeviceAddress, int count, out byte[] buffer)
        {
            if (count > 255)
                throw new ArgumentOutOfRangeException("Maximum byte count is 255!");

            ResetOneWireAndMatchDeviceRomAddress();
            Master.EnableStrongPullup();

            //byte deviceCommand = (byte)DS28E17_Commands.ReadDataWithStop;
            //byte i2cSlaveAddress = 0x89; //0x44 SHT85 address
            //byte bytesToRead = 6;
            //var sht85cmd = BitConverter.GetBytes((UInt16)0x2416).ToArray();

            var cmd = new byte[3]
            {
                (byte)DS28E17.DeviceCommand.ReadDataWithStop,
                (byte)((i2cDeviceAddress << 1) + 1), //7 bit for address, bit[0] = 1 (i2c read)
                (byte)count
            };

            var crc16 = ~CRC16.Compute(cmd); //CRC must be inverted

            foreach (byte b in cmd)
            {
                Master.OneWireWriteByte(b);
                SpinWait.SpinUntil(() => false, 10);
            }

            Master.OneWireWriteByte((byte)(crc16 & 0xFF)); //least significant byte of 16 bit CRC
            Master.OneWireWriteByte((byte)(crc16 >> 8));   //most significant byte of 16 bit CRC

            SpinWait.SpinUntil(() => false, 10);
            var bufferList = new List<byte>();
            var status = DS28E17.Status.Parse(Master.OneWireReadByte());

            var dummy = Master.OneWireReadBit(); // Dont know why, but this one bit is to much in the buffer.

            for (int i = 0; i < count; i++)
                bufferList.Add(Master.OneWireReadByte());

            buffer = bufferList.ToArray();
            return status;
        }

        #endregion Services

        #region Debug

        private byte[] CheckAndCutCrc(byte[] buffer, string parameterName = null)
        {
            if (buffer.Length != 3)
                throw new ArgumentException("Buffer length must be 3!");
            if (CRC8.ComputeChecksum(buffer) != 0)
                throw new Exception($"CRC error {parameterName} value!");

            return new byte[2] { buffer[1], buffer[0] };
        }

        private double ConvertTemperature(byte[] data)
        {
            var rawValue = BitConverter.ToUInt16(data, 0);
            return -45 + 175 * (rawValue / (Math.Pow(2, 16) - 1));
        }

        private double ConvertHumidity(byte[] data)
        {
            var rawValue = BitConverter.ToUInt16(data, 0);
            return 100 * (rawValue / (Math.Pow(2, 16) - 1));
        }

        /// <summary>
        /// Test for SHT85
        /// </summary>
        public void TestWrite()
        {
            ResetOneWireAndMatchDeviceRomAddress();

            Master.EnableStrongPullup();

            //byte deviceCommand = (byte)DS28E17_Commands.WriteDataWithStop;
            byte deviceCommand = (byte)DS28E17.DeviceCommand.WriteDataWithStop;
            //deviceCommand = 0x1F;
            byte i2cSlaveAddress = 0x44; //0x44 SHT85 address
            var sht85cmd = BitConverter.GetBytes((UInt16)0x2416).Reverse().ToArray();
            //var sht85cmd = BitConverter.GetBytes((UInt16)0x2416).ToArray();

            var i2cCommand = new List<byte>();
            i2cCommand.Add(deviceCommand);
            i2cCommand.Add((byte)(i2cSlaveAddress << 1)); //7 bit for address, bit[0] = 0 (i2c write)
            i2cCommand.Add((byte)sht85cmd.Length);
            i2cCommand.AddRange(sht85cmd);

            var crc16 = CRC16.Compute(i2cCommand.ToArray());
            crc16 = ~crc16;

            foreach (byte b in i2cCommand)
            {
                Master.OneWireWriteByte(b);
                SpinWait.SpinUntil(() => false, 10);
            }

            Master.OneWireWriteByte((byte)(crc16 & 0xFF)); // Least significant byte of 16 bit CRC
            Master.OneWireWriteByte((byte)(crc16 >> 8));   // Most significant byte of 16 bit CRC

            SpinWait.SpinUntil(() => false, 10);

            var status = Master.OneWireReadByte();
            var writeStatus = Master.OneWireReadByte();

            TestRead();
        }
        /// <summary>
        /// Test for SHT85
        /// </summary>
        public void TestRead()
        {
            ResetOneWireAndMatchDeviceRomAddress();

            Master.EnableStrongPullup();

            //byte deviceCommand = (byte)DS28E17_Commands.WriteDataWithStop;
            byte deviceCommand = (byte)DS28E17.DeviceCommand.ReadDataWithStop;
            //deviceCommand = 0x1F;
            byte i2cSlaveAddress = 0x89; //0x44 SHT85 address
            byte bytesToRead = 6;
            //var sht85cmd = BitConverter.GetBytes((UInt16)0x2416).ToArray();

            var cmd = new List<byte>();
            cmd.Add(deviceCommand);
            cmd.Add(i2cSlaveAddress); //7 bit for address, bit[0] = 1 (i2c read)
            cmd.Add(bytesToRead);

            var crc16 = CRC16.Compute(cmd.ToArray());
            crc16 = ~crc16;

            foreach (byte b in cmd)
            {
                Master.OneWireWriteByte(b);
                SpinWait.SpinUntil(() => false, 10);
            }

            Master.OneWireWriteByte((byte)(crc16 & 0xFF)); // Least significant byte of 16 bit CRC
            Master.OneWireWriteByte((byte)(crc16 >> 8));   // Most significant byte of 16 bit CRC

            SpinWait.SpinUntil(() => false, 10);
            var buffer = new List<byte>();
            var status = Master.OneWireReadByte();

            var dummy = Master.OneWireReadBit(); // Dont know why, but this one bit is to much in the buffer.

            for (int i = 0; i < bytesToRead; i++)
                buffer.Add(Master.OneWireReadByte());


            var temp = ConvertTemperature(CheckAndCutCrc(buffer.ToArray().Range(0, 3)));
            var hum = ConvertHumidity(CheckAndCutCrc(buffer.ToArray().Range(3, 3)));
        }
        #endregion Debug
    }
}
