using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GIS
{
    public partial class Map : UserControl
    {
        private float _scaleFactor = 1.0f;
        private int offsetX = 0, offsetY = 0;
        private bool isDragging = false;
        private System.Drawing.Point lastMousePos;
        private List<Layer> _layers = new();

        public Map()
        {
            InitializeComponent();
            DoubleBuffered = true;
            this.MouseDown += Map_MouseDown;
            this.MouseMove += Map_MouseMove;
            this.MouseUp += Map_MouseUp;
            this.MouseWheel += Map_MouseWheel;
        }

        public void AddLayer(Layer layer)
        {
            _layers.Add(layer);
            Invalidate();
        }

        public float ScaleFactor
        {
            get { return _scaleFactor; }
            set
            {
                _scaleFactor = Math.Max(0.1f, Math.Min(value, 10.0f));
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Центрируем координаты перед масштабированием
            g.TranslateTransform(this.Width / 2, this.Height / 2);
            g.ScaleTransform(_scaleFactor, -_scaleFactor);
            g.TranslateTransform(-this.Width / 2 + offsetX, -this.Height / 2 + offsetY);

            Pen p = new Pen(Color.Blue);

            // Вычисляем границы области видимости
            float left = (-offsetX - this.Width / 2) / _scaleFactor;
            float right = (-offsetX + this.Width / 2) / _scaleFactor;
            float top = (-offsetY - this.Height / 2) / _scaleFactor;
            float bottom = (-offsetY + this.Height / 2) / _scaleFactor;

            foreach (var layer in _layers)
            {
                if (layer.Visible)
                {
                    foreach (var obj in layer.Objects)
                    {
                        List<Point[]> vertices = ParserToPoints.Parse(obj);

                        // Проверяем видимость объекта (по крайней мере одна точка должна быть в пределах экрана)
                        bool isVisible = vertices.Any(poly => poly.Any(p =>
                            p.X >= left && p.X <= right &&
                            p.Y >= top && p.Y <= bottom
                        ));

                        if (!isVisible) continue; // Пропускаем объект, если он вне экрана

                        // Если объект видим, рисуем его
                        foreach (var poly in vertices)
                        {
                            for (int j = 0; j < poly.Length - 1; j++)
                            {
                                g.DrawLine(p,
                                    100 * poly[j].X, 100 * poly[j].Y,
                                    100 * poly[j + 1].X, 100 * poly[j + 1].Y
                                );
                            }
                        }
                    }
                }
            }
        }


        // Нажатие кнопки мыши
        private void Map_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastMousePos = e.Location;
            }
        }

        // Перемещение мыши
        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                offsetX += e.X - (int)lastMousePos.X;
                offsetY -= e.Y - (int)lastMousePos.Y;
                lastMousePos = e.Location;
                Invalidate();
            }
        }

        // Отпускание кнопки мыши
        private void Map_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        // Масштабирование с привязкой к курсору
        private void Map_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldScale = _scaleFactor;
            float scaleStep = 1.2f;
            float newScale = (e.Delta > 0) ? _scaleFactor * scaleStep : _scaleFactor / scaleStep;

            newScale = Math.Max(0.1f, Math.Min(newScale, 10.0f));

            _scaleFactor = newScale;
            Invalidate();


        }
    }
}
