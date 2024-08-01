using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ReconstructCells
{
    public class ReconBackground
    {
        private OnDemandImageLibrary Library=null;
        private void BatchSaveFine(int ImageNumber)
        {
            Bitmap b = Library[ImageNumber].ToBitmap();
            System.Diagnostics.Debug.Print(b.Width.ToString());
        }

        public void GetRoughBackground(OnDemandImageLibrary library)
        {
            Library=library;

            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            int numberOfImages = Library.Count;

            Parallel.For(0, numberOfImages, po, x => BatchSaveFine(x));
          
        }
    }
}
