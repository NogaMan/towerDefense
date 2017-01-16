using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public CellTypes CellType { get; set; }


        public enum CellTypes
        {
            Nothing,
            Road,
            Ground,
            Rocks,
            Start,
            Finish
        }
    }
}
