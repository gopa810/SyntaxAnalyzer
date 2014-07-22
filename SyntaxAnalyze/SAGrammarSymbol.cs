using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntaxAnalyze
{
    public class SAGrammarSymbol: SAGrammarItem
    {
        public SAGrammarItemType Type;
        public string Value;
        public UInt32 MinOccurences = 1;
        public UInt32 MaxOccurences = 1;
    }
}

