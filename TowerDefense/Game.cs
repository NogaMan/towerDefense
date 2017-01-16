using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TowerDefense
{
    public delegate void OnTowerAdded(Tower tower);
    public delegate void OnTowerSold(Tower tower);
    public delegate void OnTowerUpgraded(Tower tower);
    public delegate void OnTowerShot(Tower tower, Cell cell);
    public delegate void OnNapalmTowerShot(Tower tower, Cell cell, List<Cell> cells);

    public delegate void OnMonsterSpawned(Monster monster);
    public delegate void OnMonsterKilled(Monster monster);

    public delegate void OnGoldChanged(int Gold);
    public delegate void OnWaveChanged(int wave);
    public delegate void OnLivesChanged(int lives);
    public delegate void OnWin();
    public delegate void OnGameOver(int Wave);

    public class Game
    {
        public List<Monster> Monsters { get; set; }
        public List<Tower> Towers { get; set; }
        public List<Cell> Path { get; set; }
        public Cell[,] Cells { get; set; }
        public int Lives { get; set; }
        public int Gold { get; set; }
        public int Wave { get; set; }
        public OnTowerAdded onTowerAdded;
        public OnTowerSold onTowerSold;
        public OnTowerUpgraded onTowerUpgraded;
        public OnMonsterSpawned onMonsterSpawned;
        public OnMonsterKilled onMonsterKilled;
        public OnTowerShot onTowerShot;
        public OnNapalmTowerShot onNapalmTowerShot;
        public OnWaveChanged onWaveChanged;
        public OnGoldChanged onGoldChanged;
        public OnLivesChanged onLivesChanged;
        public OnWin onWin;
        public OnGameOver onGameOver;
        private bool started;
        private int MSPassed; //Сколько мс прошло с последнего спавна монстра

        private int monstersSpawned;
        private int monstersShouldBeSpawned;
        private int monstersKilled;

        public Game(int[,] pattern)
        {
            Path = new List<Cell>();
            Towers = new List<Tower>();
            Monsters = new List<Monster>();
            ParcePattern(pattern);
        }

        public void Start()
        {
            Wave = 1;
            onWaveChanged(Wave);
            Gold = 90;
            onGoldChanged(Gold);
            Lives = 20;
            onLivesChanged(Lives);

            started = false;
            monstersSpawned = 0;
            monstersKilled = 0;
            monstersShouldBeSpawned = 15 + (int)(Wave * 2.1
                );
            MSPassed = 0;
        }

        private void ParcePattern(int[,] pattern)
        {
            int height = pattern.GetLength(0);
            int width = pattern.GetLength(1);
            Cells = new Cell[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    Cells[i, j] = new Cell();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Cells[i, j].X = j;
                    Cells[i, j].Y = i;
                    switch (pattern[i, j])
                    {
                        case 0:
                            Cells[i, j].CellType = Cell.CellTypes.Nothing;
                            break;
                        case 1:
                            Cells[i, j].CellType = Cell.CellTypes.Ground;
                            break;
                        case 2:
                            Cells[i, j].CellType = Cell.CellTypes.Road;
                            break;
                        case 3:
                            Cells[i, j].CellType = Cell.CellTypes.Rocks;
                            break;
                        case 4:
                            Cells[i, j].CellType = Cell.CellTypes.Start;
                            break;
                        case 5:
                            Cells[i, j].CellType = Cell.CellTypes.Finish;
                            break;
                    }
                }
            }
            BuildPath();
        }
        
        private void BuildPath()
        {
            var query = from cell in Cells.Cast<Cell>()
                        where (cell.CellType == Cell.CellTypes.Start || cell.CellType == Cell.CellTypes.Road || cell.CellType == Cell.CellTypes.Finish)
                select cell;
            Cell currentCell = query.First((c) => c.CellType == Cell.CellTypes.Start);
            Path.Add(currentCell);
            while (currentCell.CellType != Cell.CellTypes.Finish)
            {
                currentCell = query.First((c) => ((c.X - currentCell.X == 0 && Math.Abs(c.Y - currentCell.Y) == 1) ^ (c.X - currentCell.X == 1 && Math.Abs(c.Y - currentCell.Y) == 0)) && !Path.Exists((cell)=> cell == c));
                Path.Add(currentCell);
            }
        }

        public Tower GetTower(Cell cell)
        {
            foreach (Tower tower in Towers)
                if (tower.Position.X == cell.X && tower.Position.Y == cell.Y)
                    return tower;
            return null;
        }
        
        public void AddTower(Cell cell, Tower tower)
        {
            if (GetTower(cell) == null && Gold >= tower.Price)
            {
                tower.Position = cell;
                Towers.Add(tower);
                Gold -= tower.Price;
                onGoldChanged(Gold);
                tower.Price = tower.Price / 2;
                GeneratePathCellsInRadiusOfATower(tower);
                onTowerAdded(tower);
            }
        }

        private void GeneratePathCellsInRadiusOfATower(Tower tower)
        {
            var query = from p in Path
                        where Math.Abs(p.X - tower.Position.X) <= tower.Radius && Math.Abs(p.Y - tower.Position.Y) <= tower.Radius
                        orderby Path.IndexOf(p) descending
                        select p;
            tower.PathInRaduis = query.ToList();
        }

        private List<Cell> PathCellsInRadiusOfAnotherCell(Cell cell, int radius)
        {
            var query = from p in Path
                        where Math.Abs(p.X - cell.X) <= radius && Math.Abs(p.Y - cell.Y) <= radius
                        select p;
            return query.ToList();
        }

        public void SellTower(Cell cell)
        {
            if (GetTower(cell) != null)
            {
                Tower tower = GetTower(cell);
                Gold += tower.Price;
                onGoldChanged(Gold);
                Towers.Remove(tower);
                onTowerSold(tower);
            }
        }

        public void UpgradeTower(Cell cell)
        {
            Tower tower = GetTower(cell);
            if (tower != null && Gold >= tower.UpgradeCost && tower.Level < tower.MaxLevel)
            {
                Gold -= tower.UpgradeCost;
                onGoldChanged(Gold);
                tower.LevelUp();
                onTowerUpgraded(tower);
            }
        }
        
        public void TimerTick()
        {
            UpdateMonstersPositions();
            SpawnMonstersIfNeeded();
            TowersSeek();
        }

        private void UpdateMonstersPositions()
        {
            List<Monster> monstersThatReachedFinish = new List<Monster>();
            for (int i = 0; i < Monsters.Count; i++)
            {
                if (Monsters[i].Position.X >= 15 || (int)((double)Monsters[i].MonsterImage.GetValue(Canvas.LeftProperty) / 50) >= 15)
                {
                    Monsters[i].Health = 0;
                    monstersThatReachedFinish.Add(Monsters[i]);
                    Lives -= 1;
                    onLivesChanged(Lives);
                    if (Lives == 0)
                        onGameOver(Wave);
                }
                else
                {
                    Monsters[i].Position = Cells[(int)(((double)Monsters[i].MonsterImage.GetValue(Canvas.TopProperty) + 25) / 50), (int)(((double)Monsters[i].MonsterImage.GetValue(Canvas.LeftProperty) + 25) / 50)];
                }
            }
            CleanDeadMonsters(monstersThatReachedFinish);
        }

        private void TowersSeek()
        {
            foreach (Tower tower in Towers)
            {
                if (tower.MSPassed != 0)
                {
                    tower.MSPassed += 100;
                    if (tower.MSPassed == tower.Speed )
                        tower.MSPassed = 0;
                }
                else
                {
                    Cell cellToShoot = tower.PathInRaduis.FirstOrDefault(c => Monsters.Any(m => m.Position == c));
                    if (cellToShoot != null)
                    {
                        if (tower is NapalmTower)
                        {
                            NapalmTowerShot(tower, cellToShoot);
                            tower.MSPassed += 100;
                        }
                        else
                        {
                            TowerShot(tower, cellToShoot);
                            tower.MSPassed += 100;
                        }
                    }
                }
            }
        }

        private void TowerShot(Tower towerFrom, Cell cellTo)
        {
            var list = (from m in Monsters
                        where m.Position == cellTo
                        select m).ToList();
            foreach (Monster m in list)
            {
                m.Hit(towerFrom.Damage);
            }
            CleanDeadMonsters(list);
            onTowerShot(towerFrom,cellTo);
        }

        private void NapalmTowerShot(Tower towerFrom, Cell cellTo)
        {
            var list = (from m in Monsters
                        where PathCellsInRadiusOfAnotherCell(cellTo, towerFrom.Radius).Any(cell => cell == m.Position)
                        select m).ToList();
            foreach (Monster m in list)
            {
                m.Hit(towerFrom.Damage);
            }
            CleanDeadMonsters(list);
            onNapalmTowerShot(towerFrom, cellTo, PathCellsInRadiusOfAnotherCell(cellTo, towerFrom.Radius));
        }

        private void CleanDeadMonsters(List<Monster> list)
        {
            foreach (Monster m in list)
                if (m.Health <= 0)
                {
                    Monsters.Remove(m);
                    onMonsterKilled(m);
                    Gold += m.Gold;
                    onGoldChanged(Gold);
                    monstersKilled += 1;
                }
            if (monstersShouldBeSpawned == monstersKilled || Wave % 5 == 0 && monstersShouldBeSpawned / 15 == monstersKilled)
            {
                monstersSpawned = 0;
                monstersKilled = 0;
                started = false;
                Wave += 1;
                if (Wave == 100)
                    onWin();
                onWaveChanged(Wave);
                monstersShouldBeSpawned = 15 + (int)(Wave * 3);
            }
        }

        public void SpawnMonstersIfNeeded()
        {
            if (MSPassed > 6000 || started)
            {
                if (!started)
                {
                    started = true;
                    MSPassed = 0;
                }
                if (monstersSpawned < monstersShouldBeSpawned)
                {
                    if (MSPassed == 300)
                    {
                        if (Wave % 5 == 0)
                        {
                            BossMonster boss = new BossMonster(Wave);
                            boss.Position = Path[0];
                            Monsters.Add(boss);
                            onMonsterSpawned(boss);
                            monstersSpawned += 15;
                        }
                        else
                        {
                            if (monstersSpawned < monstersShouldBeSpawned / 3)
                            {
                                FastWeakMonster monster = new FastWeakMonster(Wave);
                                monster.Position = Path[0];
                                Monsters.Add(monster);
                                onMonsterSpawned(monster);
                                monstersSpawned += 1;
                            }
                            else
                                if (monstersSpawned < monstersShouldBeSpawned * 2 / 3)
                                {
                                    StandardMonster monster = new StandardMonster(Wave);
                                    monster.Position = Path[0];
                                    Monsters.Add(monster);
                                    onMonsterSpawned(monster);
                                    monstersSpawned += 1;
                                }
                                else
                                {
                                    SlowToughMonster monster = new SlowToughMonster(Wave);
                                    monster.Position = Path[0];
                                    Monsters.Add(monster);
                                    onMonsterSpawned(monster);
                                    monstersSpawned += 1;
                                }
                        }
                        MSPassed = 0;
                    }
                    else
                    {
                        MSPassed += 100;
                    }
                }
            }
            else
            {
                MSPassed += 100;
            }
        }
    }
}
