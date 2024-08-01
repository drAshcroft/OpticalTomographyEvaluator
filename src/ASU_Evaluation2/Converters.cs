using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Threading;

namespace ASU_Evaluation2
{

    public static class Converters
    {

        public static BitmapSource CreateBitmapSourceFromBitmap(this Bitmap bitmap)
        {
            if (bitmap == null)
                bitmap = new Bitmap(10, 10);

            if (Application.Current.Dispatcher == null)
                return null; // Is it possible?

            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // You need to specify the image format to fill the stream. 
                    // I'm assuming it is PNG
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Make sure to create the bitmap in the UI thread
                    if (InvokeRequired)
                        return (BitmapSource)Application.Current.Dispatcher.Invoke(
                            new Func<Stream, BitmapSource>(CreateBitmapSourceFromBitmap),
                            DispatcherPriority.Normal,
                            memoryStream);

                    return CreateBitmapSourceFromBitmap(memoryStream);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool InvokeRequired
        {
            get { return Dispatcher.CurrentDispatcher != Application.Current.Dispatcher; }
        }

        private static BitmapSource CreateBitmapSourceFromBitmap(Stream stream)
        {
            BitmapDecoder bitmapDecoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            // This will disconnect the stream from the image completely...
            WriteableBitmap writable = new WriteableBitmap(bitmapDecoder.Frames.Single());
            writable.Freeze();

            return writable;
        }
    }




    public class BoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value;
            }
            return value;
        }

        #endregion
    }
}
