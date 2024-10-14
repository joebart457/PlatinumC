
using PlatinumC.Compiler.TargetX86.Instructions;

namespace PlatinumC.Compiler.TargetX86
{

    public class StorageLocation
    {
        public RegisterOffset Offset { get; set; }
        public StorageLocation(RegisterOffset offset)
        {
            Offset = offset;
        }

        public bool IsRegister => Offset.IsPlainRegister;
        public X86Register Register => Offset.Register;

    }


}
