using Cosmos.Assembler;
using Kernel.System.Core;
using Cosmos.IL2CPU.Plugs;

namespace Kernel
{
    public static unsafe class API
    {
        public static void Handle(ref ISR.IRQContext r)
        {

        }

        [PlugMethod(PlugRequired = true)]
        public static void DEMOCALL() { }

        [Plug(Target = typeof(API))]
        public class APIPlug
        {
            [PlugMethod(Assembler = typeof(asmAPIGen))]
            public static void DEMOCALL() { }

            public class asmAPIGen : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    new LiteralAssemblerCode("int 0x80");
                }
            }
        }
    }
}