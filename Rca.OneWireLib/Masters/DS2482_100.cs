using Rca.OneWireLib.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.I2c;

namespace Rca.OneWireLib.Masters
{
    // This implementation is based on Maxim sample implementation and has parts from ported C code
    // source: http://www.maximintegrated.com/en/app-notes/index.mvp/id/187

    /// <summary>
    /// The DS2482-100 is an I²C to 1-Wire® bridge device that interfaces directly to standard (100kHz max) or fast (400kHz max)
    /// I²C masters to perform bidirectional protocol conversion between the I²C master and any downstream 1-Wire slave devices.
    /// </summary>
    public class DS2482_100 : IOneWireMaster, IDisposable
    {
        public int ChannelCount { get; } = 8;

        public bool IsInitialized { get; private set; }


        #region Members
        int m_LastDiscrepancy;
        int m_LastFamilyDiscrepancy;
        bool m_LastDeviceFlag;
        byte m_CRC8;
        I2cDevice m_I2CDevice;

        #endregion Members

        #region Properties
        /// <summary>
        /// Enable leaky abstraction
        /// </summary>
        // public I2cDevice I2CDevice => m_I2CDevice;

        /// <summary>
        /// ROM number
        /// </summary>
        public byte[] RomNo { get; set; }

        #endregion Properties
        
        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i2cDevice"></param>
        public DS2482_100()
        {
            RomNo = new byte[8];
        }

        #endregion Constructor

        #region Services

        public void Init(I2cDevice i2cDevice)
        {
            m_I2CDevice = i2cDevice;
            IsInitialized = true;
        }

        /// <summary>
        /// Find the 'first' devices on the 1-Wire bus
        /// </summary>
        /// <returns>true: device found, ROM number in ROM_NO buffer; false: no device present</returns>
        public bool OneWireFirst()
        {
            // reset the search state
            m_LastDiscrepancy = 0;
            m_LastDeviceFlag = false;
            m_LastFamilyDiscrepancy = 0;

            return OneWireSearch();
        }

        /// <summary>
        /// Find the 'next' devices on the 1-Wire bus
        /// </summary>
        /// <returns>true: device found, ROM number in ROM_NO buffer; false: device not found, end of search</returns>
        public bool OneWireNext()
        {
            // leave the search state alone
            return OneWireSearch();
        }

