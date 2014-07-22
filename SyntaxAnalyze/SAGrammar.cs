using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntaxAnalyze
{
    public class SAGrammar
    {
        public Dictionary<string, SASymbolDefinition> Symbols = new Dictionary<string, SASymbolDefinition>();

        public SAGrammarParseMode parseMode = SAGrammarParseMode.Waiting;
        public SASymbolDefinition parsedItem = null;
        public SAGrammarItemType cachedItemType;
        public string cachedItem;
    }
}
