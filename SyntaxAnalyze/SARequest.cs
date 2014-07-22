using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntaxAnalyze
{
    public class SARequest
    {
        public string GrammarText { get; set; }
        public string InputFileText { get; set; }

        public SAGrammar Grammar { get; set; }
    }
}
