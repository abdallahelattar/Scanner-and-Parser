using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Scanner.InitiateTokens();
            MessageBox.Show("Please consider that this project is a practical experiment to prove that we understood the concepts and that its not a project for public use, and also consider that we didnt use any existing library and that we wrote the code from scratch despite that the result is perfectly right just like the lecture but the syntax tree lines may get messy in rare situations only in a very complex code with that mentioned please follow the lines of the syntax tree if it got messy rather than considering it wrong", "For your info",MessageBoxButtons.OK, MessageBoxIcon.Information);
           
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //clear
            dataGridView1.Rows.Clear();
            Scanner.Result.Clear();
            Parser.Counter = 0;
            Parser.Current = new Scanner.Token();
            treeStructure1.ClearDraw();

            //scan
            Scanner.Result.Clear();
            dataGridView1.Rows.Clear();
            Scanner.Scan(richTextBox1.Text);
            for (int i = 0; i < Scanner.Result.Count; i++)
            {
                Scanner.Token T = Scanner.Result[i];
                this.dataGridView1.Rows.Add();
                this.dataGridView1.Rows[i].HeaderCell.Value = i.ToString();
                this.dataGridView1.Rows[i].Cells[0].Value = T.TokenValue;
                this.dataGridView1.Rows[i].Cells[1].Value = T.TokenType;

            }

            //parse
            if (Scanner.Result.Count == 0)
                return;
            else
            {
                Parser.Current = Scanner.Result.ElementAt(Parser.Counter);
                try
                {
                    Parser.Program();
                    Parser.DrawTree(treeStructure1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Syntax Error at Token Number " + Parser.Counter + " - Message : " + ex.Message);
                    //clear parser
                    //richTextBox1.Clear();
                    //dataGridView1.Rows.Clear();
                    Scanner.Result.Clear();
                    Parser.Counter = 0;
                    Parser.Current = new Scanner.Token();
                    treeStructure1.ClearDraw();
                }
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            dataGridView1.Rows.Clear();
            Scanner.Result.Clear();
            Parser.Counter = 0;
            Parser.Current = new Scanner.Token();
            treeStructure1.ClearDraw();
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

       
        protected override void WndProc(ref Message msg)
        {
            const int Restore = 0xF120;
            const int SystemCommand = 0x0112;
            const int Maximize = 0xF030;
            if (msg.Msg == SystemCommand && ((int)msg.WParam == Restore || (int)msg.WParam == Maximize))
            {
                treeStructure1.RefreshDraw();
            }

            base.WndProc(ref msg);
        }

        private void treeStructure1_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            treeStructure1.Refresh();
        }
    }
    
}
