using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aStar
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PathFinding.AStar aStar = new PathFinding.AStar(13, 14, 154, 
                new int[] { 
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 
1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 
1, 0, 1, 1, 1, 0, 1, 1, 0, 0, 0, 0, 1, 
1, 0, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 
1, 0, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 
1, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 1, 
1, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 1, 
1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 
1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });

            aStar.FindPathFunction();
            string ClosedMatrix = aStar.PrintClosedMatrix();
            string TraceBack = aStar.TraceBack();
            textBox1.Text = ClosedMatrix;
            textBox2.Text = TraceBack;


        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
