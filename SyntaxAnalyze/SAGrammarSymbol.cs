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
        public int MinOccurences = 1;
        public int MaxOccurences = 1;

        public SAGrammarSymbol()
        {
        }

        public SAGrammarSymbol(string title, SAGrammarItemType tp)
        {
            Value = title;
            Type = tp;
        }
    }
}

