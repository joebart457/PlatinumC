using TokenizerCore.Interfaces;
using TokenizerCore.Model;
using TokenizerCore.Models.Constants;

namespace PlatinumC.Shared
{
    public enum SupportedType
    {
        Byte,
        Int,
        Float,
        Ptr,
        String,
        Void,
        Custom
    }
    public class ResolvedType
    {
        public SupportedType SupportedType { get; set; }
        public ResolvedType? UnderlyingType { get; set; }
        public IToken TypeName { get; set; }
        public List<(string fieldName, ResolvedType fieldType)> Fields { get; private set; }
        public ResolvedType(SupportedType supportedType, ResolvedType? underlyingType)
        {
            SupportedType = supportedType;
            UnderlyingType = underlyingType;
            Fields = new();
            TypeName = new Token(BuiltinTokenTypes.Word, SupportedType.ToString(), -1, -1);
        }

        public ResolvedType(IToken typeName, List<(string fieldName, ResolvedType fieldType)> fields)
        {
            SupportedType = SupportedType.Custom;
            UnderlyingType = null;
            Fields = fields;
            if (Fields.Any(x => x.fieldType.Is(this)))
                throw new InvalidOperationException("illegal recursive type");
            TypeName = typeName;
        }

        public bool IsPointer => SupportedType == SupportedType.Ptr;
        public bool IsCustomType => SupportedType == SupportedType.Custom;
        public bool Is(SupportedType supportedType)
        {
            return SupportedType == supportedType && UnderlyingType == null && SupportedType != SupportedType.Custom;
        }

        public bool Is(ResolvedType? resolvedType)
        {
            if (resolvedType == null) return false;
            if (SupportedType == SupportedType.Ptr) return resolvedType.SupportedType == SupportedType.Ptr && UnderlyingType!.Is(resolvedType.UnderlyingType);
            // we only validate TypeNames for custom types, not that their fields are equivalent
            if (SupportedType == SupportedType.Custom) return resolvedType.SupportedType == SupportedType.Custom && TypeName!.Lexeme == resolvedType.TypeName.Lexeme;
            return SupportedType == resolvedType.SupportedType;
        }

        public int Size()
        {
            if (SupportedType == SupportedType.Byte) return 1;
            if (SupportedType == SupportedType.Custom)
            {
                int size = Fields.Sum(x => x.fieldType.Size());
            }
            return 4;
        }

        public int StackSize()
        {
            if (SupportedType == SupportedType.Byte) return 4; // bytes are reperesented by dword to simplify stack operations and keep stack aligned
            if (SupportedType == SupportedType.Custom)
            {
                int size = Fields.Sum(x => x.fieldType.Size());
            }
            return 4;
        }
        public int ReferencedTypeSize => UnderlyingType?.Size() ?? throw new InvalidOperationException();

        public static ResolvedType Create(SupportedType supportedType)
        {
            return new ResolvedType(supportedType, null);
        }

        public static ResolvedType Create(SupportedType supportedType, ResolvedType underlyingType)
        {
            return new ResolvedType(supportedType, underlyingType);
        }

        public static ResolvedType Create(IToken typeName, List<(string fieldName, ResolvedType fieldType)> fields)
        {
            return new ResolvedType(typeName, fields);
        }

        internal int GetMemberOffset(IToken assignmentTarget)
        {
            int offset = 0;
            foreach(var field in Fields)
            {
                if (field.fieldName == assignmentTarget.Lexeme) return offset;
                offset += field.fieldType.Size();
            }
            throw new InvalidOperationException($"field {assignmentTarget.Lexeme} does not exist on type {ToString()}");
        }

        public override string ToString()
        {
            if (IsPointer) return $"{UnderlyingType}*";
            return $"{SupportedType}";
        }
    }
}
