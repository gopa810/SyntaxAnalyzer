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
        public bool Negative = false;

        public SAGrammarSymbol()
        {
        }

        public SAGrammarSymbol(string title, SAGrammarItemType tp)
        {
            Value = title;
            Type = tp;
        }
    }

    public class SAGrammarLine
    {
        public List<SAGrammarSymbol> Symbols = new List<SAGrammarSymbol>();
        public bool UnionOperator = false;

        public void Add(SAGrammarSymbol s)
        {
            Symbols.Add(s);
        }
    }
}

