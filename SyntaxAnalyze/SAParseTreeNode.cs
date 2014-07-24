using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntaxAnalyze
{
    public class SAParseTreeNode
    {
        public SAGrammarSymbol Symbol = null;

        public bool Valid = true;
        public String Value = string.Empty;
        public int Pos = 0;
        public int NextPos = 0;
        public string TextValue { get; set; }

        public List<SAParseNodeLine> Lines = new List<SAParseNodeLine>();

        public bool hasLines()
        {
            return Lines != null && Lines.Count > 0;
        }

        public void removeSymbolsWithEmptyLines()
        {
            foreach (SAParseNodeLine line in Lines)
            {
                int i = 0;
                while (i < line.Symbols.Count)
                {
                    SAParseTreeNode tn = line.Symbols[i];
                    if (tn.Lines.Count == 0 && tn.Symbol.Type == SAGrammarItemType.Identifier)
                    {
                        line.Symbols.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        public void removeNodes(SAGrammar g)
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                SAParseNodeLine line = Lines[i];
                int j = 0;
                while (j < line.Symbols.Count)
                {
                    SAParseTreeNode subtree = line.Symbols[j];
                    if (subtree.Symbol.Type == SAGrammarItemType.Identifier
                        && (Symbol.Value.Equals('*') || g.RemoveNodes.ContainsKey(Symbol.Value))
                        && g.RemoveNodes[Symbol.Value].Contains(subtree.Symbol.Value))
                    {
                        line.Symbols.RemoveAt(j);
                    }
                    else
                    {
                        line.Symbols[j].removeNodes(g);
                        j++;
                    }
                }
            }
        }

        public string combineTextFromSubnodes()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Lines.Count; i++)
            {
                SAParseNodeLine line = Lines[i];
                int j = 0;
                while (j < line.Symbols.Count)
                {
                    SAParseTreeNode subtree = line.Symbols[j];
                    if (subtree.Symbol.Type != SAGrammarItemType.Identifier)
                    {
                        sb.Append(line.Symbols[j].Value);
                    }
                    else
                    {
                        sb.Append(line.Symbols[j].combineTextFromSubnodes());
                    }
                    j++;
                }
            }
            //Lines.Clear();

            return sb.ToString();
        }

        public void jointextNodes(SAGrammar g)
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                SAParseNodeLine line = Lines[i];
                int j = 0;
                while (j < line.Symbols.Count)
                {
                    SAParseTreeNode subtree = line.Symbols[j];
                    if (subtree.Symbol.Type == SAGrammarItemType.Identifier
                        && (g.JointextNodes.ContainsKey(Symbol.Value)
                        && g.JointextNodes[Symbol.Value].Contains(subtree.Symbol.Value)) ||
                        (g.JointextNodes.ContainsKey("*")
                        && g.JointextNodes["*"].Contains(subtree.Symbol.Value)))
                    {
                        subtree.TextValue = subtree.combineTextFromSubnodes();
                    }
                    else
                    {
                        subtree.jointextNodes(g);
                    }
                    j++;
                }
            }
        }
        public void propagateNodes(List<string> pn)
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                SAParseNodeLine line = Lines[i];
                for (int j = 0; j < line.Symbols.Count; j++)
                {
                    SAParseTreeNode subtree = line.Symbols[j];
                    if (subtree.Symbol.Type == SAGrammarItemType.Identifier
                        && pn.IndexOf(subtree.Symbol.Value) >= 0)
                    {
                        int a = j + 1;
                        foreach (SAParseNodeLine subline in subtree.Lines)
                        {
                            foreach (SAParseTreeNode ptn in subline.Symbols)
                            {
                                line.Symbols.Insert(a, ptn);
                                a++;
                            }
                        }
                        subtree.Lines.Clear();
                    }
                    else
                    {
                        subtree.propagateNodes(pn);
                    }
                }
            }
        }


        public void updateNextPos()
        {
            int maxPos = -1;
            SAParseNodeLine maxNode = null;
            foreach (SAParseNodeLine ln2 in Lines)
            {
                if (maxPos < ln2.LastPosition)
                {
                    maxNode = ln2;
                    maxPos = ln2.LastPosition;
                }
            }

            if (maxNode != null)
            {
                Lines.Clear();
                Lines.Add(maxNode);
                NextPos = maxPos;
            }
        }
    }

    public class SAParseNodeLine
    {
        public List<SAParseTreeNode> Symbols = new List<SAParseTreeNode>();

        public int LastPosition = 0;

        public bool HasSuccess
        {
            get
            {
                foreach (SAParseTreeNode n in Symbols)
                {
                    if (!n.Valid)
                        return false;
                }
                return true;
            }
        }
    }
}
