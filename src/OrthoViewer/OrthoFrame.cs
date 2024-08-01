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
using itk.simple;
using Emgu.CV;
using Emgu.CV.Structure;
using ZedGraph;

namespace OrthoViewer
{
    public partial class OrthoFrame : Form
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        static unsafe extern void CopyMemory(ushort* Destination, ushort* Source, uint Length);

        private Kitware.VTK.RenderWindowControl[] renderWindowControls;
        vtkRenderer[] Renderer;
        vtkRenderWindow[] RenderWindow;
        vtkRenderWindowInteractor[] Interactor;
        vtkInteractorStyle[] InteractorStyle;
        vtkCamera[] Camera;
        vtkImageActor[] ImageActors;
        vtkImageMapToWindowLevelColors WindowLevel = vtkImageMapToWindowLevelColors.New();

        public OrthoFrame()
        {
            InitializeComponent();
        }

        public void DisplayData(Image<Gray, float>[] Volume)
        {
            button1_Click(this, EventArgs.Empty);
            ShowVolumeData(ConvertEMGUtoVTK(Volume));
        }

        private Kitware.VTK.RenderWindowControl renderWindowControl1;
        private void button1_Click(object sender, EventArgs e)
        {
            renderWindowControls = new RenderWindowControl[4];
            Renderer = new vtkRenderer[4];
            RenderWindow = new vtkRenderWindow[4];
            Interactor = new vtkRenderWindowInteractor[4];
            Camera = new vtkCamera[4];
            ImageActors = new vtkImageActor[4];
            InteractorStyle = new vtkInteractorStyle[4];

            this.renderWindowControl1 = new Kitware.VTK.RenderWindowControl();
            this.renderWindowControl1.AddTestActors = false;
            this.renderWindowControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.renderWindowControl1.Location = new System.Drawing.Point(0, 0);
            this.renderWindowControl1.Name = "renderWindowControl1";
            this.renderWindowControl1.Size = new System.Drawing.Size(400, 400);

            this.renderWindowControl1.TabIndex = 0;
            this.renderWindowControl1.TestText = null;
            this.renderWindowControl1.Load += new EventHandler(renderWindowControl1_Load);

            this.Controls.Add(this.renderWindowControl1);

            this.renderWindowControl1 = new Kitware.VTK.RenderWindowControl();
            this.renderWindowControl1.AddTestActors = false;
            this.renderWindowControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.renderWindowControl1.Location = new System.Drawing.Point(400, 0);
            this.renderWindowControl1.Name = "renderWindowControl1";
            this.renderWindowControl1.Size = new System.Drawing.Size(400, 400);

            this.renderWindowControl1.TabIndex = 0;
            this.renderWindowControl1.TestText = null;
            this.renderWindowControl1.Load += new EventHandler(renderWindowControl2_Load);

            this.Controls.Add(this.renderWindowControl1);

            this.renderWindowControl1 = new Kitware.VTK.RenderWindowControl();
            this.renderWindowControl1.AddTestActors = false;
            this.renderWindowControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.renderWindowControl1.Location = new System.Drawing.Point(0, 400);
            this.renderWindowControl1.Name = "renderWindowControl1";
            this.renderWindowControl1.Size = new System.Drawing.Size(400, 400);

            this.renderWindowControl1.TabIndex = 0;
            this.renderWindowControl1.TestText = null;
            this.renderWindowControl1.Load += new EventHandler(renderWindowControl3_Load);

            this.Controls.Add(this.renderWindowControl1);

            this.renderWindowControl1 = new Kitware.VTK.RenderWindowControl();
            this.renderWindowControl1.AddTestActors = false;
            this.renderWindowControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.renderWindowControl1.Location = new System.Drawing.Point(400, 400);
            this.renderWindowControl1.Name = "renderWindowControl1";
            this.renderWindowControl1.Size = new System.Drawing.Size(400, 400);

            this.renderWindowControl1.TabIndex = 0;
            this.renderWindowControl1.TestText = null;
            this.renderWindowControl1.Load += new EventHandler(renderWindowControl4_Load);

            this.Controls.Add(this.renderWindowControl1);
        }