        /// <summary>
        /// Perform the 1-Wire Search Algorithm on the 1-Wire bus using the existing search state.
        /// </summary>
        /// <returns>true: device found, ROM number in ROM_NO buffer; false: device not found, end of search</returns>
        public bool OneWireSearch()
        {
            int idBitNumber;
            int lastZero, romByteNumber;
            bool idBit, cmpIdBit, searchDirection, searchResult;
            byte romByteMask;

            // initialize for search
            idBitNumber = 1;
            lastZero = 0;
            romByteNumber = 0;
            romByteMask = 1;
            searchResult = false;
            m_CRC8 = 0;

            // if the last call was not the last one
            if (!m_LastDeviceFlag)
            {
                // 1-Wire reset
                if (!OneWireReset())
                {
                    // reset the search
                    m_LastDiscrepancy = 0;
                    m_LastDeviceFlag = false;
                    m_LastFamilyDiscrepancy = 0;
                    return false;
                }

                // issue the search command 
                OneWireWriteByte(0xF0);

                // loop to do the search
                do
                {
                    // read a bit and its complement
                    idBit = OneWireReadBit();
                    cmpIdBit = OneWireReadBit();

                    // check for no devices on 1-wire
                    if (idBit && cmpIdBit)
                        break;
                    else
                    {
                        // all devices coupled have 0 or 1
                        if (idBit != cmpIdBit)
                            searchDirection = idBit;  // bit write value for search
                        else
                        {
                            // if this discrepancy if before the Last Discrepancy
                            // on a previous next then pick the same as last time
                            if (idBitNumber < m_LastDiscrepancy)
                                searchDirection = ((RomNo[romByteNumber] & romByteMask) > 0);
                            else
                                // if equal to last pick 1, if not then pick 0
                                searchDirection = (idBitNumber == m_LastDiscrepancy);

                            // if 0 was picked then record its position in LastZero
                            if (!searchDirection)
                            {
                                lastZero = idBitNumber;

                                // check for Last discrepancy in family
                                if (lastZero < 9)
                                    m_LastFamilyDiscrepancy = lastZero;
                            }
                        }

                        // set or clear the bit in the ROM byte rom_byte_number
                        // with mask rom_byte_mask
                        if (searchDirection)
                            RomNo[romByteNumber] |= romByteMask;
                        else
                        {
                            var result = (byte)~romByteMask;
                            RomNo[romByteNumber] &= result;
                        }


                        // serial number search direction write bit
                        OneWireWriteBit(searchDirection);

                        // increment the byte counter id_bit_number
                        // and shift the mask rom_byte_mask
                        idBitNumber++;
                        romByteMask <<= 1;

                        // if the mask is 0 then go to new SerialNum byte rom_byte_number and reset mask
                        if (romByteMask == 0)
                        {
                            docrc8(RomNo[romByteNumber]);  // accumulate the CRC
                            romByteNumber++;
                            romByteMask = 1;
                        }
                    }
                }
                while (romByteNumber < 8);  // loop until through all ROM bytes 0-7

                // if the search was successful then
                if (!((idBitNumber < 65) || (m_CRC8 != 0)))
                {
                    // search successful so set LastDiscrepancy,LastDeviceFlag,search_result
                    m_LastDiscrepancy = lastZero;

                    // check for last device
                    if (m_LastDiscrepancy == 0)
                        m_LastDeviceFlag = true;

                    searchResult = true;
                }
            }

            // if no device found then reset counters so next 'search' will be like a first
            if (!searchResult || RomNo[0] == 0)
            {
                m_LastDiscrepancy = 0;
                m_LastDeviceFlag = false;
                m_LastFamilyDiscrepancy = 0;
                searchResult = false;
            }

            return searchResult;
        }

