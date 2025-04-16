using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIS
{
    public static class GeometryUtils
    {
        private static readonly GeometryFactory _geometryFactory = new GeometryFactory();

        public static NetTopologySuite.Geometries.Polygon ConvertToPolygon(List<Point[]> pointRings)
        {
            if (pointRings == null || pointRings.Count == 0)
                throw new ArgumentException("Полигон должен содержать хотя бы один контур!");

            // Главный внешний контур (обход должен быть против часовой стрелки)
            var shell = new LinearRing(pointRings[0].Select(p => new Coordinate(p.X, p.Y)).ToArray());

            // Внутренние кольца (если есть, обход по часовой стрелке)
            LinearRing[] holes = pointRings.Skip(1)
                .Select(ring => new LinearRing(ring.Select(p => new Coordinate(p.X, p.Y)).ToArray()))
                .ToArray();

            return _geometryFactory.CreatePolygon(shell, holes);
        }
    }
}
