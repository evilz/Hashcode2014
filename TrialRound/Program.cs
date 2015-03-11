using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrialRound.Extentions;
using TrialRound.Model;

namespace TrialRound
{
    public static class Program
    {
        public static void Main()
        {
            const string FILE_NAME = @"Samples/doodle.txt";

            Cell[,] matrix;
            Size gridSize;

            StringBuilder sbOut = new StringBuilder();

            using (var inputStream = File.OpenRead(FILE_NAME))
            {
                var reader = new StreamReader(inputStream);

                // parse first line to get Matrix Size
                gridSize = GetGridSize(reader);

                // Init grid with cell with basic score
                matrix = reader.CreateMatrix<Cell, char>(gridSize, CellFactory);

                reader.Close();
                inputStream.Close();
            }


            // Find best score for each cell
            int[] current = {0};
            var firstThread = 0;
            var max = gridSize.Width * gridSize.Height;

            Console.WriteLine();
            Console.WriteLine("####  LOOK FOR BEST SQUARE FOR ALL CELL  #####");
            Console.WriteLine();

            Parallel.For(0, gridSize.Height, (r, p) =>
            {
                for (var c = 0; c < gridSize.Width; c++)
                {
                    var cell = matrix[r, c];
                    cell.ComputeCellScoring();
                    current[0]++;

                    if (firstThread == 0)
                    {
                        firstThread = Thread.CurrentThread.ManagedThreadId;
                    }
                    if (firstThread == Thread.CurrentThread.ManagedThreadId)
                        ConsoleExtensions.ProgressBar(current[0], max, 70);
                }
            }
            );
            ConsoleExtensions.ProgressBar(max, max, 70);

            // SAVE First pass !
            //matrix.Save(Path.GetFullPath("first-pass.bin"));

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("####  START PRINT  #####");
            Console.WriteLine();

            
           var sorted = matrix
               .ToEnumerable()
               .Where(c => c.BestScore.Score >= 1)
               .OrderByDescending(c=> c.BestScore.Size)
               .ThenByDescending(c => c.BestScore.Score)
               .ToList();

            var allCellCount = sorted.Count;
          
            int instructions = 0;

            while (sorted.Any())
            {
                var first = sorted.First();

                // TROP LENT !!!
                //var currentScore = first.BestScore.Score;

                //first.BestScore = null;
                //first.ComputeCellScoring();

                //if (Math.Abs(currentScore - first.BestScore.Score) > 0.1)
                //{
                //    sorted = sorted
                //        .Where(c => c.BestScore.Score >= 1)
                //        .OrderByDescending(c => c.BestScore.Size)
                //        .ThenByDescending(c => c.BestScore.Score).ToList();
               
                //    continue;
                //}

                var bestScore = first.BestScore;
                if (bestScore.Score < 1 && bestScore.Size == 0)
                {
                   // first.HasBeenProcessed = true;
                   // sorted.RemoveAt(0);
                    continue;
                }

                bestScore.PRINTSQ(sbOut);
                instructions++;

                if(instructions%100 == 0)
                    ExportMatrixtoBitmap(matrix, instructions);

                var impatedCells = bestScore.CellInSquare
                    .SelectMany(c => c.BestScore.CellInSquare).Distinct().ToList(); //.OrderByDescending(c=>c.BestScore.Score);


                Parallel.ForEach(impatedCells, cell =>
                {
                    cell.BestScore = null;
                    cell.ComputeCellScoring();
                });
               
                sorted.RemoveAt(0);
                sorted = sorted
                        .Where(c => c.BestScore.Score >= 1)
                        .OrderByDescending(c=> c.BestScore.Size)
                        .ThenByDescending(c => c.BestScore.Score).ToList();
               
                ConsoleExtensions.ProgressBar(allCellCount - sorted.Count, allCellCount, 70, ConsoleColorSet.Blue);
            }

            var cellToDelete = matrix.ToEnumerable().Where(c => c.NeedToBeClean);
            foreach (var cell in cellToDelete)
            {
                cell.ERASECELL(sbOut);
                instructions++;
            }

            using (var sw = File.CreateText("instruction.txt"))
            {
                sw.WriteLine(instructions);
                sw.Write(sbOut.ToString());
            }
           

            Console.WriteLine("nb instrauctions : " + instructions);
            Console.WriteLine(Process.GetCurrentProcess().TotalProcessorTime.Milliseconds);
            Console.WriteLine(Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024 + "MB in RAM memory");
            Console.WriteLine(Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 + "MB in RAM memory");

            Console.WriteLine(Process.GetCurrentProcess().VirtualMemorySize64 / 1024 / 1024 + "MB in RAM memory");


            Console.ReadLine();
        }

        private static void ExportMatrixtoBitmap(Cell[,] matrix, int nbInstruction)
        {
            var outputFile = nbInstruction.ToString("000000000") + ".bmp";

            matrix.ToBitmap(
                outputFile,
                ColorExport,
                false);
        }

        private static Color ColorExport(Cell cell)
        {

            if (cell.NeedToBeClean)
                return Color.Blue;

            if (cell.HasBeenProcessed)
                return Color.Red;

            if (cell.HasBeenPrint)
                return Color.Green;

            if (cell.Value)
                return Color.Black;
            return Color.White;
        }


        private static Cell CellFactory(int y, int x, char val, Cell[,] grid)
        {
            return new Cell
            {
                Matrix = grid,
                Value = val != '.',
                Position = new Point(x, y)

            };
        }

        private static Size GetGridSize(StreamReader reader)
        {
            var gridSize = reader.ExtractValues<int>().ToArray();
            var size = new Size(gridSize[1], gridSize[0]);

            // Logger.Debug(string.Format("Width {0}, Height {1}", size.Width, size.Height));
            return size;
        }


    }
}
