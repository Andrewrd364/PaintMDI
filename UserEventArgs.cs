using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintMDI
{
    public class UserEventArgs: EventArgs
    {
        public readonly int x, y;
        public readonly bool isVisible;
        public UserEventArgs(int x, int y, bool isVisible)
        {
            this.x = x;
            this.y = y;
            this.isVisible = isVisible;
        }
        public UserEventArgs(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
