using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib.Helpers
{
    public class ByteArrayComparer
    {
        /// <summary>
        /// Compare 2 byte-arrays
        /// </summary>
        /// <param name="a">Array A</param>
        /// <param name="b">Array B</param>
        /// <returns>true: arrays are equal</returns>
        public bool Compare(byte[] a, byte[]b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }
    }
}
