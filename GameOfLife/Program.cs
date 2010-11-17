using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SharpBag;

namespace GameOfLife
{
    class Program
    {
        static int WidthM1;
        static int HeightM1;
        
        static bool[,] Board;
        static bool[,] TempBoard;

        static int MeasureGenerations = 23;

        static void Main(string[] args)
        {
            Action createBoard = () =>
            {
                //ReadBoardFromFile("Board.txt");
                GenerateRandomBoard(300, 300, 50);
            };

            createBoard();

            WidthM1 = Board.GetLength(0) - 1;
            HeightM1 = Board.GetLength(1) - 1;

            if (MeasureGenerations > 0)
            {
                int tests = 10;
                long total = 0;
                long min = -1;
                long max = -1;

                for (int j = 0; j < tests; j++)
                {
                    long time = Utils.ExecutionTime(() =>
                    {
                        for (int i = 0; i < MeasureGenerations; i++)
                        {
                            Compute();
                        }
                    });

                    if (min == -1 || time < min) min = time;
                    if (max == -1 || time > max) max = time;

                    total += time;
                    Console.WriteLine("Time: " + time);
                }

                Console.WriteLine("Average: " + (total / tests));
                Console.WriteLine("Minimum: " + min);
                Console.WriteLine("Maximum: " + max);
                Console.ReadLine();
            }
            else while (true)
            {
                Draw();
                Compute();
                if (Console.ReadLine().StartsWith("q"))
                {
                    return;
                }
            }
        }

        static void GenerateRandomBoard(int width, int height, int lifeChances)
        {
            Board = new bool[height, width];
            Random rand = new Random();

            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    int r = rand.Next(0, 100);
                    Board[x, y] = r < lifeChances;
                }
            }
        }

        static void ReadBoardFromFile(string file)
        {
            bool[][] lines = (from l in Utils.ReadLinesFromFile(file)
                              let line = l.Replace("#", "")
                              where line.Length != 0
                              select line.ToCharArray().Select(c => c != ' ').ToArray()).ToArray();

            Board = new bool[lines.Length, lines.Max(l => l.Length)];

            for (int x = 0; x < lines.Length; x++)
            {
                for (int y = 0; y < lines[x].Length; y++)
                {
                    Board[x, y] = lines[x][y];
                }
            }
        }

        static void Compute()
        {
            TempBoard = (bool[,])Board.Clone();

            (from X in 0.To(WidthM1)
             from Y in 0.To(HeightM1)
             select new { X, Y }).AsParallel().ForEach(co =>
             {
                 int aliveAround = AliveAroundCell(co.X, co.Y);
                 bool currentCell = Board[co.X, co.Y];

                 if (currentCell && (aliveAround < 2 || aliveAround > 3)) TempBoard[co.X, co.Y] = false;
                 else if (!currentCell && aliveAround == 3) TempBoard[co.X, co.Y] = true;
             });

            Board = TempBoard;
        }

        static int AliveAroundCell(int x, int y)
        {
            int alive = 0;

            if (Alive(x - 1, y - 1)) alive++;
            if (Alive(x - 1, y    )) alive++;
            if (Alive(x - 1, y + 1)) alive++;

            if (Alive(x    , y - 1)) alive++;
            if (Alive(x    , y + 1)) alive++;

            if (Alive(x + 1, y - 1)) alive++;
            if (Alive(x + 1, y    )) alive++;
            if (Alive(x + 1, y + 1)) alive++;

            return alive;
        }

        static bool Alive(int x, int y)
        {
            if (x < 0 || y < 0 || x > WidthM1 || y > HeightM1) return false;

            return Board[x, y];
        }

        static void Draw()
        {
            Console.Clear();

            for (int x = 0; x <= WidthM1; x++)
            {
                for (int y = 0; y <= HeightM1; y++)
                {
                    if (Board[x, y]) Console.Write("#");
                    else Console.Write(" ");
                }

                Console.WriteLine();
            }
        }
    }
}