        /// <summary>
        /// Verify the device with the ROM number in ROM_NO buffer is present.
        /// </summary>
        /// <returns>true : device verified present, false : device not present</returns>
        public bool OneWireVerify()
        {
            byte[] romBackup = new byte[8];
            int ldBackup, lfdBackup;
            bool ldfBackup, result;

            // keep a backup copy of the current state
            for (int i = 0; i < 8; i++)
                romBackup[i] = RomNo[i];
            ldBackup = m_LastDiscrepancy;
            ldfBackup = m_LastDeviceFlag;
            lfdBackup = m_LastFamilyDiscrepancy;

            // set search to find the same device
            m_LastDiscrepancy = 64;
            m_LastDeviceFlag = false;

            if (OneWireSearch())
            {
                // check if same device found
                result = true;
                for (int i = 0; i < 8; i++)
                {
                    if (romBackup[i] != RomNo[i])
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
                result = false;

            // restore the search state 
            for (int i = 0; i < 8; i++)
                RomNo[i] = romBackup[i];

            m_LastDiscrepancy = ldBackup;
            m_LastDeviceFlag = ldfBackup;
            m_LastFamilyDiscrepancy = lfdBackup;

            // return the result of the verify
            return result;
        }

        /// <summary>
        /// Setup the search to find the device type 'family_code' on the next call to OWNext() if it is present.
        /// </summary>
        /// <param name="family_code"></param>
        public void OneWireTargetSetup(byte family_code)
        {
            // set the search state to find SearchFamily type devices
            RomNo[0] = family_code;
            for (int i = 1; i < 8; i++)
                RomNo[i] = 0;
            m_LastDiscrepancy = 64;
            m_LastFamilyDiscrepancy = 0;
            m_LastDeviceFlag = false;
        }

        /// <summary>
        /// Setup the search to skip the current device type on the next call to OWNext().
        /// </summary>
        public void OneWireFamilySkipSetup()
        {
            // set the Last discrepancy to last family discrepancy
            m_LastDiscrepancy = m_LastFamilyDiscrepancy;
            m_LastFamilyDiscrepancy = 0;

            // check for end of list
            if (m_LastDiscrepancy == 0)
                m_LastDeviceFlag = true;
        }

        //--------------------------------------------------------------------------
        // 1-Wire Functions to be implemented for a particular platform
        //--------------------------------------------------------------------------

        /// <summary>
        /// Reset the 1-Wire bus and return the presence of any device
        /// </summary>
        /// <returns>true : device present, false : no device present</returns>
        public bool OneWireReset()
        {
            // platform specific
            // TMEX API TEST BUILD
            //return (TMTouchReset(session_handle) == 1);

            m_I2CDevice.Write(new byte[] { FunctionCommand.ONEWIRE_RESET });

            var status = ReadStatus();

            if (status.GetBit(StatusBit.ShortDetected))
            {
                throw new InvalidOperationException("One Wire short detected");
            }

            return status.GetBit(StatusBit.PresencePulseDetected);
        }

        /// <summary>
        /// Read status byte from DS2482_100
        /// </summary>
        /// <param name="setReadPointerToStatus">Set to true if read pointer should be moved to status register before reading status</param>
        /// <returns></returns>
        public byte ReadStatus(bool setReadPointerToStatus = false)
        {
            var statusBuffer = new byte[1];
            if (setReadPointerToStatus)
            {
                m_I2CDevice.WriteRead(new byte[] { FunctionCommand.SET_READ_POINTER, RegisterSelection.STATUS }, statusBuffer);
            }
            else
            {
                m_I2CDevice.Read(statusBuffer);
            }

            if (statusBuffer.Length < 1)
            {
                throw new InvalidOperationException("Read status failed");
            }

            var stopWatch = new Stopwatch();
            do
            {
                if (stopWatch.ElapsedMilliseconds > 1)
                {
                    throw new InvalidOperationException("One Wire bus busy for too long");
                }
                m_I2CDevice.Read(statusBuffer);
            } while (statusBuffer[0].GetBit(StatusBit.OneWireBusy));

            return statusBuffer[0];
        }

        public byte ReadChannelSelectionRegister()
        {
            var statusBuffer = new byte[1];
            m_I2CDevice.WriteRead(new byte[] { FunctionCommand.SET_READ_POINTER, RegisterSelection.CHANNEL_SELECTION }, statusBuffer);

            return statusBuffer[0];
        }

        void WaitForOneWireReady()
        {
            var status = new byte[1];
            var stopWatch = new Stopwatch();
            do
            {
                if (stopWatch.ElapsedMilliseconds > 5000)
                {
                    throw new InvalidOperationException("One Wire bus busy for too long");
                }
                m_I2CDevice.WriteRead(new byte[] { FunctionCommand.SET_READ_POINTER, RegisterSelection.STATUS }, status);
            } while (status[0].GetBit(StatusBit.OneWireBusy));
        }

        /// <summary>
        /// Send 8 bits of data to the 1-Wire bus
        /// </summary>
        /// <param name="byte_value">byte to send</param>
        public void OneWireWriteByte(byte byte_value)
        {
            // platform specific

            // TMEX API TEST BUILD
            //TMTouchByte(session_handle, byte_value);

            m_I2CDevice.Write(new byte[] { FunctionCommand.ONEWIRE_WRITE_BYTE, byte_value });

            ReadStatus();
        }

        /// <summary>
        /// Send 1 bit of data to teh 1-Wire bus 
        /// </summary>
        /// <param name="bit_value"></param>
        public void OneWireWriteBit(bool bit_value)
        {
            // platform specific

            // TMEX API TEST BUILD
            //TMTouchBit(session_handle, (short)bit_value);

            var byteValue = new byte();
            if (bit_value)
            {
                byteValue |= 1 << 7;
            }

            m_I2CDevice.Write(new byte[] { FunctionCommand.ONEWIRE_SINGLE_BIT, byteValue });


            ReadStatus();
        }

        /// <summary>
        /// Read 1 bit of data from the 1-Wire bus 
        /// </summary>
        /// <returns>true : bit read is 1, false : bit read is 0</returns>
        public bool OneWireReadBit()
        {
            // platform specific

            // TMEX API TEST BUILD
            //return (byte)TMTouchBit(session_handle, 0x01);

            var byteValue = new byte();

            byteValue |= 1 << 7;

            m_I2CDevice.Write(new[] { FunctionCommand.ONEWIRE_SINGLE_BIT, byteValue });

            var status = ReadStatus();

            return status.GetBit(StatusBit.SingleBitResult);
        }

        /// <summary>
        /// Read 1 bit of data from the 1-Wire bus 
        /// </summary>
        /// <returns>true : bit read is 1, false : bit read is 0</returns>
        public byte OneWireReadByte()
        {
            var buffer = new byte[1];
            m_I2CDevice.Write(new byte[] { DS2482_100.FunctionCommand.ONEWIRE_READ_BYTE });
            ReadStatus();
            m_I2CDevice.WriteRead(new byte[] { DS2482_100.FunctionCommand.SET_READ_POINTER, DS2482_100.RegisterSelection.READ_DATA }, buffer);
            return buffer[0];
        }
        
        public bool OneWireSelectChannel(int channel)
        {
            byte channelAddress;
            byte channelRegister;
            switch (channel)
            {
                case 0:
                    channelAddress = ChannelSelection.IO_0;
                    channelRegister = ChannelRegister.IO_0;
                    break;
                case 1:
                    channelAddress = ChannelSelection.IO_1;
                    channelRegister = ChannelRegister.IO_1;
                    break;
                case 2:
                    channelAddress = ChannelSelection.IO_2;
                    channelRegister = ChannelRegister.IO_2;
                    break;
                case 3:
                    channelAddress = ChannelSelection.IO_3;
                    channelRegister = ChannelRegister.IO_3;
                    break;
                case 4:
                    channelAddress = ChannelSelection.IO_4;
                    channelRegister = ChannelRegister.IO_4;
                    break;
                case 5:
                    channelAddress = ChannelSelection.IO_5;
                    channelRegister = ChannelRegister.IO_5;
                    break;
                case 6:
                    channelAddress = ChannelSelection.IO_6;
                    channelRegister = ChannelRegister.IO_6;
                    break;
                case 7:
                    channelAddress = ChannelSelection.IO_7;
                    channelRegister = ChannelRegister.IO_7;
                    break;
                default:
                    throw new Exception($"Invalid channel number ({channel})");
            }

            m_I2CDevice.Write(new byte[] { FunctionCommand.ONEWIRE_CHANNEL_SELECT, channelAddress });

            var channelRegisterContent = ReadChannelSelectionRegister();

            return channelRegister.Equals(channelRegisterContent);
        }

        public void EnableStrongPullup()
        {
            var configuration = new byte();
            configuration |= 1 << 2;
            configuration |= 1 << 7;
            configuration |= 1 << 5;
            configuration |= 1 << 4;

            m_I2CDevice.Write(new byte[] { DS2482_100.FunctionCommand.WRITE_CONFIGURATION, configuration });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_I2CDevice != null)
                    m_I2CDevice.Dispose();
            }
        }

