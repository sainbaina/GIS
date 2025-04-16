using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIS
{
    public class Enums
    {
        public enum LayerType : uint
        {
            Buildings = 0xFF808080, // grey
            Water = 0xFF0000FF,     // blue
            Plants = 0xFF00FF00,    // green
            Ground = 0xFFFFA500,    // brown
            Underline = 0xFFFFFF00  // yellow
        }
    }
}
