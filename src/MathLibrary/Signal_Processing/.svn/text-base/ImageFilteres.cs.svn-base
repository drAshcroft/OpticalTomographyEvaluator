using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MathLibrary.Signal_Processing
{
    public  class ImageFilters
    {
        public static void  AddJitter(ref Bitmap image)
        {
            AForge.Imaging.Filters.Jitter jit = new AForge.Imaging.Filters.Jitter(3);
            jit.ApplyInPlace(image);
        }
    }
}
