using System;
using System.Drawing;

/*
 * Class containing a blocks (tetris-like) piece for the Blocks game.  This class is
 * largely based on code developed by Scott Clee in Java, March 2002 (IBM DeveloperWorks).
 * Note: moved piece color routine to this class and modified the colors to match the
 * standard colors for modern versions of Testris.
 * 
 * Revised/Converted: Michael Slack
 * Date Written: 2021-11-17
 * 
 * ----------------------------------------------------------------------------
 * 
 * Revised: yyyy-mm-dd - xxxx.
 * 
 */
namespace Blocks
{
    #region Enums
    public enum BlocksType { L_Piece, J_Piece, I_Piece, Z_Piece, S_Piece, O_Piece, T_Piece }

    public enum PieceMove { Left, Right, Rotate, Down, Fall }
    #endregion

    public class BlocksPiece
    {
        #region Statics
        public static int MAX_TYPES = Enum.GetNames(typeof(BlocksType)).Length;
        #endregion

        #region Properties
        private BlocksType _type = BlocksType.L_Piece;
        public BlocksType Type { get => _type; set { _type = value; InitializeBlocks(); } }

        private Point _centerPoint = new Point();
        public Point CenterPoint { get => _centerPoint; set => _centerPoint = value; }

        private Point[] _blocks = new Point[4];
        public Point[] RelativePoints { get => _blocks; set { if (value != null) _blocks = value; } }
        #endregion

        #region Private Vars
        private int Rotation = 0, MaxRotate;
        private BlocksBoard Board;
        #endregion

        // --------------------------------------------------------------------

        public BlocksPiece(BlocksType type, BlocksBoard board)  // need to add board also
        {
            _type = type; Board = board;
            InitializeBlocks();
        }

        // --------------------------------------------------------------------

        #region Public methods
        public bool Move(PieceMove direction)
        {
            bool res = true;

            if (direction == PieceMove.Fall)
            {
                bool loop = true;
                while (loop)
                {
                    Board.RemovePiece(this);
                    _centerPoint.Y++; // drop
                    if (Board.WillFit(this))
                    {
                        Board.AddPiece(this, false);
                    }
                    else
                    {
                        _centerPoint.Y--; // undrop
                        Board.AddPiece(this, false);
                        loop = false; res = false;
                    }
                }
            }
            else
            {
                Board.RemovePiece(this);
                switch (direction)
                {
                    case PieceMove.Left: _centerPoint.X--; break;
                    case PieceMove.Right: _centerPoint.X++; break;
                    case PieceMove.Down: _centerPoint.Y++; break;
                    case PieceMove.Rotate: RotateClockwise(); break;
                    default: break;
                }
                if (Board.WillFit(this))
                {
                    Board.AddPiece(this, true);
                }
                else
                { // undo...
                    switch (direction)
                    {
                        case PieceMove.Left: _centerPoint.X++; break;
                        case PieceMove.Right: _centerPoint.X--; break;
                        case PieceMove.Down: _centerPoint.Y--; break;
                        case PieceMove.Rotate: RotateAntiClockwise(); break;
                        default: break;
                    }
                    Board.AddPiece(this, true);
                    res = false;
                }
            }

            return res;
        }

        public Color GetPieceColor()
        {
            return GetPieceColor(_type);
        }
        #endregion

        // --------------------------------------------------------------------

        #region Private methods
        private void RotateClockwiseNow()
        {
            for (int i = 0; i < 4; i ++)
            {
                int temp = _blocks[i].X;
                _blocks[i].X = -_blocks[i].Y;
                _blocks[i].Y = temp;
            }
        }

        private void RotateClockwise()
        {
            if (MaxRotate > 1)
            {
                Rotation++;
                if (MaxRotate == 2 && Rotation == 2)
                { // rotate anti-clockwise
                    RotateClockwiseNow();
                    RotateClockwiseNow();
                    RotateClockwiseNow();
                }
                else
                {
                    RotateClockwiseNow();
                }
            }
            Rotation %= MaxRotate;
        }

        private void RotateAntiClockwise()
        {
            RotateClockwise();
            RotateClockwise();
            RotateClockwise();
        }

        private void RandomRotate()
        {
            if (MaxRotate > 1)
            {
                int rr = SingleRandom.Instance.Next(MaxRotate);
                for (int i = 0; i < rr; i++) RotateClockwise();
            }
        }

        private void InitializeBlocks()
        {
            switch (_type)
            {
                case BlocksType.L_Piece:
                    _blocks[0] = new Point(0, 0);
                    _blocks[1] = new Point(-1, 0);
                    _blocks[2] = new Point(-1, 1);
                    _blocks[3] = new Point(1, 0);
                    MaxRotate = 4;
                    break;
                case BlocksType.J_Piece:
                    _blocks[0] = new Point(0, 0);
                    _blocks[1] = new Point(-1, 0);
                    _blocks[2] = new Point(1, 0);
                    _blocks[3] = new Point(1, 1);
                    MaxRotate = 4;
                    break;
                case BlocksType.I_Piece:
                    _blocks[0] = new Point(0, 0);
                    _blocks[1] = new Point(-1, 0);
                    _blocks[2] = new Point(1, 0);
                    _blocks[3] = new Point(2, 0);
                    MaxRotate = 2;
                    break;
                case BlocksType.Z_Piece:
                    _blocks[0] = new Point(0, 0);
                    _blocks[1] = new Point(-1, 0);
                    _blocks[2] = new Point(0, 1);
                    _blocks[3] = new Point(1, 1);
                    MaxRotate = 2;
                    break;
                case BlocksType.S_Piece:
                    _blocks[0] = new Point(0, 0);
                    _blocks[1] = new Point(1, 0);
                    _blocks[2] = new Point(0, 1);
                    _blocks[3] = new Point(-1, 1);
                    MaxRotate = 2;
                    break;
                case BlocksType.O_Piece:
                    _blocks[0] = new Point(0, 0);
                    _blocks[1] = new Point(0, 1);
                    _blocks[2] = new Point(-1, 0);
                    _blocks[3] = new Point(-1, 1);
                    MaxRotate = 1;
                    break;
                case BlocksType.T_Piece:
                    _blocks[0] = new Point(0, 0);
                    _blocks[1] = new Point(-1, 0);
                    _blocks[2] = new Point(1, 0);
                    _blocks[3] = new Point(0, 1);
                    MaxRotate = 4;
                    break;
            }
            RandomRotate();
        }
        #endregion

        // --------------------------------------------------------------------

        #region Public Static methods
        public static BlocksPiece GetRandomPiece(BlocksBoard board)
        {
            return new BlocksPiece((BlocksType)SingleRandom.Instance.Next(MAX_TYPES), board);
        }

        public static Color GetPieceColor(BlocksType type)
        {
            Color res = Color.Black;
            switch (type)
            {
                case BlocksType.L_Piece: res = Color.Orange; break;    // orig: Color.Turquoise
                case BlocksType.J_Piece: res = Color.Blue; break;      // orig: Color.Purple
                case BlocksType.I_Piece: res = Color.Turquoise; break; // orig: Color.Blue
                case BlocksType.Z_Piece: res = Color.Red; break;
                case BlocksType.S_Piece: res = Color.Green; break;
                case BlocksType.O_Piece: res = Color.Yellow; break;    // orig: Color.Gray
                case BlocksType.T_Piece: res = Color.Purple; break;    // orig: Color.Yellow
                default: break;
            }
            return res;
        }
        #endregion
    }
}
