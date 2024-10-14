using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlatinumC.Shared;

namespace PlatinumC.Parser
{
    public class ParsingResult
    {
        public List<Declaration> Declarations { get; set; }

        public ParsingResult(List<Declaration> declarations)
        {
            Declarations = declarations;
        }
    }
}
