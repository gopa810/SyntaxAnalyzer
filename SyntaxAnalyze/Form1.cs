using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SyntaxAnalyze
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SAEngine engine = new SAEngine();

            SARequest request = new SARequest();

            request.GrammarText = richTextBox1.Text;
            request.InputFileText = richTextBox2.Text;

            if (engine.Process(request))
            {
            }

            treeView2.Nodes.Clear();
            ShowOutputTree(treeView2.Nodes, request.OutputTree);
            treeView2.ExpandAll();
        }

        public void ShowOutputTree(TreeNodeCollection nodes, SAParseTreeNode tn)
        {
            if (!tn.Valid)
                return;
            TreeNode nn = nodes.Add(tn.Value + "(" + tn.Symbol.Value + ")");
            foreach (SAParseNodeLine nl in tn.Lines)
            {
                //TreeNode lineNode = nn.Nodes.Add("line (lastpos=" + nl.LastPosition + ")");
                TreeNode lineNode = nn;
                foreach (SAParseTreeNode item in nl.Symbols)
                {
                    ShowOutputTree(lineNode.Nodes, item);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SAEngine engine = new SAEngine();

            SARequest request = new SARequest();

            request.GrammarText = richTextBox1.Text;

            request.Grammar = engine.ConvertTextToGrammar(richTextBox1.Text);

            foreach(string symb in request.Grammar.Symbols.Keys)
            {
                SASymbolDefinition def = request.Grammar.Symbols[symb];
                TreeNode tn = treeView1.Nodes.Add(symb);
                int lineCount = 1;
                foreach (List<SAGrammarSymbol> lsym in def.Lines)
                {
                    TreeNode ltn = tn.Nodes.Add(string.Format("Line {0}", lineCount));
                    lineCount++;
                    foreach (SAGrammarSymbol ss in lsym)
                    {
                        TreeNode stn = ltn.Nodes.Add(ss.Value + "(" + ss.MinOccurences + "," + ss.MaxOccurences + ")");
                    }
                }
            }
            treeView1.ExpandAll();
        }
    }
}