        #endregion Services

        #region Test build
        static byte[] dscrcTable = (new[]
        {
            0, 94,188,226, 97, 63,221,131,194,156,126, 32,163,253, 31, 65,
            157,195, 33,127,252,162, 64, 30, 95,  1,227,189, 62, 96,130,220,
            35,125,159,193, 66, 28,254,160,225,191, 93,  3,128,222, 60, 98,
            190,224,  2, 92,223,129, 99, 61,124, 34,192,158, 29, 67,161,255,
            70, 24,250,164, 39,121,155,197,132,218, 56,102,229,187, 89,  7,
            219,133,103, 57,186,228,  6, 88, 25, 71,165,251,120, 38,196,154,
            101, 59,217,135,  4, 90,184,230,167,249, 27, 69,198,152,122, 36,
            248,166, 68, 26,153,199, 37,123, 58,100,134,216, 91,  5,231,185,
            140,210, 48,110,237,179, 81, 15, 78, 16,242,172, 47,113,147,205,
            17, 79,173,243,112, 46,204,146,211,141,111, 49,178,236, 14, 80,
            175,241, 19, 77,206,144,114, 44,109, 51,209,143, 12, 82,176,238,
            50,108,142,208, 83, 13,239,177,240,174, 76, 18,145,207, 45,115,
            202,148,118, 40,171,245, 23, 73,  8, 86,180,234,105, 55,213,139,
            87,  9,235,181, 54,104,138,212,149,203, 41,119,244,170, 72, 22,
            233,183, 85, 11,136,214, 52,106, 43,117,151,201, 74, 20,246,168,
            116, 42,200,150, 21, 75,169,247,182,232, 10, 84,215,137,107, 53
        }).Select(x => (byte)x).ToArray();


