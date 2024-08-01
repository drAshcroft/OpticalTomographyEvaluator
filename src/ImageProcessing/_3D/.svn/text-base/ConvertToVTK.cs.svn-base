using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kitware.VTK;
using System.Runtime.InteropServices;

namespace ImageProcessing._3D
{
    public static class ConverterToVTK
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        static unsafe extern void CopyMemory(ushort* Destination, ushort* Source, uint Length);

        public static vtkStructuredPoints ConvertToVTK(this float[, ,] DataCube)
        {
            float widht = DataCube.GetLength(0);

            vtkStructuredPoints sp = new vtkStructuredPoints();
            sp.SetExtent(0, DataCube.GetLength(2), 0, DataCube.GetLength(1), 0, DataCube.GetLength(0));

            float scale = 1;// 1f / widht;
            float origin = -1 * widht * scale / 2;

            sp.SetOrigin(origin, origin, origin);
            sp.SetDimensions(DataCube.GetLength(2), DataCube.GetLength(1), DataCube.GetLength(0));
            // sp.SetSpacing(1.0/widht, 1.0/widht, 1.0/widht);
            sp.SetSpacing(scale, scale, scale);
            sp.SetScalarTypeToFloat();
            sp.SetNumberOfScalarComponents(1);
            //sp.AllocateScalars();
            unsafe
            {
                float* volptr = (float*)sp.GetScalarPointer();
                fixed (float* pData = DataCube)
                {
                    CopyMemory((ushort*)(void*)volptr, (ushort*)(void*)pData, (uint)Buffer.ByteLength(DataCube));
                }
            }

            return sp;
        }
    }
}
