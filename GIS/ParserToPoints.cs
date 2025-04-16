using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
                    points.Add([new Point(point.X, point.Y)]);
                    break;

                case Line line:
                    points.Add(line.Vertices);
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
                if (feature.Geometry.Type == "Polygon")
                {
                    var coords = feature.Geometry.GetPolygonCoordinates();
                    var multiLines = new List<Point[]>();
                    foreach (var polygon in coords)
                    {
                        var ringPoints = new List<Point>();
                        foreach (var coord in polygon)
                        {
                            ringPoints.Add(new Point((float)coord[0], (float)coord[1]));
                        }
                        multiLines.Add(ringPoints.ToArray());
                    }
                    mapObjects.Add(new Multiline(multiLines));
                }
                else if (feature.Geometry.Type == "MultiPolygon")
                {
                    var coords = feature.Geometry.GetMultiPolygonCoordinates();
                    var multiLines = new List<Point[]>();
                    foreach (var polygon in coords)
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
                else if (feature.Geometry.Type == "Point")
                {
                    var coords = feature.Geometry.GetPointCoordinates();
                    if (coords != null)
                    {
                        mapObjects.Add(new Point((float)coords[0], (float)coords[1]));
                    }
                }
                else if (feature.Geometry.Type == "LineString")
                {
                    var coords = feature.Geometry.GetLineStringCoordinates();
                    var linePoints = new List<Point>();
                    foreach (var coord in coords)
                    {
                        linePoints.Add(new Point((float)coord[0], (float)coord[1]));
                    }
                    mapObjects.Add(new Line(linePoints.ToArray()));
                }
            }

            return mapObjects;
        


            //List<MapObject> mapObjects = new();
            //foreach (var feature in geoJson.Features)
            //{
            //    string type = feature.Geometry.Type;
            //    var coordinates = feature.Geometry.Coordinates;

            //    if (type == "MultiPolygon")
            //    {
            //        var multiLines = new List<Point[]>();
            //        foreach (var polygon in coordinates)
            //        {
            //            foreach (var ring in polygon)
            //            {
            //                var ringPoints = new List<Point>();
            //                foreach (var coord in ring)
            //                {
            //                    ringPoints.Add(new Point((float)coord[0], (float)coord[1]));
            //                }
            //                multiLines.Add(ringPoints.ToArray());
            //            }
            //        }
            //        mapObjects.Add(new Multiline(multiLines));
            //    }
            //}

            //return mapObjects;
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
                return new Line(new Point[] { a, b});
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

        [JsonPropertyName("coordinates")]
        public object Coordinates { get; set; }

        public List<List<List<double>>> GetPolygonCoordinates()
        {
            return Type == "Polygon"
                ? JsonSerializer.Deserialize<List<List<List<double>>>>(JsonSerializer.Serialize(Coordinates))
                : null;
        }

        public List<List<List<List<double>>>> GetMultiPolygonCoordinates()
        {
            return Type == "MultiPolygon"
                ? JsonSerializer.Deserialize<List<List<List<List<double>>>>>(JsonSerializer.Serialize(Coordinates))
                : null;
        }

        public List<double> GetPointCoordinates()
        {
            return Type == "Point"
                ? JsonSerializer.Deserialize<List<double>>(JsonSerializer.Serialize(Coordinates))
                : null;
        }

        public List<List<double>> GetLineStringCoordinates()
        {
            return Type == "LineString"
                ? JsonSerializer.Deserialize<List<List<double>>>(JsonSerializer.Serialize(Coordinates))
                : null;
        }
    }

}
