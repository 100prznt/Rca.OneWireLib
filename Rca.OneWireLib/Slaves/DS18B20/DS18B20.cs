using Rca.OneWireLib.Helpers;
using Rca.OneWireLib.Masters;
using System;
using System.Threading;
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

        /// <summary>
        /// Perform a temperatur reading and returns the result
        /// </summary>
        /// <returns>Temperature in Celcius</returns>
        public double GetTemperature()
        {
            //TODO: Async?
            ResetOneWireAndMatchDeviceRomAddress();

            Master.EnableStrongPullup();

            Master.OneWireWriteByte(FunctionCommand.ConvertT);
            SpinWait.SpinUntil(() => false, 1000);
            ResetOneWireAndMatchDeviceRomAddress();
            Master.OneWireWriteByte(FunctionCommand.ReadScratchpad);

            var scratchpadData = new byte[9];

            for (int i = 0; i < scratchpadData.Length; i++)
            {
                scratchpadData[i] = Master.OneWireReadByte();
            }

            if (CRC8Maxim.ComputeChecksum(scratchpadData) != 0)
                throw new Exception("Invalid crc");

            return ConvertTemperature(scratchpadData[DS18B20.Scratchpad.TemperatureMSB], scratchpadData[DS18B20.Scratchpad.TemperatureLSB]);
        }

        private double ConvertTemperature(byte msb, byte lsb)
        {
            double tempRead = 0;
            var isNegative = false;

            if (msb > 0xF8)
            {
                isNegative = true;
                msb = (byte)~msb;
                lsb = (byte)~lsb;

                var addOne = (UInt16)lsb;
                addOne |= (UInt16)(msb << 8);
                addOne++;

                lsb = (byte)(addOne & 0xFFu);
                msb = (byte)((addOne >> 8) & 0xFFu);
            }

            for (int i = 0; i < 8; i++)
                if (lsb.GetBit(i))
                    tempRead += Math.Pow(2, i - 4);

            for (int i = 0; i < 3; i++)
                if (msb.GetBit(i))
                    tempRead += Math.Pow(2, i + 4);

            #region original Maxim implementation
            //if (lsb.GetBit(0))
            //    tempRead += Math.Pow(2, -4);
            //if (lsb.GetBit(1))
            //    tempRead += Math.Pow(2, -3);
            //if (lsb.GetBit(2))
            //    tempRead += Math.Pow(2, -2);
            //if (lsb.GetBit(3))
            //    tempRead += Math.Pow(2, -1);
            //if (lsb.GetBit(4))
            //    tempRead += Math.Pow(2, 0);
            //if (lsb.GetBit(5))
            //    tempRead += Math.Pow(2, 1);
            //if (lsb.GetBit(6))
            //    tempRead += Math.Pow(2, 2);
            //if (lsb.GetBit(7))
            //    tempRead += Math.Pow(2, 3);
            //if (msb.GetBit(0))
            //    tempRead += Math.Pow(2, 4);
            //if (msb.GetBit(1))
            //    tempRead += Math.Pow(2, 5);
            //if (msb.GetBit(2))
            //    tempRead += Math.Pow(2, 6);
            #endregion

            if (isNegative)
                tempRead *= -1;

            return tempRead;
        }

        //public double GetTemperature()
        //{
        //    byte[] scratchpad = GetTemperatureScratchpad();

        //    return ConvertTemperature(scratchpad[DS18B20.Scratchpad.TemperatureMSB], scratchpad[DS18B20.Scratchpad.TemperatureLSB]);
        //}

        //protected byte[] GetTemperatureScratchpad()
        //{
        //    ResetOneWireAndMatchDeviceRomAddress();
        //    Master.EnableStrongPullup();
        //    StartTemperatureConversion();

        //    ResetOneWireAndMatchDeviceRomAddress();

        //    var scratchpad = ReadScratchpad();
        //    return scratchpad;
        //}

        //void StartTemperatureConversion()
        //{
        //    Master.OneWireWriteByte(FunctionCommand.ConvertT);

        //    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
        //}

        //byte[] ReadScratchpad()
        //{
        //    Master.OneWireWriteByte(FunctionCommand.ReadScratchpad);

        //    var scratchpadData = new byte[9];

        //    for (int i = 0; i < scratchpadData.Length; i++)
        //    {
        //        scratchpadData[i] = Master.OneWireReadByte();
        //    }

        //    return scratchpadData;
        //}

    }
}
