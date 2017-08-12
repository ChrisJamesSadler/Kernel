using Kernel.System.Core;
using Kernel.System.HAL;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kernel.System.IO
{
    public static unsafe class FatFileSystem
    {
        private static byte nextMountPoint = (byte)'C';
        private static List<ATAPartition> mFs;
        private static FSEntry filesystem_root_node = null;
        public static VFSDirectory rootDir = null;

        public static void Init()
        {
            mFs = new List<ATAPartition>(10);
            for (int i = 0; i < ATA.ATAPartitions.Count; i++)
            {
                if (ATA.ATAPartitions[i] is ATAPartition)
                {
                    ATAPartition part = ATA.ATAPartitions[i];
                    ReadBiosParamBlock(part, part.offset);
                }
            }
            UpdateVFS();
        }

        private static void ReadBiosParamBlock(ATAPartition hd, ulong paritionOffset)
        {
            BiosParameterBlock* bpb = (BiosParameterBlock*)Heap.alloc((uint)sizeof(BiosParameterBlock));
            hd.host.ReadBlock(paritionOffset, 1, (byte*)(uint)bpb);
            ulong fatStart = paritionOffset + bpb->reservedSectors;
            ulong fatSize = bpb->tableSize;
            ulong dataStart = fatStart + fatSize * bpb->fatCopies;
            ulong rootStart = dataStart + bpb->sectorsPerCluster * (bpb->rootCluster - 2);

            if (rootStart == 0)
            {
                Heap.free((byte*)(uint)bpb);
                return;
            }

            hd.mountPoint = nextMountPoint;
            nextMountPoint++;
            hd.fatStart = fatStart;
            hd.fatSize = fatSize;
            hd.dataStart = dataStart;
            hd.rootStart = rootStart;
            hd.sectorsPerCluster = bpb->sectorsPerCluster;
            mFs.Add(hd);
            FATDirectoryEntry* dirent = (FATDirectoryEntry*)Heap.alloc((uint)sizeof(FATDirectoryEntry) * 16);
            hd.host.ReadBlock(rootStart, 1, (byte*)(uint)dirent);
            hd.rootName = (byte*)Heap.alloc(8);
            Utils.memcpy(hd.rootName, (byte*)dirent, 8);
            Utils.strDepad(hd.rootName, 8);
            if (filesystem_root_node == null)
            {
                FSEntry entry = new FSEntry();
                entry.address = rootStart;
                entry.name = (byte*)Heap.alloc(8);
                entry.host = hd;
                filesystem_root_node = entry;
            }
            Heap.free((byte*)(uint)dirent);
            Heap.free((byte*)(uint)bpb);
        }

        public static void UpdateVFS()
        {
            FATDirectoryEntry* root = (FATDirectoryEntry*)(uint)Heap.alloc((uint)(sizeof(FATDirectoryEntry) * 16));
            byte* name = (byte*)Heap.alloc(15);
            filesystem_root_node.host.host.ReadBlock(filesystem_root_node.address, 1, (byte*)(uint)root);

            rootDir = new VFSDirectory();
            rootDir.address = filesystem_root_node.address;
            rootDir.name = (byte*)Heap.alloc(15);
            Utils.memcpy(rootDir.name, name, 15);
            if (!Utils.isChar(rootDir.name[0]))
            {
                rootDir.name[0] = filesystem_root_node.host.mountPoint;
                rootDir.name[1] = 0;
            }
            Console.WriteLine(rootDir.name);

            for (int d = 2; d < 16; d++)
            {
                Utils.memcpy(name, (byte*)(root) + (sizeof(FATDirectoryEntry) * d), 8);
                Utils.memset(name + 8, 0, 7);
                Utils.strDepad(name, 8);
                if (Utils.isChar(name[0]) && Utils.strContains(name, "~") == -1 && Utils.strLen(name) > 0)
                {
                    uint firstFileCluster = (uint)((root[d].firstClusterHi << 16) | root[d].firstClusterLow);
                    if (root[d].getEntryType == EntryType.Directory)
                    {
                        VFSDirectory dir = new VFSDirectory();
                        dir.parent = rootDir;
                        dir.address = firstFileCluster;
                        dir.name = (byte*)Heap.alloc(15);
                        Utils.memcpy(dir.name, name, 15);
                        rootDir.directories.Add(dir);
                        UpdateVFSEnumerateDirectory(filesystem_root_node.address + firstFileCluster - 2, dir);
                        Console.WriteLine("Found a directory");
                    }
                    else if (root[d].getEntryType == EntryType.File)
                    {
                        VFSFile fil = new VFSFile();
                        fil.address = firstFileCluster;
                        fil.name = (byte*)Heap.alloc(15);
                        fil.size = (int)root[d].size;
                        Utils.memcpy(fil.name, name, 15);
                        rootDir.files.Add(fil);
                        Console.WriteLine("Found a file");
                    }
                }
            }
            Heap.free(name);
            Heap.free((byte*)(uint)root);
        }

        private static void UpdateVFSEnumerateDirectory(ulong address, VFSDirectory thisDir)
        {
            FATDirectoryEntry* dirent = (FATDirectoryEntry*)Heap.alloc((uint)sizeof(FATDirectoryEntry) * 16);
            byte* name = (byte*)Heap.alloc(15);
            filesystem_root_node.host.host.ReadBlock(address, 1, (byte*)(uint)dirent);
            for (int d = 2; d < 16; d++)
            {
                Utils.memcpy(name, (byte*)(dirent) + (sizeof(FATDirectoryEntry) * d), 8);
                Utils.memset(name + 8, 0, 7);
                Utils.strDepad(name, 8);
                if (Utils.isChar(name[0]) && Utils.strContains(name, "~") == -1 && Utils.strLen(name) > 0)
                {
                    uint firstFileCluster = (uint)((dirent[d].firstClusterHi << 16) | dirent[d].firstClusterLow);
                    if (dirent[d].getEntryType == EntryType.Directory)
                    {
                        VFSDirectory dir = new VFSDirectory();
                        dir.parent = thisDir;
                        dir.address = firstFileCluster;
                        dir.name = (byte*)Heap.alloc(15);
                        Utils.memcpy(dir.name, name, 15);
                        thisDir.directories.Add(dir);
                        UpdateVFSEnumerateDirectory(firstFileCluster, dir);
                        Console.WriteLine("Found a directory");
                    }
                    else if (dirent[d].getEntryType == EntryType.File)
                    {
                        VFSFile fil = new VFSFile();
                        fil.address = firstFileCluster;
                        fil.name = (byte*)Heap.alloc(15);
                        fil.size = (int)dirent[d].size;
                        Utils.memcpy(fil.name, name, 15);
                        thisDir.files.Add(fil);
                        Console.WriteLine("Found a file");
                    }
                }
            }
            Heap.free(name);
            Heap.free((byte*)(uint)dirent);
        }

        [StructLayout(LayoutKind.Explicit, Size = 90)]
        public struct BiosParameterBlock
        {
            [FieldOffset(0)]
            public fixed byte jump[3];
            [FieldOffset(3)]
            public fixed byte softName[8];
            [FieldOffset(11)]
            public ushort bytesPerSector;
            [FieldOffset(13)]
            public byte sectorsPerCluster;
            [FieldOffset(14)]
            public ushort reservedSectors;
            [FieldOffset(16)]
            public byte fatCopies;
            [FieldOffset(17)]
            public ushort rootDirEntries;
            [FieldOffset(19)]
            public ushort totalSectors;
            [FieldOffset(21)]
            public byte mediaType;
            [FieldOffset(22)]
            public ushort fatSectorCount;
            [FieldOffset(24)]
            public ushort sectorsPerTrack;
            [FieldOffset(26)]
            public ushort headCount;
            [FieldOffset(28)]
            public uint hiddenSectors;
            [FieldOffset(32)]
            public uint totalSectorCount;

            [FieldOffset(36)]
            public uint tableSize;
            [FieldOffset(40)]
            public ushort extFlags;
            [FieldOffset(42)]
            public ushort fatVersion;
            [FieldOffset(44)]
            public uint rootCluster;
            [FieldOffset(48)]
            public ushort fatInfo;
            [FieldOffset(50)]
            public ushort backupSector;
            [FieldOffset(52)]
            public fixed byte reserved0[12];
            [FieldOffset(64)]
            public byte driveNumber;
            [FieldOffset(65)]
            public byte reserved1;
            [FieldOffset(66)]
            public byte bootSigniture;
            [FieldOffset(67)]
            public ushort volumeID;
            [FieldOffset(71)]
            public fixed byte volumeLabel[11];
            [FieldOffset(82)]
            public fixed byte fatTypeLabel[8];
        }

        [StructLayout(LayoutKind.Explicit, Size = 32)]
        public struct FATDirectoryEntry
        {
            [FieldOffset(0)]
            public fixed byte name[8];
            [FieldOffset(8)]
            public fixed byte ext[3];
            [FieldOffset(11)]
            public byte attributes;
            [FieldOffset(12)]
            public byte reserved;

            [FieldOffset(13)]
            public byte cTimeTenth;
            [FieldOffset(14)]
            public ushort cTime;
            [FieldOffset(16)]
            public ushort cDate;
            [FieldOffset(18)]
            public ushort aTime;
            [FieldOffset(20)]
            public ushort firstClusterHi;

            [FieldOffset(22)]
            public ushort wTime;
            [FieldOffset(24)]
            public ushort wDate;
            [FieldOffset(26)]
            public ushort firstClusterLow;
            [FieldOffset(28)]
            public uint size;

            public EntryType getEntryType
            {
                get
                {
                    var x = attributes & ((byte)FatAttributes.Directory | (byte)FatAttributes.VolumeID);
                    if (x == (int)EntryType.File)
                    {
                        return EntryType.File;
                    }
                    else if (x == (int)EntryType.Directory)
                    {
                        return EntryType.Directory;
                    }
                    else if (x == (int)EntryType.Root)
                    {
                        return EntryType.Root;
                    }
                    return EntryType.Unknown;
                }
            }
        }

        public enum FatAttributes : byte
        {
            Test = 0x01,
            Hidden = 0x02,
            System = 0x04,
            VolumeID = 0x08,
            Directory = 0x10,
            Archive = 0x20,
            UnusedOrDeletedEntry = 0xE5,
            LongName = 0x0F
        }

        public enum EntryType : byte
        {
            File = 0,
            Root = 0x08,
            Directory = 0x10,
            Unknown = 255
        }

        public class VFSDirectory
        {
            public byte* name;
            public ulong address;
            public VFSDirectory parent;
            public List<VFSDirectory> directories = new List<VFSDirectory>(16);
            public List<VFSFile> files = new List<VFSFile>(16);
        }

        public class VFSFile
        {
            public byte* name;
            public ulong address;
            public int size;
        }
    }

    public unsafe class FSEntry
    {
        public ulong address;
        public byte* name;
        public byte* ext;
        public FSEntry[] directories;
        public FSEntry[] files;
        public ATAPartition host;
    }
}