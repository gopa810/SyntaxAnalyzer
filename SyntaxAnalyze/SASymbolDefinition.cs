using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntaxAnalyze
{
    public class SASymbolDefinition
    {
        public List<List<SAGrammarSymbol>> Lines = new List<List<SAGrammarSymbol>>();

        private SAGrammarSymbol lastItem = null;

        public void AddItem(string str, SAGrammarItemType type)
        {
            if (type == SAGrammarItemType.Operator)
            {
                if (str.Equals("|"))
                {
                    List<SAGrammarSymbol> line = new List<SAGrammarSymbol>();
                    Lines.Add(line);
                }
            }
            else
            {
                SAGrammarSymbol item = new SAGrammarSymbol();
                item.Type = type;
                item.Value = str;
                lastItem = item;

                List<SAGrammarSymbol> line = null;
                if (Lines.Count == 0)
                {
                    line = new List<SAGrammarSymbol>();
                    Lines.Add(line);
                }
                else
                {
                    line = Lines[Lines.Count - 1];
                }

                if (line != null)
                {
                    line.Add(item);
                }
            }
        }

        internal void SetMinMaxForLastItem(int p, int p_2)
        {
            if (lastItem != null)
            {
                lastItem.MinOccurences = p;
                lastItem.MaxOccurences = p_2;
            }
        }
    }
}
