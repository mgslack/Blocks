using System.Drawing;
using System.Windows.Forms;

namespace Blocks
{
    public partial class NextPiece : UserControl
    {
        private BlocksGame _game = null;
        public BlocksGame Game { set => _game = value; }

        public NextPiece()
        {
            InitializeComponent();
        }

        private void NextPiece_Paint(object sender, PaintEventArgs e)
        {
            int width = e.ClipRectangle.Width;
            int height = e.ClipRectangle.Height;
            Graphics g = e.Graphics;

            g.Clear(BackColor);
            BlocksPiece next = null;
            if (_game != null) next = _game.NextPiece;
            if (next != null)
            {
                Point center = next.CenterPoint;
                Point[] blocks = next.RelativePoints;
                Color color = next.GetPieceColor();
                for (int i = 0; i < 4; i++)
                {
                    int c = center.X + blocks[i].X + 1;
                    int r = center.Y + blocks[i].Y + 1;
                    Rectangle rect = new Rectangle((c * width / 4) + 1, (r * height / 4) + 1,
                        (width / 4) - 1, (height / 4) - 1);
                    g.FillRectangle(new SolidBrush(color), rect);
                    g.DrawRectangle(new Pen(Color.Black), rect);
                }
            }
        }
    }
}
