using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class FastWeakMonster : Monster
    {
        public FastWeakMonster(int wave)
        {
            MaxHealth = (1 + wave) * 70;
            Health = MaxHealth;
            Armor = 2 + wave/3;
            Speed = 0.3;
            Gold = 5 + (int)(wave * 0.5);
        }
    }
}
