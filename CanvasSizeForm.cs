using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaintMDI
{
    public partial class CanvasSizeForm : Form
    {
        public event EventHandler<UserEventArgs> sendCanvasSize;
        public CanvasSizeForm()
        {
            InitializeComponent();
        }

        public int X { get; set; }
        public int Y { get; set; }
        private void button1_Click(object sender, EventArgs e)
        {
            int x, y;
            bool ok = int.TryParse(textBox1.Text, out y);
            if(ok && y >= 0)
            {
                Y = y;
            }
            ok = int.TryParse(textBox2.Text, out x);
            if (ok && x >= 0)
            {
                X = x;
            }

            if(sendCanvasSize != null && X!=0 && Y!=0) 
            {
                sendCanvasSize(this, new UserEventArgs(X, Y));
            }
        }
    }
}
