using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TowerDefense
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Game game;
        Cell SelectedCell;
        Image imgSelectedCell;
        DispatcherTimer timer;
        PathGeometry monstersAnimationPath;

        public MainWindow()
        {
            InitializeComponent();
            imgSelectedCell = new Image();
            BitmapImage imgBitmap = new BitmapImage(new Uri(@"\img\selectedCell.png", UriKind.Relative));
            imgSelectedCell.Source = imgBitmap;
            imgSelectedCell.Width = 50.4;
            imgSelectedCell.Height = 50.4;
            imgSelectedCell.SetValue(Canvas.ZIndexProperty, 2);
            canvasGame.Children.Add(imgSelectedCell);
            imgSelectedCell.Visibility = System.Windows.Visibility.Hidden;

            timer = new DispatcherTimer();

        }

        private int[,] ParcePattern(string[] strings)
        {
            int[,] pattern = new int[12, 16];
            for (int j = 0; j < strings.Length; j++)
                for (int k = 0; k < strings[j].Length; k++)
                {
                    pattern[j, k] = int.Parse(strings[j][k].ToString());
                }
            return pattern;
        }

        private void initHandlers()
        {
            game.onTowerAdded = OnTowerAddedHandler;
            game.onTowerSold = OnTowerSoldHandler;
            game.onTowerUpgraded = OnTowerUpgradedHandler;
            game.onMonsterSpawned = OnMonsterSpawnedHandler;
            game.onMonsterKilled = OnMonsterKilledHandler;
            game.onGameOver = OnGameOverHandler;
            game.onTowerShot = OnTowerShotHandler;
            game.onNapalmTowerShot = OnNapamTowerShotHandler;
            game.onWaveChanged = OnWaveChangedHandler;
            game.onGoldChanged = OnGoldChangedHandler;
            game.onLivesChanged = OnLivesChangedHandler;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            FileStream stream = new FileStream("demo.txt", FileMode.Open);
            StreamReader reader = new StreamReader(stream);
            string[] strings = new string[12];
            int i = 0;
            while (!reader.EndOfStream)
            {
                strings[i] = reader.ReadLine();
                i++;
            }
            reader.Close();
            stream.Close();

            int[,] pattern = ParcePattern(strings);
            game = new Game(pattern);
            BuildAnimationPath();
            initHandlers();
            game.Start();
            DrawPattern();
            DrawRoad();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += TimerTick;
            timer.Start();
        }

        private void DrawPattern()
        {
            foreach (Cell cell in game.Cells)
            {
                if (cell.CellType == Cell.CellTypes.Rocks || cell.CellType == Cell.CellTypes.Ground || cell.CellType == Cell.CellTypes.Nothing)
                {
                    Uri source = new Uri(@"\img\black.jpg", UriKind.Relative);
                    switch (cell.CellType)
                    {
                        case Cell.CellTypes.Ground:
                            source = new Uri(@"\img\grass.png", UriKind.Relative);
                            break;
                        case Cell.CellTypes.Rocks:
                            source = new Uri(@"\img\rock.png", UriKind.Relative);
                            break;
                    }
                    Image imgCell = new Image();
                    BitmapImage imgBitmap = new BitmapImage(source);
                    imgCell.Source = imgBitmap;
                    imgCell.SetValue(Canvas.TopProperty, (double)cell.Y * 50);
                    imgCell.SetValue(Canvas.LeftProperty, (double)cell.X * 50);
                    imgCell.Width = 50.4;
                    imgCell.Height = 50.4;
                    canvasGame.Children.Add(imgCell);
                }
            }
        }

        private void DrawRoad()
        {
            for (int i = 2; i < game.Path.Count; i++)
            {
                Uri source = new Uri(@"\img\black.jpg", UriKind.Relative);
                if (game.Path[i].X > game.Path[i - 1].X && game.Path[i - 1].X > game.Path[i - 2].X && game.Path[i].Y == game.Path[i - 1].Y && game.Path[i - 1].Y == game.Path[i - 2].Y)
                    source = new Uri(@"\img\roadHorizontal.png", UriKind.Relative);
                else
                    if (Math.Abs(game.Path[i].Y - game.Path[i - 1].Y) == 1 && Math.Abs(game.Path[i - 1].Y - game.Path[i - 2].Y) == 1 && game.Path[i].X == game.Path[i - 1].X && game.Path[i - 1].X == game.Path[i - 2].X)
                        source = new Uri(@"\img\roadVertical.png", UriKind.Relative);
                    else
                        if (game.Path[i].Y - game.Path[i - 1].Y == 0 && game.Path[i - 1].Y - game.Path[i - 2].Y == -1 && game.Path[i].X - game.Path[i - 1].X == 1 && game.Path[i - 1].X == game.Path[i - 2].X)
                            source = new Uri(@"\img\cornerBotRight.png", UriKind.Relative);
                        else
                            if (game.Path[i].Y - game.Path[i - 1].Y == 0 && game.Path[i - 1].Y - game.Path[i - 2].Y == 1 && game.Path[i].X - game.Path[i - 1].X == 1 && game.Path[i - 1].X == game.Path[i - 2].X)
                                source = new Uri(@"\img\cornerTopRight.png", UriKind.Relative);
                            else
                                if (game.Path[i].Y - game.Path[i - 1].Y == -1 && game.Path[i - 1].Y - game.Path[i - 2].Y == 0 && game.Path[i].X == game.Path[i - 1].X && game.Path[i - 1].X - game.Path[i - 2].X == 1)
                                    source = new Uri(@"\img\cornerLeftTop.png", UriKind.Relative);
                                else
                                    if (game.Path[i].Y - game.Path[i - 1].Y == 1 && game.Path[i - 1].Y - game.Path[i - 2].Y == 0 && game.Path[i].X == game.Path[i - 1].X && game.Path[i - 1].X - game.Path[i - 2].X == 1)
                                        source = new Uri(@"\img\cornerLeftBot.png", UriKind.Relative);
                Image imgCell = new Image();
                BitmapImage imgBitmap = new BitmapImage(source);
                imgCell.Source = imgBitmap;
                imgCell.SetValue(Canvas.TopProperty, (double)game.Path[i - 1].Y * 50);
                imgCell.SetValue(Canvas.LeftProperty, (double)game.Path[i - 1].X * 50);
                imgCell.Width = 50.4;
                imgCell.Height = 50.4;
                canvasGame.Children.Add(imgCell);
            }
            Image imgCellStart = new Image();
            BitmapImage imgBitmapStart = new BitmapImage(new Uri(@"\img\start.png", UriKind.Relative));
            imgCellStart.Source = imgBitmapStart;
            imgCellStart.SetValue(Canvas.TopProperty, (double)game.Path[0].Y * 50);
            imgCellStart.SetValue(Canvas.LeftProperty, (double)game.Path[0].X * 50);
            imgCellStart.Width = 50.4;
            imgCellStart.Height = 50.4;
            canvasGame.Children.Add(imgCellStart);

            Image imgCellFinish = new Image();
            BitmapImage imgBitmapFinish = new BitmapImage(new Uri(@"\img\finish.png", UriKind.Relative));
            imgCellFinish.Source = imgBitmapFinish;
            imgCellFinish.SetValue(Canvas.TopProperty, (double)game.Path[game.Path.Count - 1].Y * 50);
            imgCellFinish.SetValue(Canvas.LeftProperty, (double)game.Path[game.Path.Count - 1].X * 50);
            imgCellFinish.Width = 50.4;
            imgCellFinish.Height = 50.4;
            canvasGame.Children.Add(imgCellFinish);
        }

        private void canvasGame_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double x = Mouse.GetPosition(canvasGame).X;
            double y = Mouse.GetPosition(canvasGame).Y;
            int X = (int)x / 50;
            int Y = (int)y / 50;
            imgSelectedCell.SetValue(Canvas.TopProperty, (double)Y * 50);
            imgSelectedCell.SetValue(Canvas.LeftProperty, (double)X * 50);
            imgSelectedCell.Visibility = System.Windows.Visibility.Visible;
            SelectedCell = game.Cells[Y, X];
            FillInfo();
        }

        private void FillInfo()
        {
            txtBlockCellInfo.Text = "";
            txtBlockCellInfo.Text += "Cell Type: " + SelectedCell.CellType.ToString();
            if (game.GetTower(SelectedCell) != null)
            {
                Tower tower = game.GetTower(SelectedCell);
                txtBlockCellInfo.Text += "\nTower: " + tower.Name;
                txtBlockCellInfo.Text += "\nLevel: " + tower.Level;
                txtBlockCellInfo.Text += "\nUpgrade Cost: " + tower.UpgradeCost;
                txtBlockCellInfo.Text += "\nSells For: " + tower.Price;
                txtBlockCellInfo.Text += "\nDamage Type: " + tower.DamageType;
                txtBlockCellInfo.Text += "\nDamage: " + tower.Damage;
                txtBlockCellInfo.Text += String.Format("\nSpeed: {0:f1} per sec", (double) 1000 / tower.Speed);
                txtBlockCellInfo.Text += "\nRadius: " + tower.Radius;

                btnUpgradeTower.ToolTip = tower.UpgradeCost.ToString() + " Gold";
                btnSellTower.ToolTip = tower.Price.ToString() + " Gold";
            }
            else
            {
                btnUpgradeTower.ToolTip = null;
                btnSellTower.ToolTip = null;
                txtBlockCellInfo.Text += "\nNo tower";
            }
        }

        private void BuildAnimationPath()
        {
            monstersAnimationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure();
            pFigure.StartPoint = new Point(game.Path[0].X * 50, game.Path[0].Y * 50);
            PolyLineSegment pBezierSegment = new PolyLineSegment();
            for (int i = 1; i < game.Path.Count; i++)
            {
                pBezierSegment.Points.Add(new Point(game.Path[i].X * 50, game.Path[i].Y * 50));
            }
            pBezierSegment.Points.Add(new Point(game.Path[game.Path.Count - 1].X * 50 + 50, game.Path[game.Path.Count - 1].Y * 50));
            pFigure.Segments.Add(pBezierSegment);
            monstersAnimationPath.Figures.Add(pFigure);
            monstersAnimationPath.Freeze();
        }

        private void btnAddTower_Click(object sender, RoutedEventArgs e)
        {
            btnAddTower.ContextMenu.IsOpen = true;
        }

        private void menuItemAddCommonTower_Click(object sender, RoutedEventArgs e)
        {
            AddTower(new CommonTower());
        }

        private void menuItemAddHardTower_Click(object sender, RoutedEventArgs e)
        {
            AddTower(new HardTower());
        }

        private void menuItemAddNapalmTower_Click(object sender, RoutedEventArgs e)
        {
            AddTower(new NapalmTower());
        }

        private void AddTower(Tower tower)
        {
            if (SelectedCell != null && SelectedCell.CellType == Cell.CellTypes.Ground)
                game.AddTower(SelectedCell, tower);
        }

        private void OnTowerAddedHandler(Tower tower)
        {
            Uri source = new Uri(@"\img\Towers\slowingTower.png", UriKind.Relative); ;
            if (tower is CommonTower)
                source = new Uri(@"\img\Towers\commonTower.png", UriKind.Relative);
            else
                if (tower is HardTower)
                    source = new Uri(@"\img\Towers\hardTower.png", UriKind.Relative);
                else
                    if (tower is NapalmTower)
                        source = new Uri(@"\img\Towers\napalmTower.png", UriKind.Relative);

            Image imgTower = new Image();
            BitmapImage imgBitmap = new BitmapImage(source);
            imgTower.Source = imgBitmap;

            imgTower.SetValue(Canvas.TopProperty, (double)tower.Position.Y * 50);
            imgTower.SetValue(Canvas.LeftProperty, (double)tower.Position.X * 50);
            imgTower.SetValue(Canvas.ZIndexProperty, 1);
            imgTower.Width = 50.4;
            imgTower.Height = 50.4;
            canvasGame.Children.Add(imgTower);
            tower.TowerImage = imgTower;
            FillInfo();
        }

        private void btnSellTower_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCell != null)
                game.SellTower(SelectedCell);
        }

        private void OnTowerSoldHandler(Tower tower)
        {
            canvasGame.Children.Remove(tower.TowerImage);
            FillInfo();
        }

        private void btnUpgradeTower_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCell != null)
                game.UpgradeTower(SelectedCell);
        }

        private void OnTowerUpgradedHandler(Tower tower)
        {
            FillInfo();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            game.TimerTick();
        }

        private void OnMonsterSpawnedHandler(Monster monster)
        {
            Uri source = new Uri(@"\img\Monsters\bossMonster.png", UriKind.Relative); ;
            if (monster is StandardMonster)
                source = new Uri(@"\img\Monsters\standardMonster.png", UriKind.Relative);
            else
                if (monster is FastWeakMonster)
                    source = new Uri(@"\img\Monsters\fastWeakMonster.png", UriKind.Relative);
                else
                    if (monster is SlowToughMonster)
                        source = new Uri(@"\img\Monsters\slowToughMonster.png", UriKind.Relative);

            Image imgMonster = new Image();
            BitmapImage imgBitmap = new BitmapImage(source);
            imgMonster.Source = imgBitmap;

            imgMonster.SetValue(Canvas.TopProperty, (double)monster.Position.Y * 50);
            imgMonster.SetValue(Canvas.LeftProperty, (double)monster.Position.X * 50);
            imgMonster.SetValue(Canvas.ZIndexProperty, 1);
            imgMonster.Width = 50.4;
            imgMonster.Height = 50.4;
            canvasGame.Children.Add(imgMonster);
            monster.MonsterImage = imgMonster;



            DoubleAnimationUsingPath daPath = new DoubleAnimationUsingPath();
            daPath.Duration = TimeSpan.FromSeconds(monster.Speed * game.Path.Count);
            daPath.PathGeometry = monstersAnimationPath;
            daPath.Source = PathAnimationSource.X;
            imgMonster.BeginAnimation(Canvas.LeftProperty, daPath);


            daPath = new DoubleAnimationUsingPath();
            daPath.Duration = TimeSpan.FromSeconds(monster.Speed * game.Path.Count);
            daPath.PathGeometry = monstersAnimationPath;
            daPath.Source = PathAnimationSource.Y;
            imgMonster.BeginAnimation(Canvas.TopProperty, daPath);
        }

        private void OnMonsterKilledHandler(Monster monster)
        {
            canvasGame.Children.Remove(monster.MonsterImage);
        }

        private void OnTowerShotHandler(Tower tower, Cell cell)
        {
            Rectangle r = new Rectangle();
            r.Stroke = Brushes.Red;
            r.StrokeThickness = 2;
            r.Width = 5;
            r.Height = 5;
            r.SetValue(Canvas.TopProperty, (double)tower.Position.Y * 50 + 25);
            r.SetValue(Canvas.LeftProperty, (double)tower.Position.X * 50 + 25);
            canvasGame.Children.Add(r);
            DoubleAnimation xanim = new DoubleAnimation();
            xanim.From = (double)tower.Position.X * 50 + 25;
            xanim.To = (double)cell.X * 50 + 25;
            xanim.Duration = TimeSpan.FromMilliseconds(50);
            DoubleAnimation yanim = new DoubleAnimation();
            yanim.From = (double)tower.Position.Y * 50 + 25;
            yanim.To = (double)cell.Y * 50 + 25;
            yanim.Completed += (sender, e) =>
            {
                canvasGame.Children.Remove(r);

                Ellipse ellipseExplosion = new Ellipse();
                ellipseExplosion.Opacity = 0.5;
                ellipseExplosion.Height = 30;
                ellipseExplosion.Width = 30;
                ellipseExplosion.Stroke = Brushes.Yellow;
                ellipseExplosion.Fill = Brushes.Red;
                ellipseExplosion.SetValue(Canvas.TopProperty, (double)cell.Y * 50 + 10);
                ellipseExplosion.SetValue(Canvas.LeftProperty, (double)cell.X * 50 + 10);
                canvasGame.Children.Add(ellipseExplosion);
                DoubleAnimation strokeAnimation = new DoubleAnimation();
                DoubleAnimation opacityAnimation = new DoubleAnimation();

                strokeAnimation.From = 0;
                strokeAnimation.To = 15;
                strokeAnimation.Duration = TimeSpan.FromMilliseconds(150);
                strokeAnimation.Completed += (sender1, e1) => canvasGame.Children.Remove(ellipseExplosion);
                opacityAnimation.From = 0.5;
                opacityAnimation.To = 0;
                opacityAnimation.Duration = TimeSpan.FromMilliseconds(150);
                ellipseExplosion.BeginAnimation(Ellipse.OpacityProperty, opacityAnimation);

                ellipseExplosion.BeginAnimation(Ellipse.StrokeThicknessProperty, strokeAnimation);
            };
            yanim.Duration = TimeSpan.FromMilliseconds(50);

            r.BeginAnimation(Canvas.TopProperty, yanim);
            r.BeginAnimation(Canvas.LeftProperty, xanim);

        }

        private void OnNapamTowerShotHandler(Tower tower, Cell cell, List<Cell> damagedCells)
        {
            Rectangle r = new Rectangle();
            r.Stroke = Brushes.Red;
            r.StrokeThickness = 2;
            r.Width = 5;
            r.Height = 5;
            r.SetValue(Canvas.TopProperty, (double)tower.Position.Y * 50 + 25);
            r.SetValue(Canvas.LeftProperty, (double)tower.Position.X * 50 + 25);
            canvasGame.Children.Add(r);
            DoubleAnimation xanim = new DoubleAnimation();
            xanim.From = (double)tower.Position.X * 50 + 25;
            xanim.To = (double)cell.X * 50 + 25;
            xanim.Duration = TimeSpan.FromMilliseconds(50);
            DoubleAnimation yanim = new DoubleAnimation();
            yanim.From = (double)tower.Position.Y * 50 + 25;
            yanim.To = (double)cell.Y * 50 + 25;
            yanim.Completed += (sender, e) =>
            {
                canvasGame.Children.Remove(r);
                foreach (Cell dCell in damagedCells)
                {
                    Ellipse ellipseExplosion = new Ellipse();
                    ellipseExplosion.Opacity = 0.5;
                    ellipseExplosion.Height = 30;
                    ellipseExplosion.Width = 30;
                    ellipseExplosion.Stroke = Brushes.Yellow;
                    ellipseExplosion.Fill = Brushes.Red;
                    ellipseExplosion.SetValue(Canvas.TopProperty, (double)dCell.Y * 50 + 10);
                    ellipseExplosion.SetValue(Canvas.LeftProperty, (double)dCell.X * 50 + 10);
                    canvasGame.Children.Add(ellipseExplosion);
                    DoubleAnimation strokeAnimation = new DoubleAnimation();
                    DoubleAnimation opacityAnimation = new DoubleAnimation();

                    strokeAnimation.From = 0;
                    strokeAnimation.To = 15;
                    strokeAnimation.Duration = TimeSpan.FromMilliseconds(150);
                    strokeAnimation.Completed += (sender1, e1) => canvasGame.Children.Remove(ellipseExplosion);
                    opacityAnimation.From = 0.5;
                    opacityAnimation.To = 0;
                    opacityAnimation.Duration = TimeSpan.FromMilliseconds(150);

                    ellipseExplosion.BeginAnimation(Ellipse.StrokeThicknessProperty, strokeAnimation);
                    ellipseExplosion.BeginAnimation(Ellipse.OpacityProperty, opacityAnimation);
                }
            };
            yanim.Duration = TimeSpan.FromMilliseconds(50);

            r.BeginAnimation(Canvas.TopProperty, yanim);
            r.BeginAnimation(Canvas.LeftProperty, xanim);

        }

        private void OnWaveChangedHandler(int wave)
        {
            txtBlockWave.Text = "Wave: " + wave.ToString();
        }

        private void OnGoldChangedHandler(int gold)
        {
            txtBlockGold.Text = "Gold: " + gold.ToString();
        }

        private void OnLivesChangedHandler(int lives)
        {
            txtBlockLives.Text = "Lives: " + lives.ToString();
        }

        private void OnWinHandler()
        {
            MessageBox.Show("Congratulations! You have finished the last wave", "Victory");
            this.Close();
        }

        private void OnGameOverHandler(int wave)
        {
            MessageBox.Show("You have been holding them for " + wave.ToString() + " Waves","Defeat");
            this.Close();
        }

    }
}