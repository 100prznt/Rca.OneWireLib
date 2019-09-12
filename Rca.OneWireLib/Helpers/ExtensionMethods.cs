using Rca.OneWireLib.Slaves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib.Helpers
{
    static class ExtensionMethods
    {
        /// <summary>
        /// Pick a sigle bit from a byte
        /// </summary>
        /// <param name="b">Byte</param>
        /// <param name="bitNumber">Position of the bit.</param>
        /// <returns>Bit state</returns>
        public static bool GetBit(this byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        /// <summary>
        /// Get a specified OneWireDevice
        /// </summary>
        /// <typeparam name="T">Type of the device to be selected.</typeparam>
        /// <param name="devices">Collection of <see cref="IOneWireSlave"/></param>
        /// <returns>Selected device</returns>
        public static IEnumerable<T> GetDevices<T>(this IEnumerable<IOneWireSlave> devices) where T : IOneWireSlave
        {
            var result = new List<T>();
            foreach (var item in devices.Where(dev => dev.GetType().Equals(typeof(T))))
                result.Add((T)item);

            return result;
        }

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
