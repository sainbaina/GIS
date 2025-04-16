using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIS
{
    /// <summary>
    /// Слой для размещения объектов.
    /// Увеличение номера слоя - увеличение высоты.
    /// </summary>
    public class Layer
    {
        private List<MapObject> _objects = new();
        private float _layer;
        private bool _visible = true;
        private Color _color;
        private string _name = "";
        private int _layerType;

        public Layer(float layer, Enums.LayerType layerType)
        {
            _color = System.Drawing.Color.FromArgb((int)layerType);
            _layer = layer;
        }

        public void Add(MapObject obj)
        {
            //obj.Layer = _layer;
            _objects.Add(obj);
        }

        public void Delete(MapObject obj)
        {
            _objects.Remove(obj);
        }

        public float LayerNumber
        {
            get { return _layer; }
            set { _layer = value; }
        }

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public List<MapObject> Objects
        {
            get { return _objects; }
        }
    }
}
