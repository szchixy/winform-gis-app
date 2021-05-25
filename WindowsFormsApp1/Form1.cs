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
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Graphics graph;
        private Pen pen;
        private Brush brush;
        Color color = Color.Black;
        private int x = -1;
        private int y = -1;
        
        private enum process {Nothing, FreePen, MultiPoint, MultiLineString, MultiPolygon};
        private process status;
        private bool drawing = false;

        private List<Point> pointList = null;
        private int pointListCount = 0;
        private MultiPoint multiPoint = null;
        private MultiLineString multiLineString = null;
        private MultiPolygon multiPolygon = null;

        public Form1()
        {
            InitializeComponent();
            graph = panel1.CreateGraphics();
            graph.SmoothingMode = SmoothingMode.HighQuality;
            pen = new Pen(color, 2);
            brush = new SolidBrush(color);
            pen.StartCap = pen.EndCap = LineCap.Round;
            status = process.Nothing;
            listStatus.SelectedIndex = 0;
            boxColor.BackColor = color;
        }

        private void DrawPoints(Points points)
        {
            Pen pen1 = new Pen(points.color, 2);
            for (int i = 0; i < points.point.Count; i++)
                graph.DrawEllipse(pen1, new Rectangle(points.point[i].X - 1, points.point[i].Y - 1, 2, 2));
        }

        private void DrawLineString(Points points)
        {
            graph.DrawLines(new Pen(points.color, 2), points.point.ToArray());
        }

        private void DrawPolygon(Points points)
        {
            graph.FillPolygon(new SolidBrush(points.color), points.point.ToArray());
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if(drawing)
            {
                //
                // 左键过程中
                //
                if (e.Button == MouseButtons.Left)
                {
                    switch (status)
                    {
                        case process.MultiPoint:
                            graph.DrawEllipse(pen, new Rectangle(e.X - 1, e.Y - 1, 2, 2));
                            pointList.Add(e.Location);
                            break;
                        case process.MultiLineString:
                            graph.DrawLine(pen, new Point(x, y), e.Location);
                            pointList.Add(e.Location);
                            x = e.X;
                            y = e.Y;
                            break;
                        case process.MultiPolygon:
                            pointList.Add(e.Location);
                            ++pointListCount;
                            if (pointListCount == 2)
                                graph.DrawLine(new Pen(color, 2), new Point(x, y), e.Location);
                            else if (pointListCount >= 3)
                                graph.FillPolygon(brush, pointList.ToArray());
                            break;
                        default: break;
                    }
                }
                //
                // 右键结束
                //
                else if(e.Button == MouseButtons.Right)
                {
                    drawing = false;
                    listStatus.Enabled = true;
                    boxColor.Enabled = true;
                    switch (status)
                    {
                        case process.MultiPoint:
                            if (multiPoint == null)
                                multiPoint = new MultiPoint(new Points(pointList, color));
                            else
                                multiPoint.point.Add(new Points(pointList, color));
                            pointListCount = 0;
                            pointList = null;
                            break;
                        case process.MultiLineString:
                            if (multiLineString == null)
                                multiLineString = new MultiLineString(new Points(pointList, color));
                            else
                                multiLineString.lineString.Add(new Points(pointList, color));
                            pointListCount = 0;
                            pointList = null;
                            break;
                        case process.MultiPolygon:
                            if (multiPolygon == null)
                                multiPolygon = new MultiPolygon(new Points(pointList, color));
                            else
                                multiPolygon.polygon.Add(new Points(pointList, color));
                            pointListCount = 0;
                            pointList = null;
                            break;
                        default: break;
                    }
                }
            }
            else if(!drawing && e.Button == MouseButtons.Left && status != process.Nothing)
            {
                //
                // 左键开始
                //
                drawing = true;
                listStatus.Enabled = false;
                boxColor.Enabled = false;
                switch (status)
                {
                    case process.MultiPoint:
                        graph.DrawEllipse(pen, new Rectangle(e.X - 1, e.Y - 1, 2, 2));
                        pointList = new List<Point> { e.Location };
                        break;
                    case process.FreePen:
                        x = e.X;
                        y = e.Y;
                        break;
                    case process.MultiLineString:
                        pointList = new List<Point> { e.Location };
                        x = e.X;
                        y = e.Y;
                        break;
                    case process.MultiPolygon:
                        pointList = new List<Point> { e.Location };
                        pointListCount = 1;
                        x = e.X;
                        y = e.Y;
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
                        graph.DrawLine(pen, new Point(x, y), e.Location);
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
                    case process.MultiPoint:
                    case process.MultiLineString:
                    case process.MultiPolygon:
                        break;
                    default:
                        drawing = false;
                        listStatus.Enabled = true;
                        boxColor.Enabled = true;
                        break;
                }
            }
        }

        private void ListStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(listStatus.SelectedIndex)
            {
                case 0: status = process.Nothing; break;
                case 1: status = process.FreePen; break;
                case 2: status = process.MultiPoint; break;
                case 3: status = process.MultiLineString; break;
                case 4: status = process.MultiPolygon; break;
                default: break;
            }
            if(status != process.Nothing)
                panel1.Cursor = Cursors.Cross;
            else
                panel1.Cursor = Cursors.Default;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (multiPoint != null)
                for (int i = 0; i < multiPoint.point.Count; i++)
                    DrawPoints(multiPoint.point[i]);
            if (multiLineString != null)
                for (int i = 0; i < multiLineString.lineString.Count; i++)
                    DrawLineString(multiLineString.lineString[i]);
            if (multiPolygon != null)
                for (int i = 0; i < multiPolygon.polygon.Count; i++)
                    DrawPolygon(multiPolygon.polygon[i]);
        }

        private void boxColor_Click(object sender, EventArgs e)
        {
            DialogResult dr = colorDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                color = colorDialog1.Color;
                boxColor.BackColor = color;
                pen.Color = color;
                brush = new SolidBrush(color);
            }
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            //using(StreamReader sr = new StreamReader("input.json"))
            //{
            //    string multiPointJson = sr.ReadToEnd();
            //    textBox1.Text = multiPointJson;
            //}
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (multiPoint != null && multiLineString != null && multiPolygon != null)
            {
                dynamic featureCollection = new JObject();
                featureCollection.type = "FeatureCollection";
                featureCollection.features = new JArray();

                if (multiPoint != null)
                    for (int i = 0; i < multiPoint.point.Count; i++)
                        featureCollection.features.Add(MultiPoint2Json(multiPoint.point[i]));
                if (multiLineString != null)
                    for (int i = 0; i < multiLineString.lineString.Count; i++)
                        featureCollection.features.Add(MultiLineString2Json(multiLineString.lineString[i]));
                if (multiPolygon != null)
                    for (int i = 0; i < multiPolygon.polygon.Count; i++)
                        featureCollection.features.Add(MultiPolygon2Json(multiPolygon.polygon[i]));

                string resultJsonText = JsonConvert.SerializeObject(featureCollection);
                textBox1.Text = resultJsonText;

                using (StreamWriter sw = new StreamWriter("result.json"))
                {
                    sw.Write(resultJsonText);
                }
            }
        }

        private JObject MultiPoint2Json(Points points)
        {
            dynamic jo = new JObject();
            jo.type = "Feature";
            jo.properties = new JObject();
            jo.properties.color = new JArray();
            jo.properties.color.Add(points.color.R);
            jo.properties.color.Add(points.color.G);
            jo.properties.color.Add(points.color.B);
            jo.geometry = new JObject();
            jo.geometry.type = "MultiPoint";
            jo.geometry.coordinates = new JArray();
            for (int i = 0; i < points.point.Count; i++)
            {
                dynamic point = new JArray();
                point.Add(points.point[i].X);
                point.Add(points.point[i].Y);
                jo.geometry.coordinates.Add(point);
            }
            return jo;
        }

        private JObject MultiLineString2Json(Points points)
        {
            dynamic jo = new JObject();
            jo.type = "Feature";
            jo.properties = new JObject();
            jo.properties.color = new JArray();
            jo.properties.color.Add(points.color.R);
            jo.properties.color.Add(points.color.G);
            jo.properties.color.Add(points.color.B);
            jo.geometry = new JObject();
            jo.geometry.type = "MultiLineString";
            jo.geometry.coordinates = new JArray();
            for (int i = 0; i < points.point.Count; i++)
            {
                dynamic point = new JArray();
                point.Add(points.point[i].X);
                point.Add(points.point[i].Y);
                jo.geometry.coordinates.Add(point);
            }
            return jo;
        }

        private JObject MultiPolygon2Json(Points points)
        {
            dynamic jo = new JObject();
            jo.type = "Feature";
            jo.properties = new JObject();
            jo.properties.color = new JArray();
            jo.properties.color.Add(points.color.R);
            jo.properties.color.Add(points.color.G);
            jo.properties.color.Add(points.color.B);
            jo.geometry = new JObject();
            jo.geometry.type = "MultiPolygon";
            jo.geometry.coordinates = new JArray();
            for (int i = 0; i < points.point.Count; i++)
            {
                dynamic point = new JArray();
                point.Add(points.point[i].X);
                point.Add(points.point[i].Y);
                jo.geometry.coordinates.Add(point);
            }
            return jo;
        }

    }

    public class Points
    {
        public List<Point> point = null;
        public Color color;
        public Points(List<Point> point1, Color color1)
        {
            point = new List<Point>(point1);
            color = color1;
        }
    }

    public class MultiPoint
    {
        public List<Points> point = null;
        public MultiPoint(Points points1)
        {
            point = new List<Points> { points1 };
        }
    }

    public class MultiLineString
    {
        public List<Points> lineString = null;
        public MultiLineString(Points points1)
        {
            lineString = new List<Points> { points1 };
        }
    }

    public class MultiPolygon
    {
        public List<Points> polygon = null;
        public MultiPolygon(Points points1)
        {
            polygon = new List<Points> { points1 };
        }
    }
}
