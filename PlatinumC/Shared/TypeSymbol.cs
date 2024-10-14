using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerCore.Interfaces;

namespace PlatinumC.Shared
{
    public class TypeSymbol
    {
        public IToken Token { get; set; }
        public SupportedType SupportedType { get; set; }
        public TypeSymbol? UnderlyingType { get; set; }
        public TypeSymbol(IToken token, SupportedType supportedType, TypeSymbol? underlyingType)
        {
            Token = token;
            SupportedType = supportedType;
            UnderlyingType = underlyingType;
        }
    }
}
