using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TowerDefense
{
    public abstract class Monster
    {
        public int MaxHealth { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }
        public int Gold { get; set; }
        public double Speed { get; set; } // За сколько секунд проходит одну клетку (кратно 100 мс)
        public Cell Position { get; set; }
        public Image MonsterImage { get; set; }

        public void Hit(int damage)
        {
            Health -= (int)(((double)(100-Armor) / 100)*damage);
        }
    }
}
