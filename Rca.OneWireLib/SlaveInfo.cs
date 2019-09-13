using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib
{
    /// <summary>
    /// Information about a 1-wire slave device
    /// </summary>
    [DebuggerDisplay("{FamilyCode, nq} on ch. {MasterChannel}")]
    public class SlaveInfo
    {
        /// <summary>
        /// Family code
        /// </summary>
        public FamilyCode FamilyCode { get; set; }

        /// <summary>
        /// Unique 1-wire slave address
        /// </summary>
        public byte[] Address { get; set; }

        /// <summary>
        /// Hardware channel on the 1-wire master
        /// </summary>
        public int MasterChannel { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="address">1-wire slave address</param>
        /// <param name="masterChannel">Hardware channel on the 1-wire master</param>
        public SlaveInfo(byte[] address, int masterChannel)
        {
            Address = address;
            MasterChannel = masterChannel;

            FamilyCode = (FamilyCode)address[0];
        }
    }
}
