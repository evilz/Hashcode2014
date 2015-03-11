using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrialRound.Model
{
    public class CellScoring
    {
        private readonly Cell _cell;
        private readonly int _top;
        private readonly int _left;
        private readonly int _bottom;
        private readonly int _right;

        public int Size { get; private set; }
        public float Score { get; private set; }


        public CellScoring(Cell cell, int size)
        {
            _cell = cell;
            Size = size;
            _top = _cell.Position.Y - Size;
            _left = _cell.Position.X - Size;
            _bottom = _cell.Position.Y + Size;
            _right = _cell.Position.X + Size;
        }
        public void ComputeScore()
        {
           
            if (IsOutRange())
            {
                Score = -1;
                return;
            }

            if (CellInSquare == null)
                CellInSquare = RefreshCellInSquare();

            if (Size == 0 && !_cell.Value || CellToPrint / 2 < CellToClear)
           // if ((Size == 0 && !_cell.Value) || CellToClear != 0)
            {
                Score = 0;

            }
            else
            {
                var computed = CellToPrint / (float)(1 + CellToClear);
                Score = computed;
            }
        }

        private int CellToPrint
        {
            get { return CellInSquare.Count(c => !c.HasBeenPrint && c.Value); }
        }

        public void PRINTSQ(StringBuilder sbOut)
        {
            // Console.WriteLine("PRINTSQ {0} {1} {2} - SCORE : {3}", _cell.Position.Y, _cell.Position.X, Size, this);

            sbOut.AppendFormat("PRINTSQ {0} {1} {2}\n", _cell.Position.Y, _cell.Position.X, Size);

            _cell.HasBeenProcessed = true;

            foreach (var cell in CellInSquare)
            {
                cell.HasBeenPrint = true;
                cell.ComputeCellScoring();
            }
        }

        private bool IsOutRange()
        {
            return _top < 0 || _left < 0 || _bottom >= _cell.Matrix.GetLength(0) || _right >= _cell.Matrix.GetLength(1);
        }

        private int CellToClear
        {
            get
            {
                return CellInSquare.Count(c => !c.Value);
            }
        }

        public List<Cell> CellInSquare { get; set; }

        private List<Cell> RefreshCellInSquare()
        {
            var list = new List<Cell>();
            for (var r = _top; r <= _bottom && r < _cell.Matrix.GetLength(0); ++r)
            {
                for (var c = _left; c <= _right && c < _cell.Matrix.GetLength(1); ++c)
                {
                    list.Add(_cell.Matrix[r, c]);
                }
            }
            return list;
        }

        public override string ToString()
        {
            return string.Format("Size:{0} - Score:{1} - TotalCell:{2} - CellToPrint:{3} - CellToClear:{4}", Size, Score, CellInSquare != null ? CellInSquare.Count.ToString() : "?", CellInSquare != null ? CellToPrint.ToString() : "?", CellInSquare != null ? CellToClear.ToString() : "?");
        }
    }
}