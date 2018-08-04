using System.Drawing;
using System.Windows.Forms;

namespace NetRadio
{
    public partial class frmWait : Form
    {
        public frmWait(Point point)
        {
            InitializeComponent();
            Location = new Point(point.X + 50, point.Y + 25);
        }
    }
}
