using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using itk;
using itk.simple;
using Kitware.VTK;
using System.Diagnostics;
namespace ViewAndSegment
{
    class SliceView : RenderWindowControl
    {
        public vtkRenderer Renderer
        {
            get
            {
                return this.RenderWindow.GetRenderers().GetFirstRenderer();
            }
        }

        vtkRenderWindowInteractor _myInteractor;
        public vtkRenderWindowInteractor Interactor
        {
            get { return _myInteractor; }
        }

        public vtkCamera Camera
        {
            get
            {
                return Renderer.GetActiveCamera();
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _myInteractor = vtkRenderWindowInteractor.New();
            _myInteractor.SetRenderWindow(RenderWindow);
        }


        public int CurrentSlice
        {
            get
            {
                return Slice;
            }

            set
            {
                Slice = value;
                _SliceStatusMapper.SetInput("Slice No " + (Slice + 1).ToString() + "/" + (_MaxSlice + 1).ToString());
                SetSlicePosition(Slice);
            }
        }

        public void SliceAtPercent(double slicePercent)
        {

            Slice = (int)(_MinSlice + (_MaxSlice - _MinSlice) * slicePercent);

            if (Slice < _MinSlice) Slice = _MinSlice;
            if (Slice > _MaxSlice) Slice = _MaxSlice;


            SetSlicePosition(Slice);
            _SliceStatusMapper.SetInput("Slice No " + (Slice + 1).ToString() + "/" + (_MaxSlice + 1).ToString());
            RenderWindow.Render();
        }

        public SliceView UpSlicer
        {
            get;
            set;
        }
        public SliceView AccrossSlicer
        {
            get;
            set;
        }


        // module wide accessible variables
        // vtkImageViewer2 _ImageViewer;
        vtkTextMapper _SliceStatusMapper;
        int Slice;
        int _MinSlice;
        int _MaxSlice;

        int[] w_ext;
        vtkImageActor ImageActor;
        vtkImageClip Clip;

        int SliceOrientation = 0;
        //vtkRenderWindowInteractor InteractorStyle;

        VTK_Viewer ParentViewer;

        public void SegmentationViewer(VTK_Viewer parent, vtkImageData volume, int orientation)
        {
         
            Clip = vtkImageClip.New();
            vtkContourFilter contour = vtkContourFilter.New();
            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            vtkActor actor = vtkActor.New();

            int[] extent = volume.GetWholeExtent();
            Clip.SetInput(volume);
            Clip.SetOutputWholeExtent(extent[0], extent[1], extent[2], extent[3],
                        Slice,
                        Slice);

            contour.SetInputConnection(Clip.GetOutputPort());
            contour.SetValue(0, 100);

            mapper.SetInputConnection(contour.GetOutputPort());
            mapper.SetScalarVisibility(1);

            //Go through the graphics pipeline
            actor.SetMapper(mapper);
            actor.GetProperty().SetColor(0, 1, 0);

            Renderer.AddActor(actor);
        }

        public void CreateSliceViewer(VTK_Viewer parent, vtkImageData volume, int orientation)
        {
            ParentViewer = parent;
            //Create all the objects for the pipeline
            SliceOrientation = orientation;

            ImageActor = vtkImageActor.New();
            // clip = vtkImageClip.New();
            vtkContourFilter contour = vtkContourFilter.New();
            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            vtkActor actor = vtkActor.New();

            ImageActor.SetInput(volume);

            //Go through the visulization pipeline

            Renderer.AddActor(ImageActor);

            int[] extent = volume.GetWholeExtent();
            w_ext = extent;


            _MinSlice = 0;
            _MaxSlice = extent[1];

            switch (orientation)
            {
                case 0:
                    ImageActor.SetDisplayExtent(extent[0], extent[1], extent[2], extent[3],
                                (extent[4] + extent[5]) / 2,
                                (extent[4] + extent[5]) / 2);
                    break;
                case 1:
                    ImageActor.SetDisplayExtent(extent[0], extent[1], (extent[2] + extent[3]) / 2, (extent[2] + extent[3]) / 2, extent[4], extent[5]);
                    break;
                case 2:
                    ImageActor.SetDisplayExtent((extent[0] + extent[1]) / 2, (extent[0] + extent[1]) / 2, extent[2], extent[3], extent[4], extent[5]);
                    break;
            }



            var imageData = volume;
            double[] bounds = imageData.GetBounds();
            double[] spacing = imageData.GetSpacing();
            extent = imageData.GetExtent();

            vtkCamera camera = Renderer.GetActiveCamera();

            camera.ParallelProjectionOn();

            double xc = (bounds[0] + bounds[1]) / 2;
            double yc = (bounds[2] + bounds[3]) / 2;
            double zc = (bounds[4] + bounds[5]) / 2;

            double yd = (extent[3] - extent[2] + 1) * spacing[1];
            double d = camera.GetDistance() * 2;
            camera.SetParallelScale(0.5 * yd);
            camera.SetFocalPoint(xc, yc, zc);

            camera.SetThickness(2 * extent[1]);
            switch (orientation)
            {
                case 0:
                    camera.SetPosition(xc, yc, d);
                    break;

                case 1:
                    camera.SetPosition(xc, -1 * d, yc);
                    break;

                case 2:
                    camera.SetPosition(d, yc, xc);
                    break;

            }

            vtkInteractorStyleImage interactorStyle = vtkInteractorStyleImage.New();

            interactorStyle.MouseWheelForwardEvt += new vtkObject.vtkObjectEventHandler(interactor_MouseWheelForwardEvt);
            interactorStyle.MouseWheelBackwardEvt += new vtkObject.vtkObjectEventHandler(interactor_MouseWheelBackwardEvt);
            interactorStyle.MouseMoveEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_MouseMoveEvt);
            interactorStyle.LeftButtonReleaseEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_LeftButtonReleaseEvt);
            interactorStyle.LeftButtonPressEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_LeftButtonPressEvt);
            interactorStyle.RightButtonPressEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_RightButtonPressEvt);
            interactorStyle.RightButtonReleaseEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_RightButtonReleaseEvt);


            RenderWindow.GetInteractor().SetInteractorStyle(interactorStyle);

            vtkTextProperty sliceTextProp = vtkTextProperty.New();
            sliceTextProp.SetFontFamilyToCourier();
            sliceTextProp.SetFontSize(20);
            sliceTextProp.SetVerticalJustificationToBottom();
            sliceTextProp.SetJustificationToLeft();

            _SliceStatusMapper = vtkTextMapper.New();
            _SliceStatusMapper.SetInput("Slice No " + (Slice + 1).ToString() + "/" + (_MaxSlice + 1).ToString());
            _SliceStatusMapper.SetTextProperty(sliceTextProp);

            vtkActor2D sliceStatusActor = vtkActor2D.New();
            sliceStatusActor.SetMapper(_SliceStatusMapper);
            sliceStatusActor.SetPosition(10, 10);

            Renderer.AddActor2D(sliceStatusActor);
        }

        //public void CreateSliceViewer2(vtkStructuredPoints volume, int orientation)
        //{
        //    SliceOrientation = orientation;

        //    ImageActor = new vtkImageActor();
        //    double[] range = volume.GetScalarRange();

        //    WindowLevel.SetInput(volume);

        //    double window = range[1] - range[0];
        //    double level = 0.5 * (range[1] + range[0]);
        //    WindowLevel.SetWindow(window);
        //    WindowLevel.SetLevel(level);

        //    WindowLevel.Update();

        //    this.ImageActor.SetInput(WindowLevel.GetOutput());

        //    vtkStructuredPoints input = volume;
        //    input.UpdateInformation();
        //    w_ext = input.GetWholeExtent();

        //    // Is the slice in range ? If not, fix it

        //    _MinSlice = w_ext[this.SliceOrientation * 2];
        //    _MaxSlice = w_ext[this.SliceOrientation * 2 + 1];
        //    if (this.Slice < _MinSlice || this.Slice > _MaxSlice)
        //    {
        //        this.Slice = (int)((_MinSlice + _MaxSlice) * 0.5);
        //    }

        //    // Set the image actor
        //    Slice = 128;

        //    SetSlicePosition(Slice);

        //    Renderer.AddActor(ImageActor);
        //    // Figure out the correct clipping range
        //    //InteractorStyle = RenderWindow.GetInteractor();


        //    //vtkInteractorStyle style = new vtkInteractorStyleImage();
        //    //InteractorStyle.SetInteractorStyle(style);

        //    vtkCamera cam = this.Renderer.GetActiveCamera();

        //    cam.ParallelProjectionOn();
        //    cam.SetParallelScale(w_ext[1] / 2);

        //    if (orientation == 0)
        //        cam.SetPosition(0, 0, 100);
        //    if (orientation == 1)
        //        cam.SetPosition(0, 100, 0);
        //    if (orientation == 2)
        //        cam.SetPosition(100, 0, 0);

        //    this.Renderer.ResetCameraClippingRange();


        //    vtkInteractorStyleImage interactorStyle = vtkInteractorStyleImage.New();

        //    interactorStyle.MouseWheelForwardEvt += new vtkObject.vtkObjectEventHandler(interactor_MouseWheelForwardEvt);
        //    interactorStyle.MouseWheelBackwardEvt += new vtkObject.vtkObjectEventHandler(interactor_MouseWheelBackwardEvt);
        //    interactorStyle.MouseMoveEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_MouseMoveEvt);
        //    interactorStyle.LeftButtonReleaseEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_LeftButtonReleaseEvt);
        //    interactorStyle.LeftButtonPressEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_LeftButtonPressEvt);
        //    interactorStyle.RightButtonPressEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_RightButtonPressEvt);
        //    interactorStyle.RightButtonReleaseEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_RightButtonReleaseEvt);


        //    RenderWindow.GetInteractor().SetInteractorStyle(interactorStyle);

        //    this.Refresh();
        //    this.Update();
        //}

        private void SetSlicePosition(int sliceValue)
        {
            Slice = sliceValue;


            switch (SliceOrientation)
            {
                case 0:
                    ImageActor.SetDisplayExtent(w_ext[0], w_ext[1], w_ext[2], w_ext[3], Slice, Slice);


                    // clip.SetOutputWholeExtent(w_ext[0], w_ext[1], w_ext[2], w_ext[3],                                Slice,                                Slice);
                    break;
                case 1:
                    ImageActor.SetDisplayExtent(w_ext[0], w_ext[1], Slice, Slice, w_ext[4], w_ext[5]);
                    // clip.SetOutputWholeExtent(w_ext[0], w_ext[1], Slice, Slice, w_ext[4], w_ext[5]);
                    break;
                case 2:
                    ImageActor.SetDisplayExtent(Slice, Slice, w_ext[2], w_ext[3], w_ext[4], w_ext[5]);
                    //  clip.SetOutputWholeExtent(Slice, Slice, w_ext[2], w_ext[3], w_ext[4], w_ext[5]);
                    break;
            }



            this.Renderer.ResetCameraClippingRange();

        }

        //public void CreateSliceViewer0(vtkStructuredPoints volume, int orientation)
        //{
        //    _ImageViewer = vtkImageViewer2.New();
        //    _ImageViewer.SetSliceOrientation(orientation);
        //    _ImageViewer.SetInput(volume);
        //    // get range of slices (min is the first index, max is the last index)
        //    _ImageViewer.GetSliceRange(ref _MinSlice, ref _MaxSlice);
        //    Slice = (_MinSlice + _MaxSlice) / 2;

        //    // slice status message
        //    vtkTextProperty sliceTextProp = vtkTextProperty.New();
        //    sliceTextProp.SetFontFamilyToCourier();
        //    sliceTextProp.SetFontSize(20);
        //    sliceTextProp.SetVerticalJustificationToBottom();
        //    sliceTextProp.SetJustificationToLeft();

        //    _SliceStatusMapper = vtkTextMapper.New();
        //    _SliceStatusMapper.SetInput("Slice No " + (Slice + 1).ToString() + "/" + (_MaxSlice + 1).ToString());
        //    _SliceStatusMapper.SetTextProperty(sliceTextProp);

        //    vtkActor2D sliceStatusActor = vtkActor2D.New();
        //    sliceStatusActor.SetMapper(_SliceStatusMapper);
        //    sliceStatusActor.SetPosition(10, 10);

        //    vtkInteractorStyleImage interactorStyle = vtkInteractorStyleImage.New();

        //    interactorStyle.MouseWheelForwardEvt += new vtkObject.vtkObjectEventHandler(interactor_MouseWheelForwardEvt);
        //    interactorStyle.MouseWheelBackwardEvt += new vtkObject.vtkObjectEventHandler(interactor_MouseWheelBackwardEvt);
        //    interactorStyle.MouseMoveEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_MouseMoveEvt);
        //    interactorStyle.LeftButtonReleaseEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_LeftButtonReleaseEvt);
        //    interactorStyle.LeftButtonPressEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_LeftButtonPressEvt);
        //    interactorStyle.RightButtonPressEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_RightButtonPressEvt);
        //    interactorStyle.RightButtonReleaseEvt += new vtkObject.vtkObjectEventHandler(interactorStyle_RightButtonReleaseEvt);


        //    RenderWindow.GetInteractor().SetInteractorStyle(interactorStyle);
        //    RenderWindow.GetRenderers().InitTraversal();
        //    vtkRenderer ren;
        //    while ((ren = RenderWindow.GetRenderers().GetNextItem()) != null)
        //        ren.SetBackground(0.0, 0.0, 0.0);

        //    _ImageViewer.SetRenderWindow(RenderWindow);
        //    _ImageViewer.GetRenderer().AddActor2D(sliceStatusActor);


        //    _ImageViewer.SetSlice(Slice);
        //    _ImageViewer.SetColorLevel(18000);
        //    _ImageViewer.SetColorWindow(30000);

        //    _ImageViewer.Render();

        //    Camera.SetPosition(Camera.GetPosition()[0] + 1000, Camera.GetPosition()[1], Camera.GetPosition()[2]);
        //}



        bool RightButtonPressed = false;
        bool LeftButtonPressed = false;
        System.Drawing.Point BeginPoint;
        int[] BeginSlice = new int[2];

        void interactorStyle_RightButtonReleaseEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            RightButtonPressed = false;
        }

        void interactorStyle_RightButtonPressEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            RightButtonPressed = true;
        }


        void interactorStyle_LeftButtonReleaseEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            LeftButtonPressed = false;
        }

        void interactorStyle_LeftButtonPressEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            LeftButtonPressed = true;
            int[] testPoint = this.Interactor.GetEventPosition();
            BeginPoint = new System.Drawing.Point(testPoint[0], testPoint[1]);

            if (UpSlicer != null && AccrossSlicer !=null)
            {
                BeginSlice[0] = UpSlicer.CurrentSlice;
                BeginSlice[1] = AccrossSlicer.CurrentSlice;
            }
        }


        void interactorStyle_MouseMoveEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            if (LeftButtonPressed)
            {
                int[] testPoint = this.Interactor.GetEventPosition();

                if (UpSlicer != null)
                    UpSlicer.SliceAtPercent((double)testPoint[0] / this.Height);

                if (AccrossSlicer != null)
                    AccrossSlicer.SliceAtPercent((double)testPoint[1] / this.Width);
            }

            if (RightButtonPressed)
            {

                int[] testPoint = this.Interactor.GetEventPosition();

                double level = Int16.MaxValue * (double)testPoint[0] / this.Width;
                double window = Int16.MaxValue * (double)testPoint[1] / this.Height;
                ParentViewer.SetContrast(level, window);
            }
        }


        /// <summary>
        /// move forward to next slice
        /// </summary>
        private void MoveForwardSlice()
        {
           
            if (Slice < _MaxSlice)
            {
                Slice += 1;

                {
                    SetSlicePosition(Slice);
                    RenderWindow.Render();
                }
            }
        }


        /// <summary>
        /// move backward to next slice
        /// </summary>
        private void MoveBackwardSlice()
        {
            Debug.WriteLine(Slice.ToString());
            if (Slice > _MinSlice)
            {
                Slice -= 1;

                //if (_ImageViewer != null)
                //{
                //    _ImageViewer.SetSlice(Slice);
                //    _SliceStatusMapper.SetInput("Slice No " + (Slice + 1).ToString() + "/" + (_MaxSlice + 1).ToString());
                //    _ImageViewer.Render();
                //}
                //else
                {
                    SetSlicePosition(Slice);
                    RenderWindow.Render();
                }
            }
        }


        /// <summary>
        /// eventhanndler to process keyboard input
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            //Debug.WriteLine(DateTime.Now + ":" + msg.Msg + ", " + keyData);
            if (keyData == System.Windows.Forms.Keys.Up)
            {
                MoveForwardSlice();
                return true;
            }
            else if (keyData == System.Windows.Forms.Keys.Down)
            {
                MoveBackwardSlice();
                return true;
            }
            // don't forward the following keys
            // add all keys which are not supposed to get forwarded
            else if (
                  keyData == System.Windows.Forms.Keys.F
               || keyData == System.Windows.Forms.Keys.L
            )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// event handler for mousewheel forward event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void interactor_MouseWheelForwardEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            MoveForwardSlice();
        }


        /// <summary>
        /// event handler for mousewheel backward event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void interactor_MouseWheelBackwardEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            MoveBackwardSlice();
        }
    }
}