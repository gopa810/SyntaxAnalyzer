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

            richTextBox3.Text = "";
            if (engine.Process(request))
            {
            }

            treeView2.Nodes.Clear();
            ShowOutputTree(treeView2.Nodes, request.OutputTree);
            treeView2.ExpandAll();

            if (engine.parsedLength < request.InputFileText.Length)
            {
                richTextBox3.AppendText("Parsing has stopped at position " + engine.parsedLength + "\n");
                richTextBox3.AppendText("-------------------------\n");
                richTextBox3.AppendText(request.InputFileText.Substring(engine.parsedLength));
            }
        }

        public void ShowOutputTree(TreeNodeCollection nodes, SAParseTreeNode tn)
        {
            if (!tn.Valid)
                return;
            string nodeName;
            if (tn.Symbol.Type == SAGrammarItemType.Identifier)
            {

                nodeName = tn.Symbol.Value;
                if (tn.TextValue != null && tn.TextValue.Length > 0)
                {
                    nodeName += " => " + tn.TextValue;
                }
            }
            else
            {
                nodeName = tn.Value;
            }

            TreeNode nn = nodes.Add(nodeName);
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

            treeView1.Nodes.Clear();

            request.GrammarText = richTextBox1.Text;

            request.Grammar = engine.ConvertTextToGrammar(richTextBox1.Text);

            request.Grammar.checkConsistency();

            foreach(string symb in request.Grammar.Symbols.Keys)
            {
                SASymbolDefinition def = request.Grammar.Symbols[symb];
                TreeNode tn = treeView1.Nodes.Add(symb);
                tn.ImageIndex = tn.SelectedImageIndex = 1;
                int lineCount = 1;
                foreach (List<SAGrammarSymbol> lsym in def.Lines)
                {
                    TreeNode ltn = tn.Nodes.Add(string.Format("Line {0}", lineCount));
                    ltn.SelectedImageIndex = ltn.ImageIndex = 1;
                    lineCount++;
                    foreach (SAGrammarSymbol ss in lsym)
                    {
                        TreeNode stn = ltn.Nodes.Add(ss.Value + "(" + ss.MinOccurences + "," + ss.MaxOccurences + ")");
                        stn.SelectedImageIndex = stn.ImageIndex = (ss.Type == SAGrammarItemType.Identifier ? 1 : 2);
                    }
                }
            }

            if (request.Grammar.undefinedIdentifiers.Count > 0)
            {
                TreeNode tn2 = treeView1.Nodes.Add("Undefined");
                tn2.ImageIndex = tn2.SelectedImageIndex = 0;
                foreach (string s in request.Grammar.undefinedIdentifiers)
                {
                    TreeNode tn3 = tn2.Nodes.Add(s);
                    tn3.ImageIndex = tn3.SelectedImageIndex = 0;
                }
            }
            treeView1.ExpandAll();
        }
    }
}
