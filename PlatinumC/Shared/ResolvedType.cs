using PlatinumC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerCore.Interfaces;

namespace PlatinumC.Shared
{
    public enum SupportedType
    {
        Byte,
        Int,
        Float,
        Ptr,
        String,
        Void
    }
    public class ResolvedType
    {
        public SupportedType SupportedType { get; set; }
        public ResolvedType? UnderlyingType { get; set; }

        public ResolvedType(SupportedType supportedType, ResolvedType? underlyingType)
        {
            SupportedType = supportedType;
            UnderlyingType = underlyingType;
        }

        public bool IsPointer => SupportedType == SupportedType.Ptr;
        public bool Is(SupportedType supportedType)
        {
            return SupportedType == supportedType;
        }

        public bool Is(ResolvedType? resolvedType)
        {
            if (resolvedType == null) return false;
            if (SupportedType == SupportedType.Ptr) return  resolvedType.SupportedType == SupportedType.Ptr && UnderlyingType!.Is(resolvedType.UnderlyingType);

            return SupportedType == resolvedType.SupportedType;
        }

        public int Size => SupportedType == SupportedType.Byte ? 1 : 4;
        public int ReferencedTypeSize => UnderlyingType?.Size ?? throw new InvalidOperationException();

        public static ResolvedType Create(SupportedType supportedType)
        {
            return new ResolvedType(supportedType, null);
        }

        public static ResolvedType Create(SupportedType supportedType, ResolvedType underlyingType)
        {
            return new ResolvedType(supportedType, underlyingType);
        }

        internal int GetMemberOffset(IToken assignmentTarget)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            if (IsPointer) return $"{UnderlyingType}*";
            return $"{SupportedType}";
        }
    }
}