        /// <summary>
        /// Calculate the CRC8 of the byte value provided with the current 
        // global 'crc8' value. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>current global crc8 value</returns>
        byte docrc8(byte value)
        {
            // See Application Note 27

            // TEST BUILD
            m_CRC8 = dscrcTable[m_CRC8 ^ value];
            return m_CRC8;
        }

        #endregion Test build

        #region Internal classes
        public class FunctionCommand
        {
            public const byte DEVICE_RESET = 0xF0;
            public const byte SET_READ_POINTER = 0xE1;
            public const byte WRITE_CONFIGURATION = 0xD2;
            public const byte ONEWIRE_RESET = 0xB4;
            public const byte ONEWIRE_SINGLE_BIT = 0x87;
            public const byte ONEWIRE_WRITE_BYTE = 0xA5;
            public const byte ONEWIRE_READ_BYTE = 0x96;
            public const byte ONEWIRE_TRIPLET = 0x78;
            public const byte ONEWIRE_CHANNEL_SELECT = 0xC3;
        }

        public class RegisterSelection
        {
            public const byte STATUS = 0xF0;
            public const byte READ_DATA = 0xE1;
            public const byte CONFIGURATION = 0xC3;
            public const byte CHANNEL_SELECTION = 0xD2;
        }

        //Parameter zum Auswählen eines Kanals
        public class ChannelSelection
        {
            public const byte IO_0 = 0xF0;
            public const byte IO_1 = 0xE1;
            public const byte IO_2 = 0xD2;
            public const byte IO_3 = 0xC3;
            public const byte IO_4 = 0xB4;
            public const byte IO_5 = 0xA5;
            public const byte IO_6 = 0x96;
            public const byte IO_7 = 0x87;
        }

        //Inhalt des Channel Selection Registers nach Auswählen eines Kanals
        public class ChannelRegister
        {
            public const byte IO_0 = 0xB8;
            public const byte IO_1 = 0xB1;
            public const byte IO_2 = 0xAA;
            public const byte IO_3 = 0xA3;
            public const byte IO_4 = 0x9C;
            public const byte IO_5 = 0x95;
            public const byte IO_6 = 0x8E;
            public const byte IO_7 = 0x87;
        }

        public class StatusBit
        {
            /// <summary>
            /// Branch Direction Taken (DIR)
            /// </summary>
            public const int BranchDirectionTaken = 7;
            /// <summary>
            /// Triplet Second Bit (TSB)
            /// </summary>
            public const int TripletSecondBit = 6;
            /// <summary>
            /// Single Bit Result (SBR)
            /// </summary>
            public const int SingleBitResult = 5;
            /// <summary>
            /// Device Reset (RST)
            /// </summary>
            public const int DeviceReset = 4;
            /// <summary>
            /// Logic Level (LL)
            /// </summary>
            public const int LogicLevel = 3;
            /// <summary>
            /// Short Detected (SD)
            /// </summary>
            public const int ShortDetected = 2;
            /// <summary>
            /// Presence-Pulse Detect (PPD)
            /// </summary>
            public const int PresencePulseDetected = 1;
            /// <summary>
            /// 1-Wire Busy (1WB)
            /// </summary>
            public const int OneWireBusy = 0;
        }

        public class TripletDirection
        {
            public const byte ONE = 0x40;
            public const byte ZERO = 0x00;
        }

        #endregion Internal classes
    }
}
