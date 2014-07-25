using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntaxAnalyze
{
    public class SASymbolDefinition
    {
        public List<SAGrammarLine> Lines = new List<SAGrammarLine>();

        private SAGrammarSymbol lastItem = null;

        private SAGrammarLine lastLine = null;

        public void AddItem(string str, SAGrammarItemType type)
        {
            if (type == SAGrammarItemType.Operator)
            {
                if (str.Equals("|"))
                {
                    SAGrammarLine line = new SAGrammarLine();
                    Lines.Add(line);
                    lastLine = line;
                }
            }
            else
            {
                SAGrammarSymbol item = new SAGrammarSymbol();
                item.Type = type;
                item.Value = str;
                lastItem = item;

                SAGrammarLine line = null;
                if (Lines.Count == 0)
                {
                    line = new SAGrammarLine();
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

        public void SetNegator(bool t)
        {
            if (lastItem != null)
            {
                lastItem.Negative = t;
            }
        }

        public void setLineUnion()
        {
            if (lastLine != null)
            {
                lastLine.UnionOperator = true;
            }
        }
    }
}
