using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class SlowToughMonster : Monster
    {
        public SlowToughMonster(int wave)
        {
            MaxHealth = (1 + wave) * 150;
            Health = MaxHealth;
            Armor = 6 + 2*wave/3;
            Speed = 1;
            Gold = 5 + (int)(wave * 0.5);
        }
    }
}
