using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib
{
    public class SlaveInfo
    {
        public FamilyCode FamilyCode { get; set; }

        public byte[] Address { get; set; }

        public int MasterChannel { get; set; }

        public SlaveInfo(byte[] address, int masterChannel)
        {
            Address = address;
            MasterChannel = masterChannel;

            FamilyCode = (FamilyCode)address[7];
        }

        public Type GetSlaveType()
        {
            return FamilyCode.GetSlaveType();
        }
    }
}
