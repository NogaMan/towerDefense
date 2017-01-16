using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class CommonTower : Tower
    {
        public CommonTower()
        {
            Damage = 50;
            Name = "Common Tower";
            Radius = 1;
            Level = 1;
            MaxLevel = 15;
            DamageType = "Basic";
            Speed = 700;
            Price = 30;
            UpgradeCost = 30;
        }

        public override void LevelUp()
        {
            Level++;
            Damage += 50 + (Level-1)*5;
            Price += UpgradeCost / 2;
            UpgradeCost += 30 + Level;
            if (Speed > 300 && Level % 3 == 0)
                Speed -= 100;
        }
    }
}
