﻿using Rca.OneWireLib;
using Rca.OneWireLib.Masters;
using Rca.OneWireLib.Slaves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib_SampleApp
{
    public class Sample1
    {
        OneWireController m_OneWireController;

        public void Init()
        {
            m_OneWireController = new OneWireController();

            m_OneWireController.InitMaster<DS2482_100>(0x18, "I2C1");
            m_OneWireController.SearchSlaves();
        }

        public void GetSlaveInfo()
        {
            var infos = m_OneWireController.GetSlaveInfos();

            var infoDS18B20 = infos.FirstOrDefault(x => x.FamilyCode == FamilyCode.DS18B20);

            if (infoDS18B20 != null)
            {
                var slave1 = m_OneWireController.GetSlave<DS18B20>(infoDS18B20.MasterChannel, infoDS18B20.Address);
            }
        }
    }
}
