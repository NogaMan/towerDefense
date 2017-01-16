using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TowerDefense
{
    public abstract class Tower
    {
        public int MSPassed { get; set; } // Сколько мс прошло с последнего выстрела
        public Cell Position { get; set; }
        public List<Cell> PathInRaduis { get; set; }
        public int Damage { get; set; }
        public string Name { get; set; }
        public int Radius { get; set; }
        public int Level { get; set; }
        public int MaxLevel { get; set; }
        public string DamageType { get; set; }
        public int Speed { get; set; } // Интервал между выстрелами в мс (кратный 100 мс)
        public int Price { get; set; }
        public int UpgradeCost { get; set; }
        public virtual void LevelUp() { }
        public Image TowerImage { get; set; }
    }
}
