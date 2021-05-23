using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Graphics g;
        private Pen pen;
        private Brush brush;
        private int x = -1;
        private int y = -1;
        private enum process {Nothing, FreePen, MultiPoint, MultiLineString, MultiPolygon};
        private process status;
        private bool drawing = false;
        List<Point> polygon = null;
        private int polygonCount = 0;

        public Form1()
        {
            InitializeComponent();
            g = panel1.CreateGraphics();
            g.SmoothingMode = SmoothingMode.HighQuality;
            pen = new Pen(Color.Red, 2);
            brush = new SolidBrush(Color.Blue);
            pen.StartCap = pen.EndCap = LineCap.Round;
            status = process.Nothing;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if(drawing)
            {
                if (e.Button == MouseButtons.Left)
                {
                    switch (status)
                    {
                        case process.MultiLineString:
                            g.DrawLine(pen, new Point(x, y), e.Location);
                            x = e.X;
                            y = e.Y;
                            break;
                        case process.MultiPolygon:
                            polygon.Add(e.Location);
                            if (++polygonCount >= 3)
                                g.FillPolygon(brush, polygon.ToArray());
                            break;
                        default: break;
                    }
                }
                else if(e.Button == MouseButtons.Right)
                {
                    switch (status)
                    {
                        case process.MultiLineString:
                            drawing = false;
                            break;
                        case process.MultiPolygon:
                            polygonCount = 0;
                            polygon = null;
                            drawing = false;
                            break;
                        default: break;
                    }
                }
            }
            else if(!drawing && e.Button == MouseButtons.Left)
            {
                drawing = true;
                switch (status)
                {
                    case process.MultiPoint:
                        g.DrawEllipse(pen, new Rectangle(e.X - 1, e.Y - 1, 2, 2));
                        break;
                    case process.FreePen:
                    case process.MultiLineString:
                        x = e.X;
                        y = e.Y;
                        break;
                    case process.MultiPolygon:
                        polygon = new List<Point>{e.Location};
                        polygonCount = 1;
                        break;
                    default: break;
                }
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawing && e.Button == MouseButtons.Left)
            {
                switch (status)
                {
                    case process.FreePen:
                        g.DrawLine(pen, new Point(x, y), e.Location);
                        x = e.X;
                        y = e.Y;
                        break;
                    default: break;
                }
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (drawing)
            {
                switch (status)
                {
                    case process.MultiLineString:
                    case process.MultiPolygon:
                        break;
                    default:
                        drawing = false;
                        break;
                }
            }
        }

        private void ListStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(ListStatus.SelectedIndex)
            {
                case 0: status = process.Nothing; break;
                case 1: status = process.FreePen; break;
                case 2: status = process.MultiPoint; break;
                case 3: status = process.MultiLineString; break;
                case 4: status = process.MultiPolygon; break;
                default: break;
            }
        }
    }
}
