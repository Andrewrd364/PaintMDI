using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace PaintMDI
{
    public partial class MainForm : Form
    {
        public static Color Color { get; set; }
        public static new float Width { get; set; }
        public static Pen CurrentPen { get; set; }
        public static Tools CurrentTool { get; set; }
        public static int NumPoints { get; set; }
        public static int Skip { get; set; }

        public Point CanvasSize
        {
            get
            {
                return CanvasSize;
            }
            set
            {
                if (SendCanvasSize != null)
                    SendCanvasSize(this, new UserEventArgs(value.X, value.Y));
            }
        }
        public event EventHandler<UserEventArgs> SendCanvasSize;
        public MainForm()
        {
            InitializeComponent();
            Color = Color.Black;
            Width = 4f;
            NumPoints = 5;
            Skip = 2;
            textBox1.Text = NumPoints.ToString();
            textBox2.Text = Skip.ToString();
            CurrentPen = new Pen(Color, Width);
            CurrentPen.StartCap = LineCap.Round;
            CurrentPen.EndCap = LineCap.Round;
            CurrentTool = Tools.Pen;
            toolStripTextBox1.Text = Width.ToString();
            var controls = FindForm().Controls;
            foreach (Control control in controls)
            {
                if (control is MdiClient)
                {
                    control.BackColor = Color.FromArgb(210, 210, 210);
                }
            }
        }
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var abtForm = new AboutForm();
            abtForm.ShowDialog();
        }
        private void новыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new DocumentForm();
            form.MdiParent = this;
            form.sendXY += new EventHandler<UserEventArgs>(ShowXY);
            this.SendCanvasSize += new EventHandler<UserEventArgs>(form.ChangeCanvasSize);
            form.Show();
        }
        private void размерХолстаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(ActiveMdiChild != null)
            {
                var sizeForm = new CanvasSizeForm();
                sizeForm.sendCanvasSize += new EventHandler<UserEventArgs>(GetCanvasSize);
                sizeForm.ShowDialog();
            }
        }
        private void GetCanvasSize(object sender, UserEventArgs e)
        {
            CanvasSize = new Point(e.x, e.y);
        }
        private void ShowXY(object sender, UserEventArgs e)
        {
            if (e.isVisible)
            {
                toolStripStatusLabel2.Text = e.x.ToString();
                toolStripStatusLabel4.Text = e.y.ToString();
            }
            else
            {
                toolStripStatusLabel2.Text = "";
                toolStripStatusLabel4.Text = "";
            }
        }
        private void красныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButton1.CheckState = CheckState.Unchecked;
            CurrentPen.Width = Width;
            toolStripTextBox1.Text = Width.ToString();
            CurrentPen.Color = Color.Red;
            Color = Color.Red;
        }
        private void зеленыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButton1.CheckState = CheckState.Unchecked;
            CurrentPen.Width = Width;
            toolStripTextBox1.Text = Width.ToString();
            CurrentPen.Color = Color.Green;
            Color = Color.Green;
        }
        private void синийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButton1.CheckState = CheckState.Unchecked;
            CurrentPen.Width = Width;
            toolStripTextBox1.Text = Width.ToString();
            CurrentPen.Color = Color.Blue;
            Color = Color.Blue;
        }
        private void другойToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if(cd.ShowDialog() == DialogResult.OK) 
            {
                toolStripButton1.CheckState = CheckState.Unchecked;
                CurrentPen.Width = Width;
                toolStripTextBox1.Text = Width.ToString();
                CurrentPen.Color = cd.Color;
                Color = cd.Color;
            }
        }
        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            float w;
            bool ok = float.TryParse(toolStripTextBox1.Text, out w);
            if (!ok || w<=0)
                toolStripTextBox1.ForeColor = Color.Red;
            else
            {
                toolStripTextBox1.ForeColor = Color.Black;
                CurrentPen.Width = w;
            }
        }
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }
        public void SaveFile()
        {
            if (ActiveMdiChild != null)
            {
                var doc = ActiveMdiChild as DocumentForm;
                if(doc.isOnceSaved)
                {
                    doc.bitmap.Save(doc.savePath);
                    doc.isSaved = true;
                    doc.Text = doc.savePath;
                }
                else
                {
                    Save(doc);
                }
            }
            else
                MessageBox.Show("Ни одного файла не открыто");
        }
        public static void Save(DocumentForm doc)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.Filter = "Windows Bitmap (*.bmp)|*.bmp| Файлы JPEG (*.jpg)|*.jpg";
            ImageFormat[] ff = { ImageFormat.Bmp, ImageFormat.Jpeg };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                doc.bitmap.Save(dlg.FileName, ff[dlg.FilterIndex - 1]);
                doc.savePath = dlg.FileName;
                doc.isOnceSaved = true;
                doc.Text = dlg.FileName;
                doc.isSaved = true;
            }
        }
        private void SaveFileAs()
        {
            if (ActiveMdiChild != null)
            {
                var doc = ActiveMdiChild as DocumentForm;
                Save(doc);
            }
            else
                MessageBox.Show("Ни одного файла не открыто");
        }
        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileAs();
        }
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Windows Bitmap (*.bmp)|*.bmp| Файлы JPEG (*.jpeg, *.jpg)|*.jpeg;*.jpg";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var image = Image.FromStream(dlg.OpenFile());

                var form = new DocumentForm(new Bitmap(image), dlg.FileName);
                form.MdiParent = this;
                form.sendXY += new EventHandler<UserEventArgs>(ShowXY);
                this.SendCanvasSize += new EventHandler<UserEventArgs>(form.ChangeCanvasSize);
                form.Show();
            }
        }
        private void каскадомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }
        private void слеваНаправоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }
        private void сверхуВнизToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }
        private void упорядочитьЗначкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveFile();
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (toolStripButton1.Checked)
            {
                HidePanel();
                toolStripButton7.CheckState = CheckState.Unchecked;
                toolStripButton5.CheckState = CheckState.Unchecked;
                toolStripButton6.CheckState = CheckState.Unchecked;
                toolStripButton4.CheckState = CheckState.Unchecked;
                CurrentTool = Tools.Pen;
                CurrentPen.Color = Color.White;
                CurrentPen.Width = 15f;
                toolStripTextBox1.Text = CurrentPen.Width.ToString();
            }
            else
            {
                toolStripButton7.CheckState = CheckState.Checked;
                CurrentPen.Color = Color;
                CurrentPen.Width = Width;
                toolStripTextBox1.Text = CurrentPen.Width.ToString();
            } 
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
            {
                var doc = ActiveMdiChild as DocumentForm;
                doc.ZoomImage(0.1);
                toolStripLabel2.Text = (doc.zoom * 100).ToString() + "%";
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
            {
                var doc = ActiveMdiChild as DocumentForm;
                doc.ZoomImage(-0.1);
                toolStripLabel2.Text = (doc.zoom * 100).ToString() + "%";
            }
        }

        private void MainForm_MdiChildActivate(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
            {
                var doc = ActiveMdiChild as DocumentForm;
                toolStripLabel2.Text = (doc.zoom * 100).ToString() + "%";
            }
                
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (toolStripButton4.Checked)
            {
                HidePanel();
                CurrentTool = Tools.Line;
                CurrentPen.Color = Color;
                CurrentPen.Width = Width;
                toolStripButton1.CheckState = CheckState.Unchecked;
                toolStripButton5.CheckState = CheckState.Unchecked;
                toolStripButton6.CheckState = CheckState.Unchecked;
                toolStripButton7.CheckState = CheckState.Unchecked;
            }
            else
            {
                CurrentTool = Tools.Pen;
                toolStripButton7.CheckState = CheckState.Checked;
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (toolStripButton5.Checked)
            {
                HidePanel();
                CurrentPen.Color = Color;
                CurrentPen.Width = Width;
                CurrentTool = Tools.Circle;
                toolStripButton1.CheckState = CheckState.Unchecked;
                toolStripButton4.CheckState = CheckState.Unchecked;
                toolStripButton6.CheckState = CheckState.Unchecked;
                toolStripButton7.CheckState = CheckState.Unchecked;
            }
            else
            {
                CurrentTool = Tools.Pen;
                toolStripButton7.CheckState = CheckState.Checked;
            }
        }
        private void ShowPanel()
        {
            panel1.Enabled = true;
            panel1.Visible = true;
        }
        private void HidePanel()
        {
            panel1.Enabled = false;
            panel1.Visible = false;
        }
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (toolStripButton6.Checked)
            {
                ShowPanel();
                CurrentTool = Tools.Star;
                CurrentPen.Color = Color;
                CurrentPen.Width = Width;
                toolStripButton1.CheckState = CheckState.Unchecked;
                toolStripButton5.CheckState = CheckState.Unchecked;
                toolStripButton4.CheckState = CheckState.Unchecked;
                toolStripButton7.CheckState = CheckState.Unchecked;
            }
            else
            {
                HidePanel();
                CurrentTool = Tools.Pen;
                toolStripButton7.CheckState = CheckState.Checked;
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (toolStripButton7.Checked)
            {
                HidePanel();
                CurrentTool = Tools.Pen;
                toolStripButton1.CheckState = CheckState.Unchecked;
                toolStripButton5.CheckState = CheckState.Unchecked;
                toolStripButton6.CheckState = CheckState.Unchecked;
                toolStripButton4.CheckState = CheckState.Unchecked;
                CurrentPen.Color = Color;
                CurrentPen.Width = Width;
                toolStripTextBox1.Text = CurrentPen.Width.ToString();
            }
            else
            {
                CurrentTool = Tools.Pen;
                toolStripButton7.CheckState = CheckState.Checked;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            bool ok = int.TryParse(textBox1.Text, out int temp);
            if (!ok || temp <= 0)
                textBox1.ForeColor = Color.Red;
            else
            {
                textBox1.ForeColor = Color.Black;
                NumPoints = temp;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            bool ok = int.TryParse(textBox2.Text, out int temp);
            if (!ok || temp < 0)
                textBox2.ForeColor = Color.Red;
            else
            {
                textBox2.ForeColor = Color.Black;
                Skip = temp;
            }
        }
    }
}
