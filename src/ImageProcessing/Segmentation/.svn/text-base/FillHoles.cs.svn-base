using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageProcessing._3D;
using System.Runtime.InteropServices;
using Kitware.VTK;

namespace ImageProcessing.Segmentation
{
    public class FillHoles
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        static unsafe extern void CopyMemory(ushort* Destination, ushort* Source, uint Length);

        public static float[, ,] FillMaskHoles(float[, ,] data)
        {
        
           vtkImageData image =   data.ConvertToVTK(); 

            Kitware.VTK.vtkFillHolesFilter holes = new Kitware.VTK.vtkFillHolesFilter();

            holes.SetHoleSize(10);
            holes.SetInput(image);
            return null;   
        }
    }
}
