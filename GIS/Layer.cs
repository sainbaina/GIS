using System;
using System.Collections.Generic;
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
        private int _layer;
        private bool _visible = true;

        public Layer(int layer)
        {
            _layer = layer;
        }

        public void Add(MapObject obj)
        {
            obj.Layer = _layer;
            _objects.Add(obj);
        }

        public void Delete(MapObject obj)
        {
            _objects.Remove(obj);
        }

        public int LayerNumber
        {
            get { return _layer; }
            set { _layer = value; }
        }

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public List<MapObject> Objects
        {
            get { return _objects; }
        }
    }
}
