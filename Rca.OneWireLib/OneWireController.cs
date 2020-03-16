using Rca.OneWireLib.Helpers;
using Rca.OneWireLib.Masters;
using Rca.OneWireLib.Slaves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Rca.OneWireLib
{
    public class OneWireController
    {
        List<SlaveContainer> m_SlaveContainers;

        IOneWireMaster m_Master;

        public OneWireController()
        {

        }


        #region Services
        /// <summary>
        /// Initializes a new 1-wire master
        /// </summary>
        /// <typeparam name="T">Type of the master to be initialized.</typeparam>
        /// <param name="masterAddress">I2C address of the 1-wire master.</param>
        /// <param name="i2cController">Name of the I2C controller to which the 1-wire master is connected.</param>
        public void InitMaster<T>(byte masterAddress, string i2cController = "I2C1") where T : IOneWireMaster, new()
        {
            //Verbindung zum I²C-Buscontroller
            string aqs = I2cDevice.GetDeviceSelector(i2cController);
            var deviceInformation = DeviceInformation.FindAllAsync(aqs).AsTask().Result;

            //Verbindung zum OneWire Master einrichten und herstellen
            var masterSettings = new I2cConnectionSettings(masterAddress)
            {
                BusSpeed = I2cBusSpeed.FastMode
            };
            var i2cDevice = I2cDevice.FromIdAsync(deviceInformation[0].Id, masterSettings).AsTask().Result;

            m_Master = new T();
            m_Master.Init(i2cDevice);

            try
            {
                m_Master.OneWireReset();
            }
            catch (Exception ex)
            {
                throw new Exception("1-wire master not found!", ex);
            }
        }

        public void SelectMasterChannel(int channel)
        {
            m_Master.OneWireSelectChannel(channel);
        }

        /// <summary>
        /// Search all connected slaves
        /// </summary>
        public void SearchSlaves()
        {
            if (m_Master == null |! m_Master.IsInitialized)
                throw new ArgumentNullException("1-wire master must set before!");

            m_SlaveContainers = new List<SlaveContainer>();


            for (int i = 0; i < m_Master.ChannelCount; i++)
            {
                m_Master.OneWireSelectChannel(i);
                GetOneWireDevices(m_Master, i);
            }
        }

        /// <summary>
        /// Returns information about all found slaves.
        /// </summary>
        /// <returns>Array of <seealso cref="SlaveInfo"/> contains master channel, slave address and family code</returns>
        public SlaveInfo[] GetSlaveInfos()
        {
            return m_SlaveContainers.Select(x => x.Info).ToArray();
        }

        /// <summary>
        /// Returns the slave device of the specified type.
        /// </summary>
        /// <typeparam name="T">Slave type</typeparam>
        /// <returns>Slave device</returns>
        /// <remarks>Attention, this method should only be used if it is ensured that only one slave of the corresponding type is present on the bus.</remarks>
        public T GetSlave<T>()
        {
            //TODO: Check whether only one slave meets the conditions.
            foreach (var container in m_SlaveContainers)
            {
                if (container.Slave is T)
                    return (T)container.Slave;
            }

            throw new Exception("Device not found!");
        }

        /// <summary>
        /// Returns the slave device of the specified type, with the specified address, on the specified master channel.
        /// </summary>
        /// <typeparam name="T">Slave type</typeparam>
        /// <param name="address">slave address</param>
        /// <param name="channel">master channel</param>
        /// <returns>Slave device</returns>
        public T GetSlave<T>(int channel, byte[] address)
        {
            var bComp = new ByteArrayComparer();
            //TODO: Check whether only one slave meets the conditions.
            var container = m_SlaveContainers.FirstOrDefault(x => x.Info.MasterChannel == channel && bComp.Compare(x.Slave.OneWireAddress, address));

            if (container == null)
                throw new Exception($"Device with specified address {BitConverter.ToString(address)} not found on channel {channel}");

            if (container != null && container.Slave is T)
                return (T)container.Slave;
            else
                throw new Exception("Specified device type not matchs.");
        }

        #endregion Services

        #region Internal services

        private void GetOneWireDevices(IOneWireMaster master, int oneWireChannel)
        {
            var first = true;
            var deviceDetected = master.OneWireReset();  //prüfen, ob Geräte erkannt

            if (deviceDetected)
            {
                var result = true;
                do
                {
                    if (first)
                    {
                        first = false;
                        result = master.OneWireFirst();
                    }
                    else
                        result = master.OneWireNext();

                    if (result)
                    {
                        foreach (byte code in Enum.GetValues(typeof(FamilyCode)))
                        {
                            if (code == master.RomNo[0])
                            {
                                Type slaveType = ((FamilyCode)code).GetSlaveType();
                                var slave = (IOneWireSlave)Activator.CreateInstance(slaveType);

                                byte[] slaveRomNo = new byte[8];
                                Array.Copy(master.RomNo, slaveRomNo, 8);

                                slave.OneWireAddress = slaveRomNo;
                                slave.Master = master;

                                m_SlaveContainers.Add(new SlaveContainer(slave, oneWireChannel));
                            }
                        }
                    }
                } while (result);
            }
        }
        #endregion Internal services
    }
}
