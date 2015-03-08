using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LibLog.Logging;
using LibLog.Logging.LogProviders;

namespace TrialRound
{
    public class Cell
    {
        public Cell()
        {
            Scorings = new List<CellScoring>();
        }
        public int X { get; set; }
        public int Y { get; set; }

        public Cell[,] Matrix { get; set; }

        public bool Value { get; set; }

        public List<CellScoring> Scorings
        {
            get;
            set;
        }


        public List<CellScoring> SortedScorings
        {
            get { return Scorings.OrderByDescending(s => s.Score).ToList(); }
        }

    }
    public struct CellScoring
    {
        public Cell Cell { get; set; }

        public int Size { get; set; }

        public float Score
        {
            get
            {
                // check range
                if (TopLeft.Y < 0 || TopLeft.X < 0 || BottomRight.Y >= Cell.Matrix.GetLength(0) ||
                    BottomRight.X >= Cell.Matrix.GetLength(1))
                    return -1;

                if (Size == 0 && !Cell.Value)
                {
                    return 0;
                }

                var totalCell = (Size*2 + 1)*(Size*2 + 1);
                var cellToClean = CellToClear().Count();

                if (totalCell/2 < cellToClean)
                {
                    return 0;
                }

                var computed = (Size * 2 + 1) * (Size * 2 + 1) / (float)(1 + CellToClear().Count());
                return computed;
            }
        }

        public IEnumerable<Cell> CellToClear()
        {

            for (int i = TopLeft.Y; i <= BottomRight.Y && i < Cell.Matrix.GetLength(1) ; ++i)
            {
                for (int j = TopLeft.X; j <= BottomRight.X && j < Cell.Matrix.GetLength(0); ++j)
                {
                    if (Cell.Matrix[j, i].Value == false)
                    {
                        yield return Cell.Matrix[j, i];
                    }
                }
            }
        }

        public Point TopLeft { get { return new Point(Cell.X - Size, Cell.Y - Size); } }
        public Point BottomRight { get { return new Point(Cell.X + Size, Cell.Y + Size); } }

    }

    public class Program
    {

        private static readonly ILog Logger = LogProvider.For<Program>();

        private static Cell[,] matrix;

        private static List<Cell> CellToDelete = new List<Cell>();

        public static CellScoring ComputeCellScoringForSize(Cell cell, int size)
        {
            var scoring = new CellScoring
            {
                Cell = cell,
                Size = size
            };
            return scoring;
        }

        public static CellScoring ComputeCellScoring(Point cellIndex)
        {
            var cell = matrix[cellIndex.Y, cellIndex.X];
            var currentScoringNode = matrix[cellIndex.Y, cellIndex.X].Scorings.Last();

            for (int size = 1; true; size++)
            {

                Console.WriteLine("Analyse de la size {0}, ({1},{2})", size, cellIndex.Y, cellIndex.X);

                var newScoring = ComputeCellScoringForSize(cell, size);

                if (currentScoringNode.Score >= newScoring.Score)
                {
                    break;
                }

                currentScoringNode = newScoring;
                cell.Scorings.Add(newScoring);
            }

            //var t = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine("Best Score {0} in  size of {1}", currentWeightNode.Value.Score, currentWeightNode.Value.Size);
            //Console.ForegroundColor = t;

            return currentScoringNode;
        }


        public static void Main()
        {
            var input = @"5 7
....#..
..###..
..#.#..
..###..
..#....
";
            // Create Stream and stream reader
            // REM : use stream to convert to FileStream
            var inputStream = input.AsStream();

           // var inputStream = File.OpenRead(@"C:\Users\Vincent\Downloads\doodle.txt");

            var reader = new StreamReader(inputStream);

            // parse first line
            var gridSize = GetGridSize(reader);

            //Console.WriteLine(gridSize[0]);
            //Console.WriteLine(gridSize[1]);



            // create table  bool[,]
            matrix = reader.CreateMatrix<Cell, char>(gridSize, Initializer);

            List<CellScoring> bestScores = new List<CellScoring>();
            // read from top-left , for each 'true'  call getWeight
            // put Weight in List


            for (int r = 0; r < gridSize.Height; r++)
            {
                for (int c = 0; c < gridSize.Width; c++)
                {
                    var cellscore = ComputeCellScoring(new Point(c, r));
                    bestScores.Add(cellscore);
                }
            }


            // sort list by weight 
            var sorted = bestScores.Where(s => s.Score >= 1).OrderByDescending(w => w.Score).ToList();

            // foreach createInstruction >  list.first() , 
            var instructions = 0;
            while (sorted.Any())
            {
                var first = sorted.FirstOrDefault();

                if (first.Score == 0 && first.Size == 0)
                {
                    sorted.RemoveAt(0);
                    continue;
                }

                // IL FAUT LA LISTE DES CELL A DELETE !!!!
                Console.WriteLine("PRINTSQ {0} {1} {2} - SCORE : {3}", first.Cell.Y, first.Cell.X, first.Size, first.Score);
                instructions ++;

                CellToDelete.AddRange(first.CellToClear());
                SetValueToFalse(first);

                sorted.RemoveAt(0);

                var cellToDraw = sorted.Select(s => s.Cell);
                bestScores = cellToDraw.Select(c => c.SortedScorings.First()).ToList();
                sorted = bestScores.OrderByDescending(s => s.Score).ToList();

            }

            foreach (var tileScoring in CellToDelete)
            {
                Console.WriteLine("ERASECELL {0} {1}", tileScoring.Y, tileScoring.X);
                instructions ++;
            }

            Console.WriteLine("nb instrauctions : " + instructions);
            Console.WriteLine(Process.GetCurrentProcess().TotalProcessorTime.Milliseconds + "ms");
            Console.WriteLine(Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024 + "MB in RAM memory");
            Console.WriteLine(Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 + "MB in RAM memory");

            Console.WriteLine(Process.GetCurrentProcess().VirtualMemorySize64 / 1024 / 1024 + "MB in RAM memory");

            Console.ReadLine();
        }

        private static void SetValueToFalse(CellScoring scoring)
        {
            //DumpMatrix();
            for (int i = scoring.TopLeft.Y; i <= scoring.BottomRight.Y; i++)
            {
                for (int j = scoring.TopLeft.X; j <= scoring.BottomRight.X; j++)
                {
                    matrix[i, j].Value = false;
                }
            }
            Console.WriteLine();
            //DumpMatrix();
        }

        //private static void DumpMatrix()
        //{
        //    for (int r = 0; r < matrix.GetLength(0); r++)
        //    {
        //        for (int c = 0; c < matrix.GetLength(1); c++)
        //        {
        //            var val = matrix[r, c].Value ? "#" : ".";
        //            Console.Write(val);
        //        }
        //        Console.WriteLine();
        //    }
        //}

        private static Cell Initializer(int x, int y, char val, Cell[,] matrix)
        {
            var value = val != '.';
            var cell = new Cell
            {
                Matrix = matrix,
                Value = value,
                X = x,
                Y = y,


            };
            cell.Scorings.Add(
                new CellScoring
                {
                    Size = 0,
                    Cell = cell
                });
            return cell;
        }

        private static Size GetGridSize(StreamReader reader)
        {
            var gridSize = reader.ReadLineSplitAndParse<int>().ToArray();
            var size = new Size(gridSize[1], gridSize[0]);

            Logger.Debug(string.Format("Width {0}, Height {1}", size.Width, size.Height));
            return size;
        }
    }
}
