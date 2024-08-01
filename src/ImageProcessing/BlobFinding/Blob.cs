using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ImageProcessing.BlobFinding
{
    public class Blob
    {
        public Point Center;
        public Size CellSize;

        public int MaxSize
        {
            get
            {
                return (CellSize.Width > CellSize.Height ? CellSize.Width : CellSize.Height);
            }
        }

        public int MinSize
        {
            get
            {
                return (CellSize.Width < CellSize.Height ? CellSize.Width : CellSize.Height);
            }
        }

        public Blob(Point center, double minX, double maxX, double minY, double maxY)
        {
            int widthm =(int) Math.Abs(minX - center.X);
            int widthM = (int)Math.Abs(maxX - center.X);
            int heightm = (int)Math.Abs(minY - center.Y);
            int heightM = (int)Math.Abs(maxY - center.Y);

            Center = center;
            int width =             ( (widthm>widthM) ?  (2*widthm) : (2*widthM));
            int height = ( (heightm>heightM) ? 2*heightm :(2*heightM));
            CellSize = new Size( width  , height   );


        }
        public Blob(Point center, Size cellSize)
        {
            Center = center;
            CellSize = cellSize;
        }
    }
}
