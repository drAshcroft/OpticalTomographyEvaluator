using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using MathLibrary;

namespace ImageProcessing._2D
{
    public class MovieMaker
    {
        /// <summary>
        /// Creates a compressed AVI file from the frames
        /// </summary>
        /// <param name="AVIFilename"></param>
        /// <param name="Frames"></param>
        /// <param name="SkipFrames">The number of frames to skip between adds</param>
        public static void CreateAVIVideoEMGU(string AVIFilename, Image<Gray, float>[] Frames, int SkipFrames)
        {
            if (File.Exists(AVIFilename) == true)
                File.Delete(AVIFilename);

            var bmp = Frames[0];

            int nWidth = (int)(16 * (Math.Round((double)bmp.Width / 16d+.49) ));

            VideoWriter VW = null;

            VW = new VideoWriter(AVIFilename, CvInvoke.CV_FOURCC('M', 'J', 'P', 'G'), 15,nWidth /* bmp.Width*/, bmp.Height, true);

            float minFrame = float.MaxValue;
            float maxFrame = float.MinValue;
            for (int i = 0; i < Frames.Length; i += 10)
            {
                float mFrame = Frames[i].Data.MinArrayUnZero();
                float MFrame = Frames[i].Data.MaxArray();
                if (mFrame < minFrame) minFrame = mFrame;
                if (MFrame > maxFrame) maxFrame = MFrame;
            }
            System.Drawing.Bitmap bitmap;
            int count = 0;
            for (int n = 1; n < Frames.Length; n += SkipFrames)
            {
                if (Frames[n] != null)
                {
                  //  float MFrame = Frames[n].Data.MaxArray();
                    bitmap = Frames[n].MakeScaledBitmap(minFrame, maxFrame);// Frames[n].ScaledBitmap;
                    Bitmap temp = new Bitmap(nWidth, bitmap.Height, PixelFormat.Format32bppRgb);
                    Graphics g = Graphics.FromImage(temp);

                    g.DrawImage(bitmap, Point.Empty);

                    var frame = new Emgu.CV.Image<Bgr, byte>(temp);

                    VW.WriteFrame<Bgr, byte>(frame);
                    bitmap.Dispose();
                    count++;
                }
            }

            VW.Dispose();
        }
    }
}
