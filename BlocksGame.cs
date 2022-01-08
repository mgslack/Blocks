using System.Drawing;
using System.Threading;

/*
 * Class defines the blocks game engine/thread.  Code largely based on work by
 * Scott Clee, March 2002, in Java for IBM DevelopersWorks.
 * Added 'nextpiece' processing (can use to display next piece to load to the
 * board) and constructor for different sized boards.  Exposed delegates for
 * game events and added one (nextpiece processing and stats gathering).
 * 
 * Revised/Converted: Michael Slack
 * Date Written: 2021-11-17
 * 
 * ----------------------------------------------------------------------------
 * 
 * Revised: 2022-01-08 - Cleaned up speed up code in game thread.
 * 
 */
namespace Blocks
{
    #region Delegates
    public delegate void BlocksStartGame();

    public delegate void BlocksEndGame();

    public delegate void BlocksNewPieceGenerated();

    public delegate void BlocksScoreMade(int score, int totalLines);
    #endregion

    public class BlocksGame
    {
        #region Private consts
        private const int START_DELAY = 500;
        #endregion

        #region Properties
        private BlocksBoard _board;
        public BlocksBoard Board { get => _board; }

        private BlocksPiece _currPiece = null;
        public BlocksPiece CurrPiece { get => _currPiece; set => _currPiece = value; }

        private BlocksPiece _nextPiece = null;
        public BlocksPiece NextPiece { get => _nextPiece; set => _nextPiece = value; }

        private bool _playing = false;
        public bool Playing { get => _playing; set => _playing = value; }

        private bool _paused = false;
        public bool Paused { get => _paused; set { if (_playing) _paused = value; } }

        private int _score = 0;
        public int Score { get => _score; set => _score = value; }

        private int _totalLines = 0;
        public int TotalLines { get => _totalLines; set => _totalLines = value; }

        private BlocksStartGame _startGame = null;
        public BlocksStartGame BlocksStartGame { get => _startGame; set => _startGame = value; }

        private BlocksEndGame _endGame = null;
        public BlocksEndGame BlocksEndGame { get => _endGame; set => _endGame = value; }

        private BlocksNewPieceGenerated _NewPieceGenned = null;
        public BlocksNewPieceGenerated BlocksNewPieceGenerated { get => _NewPieceGenned; set => _NewPieceGenned = value; }

        private BlocksScoreMade _scoreMade = null;
        public BlocksScoreMade BlocksScoreMade { get => _scoreMade; set => _scoreMade = value; }
        #endregion

        #region Private vars
        private int Delay = START_DELAY;
        private Thread GameThread;
        #endregion

        // --------------------------------------------------------------------

        #region Constructors
        public BlocksGame() : this(10, 20) { }

        public BlocksGame(int width, int height)
        {
            _board = new BlocksBoard(width, height);
        }
        #endregion

        // --------------------------------------------------------------------

        #region Public methods
        public void StartGame()
        {
            if (!Playing)
            {
                _board.ResetBoard();
                _totalLines = 0; _score = 0; Delay = START_DELAY;
                _playing = true; _paused = false;
                _currPiece = null;
                _nextPiece = BlocksPiece.GetRandomPiece(_board);
                if (_scoreMade != null) _scoreMade(_score, _totalLines);
                if (_startGame != null) _startGame();
                StartGameThread();
            }
        }

        public void StopGame()
        {
            _playing = false;
            if (_endGame != null) _endGame();
        }

        public bool Move(PieceMove direction)
        {
            bool res = false;

            if (_currPiece != null && _playing && !_paused)
            {
                if (direction == PieceMove.Down || direction == PieceMove.Fall)
                {
                    if (!_currPiece.Move(direction))
                        _currPiece = null;
                    else
                        res = true;
                }
                else
                {
                    res = _currPiece.Move(direction);
                }
            }

            return res;
        }
        #endregion

        // --------------------------------------------------------------------

        #region Private method (game thread)
        private void StartGameThread()
        {
            GameThread = new Thread(() => 
            { // run...
                while (_playing)
                {
                    if (!_paused)
                    {
                        if (_currPiece == null)
                        {
                            int completeLines = 0;
                            // check for any completed lines
                            for (int rows = _board.Rows - 1; rows >= 0; rows--)
                            {
                                bool same = true;
                                for (int cols = 0; cols < _board.Columns; cols++)
                                    if (_board.GetPieceAt(cols, rows) == BlocksBoard.EMPTY_BLOCK)
                                        same = false;
                                if (same)
                                {
                                    _board.RemoveRow(rows);
                                    // need to check row again..
                                    rows++;
                                    completeLines++;
                                    _totalLines++;
                                    // hit target, speed up things
                                    // 400, 300, 200, 150, 120
                                    if (_totalLines == 10 || _totalLines == 20 || _totalLines == 30)
                                        Delay -= 100;
                                    if (_totalLines == 40) Delay -= 50;
                                    if (_totalLines == 50) Delay -= 30;
                                }
                            }
                            if (completeLines > 0)
                            {
                                // more lines completed at once, leads to bigger score
                                _score += completeLines * completeLines * 100;
                                if (_scoreMade != null) _scoreMade(_score, _totalLines);
                            }
                            _currPiece = _nextPiece;
                            _currPiece.CenterPoint = new Point(_board.Columns / 2, 1);
                            _nextPiece = BlocksPiece.GetRandomPiece(_board);
                            if (_NewPieceGenned != null) _NewPieceGenned();
                            if (_board.WillFit(_currPiece))
                            {
                                _board.AddPiece(_currPiece, true);
                            }
                            else
                            {
                                _board.AddPiece(_currPiece, true);
                                StopGame();
                                break;
                            }
                        }
                        else
                        {
                            Move(PieceMove.Down);
                        }
                    }
                    if (_currPiece != null)
                    {
                        Thread.Sleep(Delay);
                    }
                }
            });
            GameThread.Start();
        }
        #endregion
    }
}
