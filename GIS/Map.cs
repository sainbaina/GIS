using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GIS
{
    public partial class Map : UserControl
    {
        private float _scaleFactor = 0.5f;
        private float offsetX = 0, offsetY = 0;
        private bool isDragging = false;
        private System.Drawing.Point lastMousePos;
        private List<Tuple<Layer, List<List<List<Point[]>>>>> _layers = new();
        public int currLayerInd;
        private System.Windows.Forms.ToolTip tooltip = new System.Windows.Forms.ToolTip();

        private RTree _rTree = new RTree();

        List<MapObject> foundObjects;
        (Point[], Color) lastObjIn;

        // RTREE
        List<RectangleF> rtRects;
        private bool _showRects;
        public bool ShowRects
        {
            get { return _showRects; }
            set { _showRects = value; }
        }

        // MOUSE AREA
        float searchRadius = 5f;
        RectangleF searchArea = new RectangleF(0, 0, 0, 0);

        UnderlineLayer underlineLayer = new UnderlineLayer();

        public float ScaleFactor
        {
            get { return _scaleFactor; }
            set
            {
                _scaleFactor = Math.Max(0.01f, Math.Min(value, 1000.0f));
                Invalidate();
                OnScaleChanged();
            }
        }

        public Map()
        {
            InitializeComponent();
            DoubleBuffered = true;
            this.MouseDown += Map_MouseDown;
            this.MouseMove += Map_MouseMove;
            this.MouseClick += Map_MouseClick;
            this.MouseUp += Map_MouseUp;
            this.MouseWheel += Map_MouseWheel;
            this.MouseEnter += Map_MouseEnter;
            this.MouseLeave += Map_MouseLeave;
        }

        public void AddLayer(Layer layer)
        {
            List<List<List<Point[]>>> presets = new();

            foreach ((float zl, float angle) in LayerSimplifier.zoomAnglePairs)
            {
                foreach (var l in _layers)
                {
                    if (l.Item1 == layer)
                    {
                        MessageBox.Show("Unable to add the same layer");
                        return;
                    }
                }
                List<List<Point[]>> preset = new();
                foreach (var obj in layer.Objects)
                {
                    List<Point[]> simpObject = new();
                    var pointsList = ParserToPoints.Parse(obj);
                    foreach (var points in pointsList)
                    {
                        simpObject.Add(points);
                    }
                    preset.Add(simpObject);

                    _rTree.Insert(obj, simpObject);
                }
                presets.Add(preset);
            }
            var childRects = _rTree.ReturnChildRects();
            rtRects = childRects.Item1;

            _layers.Insert(0, new Tuple<Layer, List<List<List<Point[]>>>>(layer, presets));
            Invalidate();
        }

        public void RemoveLayer(Layer layer)
        {
            foreach (var l in _layers)
            {
                if (l.Item1 == layer)
                {
                    foreach (var mapObject in l.Item1.Objects)
                    {
                        _rTree.DeleteNode(mapObject);
                    }

                    _layers.Remove(l);
                    return;
                }
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float worldLeft = (-this.Width / 2f) / _scaleFactor + offsetX;
            float worldRight = (this.Width / 2f) / _scaleFactor + offsetX;
            float worldTop = (-this.Height / 2f) / _scaleFactor + offsetY;
            float worldBottom = (this.Height / 2f) / _scaleFactor + offsetY;

            g.SetClip(new Rectangle(0, 0, this.Width, this.Height));

            g.TranslateTransform(this.Width / 2f, this.Height / 2f);
            g.ScaleTransform(_scaleFactor, -_scaleFactor);
            g.TranslateTransform(-offsetX, -offsetY);

            var j = underlineLayer.GetLayersWithDrawObj();

            var combinedLayers = _layers
                .Concat(underlineLayer.GetLayersWithDrawObj())
                .Where(l => l.Item1.Visible)
                .OrderBy(l => l.Item1.LayerNumber)
                .Reverse()
                .ToList();

            foreach (var layer in combinedLayers
                .Where(l => l.Item1.Visible)
                .OrderBy(l => l.Item1.LayerNumber)
                .Reverse())
            {
                using (Brush brush = new SolidBrush(layer.Item1.Color))
                {
                    Pen pen = new Pen(brush);

                    int index = Array.FindIndex(LayerSimplifier.zoomAnglePairs, pair => pair.zoomLimit >= _scaleFactor);
                    currLayerInd = index;

                    List<List<Point[]>> objects = (index == -1) ? layer.Item2[layer.Item2.Count - 1] : layer.Item2[index];

                    foreach (var (vertices, i) in objects.Select((item, index) => (item, index)))
                    {
                        using (GraphicsPath path = new GraphicsPath())
                        {
                            bool isAreal = false;
                            List<Point[]> polies = new();
                            MapObject obj = layer.Item1.Objects[i];

                            foreach (var poly in vertices)
                            {
                                if (IsPolygonVisible(poly, worldLeft, worldRight, worldTop, worldBottom))
                                {
                                    polies.Add(poly);
                                    if (obj is Polygon polygon || obj is Multiline multiline)
                                    {
                                        lastObjIn.Item1 = poly;
                                        lastObjIn.Item2 = layer.Item1.Color;

                                        path.StartFigure();
                                        path.AddPolygon(poly.Select(pt => new PointF(100 * pt.X, 100 * pt.Y)).ToArray());
                                        isAreal = true;
                                    }
                                    else if (obj is Line line)
                                    {
                                        path.StartFigure();
                                        path.AddLines(poly.Select(pt => new PointF(100 * pt.X, 100 * pt.Y)).ToArray());
                                    }
                                    else if (obj is Point point)
                                    {
                                        float w = 1 / _scaleFactor * 10;
                                        g.FillEllipse(brush, 100 * point.X - w / 2, 100 * point.Y - w / 2, w, w);
                                    }
                                }
                            }

                            if (isAreal) g.FillPath(brush, path);
                            else g.DrawPath(pen, path);

                            if (lastObjIn.Item1 != null)
                            {
                                path.StartFigure();
                                path.AddPolygon(lastObjIn.Item1.Select(pt => new PointF(100 * pt.X, 100 * pt.Y)).ToArray());
                                g.FillPath(new SolidBrush(lastObjIn.Item2), path);
                            }
                        }
                    }
                }

                if (rtRects != null && _showRects)
                {
                    foreach (var rect in rtRects)
                    {
                        (double x1, double y1) = WorldToScreen(rect.Left, rect.Top);
                        (double x2, double y2) = WorldToScreen(rect.Right, rect.Top);
                        (double x3, double y3) = WorldToScreen(rect.Left, rect.Bottom);
                        (double x4, double y4) = WorldToScreen(rect.Right, rect.Bottom);

                        double minX = Math.Min(Math.Min(x1, x2), Math.Min(x3, x4));
                        double maxX = Math.Max(Math.Max(x1, x2), Math.Max(x3, x4));
                        double minY = Math.Min(Math.Min(y1, y2), Math.Min(y3, y4));
                        double maxY = Math.Max(Math.Max(y1, y2), Math.Max(y3, y4));

                        var newRect = new RectangleF((float)minX, (float)minY, (float)(maxX - minX), (float)(maxY - minY));
                        g.DrawRectangle(new Pen(new SolidBrush(Color.Black)), newRect.X, newRect.Y, newRect.Width, newRect.Height);
                    }
                }
            }

            if (searchArea != null)
            {
                (double screenX1, double screenY1) = WorldToScreen(searchArea.Left, searchArea.Top);
                (double screenX2, double screenY2) = WorldToScreen(searchArea.Right, searchArea.Bottom);

                float x = (float)Math.Min(screenX1, screenX2);
                float y = (float)Math.Min(screenY1, screenY2);
                float width = Math.Abs((float)(screenX2 - screenX1));
                float height = Math.Abs((float)(screenY2 - screenY1));

                g.DrawRectangle(new Pen(Color.Red), x, y, width, height);
            }
        }

        private bool IsPolygonVisible(Point[] poly, float left, float right, float top, float bottom)
        {
            foreach (var pt in poly)
            {
                float x = 100 * pt.X;
                float y = 100 * pt.Y;
                if (x >= left && x <= right && y >= top && y <= bottom)
                {
                    return true;
                }
            }
            return false;
        }

        public (double, double) ScreenToWorld(System.Drawing.Point screenPoint)
        {
            float worldX = (screenPoint.X - this.Width / 2f) / _scaleFactor + offsetX;
            float worldY = -(screenPoint.Y - this.Height / 2f) / _scaleFactor + offsetY;

            double lon = worldX / 100;
            double lat = worldY / 100;

            return (lat, lon);
        }

        public (double x, double y) WorldToScreen(double lon, double lat)
        {
            float worldX = (float)(lon * 100);
            float worldY = (float)(lat * 100);

            double screenX = worldX;
            double screenY = worldY;

            return (screenX, screenY);
        }

        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            var worldCoords = ScreenToWorld(e.Location);
            var (lat, lon) = ScreenToWorld(e.Location);
            float radiusWorld = searchRadius / _scaleFactor;

            searchArea = new RectangleF(
                (float)(worldCoords.Item2 - radiusWorld / 3.6f),
                (float)(worldCoords.Item1 - radiusWorld / 3.6f),
                radiusWorld / 1.8f,
                radiusWorld / 1.8f
            );

            Invalidate();

            if (isDragging)
            {
                offsetX -= ((e.X - lastMousePos.X) / _scaleFactor);
                offsetY += ((e.Y - lastMousePos.Y) / _scaleFactor);
                lastMousePos = e.Location;
            }
            else
            {
                tooltip.SetToolTip(this, $"{worldCoords.Item1}, {worldCoords.Item2}");
            }
        }

        private void Map_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void Map_MouseWheel(object sender, MouseEventArgs e)
        {
            bool isCtrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            if (isCtrlPressed)
            {
                float radiusStep = 1.0f;
                searchRadius = (e.Delta > 0) ? searchRadius + radiusStep : Math.Max(1.0f, searchRadius - radiusStep);
            }
            else
            {
                rtRects = _rTree.ReturnChildRects().Item1;
                float oldScale = _scaleFactor;
                float scaleStep = 1.2f;
                float newScale = (e.Delta > 0) ? _scaleFactor * scaleStep : _scaleFactor / scaleStep;
                ScaleFactor = newScale;
            }

            Invalidate();
        }

        private void Map_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var worldCoords = ScreenToWorld(e.Location);
            var selectedObject = GetObjectAt(worldCoords);

            if (selectedObject != null)
            {
                tooltip.SetToolTip(this, selectedObject.GetType().Name);
            }

            var y = (float)worldCoords.Item1;
            var x = (float)worldCoords.Item2;

            bool isShiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
            if (!isShiftPressed)
            {
                underlineLayer.ClearObjects();
                Invalidate();
            }

            foundObjects = _rTree.Query(searchArea)
                .Where(obj => obj.Layer.Visible == true)
                .ToList();

            Dictionary<MapObject, bool> dict = new();

            var checkedObjects = underlineLayer.GetCheckedObjects(foundObjects);
            underlineLayer.CheckObjects(checkedObjects);
            Invalidate();
        }

        private void Map_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastMousePos = e.Location;
            }
        }

        private void Map_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Cross;
        }

        private void Map_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        public event EventHandler ScaleChanged;

        protected virtual void OnScaleChanged()
        {
            ScaleChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        public MapObject GetObjectAt((double, double) worldCoords)
        {
            double lat = worldCoords.Item1;
            double lon = worldCoords.Item2;

            foreach (var layer in _layers.Where(l => l.Item1.Visible))
            {
                foreach (var obj in layer.Item1.Objects)
                {
                    if (IsPointInsideObject(lon, lat, obj))
                    {
                        return obj;
                    }
                }
            }
            return null;
        }

        public bool IsPointInsideObject(double lon, double lat, MapObject obj)
        {
            if (obj is Point point)
            {
                return Math.Abs(point.X - lon) < 0.1 && Math.Abs(point.Y - lat) < 0.1;
            }
            return false;
        }
    }
}