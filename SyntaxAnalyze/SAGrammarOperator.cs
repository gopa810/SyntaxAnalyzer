using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntaxAnalyze
{
    public class SAGrammarOperator: SAGrammarItem
    {
        public SAOperatorType Type;
        public List<SAGrammarItem> Items = new List<SAGrammarItem>();
    }
}
