using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class NapalmTower : Tower
    {
        public NapalmTower()
        {
            Damage = 30;
            Name = "Napalm Tower";
            Radius = 1;
            Level = 1;
            MaxLevel = 35;
            DamageType = "Splash";
            Speed = 900;
            Price = 50;
            UpgradeCost = 50;
        }

        public override void LevelUp()
        {
            Level++;
            Damage = (int)(1.3 * Damage);
            Price += UpgradeCost / 2;
            UpgradeCost = (int)(1.6 * UpgradeCost);
            if (Speed > 400 && Level % 5 == 0)
                Speed -= 100;
        }
    }
}
