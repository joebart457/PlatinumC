using PlatinumC.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Resolver
{
    public class ResolverResult
    {
        public List<TypedDeclaration> TypedDeclarations { get; set; }

        public ResolverResult(List<TypedDeclaration> typedDeclarations)
        {
            TypedDeclarations = typedDeclarations;
        }
    }
}
