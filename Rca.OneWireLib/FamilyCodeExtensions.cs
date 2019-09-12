using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib
{
    public static class FamilyCodeExtensions
    {
        public static Type GetSlaveType(this FamilyCode code)
        {
            Attribute[] attributes = code.GetAttributes();

            SlaveAttribute attr = null;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].GetType() == typeof(SlaveAttribute))
                {
                    attr = (SlaveAttribute)attributes[i];
                    break;
                }
            }

            if (attr == null)
                throw new ArgumentNullException("Slave type not set.");
            else
                return attr.SlaveType;
        }

        private static Attribute[] GetAttributes(this FamilyCode code)
        {
            var fi = code.GetType().GetField(code.ToString());
            Attribute[] attributes = (Attribute[])fi.GetCustomAttributes(typeof(Attribute), false);

            return attributes;
        }
    }
}
