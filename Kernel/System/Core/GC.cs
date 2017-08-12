using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;
using con = System.Console;
using Type = System.Type;
using Assembly = System.Reflection.Assembly;
using sys = System;
using System.Reflection;

namespace Kernel.System.Core
{
    public static unsafe class GC
    {
        static SomeGCItem v;
        static sefwsefsef f;
        static SomeOtherclass g;
        public static void Init()
        {
            Setup();
            //v = new SomeGCItem();
            //v.a = 0x0A;
            //v.b = 0x0B;
            //v.c = 0x0C;
            //v.d = 0x0D;
            //uint address = Utils.getPointer(v);

            //uint* ptr = (uint*)address;

            //Console.Write("Type value is ");
            //Console.WriteLine(ptr[0]);


            //f = new sefwsefsef();
            //f.a = 0x0A;
            //f.b = 0x0B;
            //address = Utils.getPointer(f);

            //ptr = (uint*)address;

            //Console.Write("Type value is ");
            //Console.WriteLine(ptr[0]);


            //g = new SomeOtherclass();
            //g = new SomeOtherclass();
            //g = new SomeOtherclass();
            //g = new SomeOtherclass();
            //g.a = 0x0A;
            //g.b = 0x0B;
            //address = Utils.getPointer(g);

            //ptr = (uint*)address;

            //Console.Write("Type value is ");
            //Console.WriteLine(ptr[0]);

            //for(int i = 0; i < Heap.entryCount; i++)
            //{
            //    var entry = Heap.directory->GetEntry(i);

            //    if(((uint*)entry->location)[0] == ((uint*)address)[0])
            //    {
            //        Console.WriteLine("Found instance");
            //    }
            //}

            //while (true) { }

            //Console.Write("The address is ");
            //Console.WriteLine(address);
            //byte* ptr = (byte*)address;
            //for (int i = 0; i < 4; i++)
            //{
            //    Console.Write("The value is ");
            //    Console.WriteLine(ptr[12 + i], 'h');
            //}
        }

        //public struct SomeGCItem
        //{
        //    public byte a;
        //    public byte b;
        //    public byte c;
        //    public byte d;
        //}

        public class SomeGCItem
        {
            public byte a;
            public byte b;
            public byte c;
            public byte d;
        }

        public class sefwsefsef
        {
            public string s;
            public int a;
            public int b;
        }

        public class SomeOtherclass
        {
            public int a;
            public int b;
        }

        [PlugMethod(Assembler = typeof(GCSetupPlug))]
        public static void Setup() { }
        [Plug(Target = typeof(GC))]
        public class GCSetupPlug : AssemblerMethod
        {
            public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
            {
                var allTemplates = Assembly.GetExecutingAssembly().GetTypes();
                con.WriteLine("---------------------------------");
                foreach(var template in allTemplates)
                {
                    if (!template.IsClass || typeof(sys.MulticastDelegate).IsAssignableFrom(template.BaseType) || template.IsInterface || template.Name.ToLower().Contains("<") || template.Name.ToLower().Contains("boot") || template.Name.ToLower().Contains("gc") || template.Name.ToLower().Contains("plug") || template.Name.ToLower().Contains("asm"))
                    {
                        continue;
                    }
                    con.WriteLine(template.Name);
                    foreach (var variable in template.GetFields((BindingFlags)int.MaxValue))// BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                    {
                        if (variable.FieldType.IsClass)
                        {
                            con.WriteLine("   Clas " + variable.Name);
                        }
                        else
                        {
                            con.WriteLine("   Elem " + variable.Name);
                        }
                    }
                }
                con.WriteLine("---------------------------------");
            }

            public uint GetTypeUID(Type aType)
            {
                return Cosmos.IL2CPU.ILScanner.instance.GetTypeUID(aType);
            }
        }
    }
}