using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageProcessing._3D;
using System.Runtime.InteropServices;

namespace ImageProcessing.Segmentation
{
    public class LevelSets
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        static unsafe extern void CopyMemory(ushort* Destination, ushort* Source, uint Length);

        public void SegmentVolumeConfidenceConnectedImageFilter(float[, ,] data)
        {
            itk.simple.Image image = new itk.simple.Image((uint)data.GetLength(0), (uint)data.GetLength(1), (uint)data.GetLength(2), itk.simple.PixelIDValueEnum.sitkFloat32);

            image.SetOrigin(new itk.simple.VectorDouble(new double[] { 0, 0, 0 }));
            image.SetSpacing(new itk.simple.VectorDouble(new double[] { 1, 1, 1 }));

            IntPtr buffer = image.GetBufferAsFloat();
           //  data.ConvertToVTK();
            unsafe
            {
                float* volptr = (float*)buffer;
                fixed (float* pData = data)
                {
                    CopyMemory((ushort*)(void*)volptr, (ushort*)(void*)pData, (uint)Buffer.ByteLength(data));
                }
            }


            itk.simple.CastImageFilter cast = new itk.simple.CastImageFilter();
            cast.SetOutputPixelType((int)itk.simple.PixelIDValueEnum.sitkInt16.swigValue);
            image = cast.Execute(image);


            itk.simple.ImageFileWriter writer = new itk.simple.ImageFileWriter();
            writer.SetFileName(@"c:\temp\volumet.tif");
            writer.Execute(image);


            itk.simple.OtsuThresholdImageFilter filter = new itk.simple.OtsuThresholdImageFilter();
            
            //itk.simple.OtsuThresholdImageFilter 
            itk.simple.Image threshold = filter.Execute(image);

            cast = new itk.simple.CastImageFilter();
            cast.SetOutputPixelType((int)itk.simple.PixelIDValueEnum.sitkUInt8.swigValue);
            threshold = cast.Execute(threshold);


            itk.simple.ImageFileWriter ifw = new itk.simple.ImageFileWriter();
            ifw.SetFileName(@"c:\temp\volume.tif");
            ifw.Execute(threshold);
        }
    }
}