        void renderWindowControl1_Load(object sender, EventArgs e)
        {
            SetRenderControl((RenderWindowControl)sender, 0);
        }

        private void SetRenderControl(RenderWindowControl rendercontrol, int index)
        {
            renderWindowControls[index] = rendercontrol;
            RenderWindow[index] = renderWindowControls[index].RenderWindow;
            Renderer[index] = RenderWindow[index].GetRenderers().GetFirstRenderer();
            Interactor[index] = vtkRenderWindowInteractor.New();


            if (index != 0)
            {
                vtkInteractorStyleImage interactorStyle = vtkInteractorStyleImage.New();

                interactorStyle.MouseWheelForwardEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_MouseWheelForwardEvt);
                interactorStyle.MouseWheelBackwardEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_MouseWheelBackwardEvt);
                interactorStyle.MouseMoveEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_MouseMoveEvt);
                interactorStyle.LeftButtonReleaseEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_LeftButtonReleaseEvt);
                interactorStyle.LeftButtonPressEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_LeftButtonPressEvt);
                interactorStyle.RightButtonPressEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_RightButtonPressEvt);
                interactorStyle.RightButtonReleaseEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_RightButtonReleaseEvt);

                //if (Interactor)
                //  {
                //  if (!InteractorStyle)
                //    {
                //    InteractorStyle = vtkInteractorStyleImage::New();
                //    vtkImageViewer2Callback *cbk = vtkImageViewer2Callback::New();
                //    cbk.IV = this;
                //    InteractorStyle.AddObserver(
                //      vtkCommand::WindowLevelEvent, cbk);
                //    InteractorStyle.AddObserver(
                //      vtkCommand::StartWindowLevelEvent, cbk);
                //    InteractorStyle.AddObserver(
                //      vtkCommand::ResetWindowLevelEvent, cbk);
                //    cbk.Delete();
                //    }

                //  Interactor.SetInteractorStyle(InteractorStyle);
                //  Interactor.SetRenderWindow(RenderWindow);
                //  }

                interactorStyle.AutoAdjustCameraClippingRangeOn();
                Interactor[index].SetInteractorStyle(interactorStyle);
                InteractorStyle[index] = interactorStyle;



                ImageActors[index] = new vtkImageActor();

            }

            Interactor[index].SetRenderWindow(RenderWindow[index]);
            Interactor[index].Initialize();
            //Interactor[index].Start();
            Camera[index] = Renderer[index].GetActiveCamera();
        }

        void interactorStyle_RightButtonReleaseEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            RightButtonPressed = false;
        }

        void interactorStyle_RightButtonPressEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            RightButtonPressed = true;
            interactorStyle_MouseMoveEvt(sender, e);
        }

        bool LeftButtonPressed = false;
        bool RightButtonPressed = false;
        void interactorStyle_LeftButtonPressEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            LeftButtonPressed = true;
            int index = 0;
            for (int i = 0; i < InteractorStyle.Length; i++)
                if (InteractorStyle[i] == sender)
                    index = i;

            int[] testPoint = Interactor[index].GetEventPosition();

            switch (index)
            {
                case 1:

                    this.SliceY = (int)(testPoint[0] / (double)renderWindowControls[index].Width * GridSize[1]);
                    this.SliceZ = (int)(testPoint[1] / (double)renderWindowControls[index].Width * GridSize[1]);
                    Update(2);
                    Update(3);
                    //this.Slicex--;
                    break;
                case 2:
                    this.SliceY = (int)(testPoint[0] / (double)renderWindowControls[index].Width * GridSize[1]);
                    this.SliceX = (int)(testPoint[1] / (double)renderWindowControls[index].Width * GridSize[1]);
                    Update(1);
                    Update(3);
                    break;
                case 3:
                    this.SliceX = (int)(testPoint[0] / (double)renderWindowControls[index].Width * GridSize[1]);
                    this.SliceZ = (int)(testPoint[1] / (double)renderWindowControls[index].Width * GridSize[1]);
                    Update(2);
                    Update(1);
                    break;
            }
        }

        void interactorStyle_LeftButtonReleaseEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            LeftButtonPressed = false;

        }

        void interactorStyle_MouseMoveEvt(vtkObject sender, vtkObjectEventArgs e)
        {

            if (LeftButtonPressed)
            {
                int index = 0;
                for (int i = 0; i < InteractorStyle.Length; i++)
                    if (InteractorStyle[i] == sender)
                        index = i;

                int[] testPoint = Interactor[index].GetEventPosition();

                switch (index)
                {
                    case 1:
                        this.SliceY = (int)(testPoint[0] / (double)renderWindowControls[index].Width * GridSize[1]);
                        this.SliceZ = (int)(testPoint[1] / (double)renderWindowControls[index].Width * GridSize[1]);
                        Update(2);
                        Update(3);
                        break;
                    case 2:
                        this.SliceY = (int)(testPoint[0] / (double)renderWindowControls[index].Width * GridSize[1]);
                        this.SliceX = (int)(testPoint[1] / (double)renderWindowControls[index].Width * GridSize[1]);
                        Update(1);
                        Update(3);
                        break;
                    case 3:
                        this.SliceX = (int)(testPoint[0] / (double)renderWindowControls[index].Width * GridSize[1]);
                        this.SliceZ = (int)(testPoint[1] / (double)renderWindowControls[index].Width * GridSize[1]);
                        Update(2);
                        Update(1);
                        break;
                }
            }
            else if (RightButtonPressed)
            {

                int index = 0;
                for (int i = 0; i < InteractorStyle.Length; i++)
                    if (InteractorStyle[i] == sender)
                        index = i;

                int[] testPoint = Interactor[index].GetEventPosition();

                double window = (testPoint[0] / (double)renderWindowControls[index].Width * 200);
                double level = (testPoint[1] / (double)renderWindowControls[index].Width * 200);



                WindowLevel.SetWindow(window);
                WindowLevel.SetLevel(level);

                Update(2);
                Update(1);
                Update(3);
            }
        }

        void interactorStyle_MouseWheelBackwardEvt(vtkObject sender, vtkObjectEventArgs e)
        {

            int index = 0;
            for (int i = 0; i < InteractorStyle.Length; i++)
                if (InteractorStyle[i] == sender)
                    index = i;

            switch (index)
            {
                case 1:
                    this.SliceX--;
                    break;
                case 2:
                    this.SliceZ--;
                    break;
                case 3:
                    this.SliceY--;
                    break;
            }

            Update(index);
        }

        void interactorStyle_MouseWheelForwardEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            int index = 0;
            for (int i = 0; i < InteractorStyle.Length; i++)
                if (InteractorStyle[i] == sender)
                    index = i;

            switch (index)
            {
                case 1:
                    this.SliceX++;
                    break;
                case 2:
                    this.SliceZ++;
                    break;
                case 3:
                    this.SliceY++;
                    break;
            }

            Update(index);
        }

        private void Pipeline(int index)
        {
            GridSize = DensityGrid.GetWholeExtent();
            WindowLevel.SetInput(DensityGrid);

            // if (Renderer[index] != null && ImageActors[index] != null)
            {
                Renderer[index].AddActor2D(ImageActors[index]);
            }

            // if (ImageActors[index] != null && WindowLevel != null)
            {

                WindowLevel.SetLevel(Level);
                WindowLevel.SetWindow(Window);
                ImageActors[index].SetInput(WindowLevel.GetOutput());
            }

            Orientations orientation = Orientations.XY;
            switch (index)
            {
                case 1:
                    orientation = Orientations.XY;
                    break;
                case 2:
                    orientation = Orientations.XZ;
                    break;
                case 3:
                    orientation = Orientations.YZ;
                    break;
            }

            float cX = 0;// (w_ext[1] + w_ext[0]) / 2;
            float cY = 0;// (w_ext[3] + w_ext[2]) / 2;
            float cZ = 0;// (w_ext[5] + w_ext[4]) / 2;

            Camera[index].SetFocalPoint(cX, cY, cZ);
            switch (orientation)
            {
                case Orientations.XY:
                    {
                        Camera[index].SetPosition(cX, cY, cZ + 1);//0, 0, 1); // -1 if medical ?
                        Camera[index].SetViewUp(0, 1, 0);
                    }
                    break;

                case Orientations.YZ:
                    {
                        Camera[index].SetPosition(cX, cY - 1, cZ);//0, -1, 0); // -1 if medical ?
                        Camera[index].SetViewUp(0, 0, 1);
                    }
                    break;

                case Orientations.XZ:
                    {
                        Camera[index].SetPosition(cX - 1, cY, cZ);//- 1, 0, 0); // 1 if medical ?
                        Camera[index].SetViewUp(0, 0, 1);
                    }
                    break;
            }


        }

        void renderWindowControl2_Load(object sender, EventArgs e)
        {
            SetRenderControl((RenderWindowControl)sender, 1);
        }
        void renderWindowControl3_Load(object sender, EventArgs e)
        {
            SetRenderControl((RenderWindowControl)sender, 2);
        }
        void renderWindowControl4_Load(object sender, EventArgs e)
        {
            SetRenderControl((RenderWindowControl)sender, 3);
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


        private vtkImageData ConvertEMGUtoVTK(Image<Gray, float>[] Volume)
        {
            float width = Volume[0].Width;//.GetLength(0);

            vtkStructuredPoints sp = new vtkStructuredPoints();
            sp.SetExtent(0, (int)Volume.Length, 0, (int)Volume[0].Height, 0, (int)Volume[0].Width);

            float scale = 1;// 1f / widht;
            float origin = -1 * width * scale / 2;

            sp.SetOrigin(origin, origin, origin);
            sp.SetDimensions((int)Volume.Length, (int)Volume[0].Height, (int)Volume[0].Width);
            // sp.SetSpacing(1.0/widht, 1.0/widht, 1.0/widht);
            sp.SetSpacing(scale, scale, scale);

            sp.SetScalarTypeToFloat();


            sp.SetNumberOfScalarComponents(1);
            sp.AllocateScalars();
            unsafe
            {
                float* volptr = (float*)sp.GetScalarPointer();

                for (int i = 0; i < Volume.Length; i++)
                {
                    fixed (float* pData = Volume[i].Data)
                    {
                        CopyMemory((ushort*)(void*)volptr, (ushort*)(void*)pData, (uint)(Volume[i].Data.LongLength * sizeof(float)));
                    }
                    volptr += Volume[i].Data.LongLength;
                }
            }

            return sp;
        }


        public enum Orientations : int
        {
            XY = 0, YZ = 1, XZ = 2
        }

        int SliceX = 100, SliceY = 100, SliceZ = 150;
        double Window = 188, Level = 118;
        int[] GridSize;
        vtkImageData DensityGrid;
        bool FirstRun = false;

        public void Update(int index)
        {

            if (SliceX < 0) SliceX = 0;
            if (SliceX > GridSize[1]) SliceX = GridSize[1];


            if (SliceY < 0) SliceY = 0;
            if (SliceY > GridSize[3]) SliceY = GridSize[3];


            if (SliceZ < 0) SliceZ = 0;
            if (SliceZ > GridSize[5]) SliceZ = GridSize[5];

            lbStatus.Text = "Window: " + Math.Round(Window) + "    Level: " + Math.Round(Level) + " SliceX: " + SliceX + " SliceY: " + SliceY + " SliceZ: " + SliceZ;

            Orientations orientation = Orientations.XY;
            switch (index)
            {
                case 1:
                    orientation = Orientations.XY;
                    break;
                case 2:
                    orientation = Orientations.XZ;
                    break;
                case 3:
                    orientation = Orientations.YZ;
                    break;
            }

            switch (orientation)
            {
                case Orientations.XY:
                    this.ImageActors[index].SetDisplayExtent(GridSize[0], GridSize[1], GridSize[2], GridSize[3], this.SliceX, this.SliceX);
                    break;

                case Orientations.YZ:
                    this.ImageActors[index].SetDisplayExtent(GridSize[0], GridSize[1], this.SliceY, this.SliceY, GridSize[4], GridSize[5]);
                    break;

                case Orientations.XZ:
                    this.ImageActors[index].SetDisplayExtent(this.SliceZ, this.SliceZ, GridSize[2], GridSize[3], GridSize[4], GridSize[5]);
                    break;
            }

            // Figure out the correct clipping range
            int xs = 0, ys = 0;

            switch (orientation)
            {
                case Orientations.XY:
                default:
                    xs = GridSize[1] - GridSize[0] + 1;
                    ys = GridSize[3] - GridSize[2] + 1;
                    break;

                case Orientations.YZ:
                    xs = GridSize[1] - GridSize[0] + 1;
                    ys = GridSize[5] - GridSize[4] + 1;
                    break;

                case Orientations.XZ:

                    xs = GridSize[3] - GridSize[2] + 1;
                    ys = GridSize[5] - GridSize[4] + 1;
                    break;
            }

            Renderer[index].ResetCamera();
            Camera[index].ParallelProjectionOn();
            Camera[index].SetParallelScale(xs < 150 ? 75 : (xs - 1) / 2.0);
            FirstRun = true;

            this.Renderer[index].ResetCameraClippingRange();


            RenderWindow[index].Render();
        }


        public void PlotOpacity()
        {

            GraphPane myPane = zedGraphControl1.GraphPane;

            // Set the title and axis labels
            myPane.Title.Text = "";
            myPane.XAxis.Title.Text = "Intensity";
            myPane.YAxis.Title.Text = "Opacity";

            //myPane.YAxis.Scale = ZedGraph.AxisType.Log;

            // enter some arbitrary data points
            double[] x = { 0, 255 };
            double[] y = { 0, .05 };

            // Add a green curve
            LineItem curve;
            curve = myPane.AddCurve("Opacity", x, y, Color.Green, SymbolType.Circle);
            curve.Line.Width = 1.5F;
            // Make the curve smooth with cardinal splines
            curve.Line.IsSmooth = true;
            curve.Line.SmoothTension = 0.6F;
            // Fill the symbols with white to make them opaque
            curve.Symbol.Fill = new Fill(Color.White);
            curve.Symbol.Size = 10;


            // Show the x and y axis gridlines
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;

            myPane.LineType = LineType.Normal;

            zedGraphControl1.AxisChange();

            zedGraphControl1.Update();
        }

        public void ShowVolumeData(vtkImageData DensityGrid_t)
        {
            double[] range = DensityGrid_t.GetScalarRange();

            vtkImageShiftScale shifter = new vtkImageShiftScale();
            shifter.SetShift(-1 * range[0]);
            shifter.SetScale(255 / (range[1] - range[0]));
            shifter.SetInput(DensityGrid_t);
            shifter.SetOutputScalarTypeToUnsignedChar();
            shifter.Update();

            DensityGrid = shifter.GetOutput();

            int midVTK = 100;

            vtkFixedPointVolumeRayCastMapper texMapper = vtkFixedPointVolumeRayCastMapper.New();

            //Go through the visulizatin pipeline
            texMapper.SetInput(shifter.GetOutput());

            //Set the color curve for the volume
            ctf.AddHSVPoint(0, 0, 0, 0);
            ctf.AddHSVPoint(ScaleVTK, 1, 1, 1);

            //Set the opacity curve for the volume
            spwf.AddPoint(0, 0);
            //spwf.AddPoint(midVTK * 3, 0);
            spwf.AddPoint(ScaleVTK, .05);



            //Set the gradient curve for the volume
            gpwf.AddPoint(0, 0);
            //gpwf.AddPoint(midVTK * 3, 0);
            gpwf.AddPoint(ScaleVTK, 1);

            vol.GetProperty().SetColor(ctf);
            vol.GetProperty().SetScalarOpacity(spwf);
            //  vol.GetProperty().SetGradientOpacity(gpwf);
            vol.SetMapper(texMapper);

            //Go through the Graphics Pipeline
            Renderer[0].AddVolume(vol);
            // camera[0].ParallelProjectionOn();
            Camera[0].SetFocalPoint(0, 0, 0);
            Camera[0].SetPosition(0, 0, 400);
            // ren1[0].ResetCamera();
            renderWindowControls[0].Update();
            renderWindowControls[0].Refresh();

            //ImageActors[1].SetInput(DensityGrid);
            //ImageActors[2].SetInput(DensityGrid);
            //ImageActors[3].SetInput(DensityGrid);


            Pipeline(1);
            Pipeline(2);
            Pipeline(3);


            Update(1);
            Update(2);
            Update(3);

            PlotOpacity();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);

            ImageFileReader reader = new ImageFileReader();
            reader.SetFileName(@"c:\temp\volume.mhd");
            itk.simple.Image volImage = reader.Execute();

            ShowVolumeData(ConvertITKtoVTK(volImage));
        }

        private void zedGraphControl1_Load(object sender, EventArgs e)
        {

        }

        private void OrthoFrame_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                for (int i = 0; i < renderWindowControls.Length; i++)
                    renderWindowControls[i] = null;
            }
            catch
            { }
            try
            {
                for (int i = 0; i < Renderer.Length; i++)
                {
                    Renderer[i] = null;

                }
            }
            catch
            { }
            try
            {
                for (int i = 0; i < RenderWindow.Length; i++)
                    RenderWindow[i] = null;
            }
            catch
            { }
            try
            {
                for (int i = 0; i < Interactor.Length; i++)
                    Interactor[i] = null;
            }
            catch
            { }
            try
            {
                for (int i = 0; i < InteractorStyle.Length; i++)
                    InteractorStyle[i] = null;
            }
            catch
            { }
            try
            {
                for (int i = 0; i < Camera.Length; i++)
                    Camera[i] = null;
            }
            catch
            { }
            try
            {
                for (int i = 0; i < ImageActors.Length; i++)
                    ImageActors[i] = null;
            }
            catch
            { }
        }

        private string zedGraphControl1_PointEditEvent(ZedGraph.ZedGraphControl sender, ZedGraph.GraphPane pane, ZedGraph.CurveItem curve, int iPt)
        {
            spwf.RemoveAllPoints();
            for (int i = 0; i < curve.Points.Count; i++)
            {
                spwf.AddPoint(curve.Points[i].X, curve.Points[i].Y);
            }

            renderWindowControls[0].Update();
            renderWindowControls[0].Refresh();

           
            return "";
        }

        private string zedGraphControl1_PointValueEvent(ZedGraph.ZedGraphControl sender, ZedGraph.GraphPane pane, ZedGraph.CurveItem curve, int iPt)
        {
          
            return default(string);
        }

        private int zedGraphControl1_AddPointEditEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt, PointF mousePT)
        {
            List<double> x = new List<double>();
            List<double> y = new List<double>();

            int insertAt = 1;
            for (int i = 0; i < curve.Points.Count; i++)
            {
                x.Add(curve.Points[i].X);
                y.Add(curve.Points[i].Y);

                if (mousePT.X > x[i]) insertAt = i + 1;
            }

            if (insertAt >= curve.Points.Count - 1)
                insertAt = curve.Points.Count - 1;

            x.Insert(insertAt, mousePT.X);
            y.Insert(insertAt, mousePT.Y);

            curve.Clear();
            spwf.RemoveAllPoints();
            for (int i = 0; i < x.Count; i++)
            {
                curve.AddPoint(x[i], y[i]);
                spwf.AddPoint(x[i], y[i]);
            }

            renderWindowControls[0].Update();
            renderWindowControls[0].Refresh();

            
            return insertAt;
        }
    }
}
