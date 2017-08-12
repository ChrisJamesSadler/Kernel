using Kernel.System;
using Kernel.System.Core;
using Kernel.System.Drivers;
using Kernel.System.HAL;
using Kernel.System.HAL.PCI;
using Kernel.System.Threading;
using System.Collections.Generic;
using Kernel.System.Audio;

namespace Kernel.Application
{
    public unsafe delegate void CommandMethod();// byte* msg);
    public unsafe class SCI : Shell
    {
        public static List<ICommand> Commands = new List<ICommand>(100);
        public static byte* content;
        public AudioStream stream;

        public new void Main()
        {
            AddCommand("clear", CMD_Clear, "Clears the display");
            AddCommand("help", CMD_Help, "List all commands");
            AddCommand("halt", CMD_Shutdown, "Power down the computer");
            AddCommand("reboot", CMD_Reboot, "Restart the computer");
            AddCommand("ps", CMD_ListProcesses, "List the running processes");
            AddCommand("lshw", CMD_ListHardware, "List the installed hardware");
            AddCommand("lsmod", CMD_ListDrivers, "List all running drivers");
            AddCommand("echo", CMD_Echo, "Echo the user input");
            AddCommand("gui", CMD_GUI, "Start the graphical interface");
            stream = new AudioStream(400);
            for (int i = 0; i < 1; i++)
            {
                stream.AddSample(207, 400);
                stream.AddSample(277, 300);
                stream.AddSample(329, 200);
                Thread.Sleep(500);
            }
            while (true)
            {
                Console.Write("CMD>>");
                content = Console.ReadLine();

                int length = 0;
                var ptr = content;
                while(*ptr != 0)
                {
                    ptr++;
                    length++;
                }

                bool found = false;
                foreach(var cmd in Commands)
                {
                    if(Utils.Compare(content, cmd.trigger))
                    {
                        found = true;
                        cmd.Invoke(content);
                    }
                }
                if(!found)
                {
                    foreach (var cmd in Commands)
                    {
                        if (Utils.StartsWith(content, cmd.trigger))
                        {
                            found = true;
                            cmd.Invoke(content);
                        }
                    }
                }
                if(!found && length > 0)
                {
                    Console.Write("Unknown Command ");
                    Console.WriteLine(content);
                }

                Heap.free(content);
                if (Console.X != 0)
                {
                    Console.NewLine();
                }
            }
        }

        public void AddCommand(byte* t, uint address, byte* d)
        {
            int strLength = 0;
            byte* ptr = t;
            while (*ptr != 0)
            {
                strLength++;
                ptr++;
            }
            int ptrLength = 0;
            ptr = d;
            while (*ptr != 0)
            {
                ptrLength++;
                ptr++;
            }
            char[] arr1 = new char[strLength];
            char[] arr2 = new char[ptrLength];
            AddCommand(new string(arr1), address, new string(arr2));
        }

        public void AddCommand(string t, CommandMethod address, string desc)
        {
            AddCommand(t, (uint)address.GetHashCode(), desc);
        }

        public void AddCommand(string t, uint address, string desc)
        {
            ICommand newCommand = new ICommand();
            newCommand.trigger = t;
            newCommand.method = address;
            newCommand.description = desc;
            Commands.Add(newCommand);
        }

        public static void CMD_Clear()//byte* param)
        {
            Console.Clear();
        }

        public static void CMD_Help()//byte* param)
        {
            for(int i = 0; i < Commands.Count; i++)
            {
                Console.Write(Commands[i].trigger);
                Console.Write("---");
                Console.WriteLine(Commands[i].description);
            }
        }

        public static void CMD_Shutdown()//byte* param)
        {
            ACPI.Shutdown();
        }

        public static void CMD_Reboot()//byte* param)
        {
            ACPI.Reboot();
        }

        public static void CMD_ListProcesses()//byte* param)
        {
            foreach (var thread in Scheduler.ActiveThreads)
            {
                Console.WriteLine(thread.name);
            }
        }

        public static void CMD_ListHardware()//byte* param)
        {
            foreach (var dev in PCI.PCIDevices)
            {
                Console.WriteLine(dev.Label);
            }
            foreach (var dev in DiskDrive.DetectedDiskDrives)
            {
                Console.WriteLine(dev.Label);
            }
        }

        public static void CMD_ListDrivers()//byte* param)
        {
            foreach (var dev in Driver.Drivers)
            {
                Console.WriteLine(dev.Label);
            }
        }

        public static void CMD_Echo()//byte* param)
        {
            byte* ptr = content + 5;
            while (*ptr != 0)
            {
                Console.Write((char)*ptr);
                ptr++;
            }
        }

        public static void CMD_GUI()//byte* param)
        {
            var desktop = new Thread(new SGI().Main, "GUI");
            desktop.Start();
            while (Thread.IsThreadAlive(desktop.TID))
            {
                CPU.Halt();
            }
        }

        public class ICommand
        {
            public string trigger;
            public uint method;
            public string description;

            public void Invoke(byte* ptr)
            {
                if (method != 0)
                {
                    Caller.CallCode(method, null);// (uint)ptr);
                }
            }
        }
    }
}


//int commandLength = 0;
//byte* commandPtr = content;
//while(*commandPtr != 0)
//{
//    commandPtr++;
//    commandLength++;
//}
//if(commandLength == 0)
//{
//    continue;
//}
//if(Contains(content, '|'))
//{
//    // split command bit
//}
//if (Compare(content, "halt"))
//{
//    ACPI.Shutdown();
//}
//else if (Compare(content, "reboot"))
//{
//    ACPI.Reboot();
//}
//else if (Compare(content, "ps"))
//{
//    foreach (var thread in Scheduler.ActiveThreads)
//    {
//        Console.WriteLine(thread.name);
//    }
//}
//else if(Compare(content, "lshw"))
//{
//    foreach(var dev in PCI.PCIDevices)
//    {
//        Console.WriteLine(dev.Label);
//    }
//    foreach(var dev in ATA.ATABlockDevices)
//    {
//        Console.WriteLine(dev.Label);
//    }
//}
//else if(Compare(content, "lsmod"))
//{
//    foreach(var dev in Driver.Drivers)
//    {
//        Console.WriteLine(dev.Label);
//    }
//}
//else if (StartsWith(content, "echo "))
//{
//    byte* ptr = content + 5;
//    while (*ptr != 0)
//    {
//        Console.Write((char)*ptr);
//        ptr++;
//    }
//}
//else if (Compare(content, "test"))
//{

//}
//else if(Compare(content, "gui"))
//{
//    var desktop = new Thread(new SGI().Main, "GUI");
//    desktop.Start();
//    while(Thread.IsThreadAlive(desktop.TID))
//    {
//        CPU.Halt();
//    }
//}
//else
//{
//    Console.Write("Unknown Command ");
//    byte* tmp = content;
//    while (*tmp != 0)
//    {
//        Console.Write((char)*tmp);
//        tmp++;
//    }
//}