using Rca.OneWireLib.Slaves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SlaveAttribute : Attribute
    {
        public Type SlaveType { get; set; }

        public SlaveAttribute(Type slaveType)
        {
            if (slaveType.GetInterfaces().Contains(typeof(IOneWireSlave)))
                SlaveType = slaveType;
            else
                throw new ArgumentException("Wrong slave type, IOneWireDevice not found.");
        }
    }
}
