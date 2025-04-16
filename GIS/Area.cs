using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIS
{
    public class Area
    {
        private float _x, _y, _w, _h;
        public Area(float x, float y, float width, float height)
        {
            _x = x;
            _y = y;
            _w = width;
            _h = height;
        }
        public float X {
            get { return _x; }
            set { _x = value; }
        }
        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }
        public float Width
        {
            get { return _w; }
            set { _w = value; }
        }
        public float Height
        {
            get { return _h; }
            set { _h = value; }
        }
    }
}
