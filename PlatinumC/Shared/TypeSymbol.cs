using TokenizerCore.Interfaces;

namespace PlatinumC.Shared
{
    public class TypeSymbol
    {
        public IToken Token { get; set; }
        public SupportedType SupportedType { get; set; }
        public TypeSymbol? UnderlyingType { get; set; }
        public int ArraySize { get; set; }
        public TypeSymbol(IToken token, SupportedType supportedType, TypeSymbol? underlyingType, int arraySize = 1)
        {
            Token = token;
            SupportedType = supportedType;
            UnderlyingType = underlyingType;
        }
    }
}
