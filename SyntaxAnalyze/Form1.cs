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
        }
    }
}
