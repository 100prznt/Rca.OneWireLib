using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib.Helpers
{
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Select a specified range of an byte-array
        /// </summary>
        /// <param name="data">Datasource</param>
        /// <param name="startIndex">Start index</param>
        /// <param name="lenght">Lenght</param>
        /// <returns></returns>
        public static byte[] Range(this byte[] data, int startIndex, int lenght = 0)
        {
            if (lenght <= 0)
                lenght = data.Length - startIndex;

            var subData = new byte[lenght];
            Array.Copy(data, startIndex, subData, 0, lenght);

            return subData;
        }
    }
}
