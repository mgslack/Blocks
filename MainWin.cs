using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using GameStatistics;

/*
 * Class defines a partial form class of the main window for the Blocks game (a
 * Tetris clone).  Game is heavily based on Java code created by Scott Clee for
 * IBM DevelopersWorks in March 2002.
 * Spiritually, blocks was a conversion of the Son-of-Tetris project, rewritten
 * in Turbo Pascal.  Later, whas rewritten in Java, and took advantage of the 
 * TetrisBean components developed by Scott Lee.
 * 
 * Author: Michael Slack
 * Date Written: 2021-11-18
 * 
 * ----------------------------------------------------------------------------
 * 
 * Revised: yyyy-mm-dd - xxxx.
 * 
 */
namespace Blocks
{
    public partial class MainWin : Form
    {
        #region Private consts
        private const string HTML_HELP_FILE = "Blocks_help.html";
        private const string S_START = ":-)";
        private const string S_PAUSED = "< Paused >";
        private const string S_BTN_UN = "&Unpause";
        private const string S_BTN_P = "&Pause";
        private const string S_OVER = "Game Over";
        private const string STAT_GAMES_PLAYED = "Games Played";
        private const string STAT_HIGH_SCORE = "Highest Score Made";
        private const string STAT_MOST_LINES = "Most Lines Removed";
        private const string STAT_CURR_PIECES = "Pieces Added in Current or Last Game";
        private const string STAT_MOST_PIECES = "Most Pieces Added in Any Game";
        #endregion

        #region Registry Constants
        private const string REG_NAME = @"HKEY_CURRENT_USER\Software\Slack and Associates\Games\Blocks";
        private const string REG_KEY1 = "PosX";
        private const string REG_KEY2 = "PosY";
        #endregion

        #region Private vars
        private BlocksGame game = new BlocksGame();
        private Statistics stats = new Statistics(REG_NAME);
        #endregion

        // --------------------------------------------------------------------

        #region Private methods
        private void LoadRegistryValues()
        {
            int winX = -1, winY = -1;

            try
            {
                winX = (int)Registry.GetValue(REG_NAME, REG_KEY1, winX);
                winY = (int)Registry.GetValue(REG_NAME, REG_KEY2, winY);
            }
            catch (Exception) { /* ignore, go with defaults */ }

            if ((winX != -1) && (winY != -1)) this.SetDesktopLocation(winX, winY);
        }

        private void SetupContextMenu()
        {
            ContextMenu mnu = new ContextMenu();
            MenuItem mnuStats = new MenuItem("Game Statistics");
            MenuItem sep = new MenuItem("-");
            MenuItem mnuHelp = new MenuItem("Help");
            MenuItem mnuAbout = new MenuItem("About");

            mnuStats.Click += new EventHandler(MnuStats_Click);
            mnuHelp.Click += new EventHandler(MnuHelp_Click);
            mnuAbout.Click += new EventHandler(MnuAbout_Click);
            mnu.MenuItems.AddRange(new MenuItem[] { mnuStats, sep, mnuHelp, mnuAbout });
            this.ContextMenu = mnu;
        }

        private void InitControlsAndEvents()
        {
            game.Board.BoardPieceAdded += BBBoardPieceAdded;
            game.Board.BoardRowRemoved += BBBoardRowRemoved;
            game.BlocksStartGame += BlksStartGame;
            game.BlocksEndGame += BlksEndGame;
            game.BlocksNewPieceGenerated += BlksNewPieceGenerated;
            game.BlocksScoreMade += BlksScoreMade;
            stats.GameName = this.Text;
            bbPanel.Board = game.Board;
            npPanel.Game = game;
        }
        #endregion

        // --------------------------------------------------------------------

        public MainWin()
        {
            InitializeComponent();
        }

        // --------------------------------------------------------------------

        #region Form event handlers
        private void MainWin_Load(object sender, EventArgs e)
        {
            LoadRegistryValues();
            SetupContextMenu();
            InitControlsAndEvents();
        }

        private void MainWin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (game.Playing) game.Playing = false;
        }

