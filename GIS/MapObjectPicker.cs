using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIS
{
    public static class MapObjectPicker
    {
        static void GetObjectsInPosition()
        {

        }

        static void HighlightObject(MapObject obj)
        {
            // highlight color -> yellow
            obj.Color = Color.Yellow;
        }

        static void UndoHighlightObject(Layer layer, MapObject obj)
        {
            obj.Color = layer.Color;
        }


    }
}
