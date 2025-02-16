using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace GIS
{
    public static class ParserToPoints
    {
        public static List<Point[]> Parse(MapObject obj)
        {
            List<Point[]> points = new List<Point[]>();
            switch (obj)
            {
                case Point point:
                    points.Append([new Point(point.X, point.Y)]);
                    break;

                case Line line:
                    points.Append([line.A, line.B]);
                    break;

                case Polygon polygon:
                    Point[] tmp = polygon.GetAllVert;
                    tmp.Append(polygon[0]);
                    points.Append(tmp);
                    break;

                case Multiline multiline:
                    points = multiline.GetAllLines;
                    break;
            }
            return points;
        }
    }

    public static class GEOJsonToPrimitives
    {
        public static List<MapObject> Parse(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var geoJson = JsonSerializer.Deserialize<GeoJson>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return ConvertToMapObjects(geoJson);
        }

        private static List<MapObject> ConvertToMapObjects(GeoJson geoJson)
        {
            List<MapObject> mapObjects = new();
            foreach (var feature in geoJson.Features)
            {
                string type = feature.Geometry.Type;
                var coordinates = feature.Geometry.Coordinates;

                //if (type == "Point")
                //{
                //    var point = new Point((float)coordinates[0][0][0], (float)coordinates[0][0][1]);
                //    mapObjects.Add(point);
                //}
                //else if (type == "LineString")
                //{
                //    var points = new List<Point>();
                //    foreach (var coord in coordinates)
                //    {
                //        points.Add(new Point((float)coord[0][0], (float)coord[0][1]));
                //    }
                //    mapObjects.Add(new Multiline(new List<Point[]> { points.ToArray() }));
                //}
                //else if (type == "Polygon")
                //{
                //    var polygons = new List<Point>();
                //    foreach (var coord in coordinates[0]) // First array is the outer boundary
                //    {
                //        polygons.Add(new Point((float)coord[0], (float)coord[1]));
                //    }
                //    mapObjects.Add(new Polygon(polygons.ToArray()));
                //}
                //else 
                if (type == "MultiPolygon")
                {
                    var multiLines = new List<Point[]>();
                    foreach (var polygon in coordinates)
                    {
                        foreach (var ring in polygon)
                        {
                            var ringPoints = new List<Point>();
                            foreach (var coord in ring)
                            {
                                ringPoints.Add(new Point((float)coord[0], (float)coord[1]));
                            }
                            multiLines.Add(ringPoints.ToArray());
                        }
                    }
                    mapObjects.Add(new Multiline(multiLines));
                }
            }

            return mapObjects;
        }

        private static Point ParsePoint(object coordinates)
        {
            if (coordinates is double[] coords && coords.Length == 2)
            {
                return new Point((float)coords[0], (float)coords[1]);
            }
            throw new ArgumentException("Invalid coordinates for Point");
        }

        private static Line ParseLine(object coordinates)
        {
            if (coordinates is double[][] coords && coords.Length >= 2)
            {
                Point a = new Point((float)coords[0][0], (float)coords[0][1]);
                Point b = new Point((float)coords[1][0], (float)coords[1][1]);
                return new Line(a, b);
            }
            throw new ArgumentException("Invalid coordinates for LineString");
        }

        private static Polygon ParsePolygon(object coordinates)
        {
            if (coordinates is double[][][] coords && coords.Length > 0)
            {
                List<Point> points = new();
                foreach (var point in coords[0])
                {
                    points.Add(new Point((float)point[0], (float)point[1]));
                }
                return new Polygon(points.ToArray());
            }
            throw new ArgumentException("Invalid coordinates for Polygon");
        }

        private static Multiline ParseMultiline(object coordinates)
        {
            if (coordinates is double[][][] coords)
            {
                List<Point[]> lines = new();
                foreach (var line in coords)
                {
                    List<Point> points = new();
                    foreach (var point in line)
                    {
                        points.Add(new Point((float)point[0], (float)point[1]));
                    }
                    lines.Add(points.ToArray());
                }
                return new Multiline(lines);
            }
            throw new ArgumentException("Invalid coordinates for MultiLineString");
        }
    }
    public class GeoJson
    {
        public string Type { get; set; }
        public List<Feature> Features { get; set; }
    }

    public class Feature
    {
        public Geometry Geometry { get; set; }
    }

    public class Geometry
    {
        public string Type { get; set; }
        public List<List<List<List<double>>>> Coordinates { get; set; }
    }
}
