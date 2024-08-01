using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Kitware.VTK;
using itk;
using itk.simple;

namespace ViewAndSegment
{
    public partial class VTK_Viewer : Form
    {


        public VTK_Viewer()
        {
            InitializeComponent();
            this.renderWindowControl1 = InitializedRenderControl(new SliceView());
            this.renderWindowControl2 = InitializedRenderControl(new SliceView());
            this.renderWindowControl3 = InitializedRenderControl(new SliceView());
            this.renderWindowControl4 = InitializedRenderControl(new SliceView());

            this.tableLayoutPanel1.Controls.Add(this.renderWindowControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.renderWindowControl2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.renderWindowControl3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.renderWindowControl4, 1, 1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SegmentVolume(@"c:\temp\volume.mhd");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }


        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        static unsafe extern void CopyMemory(ushort* Destination, ushort* Source, uint Length);

        private SliceView renderWindowControl1;
        private SliceView renderWindowControl2;
        private SliceView renderWindowControl3;
        private SliceView renderWindowControl4;

        vtkImageMapToWindowLevelColors WindowLevel;// = vtkImageMapToWindowLevelColors.New();

        public void SetContrast(double level, double window)
        {
            WindowLevel.SetLevel(level);
            WindowLevel.SetWindow(window);

            renderWindowControl2.Refresh();
            renderWindowControl2.Update();

            renderWindowControl3.Refresh();
            renderWindowControl3.Update();

            renderWindowControl4.Refresh();
            renderWindowControl4.Update();
        }


        private SliceView InitializedRenderControl(SliceView control)
        {

            control.AddTestActors = false;
            control.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            control.Location = new System.Drawing.Point(3, 3);
            control.Name = "renderWindowControl1";
            control.Size = new System.Drawing.Size(this.Width, this.Height);
            control.Dock = DockStyle.Fill;
            control.TabIndex = 0;
            control.TestText = null;

            return control;

        }

        vtkColorTransferFunction ctf = vtkColorTransferFunction.New();
        vtkPiecewiseFunction spwf = vtkPiecewiseFunction.New();
        vtkPiecewiseFunction gpwf = vtkPiecewiseFunction.New();
        vtkVolume vol = vtkVolume.New();
        double ScaleVTK = 255;



        private vtkImageData ConvertITKtoVTK(itk.simple.Image dataCube)
        {
            float width = dataCube.GetWidth();//.GetLength(0);

            vtkStructuredPoints sp = new vtkStructuredPoints();
            sp.SetExtent(0, (int)dataCube.GetWidth(), 0, (int)dataCube.GetHeight(), 0, (int)dataCube.GetDepth());

            float scale = 1;// 1f / widht;
            float origin = -1 * width * scale / 2;

            sp.SetOrigin(origin, origin, origin);
            sp.SetDimensions((int)dataCube.GetWidth(), (int)dataCube.GetHeight(), (int)dataCube.GetDepth());
            // sp.SetSpacing(1.0/widht, 1.0/widht, 1.0/widht);
            sp.SetSpacing(scale, scale, scale);
            if (dataCube.GetPixelIDTypeAsString() == "16-bit signed integer")
                sp.SetScalarTypeToShort();
            else if (dataCube.GetPixelIDTypeAsString() == "32-bit float")
                sp.SetScalarTypeToFloat();
            else
                sp.SetScalarTypeToUnsignedChar();

            sp.SetNumberOfScalarComponents(1);
            sp.AllocateScalars();
            unsafe
            {
                float* volptr = (float*)sp.GetScalarPointer();
                if (dataCube.GetPixelIDTypeAsString() == "16-bit signed integer")
                {
                    float* pData = (float*)dataCube.GetBufferAsInt16();
                    CopyMemory((ushort*)(void*)volptr, (ushort*)(void*)pData, (uint)(dataCube.GetWidth() * (int)dataCube.GetHeight() * (int)dataCube.GetDepth() * 2));
                }
                else if (dataCube.GetPixelIDTypeAsString() == "32-bit float")
                {
                    float* pData = (float*)dataCube.GetBufferAsFloat();
                    CopyMemory((ushort*)(void*)volptr, (ushort*)(void*)pData, (uint)(dataCube.GetWidth() * (int)dataCube.GetHeight() * (int)dataCube.GetDepth() * sizeof(float)));

                }
                else
                {
                    float* pData = (float*)dataCube.GetBufferAsUInt8();
                    CopyMemory((ushort*)(void*)volptr, (ushort*)(void*)pData, (uint)(dataCube.GetWidth() * (int)dataCube.GetHeight() * (int)dataCube.GetDepth() * 1));

                }
            }

            return sp;
        }

        public void ShowVolumeData(itk.simple.Image DataCube)
        {

            itk.simple.StatisticsImageFilter stats = new StatisticsImageFilter();
            stats.Execute(DataCube);

            double minVTK = stats.GetMinimum();
            double midVTK = stats.GetMean();
            ScaleVTK = stats.GetMaximum();

            vtkImageData sp = ConvertITKtoVTK(DataCube);

            vtkFixedPointVolumeRayCastMapper texMapper = vtkFixedPointVolumeRayCastMapper.New();

            //Go through the visulizatin pipeline
            texMapper.SetInput(sp);

            //Set the color curve for the volume
            ctf.AddHSVPoint(0, 0, 0, 0);
            ctf.AddHSVPoint(midVTK, 1, 1, 1);

            //Set the opacity curve for the volume
            spwf.AddPoint(0, 0);
            spwf.AddPoint(midVTK * 3, 0);
            spwf.AddPoint(ScaleVTK, .05);

            //Set the gradient curve for the volume
            gpwf.AddPoint(0, 0);
            gpwf.AddPoint(midVTK * 3, 0);
            gpwf.AddPoint(ScaleVTK, .1);

            vol.GetProperty().SetColor(ctf);
            vol.GetProperty().SetScalarOpacity(spwf);
            //  vol.GetProperty().SetGradientOpacity(gpwf);
            vol.SetMapper(texMapper);

            //Go through the Graphics Pipeline
            renderWindowControl1.Renderer.AddVolume(vol);
            renderWindowControl1.Camera.ParallelProjectionOn();
            renderWindowControl1.Camera.SetFocalPoint(0, 0, 0);
            renderWindowControl1.Camera.SetPosition(0, 0, 200);
            renderWindowControl1.Camera.SetParallelScale(200);

            renderWindowControl1.Refresh();
            renderWindowControl1.Update();

            WindowLevel = vtkImageMapToWindowLevelColors.New();
            WindowLevel.SetInput(sp);
            double[] range = sp.GetScalarRange();
            double window = range[1] - range[0];
            double level = 0.5 * (range[1] + range[0]);
            WindowLevel.SetWindow(window);
            WindowLevel.SetLevel(level);
            WindowLevel.Update();

            renderWindowControl2.CreateSliceViewer(this, WindowLevel.GetOutput(), 0);
            renderWindowControl2.UpSlicer = renderWindowControl3;
            renderWindowControl2.AccrossSlicer = renderWindowControl4;

            //renderWindowControl3.CreateSliceViewer(this, WindowLevel.GetOutput(), 0);
            //renderWindowControl3.UpSlicer = renderWindowControl2;
            //renderWindowControl3.AccrossSlicer = renderWindowControl4;

            renderWindowControl4.CreateSliceViewer(this, WindowLevel.GetOutput(), 2);
            renderWindowControl4.UpSlicer = renderWindowControl3;
            renderWindowControl4.AccrossSlicer = renderWindowControl2;

            renderWindowControl2.Refresh();
            //   renderWindowControl3.Refresh();
            renderWindowControl4.Refresh();
        }


        public void SegmentVolume(string filename)
        {

            ImageFileReader reader = new ImageFileReader();
            reader.SetFileName(filename);
            itk.simple.Image volImage = reader.Execute();
            ShowVolumeData(volImage);

            //Int16[, ,] densityGrid = new Int16[volImage.GetWidth(), volImage.GetHeight(), volImage.GetDepth()];

            //unsafe
            //{
            //    fixed (Int16* pDensity = densityGrid)
            //    {
            //        CopyMemory((ushort*)(void*)pDensity, (ushort*)volImage.GetBufferAsInt16(), (uint)Buffer.ByteLength(densityGrid));
            //    }
            //}

            //float[, ,] densityGridF = new float[volImage.GetWidth(), volImage.GetHeight(), volImage.GetDepth()];
            //for (int i = 0; i < densityGridF.GetLength(0); i++)
            //{
            //    for (int j = 0; j < densityGridF.GetLength(1); j++)
            //    {
            //        for (int k = 0; k < densityGridF.GetLength(2); k++)
            //        {
            //            densityGridF[i, j, k] = densityGrid[i, j, k];
            //        }
            //    }
            //}

            //Tuple<double[, ,], double[, ,]> Masks = HolesOtsu.DoHoleOtsu(densityGridF);

            //itk.simple.Image cyto = new itk.simple.Image(volImage.GetWidth(), volImage.GetHeight(), volImage.GetDepth(), PixelIDValueEnum.sitkFloat64);

            //double[, ,] cytoF = Masks.Item1;
            //unsafe
            //{
            //    fixed (double* pCytoF = cytoF)
            //    {
            //        CopyMemory((ushort*)cyto.GetBufferAsDouble(), (ushort*)(void*)pCytoF, (uint)Buffer.ByteLength(densityGrid));
            //    }
            //}

            //vtkImageData sp = ConvertITKtoVTK(cyto);

            //vtkImageCast caster = new vtkImageCast();
            //caster.SetInput(sp);
            //caster.SetOutputScalarTypeToShort();
            //caster.Update();

            //vtkImageShiftScale shifter = new vtkImageShiftScale();
            //shifter.SetShift(0);
            //shifter.SetScale(1d);
            //shifter.SetInput(caster.GetOutput());
            //shifter.SetOutputScalarTypeToUnsignedChar();
            //shifter.Update();

            //renderWindowControl3.CreateSliceViewer(this, shifter.GetOutput(), 0);

            //renderWindowControl3.Refresh();
        }

        public void LoadData(string filename)
        {
            ImageFileReader reader = new ImageFileReader();
            reader.SetFileName(filename);
            itk.simple.Image volImage = reader.Execute();
            ShowVolumeData(volImage);


            itk.simple.GradientMagnitudeRecursiveGaussianImageFilter otsuThreshold = new GradientMagnitudeRecursiveGaussianImageFilter();
            // otsuThreshold.SetNumberOfHistogramBins(255);
            otsuThreshold.SetSigma(500);
            volImage = otsuThreshold.Execute(volImage);

            vtkImageData sp = ConvertITKtoVTK(volImage);

            //vtkImageCast caster = new vtkImageCast();
            //caster.SetInput(sp);
            //caster.SetOutputScalarTypeToShort();
            //caster.Update();

            vtkImageShiftScale shifter = new vtkImageShiftScale();
            shifter.SetShift(0);
            shifter.SetScale(1d);
            shifter.SetInput(sp);
            shifter.SetOutputScalarTypeToUnsignedChar();
            shifter.Update();

            renderWindowControl3.CreateSliceViewer(this, shifter.GetOutput(), 0);
            //renderWindowControl3.CreateSliceViewer(this, caster.GetOutput(), 0);

            renderWindowControl3.Refresh();
            Console.WriteLine("Loaded");
        }
    }
}
