using System.Collections.Generic;
using static Kernel.System.HAL.ATABlockDevice;

namespace Kernel.System.HAL
{
    public static unsafe class ATA
    {
        public static List<ATABlockDevice> ATABlockDevices = new List<ATABlockDevice>(32);
        public static List<ATAPartition> ATAPartitions = new List<ATAPartition>(32);

        public static void Init()
        {
            DetectDrive(ControllerIdEnum.Primary, BusPositionEnum.Master);
            DetectDrive(ControllerIdEnum.Primary, BusPositionEnum.Slave);
            DetectDrive(ControllerIdEnum.Secondary, BusPositionEnum.Master);
            DetectDrive(ControllerIdEnum.Secondary, BusPositionEnum.Slave);
            foreach (ATAPartition part in ATAPartitions)
            {
                for (int i = 0; i < 40; i++)
                {
                    Console.Write((char)part.host.DeviceLa[i]);
                }
                Console.Write(" Partition ");
                Console.Write(part.id);
                Console.NewLine();
            }
        }

        public static void DetectDrive(ControllerIdEnum aControllerID, BusPositionEnum aBusPosition)
        {
            var dev = new ATABlockDevice(aControllerID == ControllerIdEnum.Primary, aControllerID, aBusPosition);
            if(dev.Exists)
            {
                ATABlockDevices.Add(dev);
            }
        }
    }
}
