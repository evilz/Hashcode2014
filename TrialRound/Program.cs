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
    public struct TileScoring
    {
        public TileScoring[,] Matrix { get; set; }
        public int Size { get; set; }

        public float Score
        {
            get
            {
                // check range
                if (TopLeft.Y < 0 || TopLeft.X < 0 || BottomRight.Y >= Matrix.GetLength(0) ||
                    BottomRight.X >= Matrix.GetLength(1))
                    return -1;

                if (Size == 0 && !Value)
                {
                    return 0;
                }
                var computed = (Size * 2 + 1) * (Size * 2 + 1) / (float)(1 + CellToClear().Count());
                return computed;
            }
        }

        public int X { get; set; }
        public int Y { get; set; }
        public bool Value { get; set; }

        public IEnumerable<TileScoring> CellToClear()
        {

            for (int i = TopLeft.Y; i <= BottomRight.Y; ++i)
            {
                for (int j = TopLeft.X; j <= BottomRight.X; ++j)
                {
                    if (Matrix[i, j].Value == false)
                    {
                        yield return Matrix[i, j];
                    }
                }
            }
        } 

       public Point TopLeft{ get { return new Point(X - Size, Y - Size); }}
       public Point BottomRight{ get { return new Point(X + Size, Y + Size); }}

        public TileScoring Clone()
        {
            return new TileScoring
            {
                Size = this.Size,
                X = X,
                Y = Y,
                Value = Value,
                Matrix = Matrix
            };
        }
    }

    public class Program
    {

        private static readonly ILog Logger = LogProvider.For<Program>();

        private static TileScoring[,] matrix;

        private static List<TileScoring> CellToDelete =  new List<TileScoring>();

        public static TileScoring ComputeTileScoringForSize(Point cellIndex, int size)
        {
            var scoring = matrix[cellIndex.Y, cellIndex.X].Clone();
            scoring.Size = size;
            return scoring;
        }

        public static TileScoring ComputeTileScoring(Point cellIndex)
        {
            var currentScoringNode = matrix[cellIndex.Y, cellIndex.X];
            

            for (int size = 1; true; size++)
            {

                Console.WriteLine("Analyse de la size {0}, ({1},{2})", size, cellIndex.Y, cellIndex.X);

                var newScoring = ComputeTileScoringForSize(cellIndex, size);

                if (currentScoringNode.Score >= newScoring.Score)
                {
                    break;
                }

                currentScoringNode = newScoring;
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
            var reader = new StreamReader(inputStream);

            // parse first line
            var gridSize = GetGridSize(reader);
            
            //Console.WriteLine(gridSize[0]);
            //Console.WriteLine(gridSize[1]);



            // create table  bool[,]
            matrix = reader.CreateMatrix<TileScoring,char>(gridSize, Initializer  );

            List<TileScoring> allWeights = new List<TileScoring>();
            // read from top-left , for each 'true'  call getWeight
            // put Weight in List
           
          
            for (int r = 0; r < gridSize.Height; r++)
            {
                for (int c = 0; c < gridSize.Width; c++)
                {
                    var weight = ComputeTileScoring(new Point(c,r));
                    allWeights.Add(weight);
                }
            }

           
            // sort list by weight 
            var sorted = allWeights.Where(s => s.Score >= 1).OrderByDescending(w => w.Score).ToList();
            
            // foreach createInstruction >  list.first() , 
            while (sorted.Any())
           {
                var first = sorted.FirstOrDefault();
                
                    if (first.Score == 0 && first.Size == 0)
                    {
                        sorted.RemoveAt(0);
                        continue;
                    }

                    // IL FAUT LA LISTE DES CELL A DELETE !!!!
                    Console.WriteLine("PRINTSQ {0} {1} {2} - SCORE : {3}", first.Y, first.X, first.Size, first.Score);

                    CellToDelete.AddRange(first.CellToClear());
                    SetValueToFalse(first);
                    
                    sorted.RemoveAt(0);


                    sorted = sorted.Where(s => s.Score >= 1).OrderByDescending(w => w.Score).ToList();
              

           }

            foreach (var tileScoring in CellToDelete)
            {
                Console.WriteLine("ERASECELL {0} {1}", tileScoring.Y, tileScoring.X);
            }

            Console.WriteLine(Process.GetCurrentProcess().TotalProcessorTime.Milliseconds + "ms");
            Console.WriteLine(Process.GetCurrentProcess().WorkingSet64 /1024 /1024 + "MB in RAM memory");
            Console.WriteLine(Process.GetCurrentProcess().PrivateMemorySize64 /1024 /1024 + "MB in RAM memory");

            Console.WriteLine(Process.GetCurrentProcess().VirtualMemorySize64 / 1024 / 1024 + "MB in RAM memory");

            Console.ReadLine();
        }

        private static void SetValueToFalse(TileScoring scoring)
        {
            DumpMatrix();
            for (int i = scoring.TopLeft.Y ; i <= scoring.BottomRight.Y; i++)
            {
                for (int j = scoring.TopLeft.X; j <= scoring.BottomRight.X; j++)
                {
                    matrix[i, j].Value = false;
                }
            }
            Console.WriteLine();
            DumpMatrix();
        }

        private static void DumpMatrix()
        {
            for (int r = 0; r < matrix.GetLength(0); r++)
            {
                for (int c = 0; c < matrix.GetLength(1); c++)
                {
                    var val = matrix[r, c].Value ? "#" : ".";
                    Console.Write(val);
                }
                Console.WriteLine();
            }
        }

        private static TileScoring Initializer(int x, int y, char val, TileScoring[,] matrix)
        {
            var value = val != '.';
            return new TileScoring
            {
                Size = 0,
                Value = value,
                X = x,
                Y = y,
                Matrix = matrix
            };

        }

        private static Size GetGridSize(StreamReader reader)
        {
            var gridSize = reader.ReadLineSplitAndParse<int>().ToArray();
            var size = new Size(gridSize[1], gridSize[0]);
            
            Logger.Debug(string.Format("Width {0}, Height {1}",size.Width, size.Height));
            return size;
        }
    }
}
