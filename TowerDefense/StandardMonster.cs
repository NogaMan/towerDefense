using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class StandardMonster : Monster
    {
        public StandardMonster(int wave)
        {
            MaxHealth = (1 + wave) * 90;
            Health = MaxHealth;
            Armor = 4 + wave/2;
            Speed = 0.6;
            Gold = 5 + (int)(wave * 0.5);
        }
    }
}
