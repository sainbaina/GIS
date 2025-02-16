using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GIS
{
    public class MapObject
    {
        private int _sublayer = 0;
        public int Layer
        {
            get { return _sublayer; }
            set { _sublayer = value; }
        }
    }

    public class Point : MapObject
    {
        private float _x;
        private float _y;

        public Point(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public float X
        {
            get { return _x; }
            set { _x = value; }
        }

        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }
    }


    internal class Line : MapObject
    {
        private Point _a;
        private Point _b;

        public Line(Point a, Point b)
        {
            _a = a;
            _b = b;
        }

        public Point A
        {
            get { return _a; }
            set { _a = value; }
        }

        public Point B
        {
            get { return _b; }
            set { _b = value; }
        }
    }

    internal class Polygon : MapObject
    {
        private Point[] _vertices;

        public Polygon(Point[] vertexes)
        {
            _vertices = vertexes;
        }

        public Point[] GetAllVert
        {
            get { return _vertices; }
        }

        public Point[] SetAllVert
        {
            set { _vertices = value; }
        }

        public Point this[int index]
        {
            get { return _vertices[index]; }
            set { _vertices[index] = value; }
        }
    }

    public class Multiline : MapObject
    {
        private List<Point[]> _lines;

        public Multiline(List<Point[]> lines)
        {
            _lines = lines;
        }

        public List<Point[]> GetAllLines
        {
            get { return _lines; }
        }

        public List<Point[]> SetAllLines
        {
            set { _lines = value; }
        }

        public Point[] this[int index]
        {
            get { return _lines[index]; }
            set { _lines[index] = value; }
        }
    }

    internal class Text : MapObject
    {
        private string _text;
        private Point _position;
        private double _angle;

        public Text(string text, Point pos, double angle = 0)
        {
            _text = text;
            _position = pos;
            _angle = angle;
        }

        public string Contents
        {
            get { return _text; }
            set { _text = value; }
        }

        public Point Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public double Angle
        {
            get { return _angle; }
            set { _angle = value; }
        }
    }
}