        private void MainWin_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                Registry.SetValue(REG_NAME, REG_KEY1, this.Location.X);
                Registry.SetValue(REG_NAME, REG_KEY2, this.Location.Y);
            }
        }

        private void MainWin_KeyUp(object sender, KeyEventArgs e)
        {
            int K = e.KeyValue;

            // check numpad keys, no numlock and other special keys
            switch (e.KeyCode)
            {
                case Keys.Left: K = 100; break;
                case Keys.Clear: K = 101; break;
                case Keys.Right: K = 102; break;
                case Keys.Space:
                case Keys.Down: K = 98; break;
                case Keys.Escape: e.Handled = true; BtnPause_Click(sender, EventArgs.Empty); break;
                default: break;
            }

            if (!e.Handled)
            {
                switch (K)
                {
                    case 98: e.Handled = true; game.Move(PieceMove.Fall); btnDrop.Focus(); break;
                    case 100: e.Handled = true; game.Move(PieceMove.Left); btnLeft.Focus(); break;
                    case 101: e.Handled = true; game.Move(PieceMove.Rotate); btnRotate.Focus(); break;
                    case 102: e.Handled = true; game.Move(PieceMove.Right); btnRight.Focus(); break;
                    default: break;
                }
            }
        }

        private void BtnQuit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            game.StartGame();
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            bool paused = !game.Paused;

            game.Paused = paused;
            if (paused)
            {
                lblMsg.Text = S_PAUSED; btnPause.Text = S_BTN_UN;
            }
            else
            {
                lblMsg.Text = S_START; btnPause.Text = S_BTN_P;
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            game.StopGame();
        }

        private void BtnLeft_Click(object sender, EventArgs e)
        {
            game.Move(PieceMove.Left);
        }

        private void BtnRotate_Click(object sender, EventArgs e)
        {
            game.Move(PieceMove.Rotate);
        }

        private void BtnRight_Click(object sender, EventArgs e)
        {
            game.Move(PieceMove.Right);
        }

        private void BtnDrop_Click(object sender, EventArgs e)
        {
            game.Move(PieceMove.Fall);
        }

        private void MnuStats_Click(object sender, EventArgs e)
        {
            if (game.Playing) BtnPause_Click(sender, EventArgs.Empty);
            stats.ShowStatistics(this);
        }

        private void MnuHelp_Click(object sender, EventArgs e)
        {
            var asm = Assembly.GetEntryAssembly();
            var asmLocation = Path.GetDirectoryName(asm.Location);
            var htmlPath = Path.Combine(asmLocation, HTML_HELP_FILE);

            if (game.Playing) BtnPause_Click(sender, EventArgs.Empty);
            try
            {
                Process.Start(htmlPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Cannot load help: " + ex.Message, "Blocks: Help Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void MnuAbout_Click(object sender, EventArgs e)
        {
            if (game.Playing) BtnPause_Click(sender, EventArgs.Empty);

            AboutBox about = new AboutBox();
            about.ShowDialog(this);
            about.Dispose();
        }
        #endregion

        // --------------------------------------------------------------------

        #region Game event handlers
        private void BBBoardPieceAdded()
        {
            bbPanel.Refresh();
        }

        private void BBBoardRowRemoved()
        {
            bbPanel.Refresh();
        }

        private void BlksStartGame()
        {
            stats.StartGameNoGSS(true);
            stats.IncCustomStatistic(STAT_GAMES_PLAYED);
            stats.SetCustomStatistic(STAT_CURR_PIECES, 0);
            lblMsg.Text = S_START;
            btnPause.Enabled = true;
            btnPause.Focus();
            btnStop.Enabled = true;
            btnStart.Enabled = false;
            btnLeft.Enabled = true; btnRotate.Enabled = true; btnRight.Enabled = true;
            btnDrop.Enabled = true;
        }

        private void BlksEndGame()
        {
            stats.GameDone();
            lblMsg.Text = S_OVER;
            btnStart.Enabled = true;
            btnStart.Focus();
            btnPause.Enabled = false;
            btnStop.Enabled = false;
            btnLeft.Enabled = false; btnRotate.Enabled = false; btnRight.Enabled = false;
            btnDrop.Enabled = false;
        }

        private void BlksNewPieceGenerated()
        {
            stats.IncCustomStatistic(STAT_CURR_PIECES);
            if (stats.CustomStatistic(STAT_MOST_PIECES) < stats.CustomStatistic(STAT_CURR_PIECES))
                stats.SetCustomStatistic(STAT_MOST_PIECES, stats.CustomStatistic(STAT_CURR_PIECES));
            npPanel.Refresh();
        }

        private void BlksScoreMade(int score, int totalLines)
        {
            lblScore.Text = "" + score;
            lblLines.Text = "" + totalLines;
            if (stats.CustomStatistic(STAT_HIGH_SCORE) < score)
                stats.SetCustomStatistic(STAT_HIGH_SCORE, score);
            if (stats.CustomStatistic(STAT_MOST_LINES) < totalLines)
                stats.SetCustomStatistic(STAT_MOST_LINES, totalLines);
        }
        #endregion
    }
}
