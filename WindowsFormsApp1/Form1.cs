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
        Color paintColor;
        int penSize;
        private Pen pen;
        private Brush brush;
        private int x = -1;
        private int y = -1;
        
        private enum process {Nothing, FreePen, MultiPoint, LineString, Polygon};
        private process status;
        private bool drawing;

        private List<Point> pointList = null;
        private int pointListCount = 0;

        public List<Geometry> geometryList = null;
        private FeatureCollection featureCollection = null;

        private string inputJsonText = "";
        private string outputJsonText = "";

        public Form1()
        {
            InitializeComponent();
            graph = panel1.CreateGraphics();
            graph.SmoothingMode = SmoothingMode.HighQuality;
            paintColor = Color.Black;
            penSize = 2;
            pen = new Pen(paintColor, penSize);
            pen.StartCap = pen.EndCap = LineCap.Round;
            brush = new SolidBrush(paintColor);

            listStatus.SelectedIndex = 0;
            boxColor.BackColor = paintColor;
            BarPenSize.Value = penSize;

            status = process.Nothing;
            drawing = false;

            geometryList = new List<Geometry>();
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
                            graph.FillEllipse(brush, new Rectangle(e.X - penSize, e.Y - penSize, penSize * 2 - 1, penSize * 2 - 1));
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
                                graph.DrawLine(new Pen(paintColor, 2), new Point(x, y), e.Location);
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
                    ChangeEnable(true);

                    // 存储对象
                    if (status != process.FreePen)
                        geometryList.Add(new Geometry(pointList, paintColor, penSize, status.ToString()));

                    switch (status)
                    {
                        case process.MultiPoint:
                        case process.LineString:                           
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
                ChangeEnable(false);
                switch (status)
                {
                    case process.MultiPoint:
                        graph.FillEllipse(brush, new Rectangle(e.X - penSize, e.Y - penSize, penSize * 2 - 1, penSize * 2 - 1));
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
            if (drawing)
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
                        ChangeEnable(true);
                        break;
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            DrawGeometryList(geometryList);
        }

        private void DrawGeometryList(List<Geometry> geometryList1)
        {
            if(geometryList.Count != 0)
                for (int i = 0; i < geometryList1.Count; i++)
                    DrawGeometry(geometryList1[i]);
        }

        private void DrawGeometry(Geometry geometry)
        {
            if (geometry.geoType == "MultiPoint")
                for (int i = 0; i < geometry.point.Count; i++)
                    graph.FillEllipse(new SolidBrush(geometry.color), new Rectangle(geometry.point[i].X - geometry.size, geometry.point[i].Y - geometry.size, geometry.size * 2 - 1, geometry.size * 2 - 1));
            else if (geometry.geoType == "LineString")
                graph.DrawLines(new Pen(geometry.color, geometry.size), geometry.point.ToArray());
            else if (geometry.geoType == "Polygon")
                graph.FillPolygon(new SolidBrush(geometry.color), geometry.point.ToArray());
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

        private void BarPenSize_ValueChanged(object sender, EventArgs e)
        {
            penSize = BarPenSize.Value;
            pen.Width = penSize;
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
                    //
                    // 解析json
                    //
                    if (featureCollection.features.Count != 0)
                    {
                        geometryList = new List<Geometry>();

                        foreach (dynamic feature in featureCollection.features)
                        {
                            dynamic colorArray = feature.properties.paintColor;
                            List<int> colorList = new List<int>();
                            foreach (int i in colorArray)
                                colorList.Add(i);
                            Color color = Color.FromArgb(colorList[0], colorList[1], colorList[2]);

                            int size = 0;
                            if (feature.geometry.type.ToString() != "Polygon")
                                size = feature.properties.penSize;

                            dynamic coordinatesArray = feature.geometry.coordinates;
                            List<Point> pointList = new List<Point>();
                            foreach (dynamic pointArray in coordinatesArray)
                            {
                                List<int> xy = new List<int>();
                                foreach (int i in pointArray)
                                    xy.Add(i);
                                pointList.Add(new Point(xy[0], xy[1]));
                            }

                            geometryList.Add(new Geometry(pointList, color, size, feature.geometry.type.ToString()));
                        }

                        graph.Clear(Color.White);
                        DrawGeometryList(geometryList);
                    }
                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if(geometryList.Count != 0)
            {
                featureCollection = new FeatureCollection();
                for(int i = 0; i < geometryList.Count; i++)
                    featureCollection.features.Add(Geometry2JObject(geometryList[i]));
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

        private void ChangeEnable(bool isEnable)
        {
            drawing = !isEnable;
            listStatus.Enabled = isEnable;
            boxColor.Enabled = isEnable;
            BarPenSize.Enabled = isEnable;
            buttonOpen.Enabled = isEnable;
            buttonSave.Enabled = isEnable;
        }

        private JObject Geometry2JObject(Geometry geometry)
        {
            dynamic jo = new JObject();
            jo.type = "Feature";
            jo.properties = new JObject();
            jo.properties.paintColor = new JArray();
            jo.properties.paintColor.Add(geometry.color.R);
            jo.properties.paintColor.Add(geometry.color.G);
            jo.properties.paintColor.Add(geometry.color.B);
            if (geometry.geoType != "Polygon")
                jo.properties.penSize = geometry.size;
            jo.geometry = new JObject();
            jo.geometry.type = geometry.geoType;
            jo.geometry.coordinates = new JArray();
            for (int i = 0; i < geometry.point.Count; i++)
            {
                dynamic point = new JArray();
                point.Add(geometry.point[i].X);
                point.Add(geometry.point[i].Y);
                jo.geometry.coordinates.Add(point);
            }
            return jo;
        }
    }

    public class Geometry
    {
        public List<Point> point = null;
        public Color color;
        public int size;
        public string geoType;
        public Geometry(List<Point> point1, Color color1, int size1, string geoType1)
        {
            point = new List<Point>(point1);
            color = color1;
            size = size1;
            geoType = geoType1;
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
