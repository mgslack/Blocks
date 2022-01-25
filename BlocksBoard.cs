using System.Drawing;

/*
 * Class representing a game board for blocks (tetris-like) game.  Code is largely based on
 * code written by Scott Clee, March 2002 in Java.  Added random rotation ability to piece
 * start position and rotate into any position to start the piece at.
 * 
 * Revised/Converted: Michael Slack
 * Date Written: 2021-11-17
 * 
 * ----------------------------------------------------------------------------
 * 
 * Revised: 2022-01-25 - Removed 'set' on some of the properties, should be read-only.
 * 
 */
namespace Blocks
{
    #region Delegates
    public delegate void BoardPieceAdded();

    public delegate void BoardRowRemoved();
    #endregion

    public class BlocksBoard
    {
        #region Public consts
        public const int EMPTY_BLOCK = -1;
        #endregion

        #region Properties
        private int _cols = 0;
        public int Columns { get => _cols; }

        private int _rows = 0;
        public int Rows { get => _rows; }

        private BoardPieceAdded _boardPieceAdded = null;
        public BoardPieceAdded BoardPieceAdded { get => _boardPieceAdded; set => _boardPieceAdded = value; }

        private BoardRowRemoved _boardRowRemoved = null;
        public BoardRowRemoved BoardRowRemoved { get => _boardRowRemoved; set => _boardRowRemoved = value; }
        #endregion

        #region Private vars
        private int[,] Board;
        #endregion

        // --------------------------------------------------------------------

        public BlocksBoard(int cols, int rows)
        {
            _cols = cols;
            _rows = rows;
            ResetBoard();
        }

        // --------------------------------------------------------------------

        #region Public methods
        public void ResetBoard()
        {
            Board = new int[_cols, _rows];
            for (int c = 0; c < _cols; c++)
                for (int r = 0; r < _rows; r++)
                    Board[c, r] = EMPTY_BLOCK;
        }

        public int GetPieceAt(int c, int r)
        {
            return Board[c, r];
        }

        public void SetPieceAt(int c, int r, int value)
        {
            Board[c, r] = value;
        }

        public void AddPiece(BlocksPiece piece, bool notify)
        {
            if (piece != null)
            {
                Point center = piece.CenterPoint;
                Point[] blocks = piece.RelativePoints;
                for (int i = 0; i < 4; i++)
                {
                    int c = center.X + blocks[i].X;
                    int r = center.Y + blocks[i].Y;
                    Board[c, r] = (int)piece.Type;
                }
                if (notify && _boardPieceAdded != null) _boardPieceAdded();
            }
        }

        public void RemovePiece(BlocksPiece piece)
        {
            if (piece != null)
            {
                Point center = piece.CenterPoint;
                Point[] blocks = piece.RelativePoints;
                for (int i = 0; i < 4; i++)
                {
                    int c = center.X + blocks[i].X;
                    int r = center.Y + blocks[i].Y;
                    Board[c, r] = EMPTY_BLOCK;
                }
            }
        }

        public void RemoveRow(int row)
        {
            for (int r = row; r > 0; r--)
                for (int c = 0; c < _cols; c++)
                    Board[c, r] = Board[c, r - 1];
            for (int c = 0; c < _cols; c++)
                Board[c, 0] = EMPTY_BLOCK;
            if (_boardRowRemoved != null) _boardRowRemoved();
        }

        public bool WillFit(BlocksPiece piece)
        {
            bool res = true;

            if (piece != null)
            {
                Point center = piece.CenterPoint;
                Point[] blocks = piece.RelativePoints;
                for (int i = 0; i < 4 && res; i++)
                {
                    int c = center.X + blocks[i].X;
                    int r = center.Y + blocks[i].Y;
                    if (c < 0 || c >= _cols || r < 0 || r >= _rows) res = false;
                    if (res && Board[c, r] != EMPTY_BLOCK) res = false;
                }
            }

            return res;
        }
        #endregion
    }
}
