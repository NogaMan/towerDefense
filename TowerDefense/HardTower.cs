using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class HardTower : Tower
    {
        public HardTower()
        {
            Damage = 200;
            Name = "Hard Tower";
            Radius = 2;
            Level = 1;
            MaxLevel = 25;
            DamageType = "Basic";
            Speed = 1300;
            Price = 60;
            UpgradeCost = 60;
        }

        public override void LevelUp()
        {
            Level++;
            Damage += 200 + (Level - 1) * 10;
            Price += UpgradeCost / 2;
            UpgradeCost += 60 + 2*Level;
            if (Speed > 700 && Level % 4 == 0)
                Speed -= 100;
        }
    }
}
