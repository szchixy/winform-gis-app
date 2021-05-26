using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        Color paintColor = Color.Black;
        int penSize;
        private int x = -1;
        private int y = -1;
        
        private enum process {Nothing, FreePen, MultiPoint, LineString, Polygon};
        private process status;
        private bool drawing;

        private List<Point> pointList = null;
        private int pointListCount = 0;
        private FeatureCollection featureCollection = null;

        private string inputJsonText = null;
        private string outputJsonText = null;

        public Form1()
        {
            InitializeComponent();
            graph = panel1.CreateGraphics();
            graph.SmoothingMode = SmoothingMode.HighQuality;
            penSize = 2;
            pen = new Pen(paintColor, penSize);
            brush = new SolidBrush(paintColor);
            pen.StartCap = pen.EndCap = LineCap.Round;
            status = process.Nothing;
            drawing = false;
            listStatus.SelectedIndex = 0;
            boxColor.BackColor = paintColor;

            featureCollection = new FeatureCollection();
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
                        case process.LineString:
                            graph.DrawLine(pen, new Point(x, y), e.Location);
                            pointList.Add(e.Location);
                            x = e.X;
                            y = e.Y;
                            break;
                        case process.Polygon:
                            pointList.Add(e.Location);
                            ++pointListCount;
                            if (pointListCount == 2)
                                graph.DrawLine(new Pen(paintColor, penSize), new Point(x, y), e.Location);
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

                    // 存储对象
                    if(status != process.FreePen)
                        featureCollection.features.Add(Geometry2JObject(new Points(pointList, paintColor), status.ToString()));

                    switch (status)
                    {
                        case process.MultiPoint:
                            pointListCount = 0;
                            pointList = null;
                            break;
                        case process.LineString:                           
                            pointListCount = 0;
                            pointList = null;
                            break;
                        case process.Polygon:
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
                    case process.LineString:
                        pointList = new List<Point> { e.Location };
                        x = e.X;
                        y = e.Y;
                        break;
                    case process.Polygon:
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
                    case process.LineString:
                    case process.Polygon:
                        break;
                    default:
                        drawing = false;
                        listStatus.Enabled = true;
                        boxColor.Enabled = true;
                        break;
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            DrawFeatureCollection();
        }

        private void DrawFeatureCollection()
        {
            if (featureCollection.features.Count != 0)
            {
                foreach (dynamic feature in featureCollection.features)
                {
                    dynamic colorArray = feature.properties.color;
                    List<int> colorList = new List<int>();
                    foreach (int i in colorArray)
                        colorList.Add(i);
                    Color color = Color.FromArgb(colorList[0], colorList[1], colorList[2]);

                    dynamic coordinatesArray = feature.geometry.coordinates;
                    List<Point> pointList = new List<Point>();
                    foreach (dynamic pointArray in coordinatesArray)
                    {
                        List<int> xy = new List<int>();
                        foreach (int i in pointArray)
                            xy.Add(i);
                        pointList.Add(new Point(xy[0], xy[1]));
                    }

                    DrawFeature(new Points(pointList, color), feature.geometry.type.ToString());
                }
            }
        }

        private void DrawFeature(Points points, string geoType)
        {
            if (geoType == "MultiPoint")
            {
                Pen pen1 = new Pen(points.color, penSize);
                for (int i = 0; i < points.point.Count; i++)
                    graph.DrawEllipse(pen1, new Rectangle(points.point[i].X - 1, points.point[i].Y - 1, 2, 2));
            }
            else if (geoType == "LineString")
                graph.DrawLines(new Pen(points.color, penSize), points.point.ToArray());
            else if (geoType == "Polygon")
                graph.FillPolygon(new SolidBrush(points.color), points.point.ToArray());
        }

        private void ListStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (listStatus.SelectedIndex)
            {
                case 0: status = process.Nothing; break;
                case 1: status = process.FreePen; break;
                case 2: status = process.MultiPoint; break;
                case 3: status = process.LineString; break;
                case 4: status = process.Polygon; break;
                default: break;
            }
            if (status != process.Nothing)
                panel1.Cursor = Cursors.Cross;
            else
                panel1.Cursor = Cursors.Default;
        }

        private void boxColor_Click(object sender, EventArgs e)
        {
            DialogResult dr = colorDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                paintColor = colorDialog1.Color;
                boxColor.BackColor = paintColor;
                pen.Color = paintColor;
                brush = new SolidBrush(paintColor);
            }
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                {
                    inputJsonText = sr.ReadToEnd();
                    featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(inputJsonText);
                    DrawFeatureCollection();
                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (featureCollection.features.Count != 0)
            {
                outputJsonText = JsonConvert.SerializeObject(featureCollection);

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                    {
                        sw.Write(outputJsonText);
                    }
                }
            }
        }

        private JObject Geometry2JObject(Points points, string geoType)
        {
            dynamic jo = new JObject();
            jo.type = "Feature";
            jo.properties = new JObject();
            jo.properties.color = new JArray();
            jo.properties.color.Add(points.color.R);
            jo.properties.color.Add(points.color.G);
            jo.properties.color.Add(points.color.B);
            jo.geometry = new JObject();
            jo.geometry.type = geoType;
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

    public class FeatureCollection
    {
        public string type = "";
        public JArray features = null;
        public FeatureCollection()
        {
            type = "FeatureCollection";
            features = new JArray();
        }
    }
}
