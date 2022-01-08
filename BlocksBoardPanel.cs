using System.Drawing;
using System.Windows.Forms;

/*
 * Class defining the component representing the visual Blocks board.  This is
 * where the blocks are displayed and moved to the user (panel in main window).
 * 
 * Author: Michael G. Slack
 * Date Written: 2021-11-18
 * 
 * ----------------------------------------------------------------------------
 * 
 * Date Revised: yyyy-mm-dd - xxxx.
 * 
 */
namespace Blocks
{
    public partial class BlocksBoardPanel : UserControl
    {
        #region Properties
        private BlocksBoard _board = null;
        public BlocksBoard Board { set => _board = value; }
        #endregion

        public BlocksBoardPanel()
        {
            InitializeComponent();
        }

        #region Private component events
        private void BlocksBoardPanel_Paint(object sender, PaintEventArgs e)
        {
            int width = e.ClipRectangle.Width;
            int height = e.ClipRectangle.Height;
            Graphics g = e.Graphics;

            g.Clear(BackColor);
            if (_board != null)
            {
                int numCols = _board.Columns;
                int numRows = _board.Rows;

                for (int c = 0; c < numCols; c++)
                    for (int r = 0; r < numRows; r++)
                    {
                        int piece = _board.GetPieceAt(c, r);
                        if (piece != BlocksBoard.EMPTY_BLOCK)
                        {
                            Color color = BlocksPiece.GetPieceColor((BlocksType)piece);
                            Rectangle rect = new Rectangle((c * width / numCols) + 1,
                                (r * height / numRows) + 1,
                                (width / numCols) - 1, (height / numRows) - 1);
                            g.FillRectangle(new SolidBrush(color), rect);
                            g.DrawRectangle(new Pen(Color.Black), rect);
                        }
                    }
            }
        }
        #endregion
    }
}
