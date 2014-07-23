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

        public List<SAParseNodeLine> Lines = new List<SAParseNodeLine>();
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
