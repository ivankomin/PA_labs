using System.Collections.Generic;

namespace pa_lab3
{
    public enum Difficulty { Easy, Medium, Hard }
    public enum Player { User, Computer }

    public struct Move
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public Move(int r, int c) { Row = r; Col = c; }
    }

    public class GameState
    {
        public int Rows { get; }
        public int Cols { get; }
        private int[] _rowLengths;

        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            _rowLengths = new int[rows];
            for (int i = 0; i < rows; i++) _rowLengths[i] = cols;
        }

        private GameState(int rows, int cols, int[] lengths)
        {
            Rows = rows;
            Cols = cols;
            _rowLengths = (int[])lengths.Clone();
        }

        public bool IsCellActive(int r, int c)
        {
            if (r >= Rows) return false;
            return c < _rowLengths[r];
        }

        public GameState MakeMove(Move move)
        {
            var newState = new GameState(Rows, Cols, _rowLengths);
            for (int r = move.Row; r < Rows; r++)
            {
                if (newState._rowLengths[r] > move.Col)
                {
                    newState._rowLengths[r] = move.Col;
                }
            }
            return newState;
        }

        public List<Move> GetLegalMoves()
        {
            var moves = new List<Move>();
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < _rowLengths[r]; c++)
                {
                    moves.Add(new Move(r, c));
                }
            }
            return moves;
        }

        public bool IsGameOver() => _rowLengths[0] == 0;
        public GameState Clone() => new GameState(Rows, Cols, _rowLengths);
    }
}