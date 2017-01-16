using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class BossMonster : Monster
    {
        public BossMonster(int wave)
        {
            MaxHealth = (1 + wave)*600;
            Health = MaxHealth;
            Armor = 10 + 2 * wave / 3;
            Speed = 1.4;
            Gold = 40 + (int)(wave * 3);
        }
    }
}
