using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIS
{
    public class UnderlineLayer
    {
        Dictionary<MapObject, bool> checkedObjects = new();

        public void ClearObjects()
        {
            checkedObjects.Clear();
        }

        public Dictionary<MapObject, bool> GetCheckedObjects(List<MapObject> objects)
        {
            foreach (var obj in objects)
            {
                if (!checkedObjects.ContainsKey(obj))
                {
                    checkedObjects[obj] = true;
                }
            }
            return checkedObjects;
        }

        public void CheckObjects(Dictionary<MapObject, bool> objects)
        {
            foreach (var obj in objects)
            {
                if (checkedObjects.ContainsKey(obj.Key))
                {
                    checkedObjects[obj.Key] = obj.Value;
                }
            }
        }

        public List<Tuple<Layer, List<List<List<Point[]>>>>> GetLayersWithDrawObj()
        {
            if (checkedObjects == null || checkedObjects.Count == 0)
                return new List<Tuple<Layer, List<List<List<Point[]>>>>>();

            return checkedObjects
                .Where(kvp => kvp.Value) 
                .Select(kvp => kvp.Key) 
                .GroupBy(obj => obj.Layer) 
                .Select(group =>
                {
                    var layer = new Layer((float)group.Key.LayerNumber, Enums.LayerType.Underline);

                    foreach (var item in group)
                    {
                        layer.Add(item);
                    }

                    var points = group
                        .Select(item => ParserToPoints.Parse(item)) 
                        .ToList(); 

                    var nestedPoints = new List<List<List<Point[]>>>
                    {
                        points
                    };

                    return Tuple.Create(layer, nestedPoints);
                })
                .ToList();
        }
    }
}
