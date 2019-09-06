using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rca.OneWireLib
{
    /// <summary>
    /// Once the bus master has detected a presence, it can issue one of the seven ROM function commands that
    /// the DS28E17 supports. All ROM function commands are 8 bits long.
    /// </summary>
    public class RomCommand
    {
        /// <summary>
        /// When a system is initially brought up, the bus master might not know the number of devices on the
        /// 1-Wire bus or their ROM ID numbers. By taking advantage of the wired-AND property of the bus, the
        /// master can use a process of elimination to identify the ID of all slave devices. For each bit in
        /// the ID number, starting with the least significant bit, the bus master issues a triplet of time
        /// slots. On the first slot, each slave device participating in the search outputs the true value of
        /// its ID number bit. On the second slot, each slave device participating in the search outputs the
        /// complemented value of its ID number bit. On the third slot, the master writes the true value of the
        /// bit to be selected. All slave devices that do not match the bit written by the master stop
        /// participating in the search. If both of the read bits are zero, the master knows that slave devices
        /// exist with both states of the bit. By choosing which state to write, the bus master branches in the
        /// search tree. After one complete pass, the bus master knows the ROM ID number of a single device.
        /// Additional passes identify the ID numbers of the remaining devices.
        /// </summary>
        public const byte Search = 0xF0;
        /// <summary>
        /// The Read ROM command allows the bus master to read the DS28E17’s 8-bit family code, unique 48-bit
        /// serial number, and 8-bit CRC. This command can only be used if there is a single slave on the bus.
        /// If more than one slave is present on the bus, a data collision occurs when all slaves try to transmit
        /// at the same time (open drain produces a wired-AND result). The resultant family code and 48-bit serial
        /// number result in a mismatch of the CRC.
        /// </summary>
        public const byte Read = 0x33;
        /// <summary>
        /// The Match ROM command, followed by a 64-bit ROM sequence, allows the bus master to address a specific
        /// DS28E17 on a multidrop bus. Only the DS28E17 that exactly matches the 64-bit ROM sequence responds to
        /// the subsequent Device command. All other slaves wait for a reset pulse. This command can be used with
        /// a single device or multiple devices on the bus.
        /// </summary>
        public const byte Match = 0x55;
        /// <summary>
        /// This command can save time in a single-drop bus system by allowing the bus master to access the Device
        /// commands without providing the 64-bit ROM ID. If more than one slave is present on the bus and, for
        /// example, a read command is issued following the Skip ROM command, data collision occurs on the bus as
        /// multiple slaves transmit simultaneously (open-drain pulldowns produce a wired-AND result).
        /// </summary>
        public const byte Skip = 0xCC;
        /// <summary>
        /// To maximize the data throughput in a multidrop environment, the Resume command is available. This
        /// command checks the status of the RC bit and, if it is set, directly transfers control to the Device
        /// commands, similar to a Skip ROM command. The only way to set the RC bit is through successfully
        /// executing the Match ROM, Search ROM, or Overdrive-Match ROM command. Once the RC bit is set, the device
        /// can repeatedly be accessed through the Resume command. Accessing another device on the bus clears the
        /// RC bit, preventing two or more devices from simultaneously responding to the Resume command.
        /// </summary>
        public const byte Resume = 0xA5;
        /// <summary>
        /// On a single-drop bus this command can save time by allowing the bus master to access the Device commands
        /// without providing the 64-bit ROM ID. Unlike the normal Skip ROM command, the Overdrive-Skip ROM command
        /// sets the DS28E17 into the overdrive mode (OD = 1). All communication following this command must occur at
        /// overdrive speed until a reset pulse of minimum 480μs duration resets all devices on the bus to standard
        /// speed (OD = 0).
        /// </summary>
        public const byte OverdriveSkip = 0x3C;
        /// <summary>
        /// The Overdrive-Match ROM command followed by a 64-bit ROM sequence transmitted at overdrive speed allows
        /// the bus master to address a specific DS28E17 on a multidrop bus and to simultaneously set it in overdrive
        /// mode. Only the DS28E17 that exactly matches the 64-bit ROM sequence responds to the subsequent Device
        /// command. Slaves already in overdrive mode from a previous Overdrive-Skip ROM or successful Overdrive-
        /// Match ROM command remain in overdrive mode. All overdrive-capable slaves return to standard speed at the
        /// next reset pulse of minimum 480μs duration. The Overdrive-Match ROM command can be used with a single
        /// device or multiple devices on the bus.
        /// </summary>
        public const byte OverdriveMatch = 0x69;
        /// <summary>
        /// Only for DS18B20 ???
        /// </summary>
        public const byte AlarmSearch = 0xEC;
    }
}