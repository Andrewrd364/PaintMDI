using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaintMDI
{
    public partial class DocumentForm : Form
    {
        private int x, y;
        private bool isActive;
        public bool isOnceSaved = false;
        public bool isSaved;
        public Bitmap bitmap;
        public string savePath;
        private static int formCounter = 1;
        private static string name = "Unnamed";
        private Graphics g;
        private float xRatio;
        private float yRatio;
        public double zoom = 1;
        private Size canvasSize;
        private Bitmap bmpTemp;

        public DocumentForm()
        {
            InitializeComponent();
            Text = name + " " + formCounter.ToString();
            bitmap = new Bitmap(1000, 1000);
            ResizePictureBox();
            pictureBox1.Image = bitmap;
            formCounter++;
            g = Graphics.FromImage(bitmap);
            GraphicsSettings(ref g);
            g.Clear(Color.White);
            xRatio = (float)pictureBox1.Width / bitmap.Width;
            yRatio = (float)pictureBox1.Height / bitmap.Height;
            canvasSize = new Size(pictureBox1.Width, pictureBox1.Height);
        }
        public DocumentForm(Bitmap bmp, string name)
        {
            InitializeComponent();
            Text = name;
            formCounter++;
            bitmap = bmp;
            ResizePictureBox();
            pictureBox1.Image = bitmap;
            g = Graphics.FromImage(bitmap);
            GraphicsSettings(ref g);
            xRatio = (float)pictureBox1.Width / bitmap.Width;
            yRatio = (float)pictureBox1.Height / bitmap.Height;
            canvasSize = new Size(pictureBox1.Width, pictureBox1.Height);
        }
        private void GraphicsSettings(ref Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
        }
        public event EventHandler<UserEventArgs> sendXY;
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            x = (int)Math.Round(e.X/xRatio);
            y = (int)Math.Round(e.Y/yRatio);
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int actualX = (int)Math.Round(e.X / xRatio);
            int actualY = (int)Math.Round(e.Y / yRatio);
            if (sendXY != null)
                sendXY(this, new UserEventArgs(actualX, actualY, true));
            if (e.Button == MouseButtons.Left)
            {
                switch (MainForm.CurrentTool)
                {
                    case Tools.Pen:
                        g.DrawLine(MainForm.CurrentPen, x, y, actualX, actualY);
                        x = actualX;
                        y = actualY;
                        break;
                    case Tools.Circle:
                        bmpTemp = (Bitmap)bitmap.Clone();
                        g = Graphics.FromImage(bmpTemp);
                        g.DrawEllipse(MainForm.CurrentPen, new Rectangle(x, y, actualX - x, actualY - y));
                        pictureBox1.Image = bmpTemp;
                        break;
                    case Tools.Line:
                        bmpTemp = (Bitmap)bitmap.Clone();
                        g = Graphics.FromImage(bmpTemp);
                        g.DrawLine(MainForm.CurrentPen, x, y, actualX, actualY);
                        pictureBox1.Image = bmpTemp;
                        break;
                    case Tools.Star:
                        bmpTemp = (Bitmap)bitmap.Clone();
                        g = Graphics.FromImage(bmpTemp);
                        DrawStar(g, MainForm.CurrentPen, MainForm.NumPoints, MainForm.Skip, new Rectangle(x, y, actualX - x, actualY - y));
                        pictureBox1.Image = bmpTemp;
                        break;
                }
                pictureBox1.Refresh();
                isSaved = false;
            }
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            if (sendXY != null)
                sendXY(this, new UserEventArgs(0, 0, false));
        }

        private void DocumentForm_Activated(object sender, EventArgs e)
        {
            isActive = true;
        }

        private void DocumentForm_Deactivate(object sender, EventArgs e)
        {
            isActive = false;
        }

        private void DocumentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isSaved)
            {
                DocumentClosingForm clsForm = new DocumentClosingForm();
                switch (clsForm.ShowDialog())
                {
                    case DialogResult.OK:
                        var parent = MdiParent as MainForm;
                        parent.SaveFile();
                        if(!isSaved)
                            e.Cancel = true;
                        this.Dispose();
                        g.Dispose();
                        GC.Collect();
                        break;
                    case DialogResult.No:
                        e.Cancel = false;
                        this.Dispose();
                        g.Dispose();
                        GC.Collect();
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }
        private void ChangeResolutionRatio()
        {
            xRatio = (float)pictureBox1.Width / bitmap.Width;
            yRatio = (float)pictureBox1.Height / bitmap.Height;
        }
        private void ResizePictureBox()
        {
            float resolution = (float)Math.Min(bitmap.Width, bitmap.Height) / Math.Max(bitmap.Width, bitmap.Height);

            if (bitmap.Width < bitmap.Height)
                pictureBox1.Width = (int)Math.Round(pictureBox1.Width * resolution);
            if (bitmap.Width > bitmap.Height)
                pictureBox1.Height = (int)Math.Round(pictureBox1.Height * resolution);

            ChangeResolutionRatio();

            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
            canvasSize.Width = pictureBox1.Width; canvasSize.Height = pictureBox1.Height;
        }

        public void ChangeCanvasSize(object sender, UserEventArgs e)
        {
            if(isActive)
            {
                Size newSize = new Size(e.x, e.y);
                var bitmapTemp = new Bitmap(e.x, e.y);
                g = Graphics.FromImage(bitmapTemp);
                GraphicsSettings(ref g);
                g.DrawImage(bitmap, 0, 0);
                
                bitmap = bitmapTemp;
                pictureBox1.Size = new Size(500, 500);
                ResizePictureBox();
                
                pictureBox1.Image = bitmap;
                pictureBox1.Refresh();
                isSaved = false;


                ChangeResolutionRatio();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            switch (MainForm.CurrentTool)
            {
                case Tools.Circle:
                    bitmap = bmpTemp;
                    break;
                case Tools.Line:
                    bitmap = bmpTemp;
                    break;
                case Tools.Star:
                    bitmap = bmpTemp;
                    break;
            }
            if (!(Text[0] is '*') && e.Button == MouseButtons.Left)
                Text = "*" + Text;
        }

        public void ZoomImage(double ratio)
        {
            zoom += ratio;

            pictureBox1.Width = (int)Math.Round(zoom * canvasSize.Width);
            pictureBox1.Height = (int)Math.Round(zoom * canvasSize.Height);
            ChangeResolutionRatio();
            pictureBox1.Invalidate();
        }
        private void DrawStar(Graphics gr, Pen the_pen, int num_points, int skip, Rectangle rect)
        {
            PointF[] star_points = MakeStarPoints(-Math.PI / 2, num_points, skip, rect);

            gr.DrawPolygon(the_pen, star_points);
        }
        private PointF[] MakeStarPoints(double start_theta, int num_points, int skip, Rectangle rect)
        {
            double theta, dtheta;
            PointF[] result;
            float rx = rect.Width / 2f;
            float ry = rect.Height / 2f;
            float cx = rect.X + rx;
            float cy = rect.Y + ry;

            if (skip == 1)
            {
                result = new PointF[num_points];
                theta = start_theta;
                dtheta = 2 * Math.PI / num_points;
                for (int i = 0; i < num_points; i++)
                {
                    result[i] = new PointF(
                        (float)(cx + rx * Math.Cos(theta)),
                        (float)(cy + ry * Math.Sin(theta)));
                    theta += dtheta;
                }
                return result;
            }

            double concave_radius =
                CalculateConcaveRadius(num_points, skip);

            result = new PointF[2 * num_points];
            theta = start_theta;
            dtheta = Math.PI / num_points;
            for (int i = 0; i < num_points; i++)
            {
                result[2 * i] = new PointF(
                    (float)(cx + rx * Math.Cos(theta)),
                    (float)(cy + ry * Math.Sin(theta)));
                theta += dtheta;
                result[2 * i + 1] = new PointF(
                    (float)(cx + rx * Math.Cos(theta) * concave_radius),
                    (float)(cy + ry * Math.Sin(theta) * concave_radius));
                theta += dtheta;
            }
            return result;
        }
        private double CalculateConcaveRadius(int num_points, int skip)
        {
            if (num_points < 5) return 0.33f;

            double dtheta = 2 * Math.PI / num_points;
            double theta00 = -Math.PI / 2;
            double theta01 = theta00 + dtheta * skip;
            double theta10 = theta00 + dtheta;
            double theta11 = theta10 - dtheta * skip;

            PointF pt00 = new PointF(
                (float)Math.Cos(theta00),
                (float)Math.Sin(theta00));
            PointF pt01 = new PointF(
                (float)Math.Cos(theta01),
                (float)Math.Sin(theta01));
            PointF pt10 = new PointF(
                (float)Math.Cos(theta10),
                (float)Math.Sin(theta10));
            PointF pt11 = new PointF(
                (float)Math.Cos(theta11),
                (float)Math.Sin(theta11));

            bool lines_intersect, segments_intersect;
            PointF intersection, close_p1, close_p2;
            FindIntersection(pt00, pt01, pt10, pt11,
                out lines_intersect, out segments_intersect,
                out intersection, out close_p1, out close_p2);

            return Math.Sqrt(
                intersection.X * intersection.X +
                intersection.Y * intersection.Y);
        }
        private void FindIntersection(
    PointF p1, PointF p2, PointF p3, PointF p4,
    out bool lines_intersect, out bool segments_intersect,
    out PointF intersection,
    out PointF close_p1, out PointF close_p2)
        {
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                    / denominator;
            if (float.IsInfinity(t1))
            {
                lines_intersect = false;
                segments_intersect = false;
                intersection = new PointF(float.NaN, float.NaN);
                close_p1 = new PointF(float.NaN, float.NaN);
                close_p2 = new PointF(float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            float t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                    / -denominator;

            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }
    }
}
