using System.Drawing;
using System.Text;
using Newtonsoft.Json;

namespace TrialRound.Model
{
    public class Cell
    {
        public Point Position { get; set; }

        public bool Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2} - {3} - {4} - {5}", Value ? "TO_PRINT" : "EMPTY", BestScore.Score, HasBeenPrint ? "PRINTED" : "NOT PRINTED", HasBeenProcessed ? "PROCESSED" : "NOT PROCESSED", NeedToBeClean ?  "TOBECLEAN":string.Empty, BestScore);
        }

        [JsonIgnore]
        public Cell[,] Matrix { get; set; }

        public bool HasBeenPrint { get; set; }

        public bool HasBeenProcessed { get; set; }

        public bool NeedToBeClean { get { return HasBeenPrint && !Value; } }

        public CellScoring BestScore { get; set; }

        public void ComputeCellScoring()
        {
            for (int size = 0;; size++)
            {
                //Console.WriteLine("[{0}, {1}] in {2})", Y, X, size);

                var newScoring = ComputeCellScoringForSize(size);
                if (newScoring == null)
                    break;

                if (BestScore != null && BestScore.Score >= newScoring.Score)
                {
                    break;
                }

                BestScore = newScoring;
            }
        }

        private CellScoring ComputeCellScoringForSize(int size)
        {
            var scoring = new CellScoring(this, size);
            scoring.ComputeScore();
            return scoring;
        }


        public void ERASECELL(StringBuilder sbOut)
        {
            HasBeenPrint = false;
            HasBeenProcessed = true;
            sbOut.AppendFormat("ERASECELL {0} {1}\n", Position.Y, Position.X);
            //  Console.WriteLine("ERASECELL {0} {1}", Position.Y, Position.X);

        }

    }
}