using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace GIS
{
    public class RTree
    {
        private List<RTreeNode> _nodes = new();
        private List<RTreeNode> _scaleNodes = new();

        public List<RTreeNode> ScaleNodes
        {
            get { return _scaleNodes; }
            set { _scaleNodes = value; }
        }

        public RTree(int capacity = 4)
        {
        }

        public (List<RectangleF>, List<RTreeNode>) ReturnChildRects()
        {
            List<RectangleF> rects = new();
            List<RTreeNode> allNodes = new();

            foreach (var node in _nodes.ToList())
            {
                allNodes.Add(node); // include root node
                allNodes.AddRange(SeekChildren(node));
            }

            foreach (var node in _scaleNodes.ToList())
            {
                allNodes.Add(node); // include root node
                allNodes.AddRange(SeekChildren(node));
            }

            foreach (var node in allNodes)
                rects.Add(node.GetBoundingBox());

            return (rects, allNodes);
        }


        private List<RTreeNode> SeekChildren(RTreeNode node)
        {
            List<RTreeNode> retChildren = new();
            foreach (var child in node.Children)
            {
                retChildren.Add(child);
                retChildren.AddRange(SeekChildren(child));
            }
            return retChildren;
        }


        public void Insert(MapObject obj, List<Point[]> geometry)
        {
            var newNode = new RTreeNode(obj, geometry, null);

            // If no nodes exist, just add it as root
            if (_nodes.Count == 0)
            {
                _nodes.Add(newNode);
                return;
            }

            bool inserted = false;

            foreach (var rootNode in _nodes.ToList())
            {
                if (TryInsertIntoTree(rootNode, newNode))
                {
                    inserted = true;
                    break;
                }
            }

            // If not inserted anywhere, treat it as a new top-level node
            if (!inserted)
            {
                _nodes.Add(newNode);
            }

            // Ensure non-intersecting top-level nodes
            ResolveTopLevelIntersections();
        }

        private bool TryInsertIntoTree(RTreeNode parent, RTreeNode current)
        {
            var parentBB = parent.GetBoundingBox();
            var currentBB = CalculateBoundingBox(current.Geometry);

            // Case 1: Current node is larger than the parent and contains it
            if (currentBB.Contains(parentBB) || (GetArea(currentBB) >= GetArea(parentBB) && currentBB.IntersectsWith(parentBB)))
            {
                ReplaceParentWithCurrent(parent, current);
                return true;
            }

            // Case 2: Current node is smaller than the parent and intersects/contained in it
            if (parentBB.Contains(currentBB) || parentBB.IntersectsWith(currentBB))
            {
                return AddAsChildOrReassign(parent, current);
            }

            // Case 3: No relationship, check children recursively
            foreach (var child in parent.Children.ToList())
            {
                if (TryInsertIntoTree(child, current))
                {
                    return true;
                }
            }

            return false;
        }

        private void ReplaceParentWithCurrent(RTreeNode parent, RTreeNode current)
        {
            // Replace parent with current in its grandparent's children list
            if (parent.Parent != null)
            {
                var grand = parent.Parent;
                int index = grand.Children.IndexOf(parent);
                if (index != -1)
                {
                    grand.Children[index] = current;
                }
            }
            else
            {
                // Root node — replace in _nodes
                int index = _nodes.IndexOf(parent);
                if (index != -1)
                {
                    _nodes[index] = current;
                }
            }

            current.Parent = parent.Parent;
            parent.Parent = current;
            current.AddChild(parent);
        }

        private RectangleF CalculateBoundingBox(List<Point[]> geometry)
        {
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var ring in geometry)
            {
                foreach (var point in ring)
                {
                    minX = Math.Min(minX, point.X);
                    minY = Math.Min(minY, point.Y);
                    maxX = Math.Max(maxX, point.X);
                    maxY = Math.Max(maxY, point.Y);
                }
            }

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        private bool AddAsChildOrReassign(RTreeNode parent, RTreeNode current)
        {
            var currentBB = CalculateBoundingBox(current.Geometry);

            // Check if current is larger than any of the parent's children
            List<RTreeNode> smallerChildren = new();
            foreach (var child in parent.Children.ToList())
            {
                var childBB = child.GetBoundingBox();
                if (currentBB.Contains(childBB) || (GetArea(currentBB) >= GetArea(childBB) && currentBB.IntersectsWith(childBB)))
                {
                    smallerChildren.Add(child);
                }
            }

            if (smallerChildren.Count > 0)
            {
                // Add current as a child of the parent and reassign smaller children to it
                parent.Children.RemoveAll(smallerChildren.Contains);
                foreach (var child in smallerChildren)
                {
                    current.AddChild(child);
                }
                parent.AddChild(current);
                return true;
            }

            // Otherwise, add current as a child of one of the intersecting children
            foreach (var child in parent.Children.ToList())
            {
                var childBB = child.GetBoundingBox();
                if (childBB.Contains(currentBB) || childBB.IntersectsWith(currentBB))
                {
                    return TryInsertIntoTree(child, current);
                }
            }

            // No suitable child found, add current directly under the parent
            parent.AddChild(current);
            return true;
        }

        private void ResolveTopLevelIntersections()
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                for (int j = i + 1; j < _nodes.Count; j++)
                {
                    var node1 = _nodes[i];
                    var node2 = _nodes[j];

                    var bb1 = node1.GetBoundingBox();
                    var bb2 = node2.GetBoundingBox();

                    if (bb1.IntersectsWith(bb2))
                    {
                        if (GetArea(bb1) >= GetArea(bb2))
                        {
                            _nodes.RemoveAt(j);
                            node1.AddChild(node2);
                        }
                        else
                        {
                            _nodes.RemoveAt(i);
                            node2.AddChild(node1);
                        }
                        j--; // Adjust index after removal
                    }
                }
            }
        }

        private float GetArea(RectangleF rect) => rect.Width * rect.Height;

        private bool IsRayIntersectingSegment(PointF point, PointF segmentStart, PointF segmentEnd)
        {
            // Убедимся, что сегмент вертикально корректен
            if (segmentStart.Y > segmentEnd.Y)
            {
                (segmentStart, segmentEnd) = (segmentEnd, segmentStart);
            }

            // Проверяем, что точка находится в пределах Y-координат сегмента
            if (point.Y == segmentStart.Y || point.Y == segmentEnd.Y)
            {
                point.Y += 0.00001f; // Сдвигаем точку, чтобы избежать краевых случаев
            }

            if (point.Y < segmentStart.Y || point.Y > segmentEnd.Y)
            {
                return false; // Точка вне вертикальных границ сегмента
            }

            if (point.X >= Math.Max(segmentStart.X, segmentEnd.X))
            {
                return false; // Точка правее сегмента
            }

            if (point.X < Math.Min(segmentStart.X, segmentEnd.X))
            {
                return true; // Точка левее сегмента
            }

            // Вычисляем точку пересечения луча с сегментом
            float intersectionX = segmentStart.X + (point.Y - segmentStart.Y) * (segmentEnd.X - segmentStart.X) / (segmentEnd.Y - segmentStart.Y);

            return point.X <= intersectionX;
        }

        private bool IsPointInsidePolygon(PointF point, List<Point[]> polygon)
        {
            int intersectionCount = 0;

            foreach (var ring in polygon)
            {
                for (int i = 0; i < ring.Length; i++)
                {
                    PointF segmentStart = new PointF(ring[i].X, ring[i].Y);
                    PointF segmentEnd = new PointF(ring[(i + 1) % ring.Length].X, ring[(i + 1) % ring.Length].Y); 

                    if (IsRayIntersectingSegment(point, segmentStart, segmentEnd))
                    {
                        intersectionCount++;
                    }
                }
            }

            return intersectionCount % 2 == 1; // Нечетное количество пересечений = внутри
        }


        /// <summary>
        /// Query
        /// </summary>
        public List<MapObject> Query(RectangleF area)
        {
            List<MapObject> foundObjects = new();

            PointF centerPoint = new PointF(area.X + area.Width / 2, area.Y + area.Height / 2);

            foreach (var node in _nodes.ToList())
                InspectFullyContained(node, area, foundObjects);

            if (foundObjects.Count > 0)
            {
                return foundObjects;
            }

            foreach (var node in _nodes.ToList())
            {
                var containingNodes = FindNodesContainingArea(node, area);
                foreach (var containingNode in containingNodes)
                {
                    if (IsPointInsidePolygon(centerPoint, containingNode.Geometry))
                    {
                        foundObjects.Add(containingNode.MapObject);
                    }
                    //foundObjects.Add(containingNode.MapObject);
                }
            }

            foreach (var obj in foundObjects)
            {
                List<Point[]> parsed = ParserToPoints.Parse(obj);

            }

            return foundObjects;
        }

        private void InspectFullyContained(RTreeNode node, RectangleF area, List<MapObject> foundObjects)
        {
            var bounds = node.GetBoundingBox();
            if (!area.Contains(bounds))
            {
                foreach (var child in node.Children)
                    InspectFullyContained(child, area, foundObjects);
            }
            else
            {
                if (IsGeometryFullyContained(node.Geometry, area))
                    foundObjects.Add(node.MapObject);
            }
        }

        private bool IsGeometryFullyContained(List<Point[]> geometry, RectangleF area)
        {
            foreach (var ring in geometry)
            {
                foreach (var point in ring)
                {
                    if (!area.Contains(point.X, point.Y))
                        return false; // At least one point is outside the area
                }
            }
            return true; // All points are inside the area
        }

        public void DeleteNode(MapObject mapObject)
        {
            // Удаление узла из корневых узлов
            _nodes.RemoveAll(node => node.MapObject == mapObject);

            // Удаление узла из масштабных узлов
            _scaleNodes.RemoveAll(node => node.MapObject == mapObject);

            // Рекурсивное удаление из всех дочерних узлов
            foreach (var node in _nodes.ToList())
            {
                RemoveChildNode(node, mapObject);
            }

            foreach (var node in _scaleNodes.ToList())
            {
                RemoveChildNode(node, mapObject);
            }
        }

        private void RemoveChildNode(RTreeNode node, MapObject mapObject)
        {
            // Удаление дочерних узлов
            node.Children.RemoveAll(child => child.MapObject == mapObject);

            // Рекурсивный обход дочерних узлов
            foreach (var child in node.Children.ToList())
            {
                RemoveChildNode(child, mapObject);
            }
        }

        private List<RTreeNode> FindNodesContainingArea(RTreeNode node, RectangleF area)
        {
            List<RTreeNode> containingNodes = new();

            // Check if the current node's bounding box fully contains the query area
            var bounds = node.GetBoundingBox();
            if (bounds.Contains(area) || bounds.IntersectsWith(area))
            {
                containingNodes.Add(node);
            }

            // Recursively check child nodes
            foreach (var child in node.Children)
            {
                var childContainingNodes = FindNodesContainingArea(child, area);
                containingNodes.AddRange(childContainingNodes);
            }

            return containingNodes;
        }



        public class RTreeNode
        {
            public MapObject MapObject { get; }
            public List<Point[]> Geometry { get; }
            private List<RTreeNode> _children = new();
            private RTreeNode _parent;

            public RTreeNode Parent { get => _parent; set => _parent = value; }
            public List<RTreeNode> Children => _children;

            public RTreeNode(MapObject mapObject, List<Point[]> geometry, RTreeNode parent)
            {
                MapObject = mapObject;
                Geometry = geometry ?? new List<Point[]>();
                Parent = parent;
            }

            public void AddChild(RTreeNode child)
            {
                _children.Add(child);
                child.Parent = this;
            }

            public RectangleF GetBoundingBox()
            {
                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;

                foreach (var ring in Geometry)
                {
                    foreach (var point in ring)
                    {
                        minX = Math.Min(minX, point.X);
                        minY = Math.Min(minY, point.Y);
                        maxX = Math.Max(maxX, point.X);
                        maxY = Math.Max(maxY, point.Y);
                    }
                }

                return new RectangleF(minX, minY, maxX - minX, maxY - minY);
            }
        }
    }
}
