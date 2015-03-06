using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace TrialRound
{
    public class TileInfo
    {
        public int Size { get; set; }
        public float Score { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

    }

    public class Program
    {
        public static IEnumerable<string> ExtractLine(StringReader reader)
        {
            while (true)
            {
                var line = reader.ReadLine();

                if (line == null)
                    yield break;

                yield return line;
            }
        }

        public static TileInfo GetWeight(bool[,] grid, int r, int c)
        {
            var currentWeightNode = new LinkedListNode<TileInfo>(new TileInfo()
            {
                X = c,
                Y = r,
                Score = grid[r,c] ? 1.0F : 0,
                Size = 0
            });

            var previous = new LinkedList<TileInfo>();
            previous.AddFirst(currentWeightNode);


            for (int size = 1; true; size++)
            {
                var bottom = r + size;
                var top = r - size;
                var left = c - size;
                var right = c + size;

                //var topLeftCorner = new Point(
                //    left,
                //    top
                //);
                //var topRightCorner = new Point(
                //    right,
                //    top
                //);
                //var bottomLeftCorner = new Point(
                //    left,
                //    bottom
                //);
                //var bottomRightCorner = new Point(
                //    right,
                //    bottom
                //);
                Console.WriteLine("Analyse de la size {0}, ({1},{2})", size, r, c);
                
                if (top < 0 || left < 0 || bottom >= grid.GetLength(0) || right >= grid.GetLength(1))
                    break;

                //Console.WriteLine("Analyse du point {0}", topLeftCorner);
                //Console.WriteLine("Analyse du point {0}", topRightCorner);
                //Console.WriteLine("Analyse du point {0}", bottomLeftCorner);
                //Console.WriteLine("Analyse du point {0}", bottomRightCorner);

                var countWhite = 0;

                for (int i = top; i <= r + size; ++i)
                {
                    for (int j = left; j <= c + size; ++j)
                    {
                        if (grid[i, j] == false)
                        {
                            countWhite++;
                        }

                        //Console.WriteLine("Analyse de {0},{1}", i, j);
                    }
                }
                
                float score = (size*2+1)*(size*2+1) / (float)(1 + countWhite);

                //Console.WriteLine("Score {0} countWhite {1}", score, countWhite);

                var newWeight = new TileInfo
                {
                     X = c,
                Y = r,
                    Size = size,
                    Score = score
                };

                previous.AddAfter(currentWeightNode, newWeight);

                if (currentWeightNode.Next.Value.Score < currentWeightNode.Value.Score)
                {
                    break;
                }

                currentWeightNode = currentWeightNode.Next;
            }

            var t = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Best Score {0} in  size of {1}", currentWeightNode.Value.Score, currentWeightNode.Value.Size);
            Console.ForegroundColor = t;

            return currentWeightNode.Value;
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
            var grid = new bool[gridSize[0], gridSize[1]];


            var gridFromFile = (from i in ExtractLine(reader)
                                select i.ToArray())
                                .ToArray();

            for (var i = 0; i < gridSize[0]; ++i)
                for (var j = 0; j < gridSize[1]; ++j)
                    grid[i, j] = gridFromFile[i][j] == '.' ? false : true;

            List<TileInfo> allWeights = new List<TileInfo>();
            // read from top-left , for each 'true'  call getWeight
            // put Weight in List

            for (int r = 0; r < gridSize[0]; r++)
            {
                for (int c = 0; c < gridSize[1]; c++)
                {
                    var weight = GetWeight(grid, r, c);
                    allWeights.Add(weight);
                }
            }

           
            // sort list by weight 
            var sorted = allWeights.OrderByDescending(w => w.Score).ToList();
            
            // foreach createInstruction >  list.first() , 
            while (sorted.Any())
           {
                var first = sorted.FirstOrDefault();
                if (first != null)
                {
                    if (first.Score == 0 && first.Size == 0)
                    {
                        sorted.RemoveAt(0);
                        continue;
                    }

                    // IL FAUT LA LISTE DES CELL A DELETE !!!!
                    Console.WriteLine("PRINTSQ {0} {1} {2} - SCORE : {3}", first.X, first.Y, first.Size, first.Score);

                    //    var bottom = first.Y + first.Size;
                    //    var top = first.Y - first.Size;
                    //    var left = first.X - first.Size;
                    //    var right = first.X + first.Size;

                    //      for (int i = top; i <= r + size; ++i)
                    //{
                    //    for (int j = left; j <= c + size; ++j)
                    //    {

                    //   // remove All Cell Impacted
                    //}
                    sorted.RemoveAt(0);
                }
            
        }
            Console.WriteLine(Process.GetCurrentProcess().TotalProcessorTime.Milliseconds + "ms");
            Console.WriteLine(Process.GetCurrentProcess().WorkingSet64 /1024 /1024 + "MB in RAM memory");
            Console.WriteLine(Process.GetCurrentProcess().PrivateMemorySize64 /1024 /1024 + "MB in RAM memory");

            Console.WriteLine(Process.GetCurrentProcess().VirtualMemorySize64 / 1024 / 1024 + "MB in RAM memory");

            Console.ReadLine();
        }

        private static object GetGridSize(StreamReader reader)
        {
            var gridSize = reader.ReadLineSplitAndParse<int>();

            return new Size
            var firstLine = reader.ReadLine();
            var gridSize = firstLine.Split(' ').Select(s => Convert.ToInt32(s)).ToArray();
            return gridSize;
        }
    }
}
