using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib
{
    /// <summary>
    /// CRC-8 checksum class
    /// Source: http://sanity-free.org/146/crc8_implementation_in_csharp.html
    /// </summary>
    [Obsolete]
    public static class CRC8
    {
        #region Constants
        /// <summary>
        /// Polynom
        /// </summary>
        const byte POLY = 0x31; // SHT85 polynom = x8 + x5 + x4 + 1

        #endregion Constants

        static byte[] table = new byte[256];

        static CRC8()
        {
            for (int i = 0; i < 256; ++i)
            {
                int temp = i;
                for (int j = 0; j < 8; ++j)
                {
                    if ((temp & 0x80) != 0)
                        temp = (temp << 1) ^ POLY;
                    else
                        temp <<= 1;
                }
                table[i] = (byte)temp;
            }
        }

        /// <summary>
        /// Compute checksum
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>CRC-8 checksum</returns>
        public static byte ComputeChecksum(params byte[] bytes)
        {
            byte crc = 0xFF;
            if (bytes != null && bytes.Length > 0)
            {
                foreach (byte b in bytes)
                    crc = table[crc ^ b];
            }

            return crc;
        }
    }
}
