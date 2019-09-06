using Rca.OneWireLib.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib.Slaves
{
    /// <summary>
    /// The DS28EC20 is a 20480-bit, 1-Wire® EEPROM organized as 80 memory pages of 256 bits each.
    /// </summary>
    public class DS28EC20 : SlaveBase
    {
        #region Members
        /// <summary>
        /// internal representation of the eeprom data
        /// </summary>
        private byte[] m_Data;

        #endregion Members

        #region Sericves
        public override void Initialize()
        {
            m_Data = ReadEEPROM();
        }

        /// <summary>
        /// Fetches the specified bytes from the data array.
        /// </summary>
        /// <param name="block">Block</param>
        /// <param name="page">Page</param>
        /// <param name="byteOffset">Offset</param>
        /// <param name="byteCount">Byte count</param>
        /// <returns>specified bytes</returns>
        public byte[] GetBytes(int block, int page, int byteOffset = 0, int byteCount = 0x0020)
        {
            int startAddress = block * 0x0100 + page * 0x0020 + byteOffset;

            var data = new byte[byteCount];
            for (int i = 0; i < data.Length; i++)
                data[i] = 0xFF;

            for (int i = 0; i < byteCount; i++)
            {
                int address = startAddress + i;
                if (0 <= address && address < m_Data.Length)
                {
                    data[i] = m_Data[address];
                }
            }

            return data;
        }

        #endregion Sericves

        #region Internal serives
        /// <summary>
        /// Reads the whole EEPROM at once.
        /// </summary>
        /// <param name="startaddress">Start address</param>
        /// <param name="count">Count</param>
        /// <returns>All bytes</returns>
        private byte[] ReadEEPROM(int startaddress = 0, int count = 0x0A40)
        {
            var sendBuffer = new byte[3]
            {
                0xA5,
                (byte)(startaddress),
                (byte)(startaddress >> 8)
            };

            foreach (var item in sendBuffer)
                Master.OneWireWriteByte(item);

            var readBuffer = new byte[0x0022 * Convert.ToUInt16((count - 1) / 0x0020 + 1)];

            for (int i = 0; i < readBuffer.Length; i++)
                readBuffer[i] = Master.OneWireReadByte();


            uint crc16 = 0;
            for (int i = 0; i < sendBuffer.Length; i++)
                crc16 = CRC16.Compute(sendBuffer[i], crc16);

            int dataindex = 0;
            var data = new byte[count];
            for (int i = 0; i < data.Length; i++)
                data[i] = 0xFF;

            for (int i = 0; i < readBuffer.Length; i++)
            {
                int address = startaddress + dataindex;
                if (address >= 0x0A40) break;
                if (dataindex < count) data[dataindex] = readBuffer[i];
                dataindex++;

                crc16 = CRC16.Compute(readBuffer[i], crc16);
                if (i > 0 && (address % 0x0020 == 0x001F))
                {
                    i += 2;
                    if (i < readBuffer.Length)
                    {
                        UInt16 sollcrc16 = (byte)~readBuffer[i];
                        sollcrc16 <<= 8;
                        sollcrc16 |= (byte)~readBuffer[i - 1];
                        if (sollcrc16 != crc16) return null;
                        crc16 = 0;
                    }
                }
            }

            return data;
        }

        #endregion Internal serives

    }
}