using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GIS
{
    public class QuadTree
    {
        private readonly int _capacity;
        private readonly RectangleF _bounds;
        private readonly Dictionary<MapObject, List<Point[]>> _objects = new();
        private readonly QuadTree[] _children = new QuadTree[4];

        public QuadTree(RectangleF bounds, int capacity = 4)
        {
            _bounds = bounds;
            _capacity = capacity;
        }

        public void Insert(List<Point[]> polygon, MapObject obj)
        {
            // Early return if polygon does not intersect the quadtree bounds
            if (!PolygonIntersectsRectangle(polygon, _bounds))
                return;

            // Insert into the current node if there's space
            if (_objects.Count < _capacity)
            {
                _objects[obj] = polygon;
            }
            else
            {
                // Subdivide if needed
                if (_children[0] == null)
                    Subdivide();

                // Recursively insert into children
                foreach (var child in _children)
                {
                    child.Insert(polygon, obj);
                }
            }
        }

        private bool PolygonIntersectsRectangle(List<Point[]> polygon, RectangleF rect)
        {
            foreach (var ring in polygon)
            {
                // Check for line intersections with rectangle edges
                for (int i = 0; i < ring.Length; i++)
                {
                    var p1 = ring[i];
                    var p2 = ring[(i + 1) % ring.Length];

                    if (LineIntersectsRectangle(p1, p2, rect))
                        return true;
                }
            }

            // If no intersection, check if the polygon is inside the rectangle or vice versa
            return PolygonContainsRectangle(polygon, rect) || RectangleContainsPolygon(rect, polygon);
        }

        private bool LineIntersectsRectangle(Point p1, Point p2, RectangleF rect)
        {
            // Check all four sides of the rectangle
            return LineIntersectsLine(p1, p2, new Point((int)rect.Left, (int)rect.Top), new Point((int)rect.Right, (int)rect.Top)) ||
                   LineIntersectsLine(p1, p2, new Point((int)rect.Right, (int)rect.Top), new Point((int)rect.Right, (int)rect.Bottom)) ||
                   LineIntersectsLine(p1, p2, new Point((int)rect.Right, (int)rect.Bottom), new Point((int)rect.Left, (int)rect.Bottom)) ||
                   LineIntersectsLine(p1, p2, new Point((int)rect.Left, (int)rect.Bottom), new Point((int)rect.Left, (int)rect.Top));
        }

        private bool PolygonContainsRectangle(List<Point[]> polygon, RectangleF rect)
        {
            // A more efficient check would be using NetTopologySuite's geometric operations
            return polygon.Any(ring => ring.All(p => rect.Contains(new PointF(p.X, p.Y))));
        }

        private bool RectangleContainsPolygon(RectangleF rect, List<Point[]> polygon)
        {
            // A more efficient check would be using NetTopologySuite's geometric operations
            return polygon.Any(ring => ring.All(p => rect.Contains(new PointF(p.X, p.Y))));
        }

        private bool LineIntersectsLine(Point a1, Point a2, Point b1, Point b2)
        {
            // Line intersection logic using orientation test
            int d1 = Direction(b1, b2, a1);
            int d2 = Direction(b1, b2, a2);
            int d3 = Direction(a1, a2, b1);
            int d4 = Direction(a1, a2, b2);

            if (d1 != d2 && d3 != d4)
                return true;

            return (d1 == 0 && OnSegment(b1, b2, a1)) ||
                   (d2 == 0 && OnSegment(b1, b2, a2)) ||
                   (d3 == 0 && OnSegment(a1, a2, b1)) ||
                   (d4 == 0 && OnSegment(a1, a2, b2));
        }

        private int Direction(Point a, Point b, Point c)
        {
            float val = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            return val == 0 ? 0 : (val > 0 ? 1 : -1);
        }

        private bool OnSegment(Point a, Point b, Point c)
        {
            return Math.Min(a.X, b.X) <= c.X && c.X <= Math.Max(a.X, b.X) &&
                   Math.Min(a.Y, b.Y) <= c.Y && c.Y <= Math.Max(a.Y, b.Y);
        }

        private void Subdivide()
        {
            // Subdivide the quadtree into 4 children
            float hw = _bounds.Width / 2;
            float hh = _bounds.Height / 2;

            _children[0] = new QuadTree(new RectangleF(_bounds.Left, _bounds.Top, hw, hh), _capacity);
            _children[1] = new QuadTree(new RectangleF(_bounds.Left + hw, _bounds.Top, hw, hh), _capacity);
            _children[2] = new QuadTree(new RectangleF(_bounds.Left, _bounds.Top + hh, hw, hh), _capacity);
            _children[3] = new QuadTree(new RectangleF(_bounds.Left + hw, _bounds.Top + hh, hw, hh), _capacity);

            foreach (var obj in _objects)
            {
                foreach (var child in _children)
                {
                    child.Insert(obj.Value, obj.Key);
                }
            }

            _objects.Clear();
        }

        public List<MapObject> Query(RectangleF area)
        {
            List<MapObject> found = new();

            if (!_bounds.IntersectsWith(area))
                return found;

            foreach (var obj in _objects)
            {
                if (PolygonIntersectsRectangle(obj.Value, area))
                {
                    found.Add(obj.Key);
                }
            }

            if (_children[0] != null)
            {
                foreach (var child in _children)
                {
                    found.AddRange(child.Query(area));
                }
            }

            return found;
        }

        public List<MapObject> GetOuterPoligon(Point point)
        {
            Dictionary<MapObject, List<Point[]>> found = new();

            foreach (var item in _objects)
            {
                if (item.Key.GetType() == typeof(Multiline))
                {
                    foreach (var val in item.Value)
                    {
                        bool res = IsPointInPolygon(point, val.ToList());
                        if (res && !found.Contains(item))
                        {
                            found.Add(item.Key, item.Value);
                        }
                    }
                }
            }

            return found.Keys.ToList();
        }

        // Метод для проверки, лежит ли точка внутри многоугольника
        public static bool IsPointInPolygon(Point point, List<Point> polygon)
        {
            int n = polygon.Count;
            bool inside = false;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                Point p1 = polygon[i];
                Point p2 = polygon[j];

                // Проверка пересечения луча
                if (point.Y > Math.Min(p1.Y, p2.Y) && point.Y <= Math.Max(p1.Y, p2.Y) && point.X <= Math.Max(p1.X, p2.X))
                {
                    if (p1.Y != p2.Y)
                    {
                        float xinters = (point.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                        if (p1.X == p2.X || point.X <= xinters)
                        {
                            inside = !inside;
                        }
                    }
                }
            }

            return inside;
        }
    }

    //public class PointInPolygon
    //{
        
    //}
}
