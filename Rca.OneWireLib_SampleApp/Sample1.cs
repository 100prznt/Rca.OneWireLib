using Rca.OneWireLib;
using Rca.OneWireLib.Masters;
using Rca.OneWireLib.Slaves;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib_SampleApp
{
    public class Sample1
    {
        OneWireController m_OneWireController;

        public void InitMaster()
        {
            m_OneWireController = new OneWireController();

            m_OneWireController.InitMaster<DS2482_100>(0x18, "I2C1");
            m_OneWireController.SearchSlaves();
        }

        /// <summary>
        /// Sample for DS18B20
        /// </summary>
        /// <returns>true: success; false: error</returns>
        public bool DS18B20_Sample()
        {
            var infos = m_OneWireController.GetSlaveInfos(); //Get info of all connected slaves

            var infoDS18B20 = infos.FirstOrDefault(x => x.FamilyCode == FamilyCode.DS18B20); //look for a DS18B20 device

            if (infoDS18B20 != null) //if DS18B20 available
            {
                m_OneWireController.SelectMasterChannel(infoDS18B20.MasterChannel); //Select the master channel!

                var thermometer = m_OneWireController.GetSlave<DS18B20>(infoDS18B20.MasterChannel, infoDS18B20.Address); //Get the slave (DS18B20) device instance

                var temperature = thermometer.GetTemperature(); //Slave operation

                Debug.WriteLine($"Temperature reading: {temperature} °C"); //Print out
                return true;
            }
            else
            {
                Debug.WriteLine("DS18B20 not available!");
                return false;
            }
        }
    }
}
