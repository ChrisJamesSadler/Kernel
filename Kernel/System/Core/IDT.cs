using sys = System;
using XSharp.Compiler;
using Cosmos.Assembler;
using System.Reflection;
using Cosmos.IL2CPU.Plugs;

namespace Kernel.System.Core
{
    public static unsafe class IDT
    {
        public static void Init()
        {
            var idtp = Label.getIDTPointer();
            idtp->limit = (ushort)((sizeof(Label.IDTEntry) * 256) - 1);
            idtp->_base = (uint)Label.getIDT();
            Update();
            Load();
        }

        [PlugMethod(PlugRequired = true)]
        public static void Load() { }

        [PlugMethod(PlugRequired = true)]
        public static void Update() { }

        [Plug(Target = typeof(IDT))]
        public class IDTPlug
        {
            private static MethodBase GetMethodDef(Assembly aAssembly, string aType, string aMethodName, bool aErrorWhenNotFound)
            {
                sys.Type xType = aAssembly.GetType(aType, false);
                if (xType != null)
                {
                    MethodBase xMethod = xType.GetMethod(aMethodName);
                    if (xMethod != null)
                    {
                        return xMethod;
                    }
                }
                if (aErrorWhenNotFound)
                {
                    throw new sys.Exception("Method '" + aType + "::" + aMethodName + "' not found!");
                }
                return null;
            }

            [PlugMethod(Assembler = typeof(asmIDTFlush))]
            public static void Load() { }

            public class asmIDTFlush : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    new LiteralAssemblerCode("lidt[_NATIVE_IDT_Pointer]");
                }
            }

            [PlugMethod(Assembler = typeof(asmIDTGen))]
            public static void Update() { }

            public class asmIDTGen : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    int start = 0;
                    for (int i = start; i < 256; i++)
                    {
                        if (i == 1 || i == 3)
                        {
                            //continue; // was used for debugging
                        }

                        XS.Set(XSRegisters.EAX, "_isr" + i.ToString("X2"));
                        XS.Set("_NATIVE_IDT_Contents", XSRegisters.AL, destinationDisplacement: (i * 8) + 0);
                        XS.Set("_NATIVE_IDT_Contents", XSRegisters.AH, destinationDisplacement: (i * 8) + 1);
                        XS.Set("_NATIVE_IDT_Contents", 0x08, destinationDisplacement: (i * 8) + 2, size: XSRegisters.RegisterSize.Byte8);
                        XS.Set("_NATIVE_IDT_Contents", 0x00, destinationDisplacement: (i * 8) + 4, size: XSRegisters.RegisterSize.Byte8);
                        XS.Set("_NATIVE_IDT_Contents", 0x8E, destinationDisplacement: (i * 8) + 5, size: XSRegisters.RegisterSize.Byte8);
                        XS.ShiftRight(XSRegisters.EAX, 16);
                        XS.Set("_NATIVE_IDT_Contents", XSRegisters.AL, destinationDisplacement: (i * 8) + 6);
                        XS.Set("_NATIVE_IDT_Contents", XSRegisters.AH, destinationDisplacement: (i * 8) + 7);
                    }
                    XS.Jump("skip_isrhandlers");
                    var xInterruptsWithParam = new[] { 8, 10, 11, 12, 13, 14 };

                    for (int j = start; j < 256; j++)
                    {
                        XS.Label("_isr" + j.ToString("X2"));
                        XS.ClearInterruptFlag();
                        XS.Call("__INTERRUPT_OCCURRED__");

                        if (global::System.Array.IndexOf(xInterruptsWithParam, j) == -1)
                        {
                            XS.Push(0);
                        }
                        XS.Push((uint)j);
                        XS.Jump("CommonISRBase");
                    }
                    XS.Label("CommonISRBase");
                    XS.PushAllRegisters();
                    XS.Set(XSRegisters.EAX, XSRegisters.ESP);
                    XS.Set("static_field__Kernel_System_Core_ISR_old_esp", XSRegisters.EAX, destinationIsIndirect: true);

                    XS.Sub(XSRegisters.ESP, 4);
                    XS.Set(XSRegisters.EAX, XSRegisters.ESP);

                    XS.And(XSRegisters.ESP, 0xfffffff0);
                    XS.Sub(XSRegisters.ESP, 512);
                    XS.SSE.FXSave(XSRegisters.ESP, isIndirect: true);
                    XS.Set(XSRegisters.EAX, XSRegisters.ESP, destinationIsIndirect: true);

                    XS.Push(XSRegisters.EAX);
                    XS.Push(XSRegisters.EAX);

                    XS.Set("static_field__Kernel_System_Core_ISR_Registers", XSRegisters.EAX, destinationIsIndirect: true);
                    MethodBase xHandler = GetMethodDef(typeof(ISR).Assembly, typeof(ISR).FullName, "CommonISRHandler", true);
                    XS.Call(LabelName.Get(xHandler));

                    XS.Pop(XSRegisters.EAX);
                    XS.SSE.FXRestore(XSRegisters.ESP, isIndirect: true);

                    XS.Set(XSRegisters.ESP, XSRegisters.EAX);
                    XS.Add(XSRegisters.ESP, 4);

                    XS.Set(XSRegisters.EAX, "static_field__Kernel_System_Core_ISR_old_esp", sourceIsIndirect: true);
                    XS.Set(XSRegisters.ESP, XSRegisters.EAX);
                    XS.PopAllRegisters();

                    XS.Add(XSRegisters.ESP, 8);
                    XS.InterruptReturn();
                    XS.Label("skip_isrhandlers");
                }
            }
        }
    }
}