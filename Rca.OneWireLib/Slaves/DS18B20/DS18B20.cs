using Rca.OneWireLib.Helpers;
using Rca.OneWireLib.Masters;
using System;
using System.Threading.Tasks;

namespace Rca.OneWireLib.Slaves
{
    /// <summary>
    /// The DS18B20 digital thermometer provides 9-bit to 12-bit celsius temperature measurements
    /// and has an alarm function with nonvolatile user-programmable upper and lower trigger points.
    /// </summary>
    public partial class DS18B20 : SlaveBase
    {
        public override void Initialize()
        {

        }

        public double GetTemperature()
        {
            byte[] scratchpad = GetTemperatureScratchpad();

            return GetTemp_Read(scratchpad[DS18B20.Scratchpad.TemperatureMSB], scratchpad[DS18B20.Scratchpad.TemperatureLSB]);
        }

        internal virtual double GetTemp_Read(byte msb, byte lsb)
        {
            double temp_read = 0;
            var negative = false;

            if (msb > 0xF8)
            {
                negative = true;
                msb = (byte)~msb;
                lsb = (byte)~lsb;
                var addOne = (ushort)lsb;
                addOne |= (ushort)(msb << 8);
                addOne++;
                lsb = (byte)(addOne & 0xFFu);
                msb = (byte)((addOne >> 8) & 0xFFu);
            }

            if (lsb.GetBit(0))
            {
                temp_read += Math.Pow(2, -4);
            }
            if (lsb.GetBit(1))
            {
                temp_read += Math.Pow(2, -3);
            }
            if (lsb.GetBit(2))
            {
                temp_read += Math.Pow(2, -2);
            }
            if (lsb.GetBit(3))
            {
                temp_read += Math.Pow(2, -1);
            }
            if (lsb.GetBit(4))
            {
                temp_read += Math.Pow(2, 0);
            }
            if (lsb.GetBit(5))
            {
                temp_read += Math.Pow(2, 1);
            }
            if (lsb.GetBit(6))
            {
                temp_read += Math.Pow(2, 2);
            }
            if (lsb.GetBit(7))
            {
                temp_read += Math.Pow(2, 3);
            }
            if (msb.GetBit(0))
            {
                temp_read += Math.Pow(2, 4);
            }
            if (msb.GetBit(1))
            {
                temp_read += Math.Pow(2, 5);
            }
            if (msb.GetBit(2))
            {
                temp_read += Math.Pow(2, 6);
            }

            if (negative)
            {
                temp_read = temp_read * -1;
            }

            return temp_read;
        }

        protected byte[] GetTemperatureScratchpad()
        {
            ResetOneWireAndMatchDeviceRomAddress();
            Master.EnableStrongPullup();
            StartTemperatureConversion();

            ResetOneWireAndMatchDeviceRomAddress();

            var scratchpad = ReadScratchpad();
            return scratchpad;
        }

        void StartTemperatureConversion()
        {
            Master.OneWireWriteByte(FunctionCommand.ConvertT);

            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
        }

        byte[] ReadScratchpad()
        {
            Master.OneWireWriteByte(FunctionCommand.ReadScratchpad);

            var scratchpadData = new byte[9];

            for (int i = 0; i < scratchpadData.Length; i++)
            {
                scratchpadData[i] = Master.OneWireReadByte();
            }

            return scratchpadData;
        }

    }
}
