using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Kitware.VTK;
using System.IO;
using System.Drawing;
namespace ASU_Evaluation2
{
    public partial class Three_D_View : UserControl
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        static unsafe extern void CopyMemory(ushort* Destination, ushort* Source, uint Length);

        private Kitware.VTK.RenderWindowControl renderWindowControl1;
        vtkRenderer ren1;
        vtkRenderWindow renWin;
        vtkRenderWindowInteractor iren;

        vtkCamera camera;
        
        public Three_D_View()
        {
            InitializeComponent();
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
            // splitContainer1.Panel2.Controls.Add(this.renderWindowControl1);
            //  this.tableLayoutPanel1.Controls.Add(this.renderWindowControl1, 0, 0);
            //this.Controls.Add(this.renderWindowControl1);


        }

        void renderWindowControl1_Load(object sender, EventArgs e)
        {
            // Create the RenderWindow, Renderer and both Actors
            ren1 = renderWindowControl1.RenderWindow.GetRenderers().GetFirstRenderer();
            renWin = renderWindowControl1.RenderWindow;// vtkRenderWindow.New();
            //renWin.AddRenderer(ren1);
            iren = vtkRenderWindowInteractor.New();
            iren.SetRenderWindow(renWin);
        }

        public void SaveSceneAsObj(string FilePrefix)
        {
            vtkOBJExporter o = vtkOBJExporter.New();
            o.SetInput(renWin);
            o.SetFilePrefix(FilePrefix);
            o.Write();

        }

        public void ShowGraphicsObject(object GraphicObject)
        {
            if (typeof(vtkActor).IsAssignableFrom(GraphicObject.GetType()))
            {
                ren1.AddActor((vtkActor)GraphicObject);
            }
            else if (GraphicObject.GetType() == typeof(vtkOpenGLActor))
            {
                ren1.AddActor((vtkActor)GraphicObject);
            }
            else if (GraphicObject.GetType() == typeof(vtkPolyData))
            {
                vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
                mapper.SetInput(((vtkPolyData)GraphicObject));

                vtkActor triangulation = vtkActor.New();
                triangulation.SetMapper(mapper);
                triangulation.GetProperty().SetColor(1, 0, 0);


                ren1.AddActor(triangulation);
            }
            ren1.Render();
        }

        /// <summary>
        /// Only for showing data
        /// </summary>
        /// <param name="DataCube"></param>
        public void IsoSurface(float[, ,] DataCube, float Threshold, bool Continous, Color color)
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

            DataCube = null;

            vtkContourFilter Marching = new vtkContourFilter();
            Marching.SetInput(sp);
            Marching.SetValue(0, Threshold);
            if (Continous)
            {
                Marching.ComputeGradientsOn();
                Marching.ComputeNormalsOn();
                Marching.ComputeScalarsOn();
            }
            Marching.Update();




            vtkSmoothPolyDataFilter smoothFilter = vtkSmoothPolyDataFilter.New();
            smoothFilter.SetInputConnection(Marching.GetOutputPort());
            smoothFilter.SetNumberOfIterations(8);
            smoothFilter.SetRelaxationFactor(.5);
            smoothFilter.Update();



            vtkPolyDataMapper boneMapper = vtkPolyDataMapper.New();
            /*  if (false  )
              {
                  //get information about the original
                  double[] bounds;
                  vtkPolyData contours = smoothFilter.GetOutput();
                  bounds = contours.GetBounds();
                  double[] center;
                  center = contours.GetCenter();


                  vtkTransformPolyDataFilter transformFilter = vtkTransformPolyDataFilter.New();
                  transformFilter.SetInput(contours);
                  vtkTransform transform = vtkTransform.New();
                  transformFilter.SetTransform(transform);
                  transform.Translate(-center[0], -center[1], -center[2]);
                  transformFilter.Update();


                  vtkTransformPolyDataFilter transformFilter2 = vtkTransformPolyDataFilter.New();
                  transformFilter2.SetInput(transformFilter.GetOutput());
                  vtkTransform transform2 = vtkTransform.New();
                  transformFilter2.SetTransform(transform2);
                  double scaleX = 1d / (bounds[1] - bounds[0]);
                  double scaleY = 1d / (bounds[3] - bounds[2]);
                  double scaleZ = 1d / (bounds[5] - bounds[4]);
                  transform2.Scale(scaleX, scaleY, scaleZ);
                  transformFilter2.Update();

                  boneMapper.SetInputConnection(transformFilter2.GetOutputPort());
                  boneMapper.ScalarVisibilityOff();

              }
              else
              {
                  boneMapper.SetInputConnection(smoothFilter.GetOutputPort());
                  boneMapper.ScalarVisibilityOff();
              }*/
            boneMapper.SetInputConnection(smoothFilter.GetOutputPort());
            boneMapper.ScalarVisibilityOff();


            vtkActor bone = vtkActor.New();
            bone.SetMapper(boneMapper);

            bone.GetProperty().SetColor(color.R / 255f, color.G / 255f, color.B / 255f);
            bone.GetProperty().SetOpacity(.2);
            //Go through the Graphics Pipeline

            ren1.AddActor(bone);

            camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            //camera.DebugOn();


            //double cameraDistance = (Bounds[1] - Bounds[0]) * 4;



            camera.SetFocalPoint(0, 0, 0);
            camera.SetPosition(0, 0, 200);
            camera.SetParallelScale(200);
            // camera.SetClippingRange(cameraDistance *.5, cameraDistance *1.5);

        }

        /// <summary>
        /// Only for showing data
        /// </summary>
        /// <param name="DataCube"></param>
        public void IsoSurface(ushort[, ,] DataCube, ushort Threshold)
        {
            /* vtkStructuredPoints sp = new vtkStructuredPoints();
             sp.SetExtent(0, DataCube.GetLength(2), 0, DataCube.GetLength(1), 0, DataCube.GetLength(0));
             sp.SetOrigin(0, 0, 0);
             sp.SetDimensions(DataCube.GetLength(2), DataCube.GetLength(1), DataCube.GetLength(0));
             sp.SetSpacing(1.0, 1.0, 1.0);
             sp.SetScalarTypeToUnsignedShort();
             sp.SetNumberOfScalarComponents(1);
             //sp.AllocateScalars();
             unsafe
             {
                 ushort* volptr = (ushort*)sp.GetScalarPointer();
                 fixed (ushort* pData = DataCube)
                 {
                     CopyMemory(volptr, pData, (uint)Buffer.ByteLength(DataCube));
                 }
             }

             DataCube = null;*/

            vtkImageData sp = vtkImageData.New();

            sp.SetDimensions(DataCube.GetLength(2), DataCube.GetLength(1), DataCube.GetLength(0));
            sp.SetNumberOfScalarComponents(1);
            sp.SetScalarTypeToUnsignedShort();
            unsafe
            {
                ushort* volptr = (ushort*)sp.GetScalarPointer();
                fixed (ushort* pData = DataCube)
                {
                    CopyMemory(volptr, pData, (uint)Buffer.ByteLength(DataCube));
                }
            }
            DataCube = null;


            vtkImageGaussianSmooth gs = vtkImageGaussianSmooth.New();
            gs.SetInput(sp);
            gs.SetDimensionality(3);
            gs.SetRadiusFactors(1, 1, 1);
            gs.Update();


            vtkContourFilter Marching = new vtkContourFilter();
            Marching.SetInput(gs.GetOutput());
            //Marching.SetInput(sp);
            Marching.SetValue(0, Threshold);
            Marching.Update();

            vtkSmoothPolyDataFilter smoothFilter = vtkSmoothPolyDataFilter.New();
            smoothFilter.SetInputConnection(Marching.GetOutputPort());
            smoothFilter.SetNumberOfIterations(5);
            smoothFilter.SetRelaxationFactor(0.5);
            smoothFilter.Update();

            vtkPolyDataMapper boneMapper = vtkPolyDataMapper.New();
            boneMapper.SetInputConnection(smoothFilter.GetOutputPort());
            boneMapper.ScalarVisibilityOff();

            vtkActor bone = vtkActor.New();
            bone.SetMapper(boneMapper);

            //Go through the Graphics Pipeline
            ren1.AddActor(bone);
        }

      
        /// <summary>
        /// Converts a list of doubles and a index area into a vtk surface for display
        /// </summary>
        /// <param name="Points"></param>
        /// <param name="Index"></param>
        public void LoadPolyMesh(double[][] Points, int[] Index)
        {
            vtkPoints points = new vtkPoints();

            for (int i = 0; i < Points.Length; i++)
            {
                points.InsertNextPoint(Points[i][0], Points[i][1], Points[i][2]);
            }

            vtkCellArray cells = new vtkCellArray();
            for (int j = 0; j < Index.Length; j++)
            {
                if ((j % 3) == 0)
                    cells.InsertNextCell(3);
                cells.InsertCellPoint(Index[j]);
            }

            vtkPolyData poly = new vtkPolyData();
            poly.SetPoints(points);
            poly.SetStrips(cells);

            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            vtkActor actor = vtkActor.New();

            mapper.SetInput(poly);
            // mapper.SetScalarVisibility(1);

            //Go through the graphics pipeline
            actor.SetMapper(mapper);
            actor.GetProperty().SetColor(0, 1, 0);
            actor.GetProperty().SetOpacity(.4);
            // Renderer2.AddActor(actor);
        }


        vtkColorTransferFunction ctf = vtkColorTransferFunction.New();
        vtkPiecewiseFunction spwf = vtkPiecewiseFunction.New();
        vtkPiecewiseFunction gpwf = vtkPiecewiseFunction.New();
        vtkVolume vol = vtkVolume.New();
        int ScaleVTK = 255;
        public void ShowVolumeData(float[, ,] DataCube)
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



            vtkFixedPointVolumeRayCastMapper texMapper = vtkFixedPointVolumeRayCastMapper.New();


            //Go through the visulizatin pipeline
            texMapper.SetInput(sp);

            ScaleVTK = 1;

            //Set the color curve for the volume
            ctf.AddHSVPoint(0, .67, .07, 1);
            ctf.AddHSVPoint(94 * ScaleVTK, .67, .07, 1);
            ctf.AddHSVPoint(139 * ScaleVTK, 0, .5, 1);
            ctf.AddHSVPoint(160 * ScaleVTK, .28, .047, 1);
            ctf.AddHSVPoint(254 * ScaleVTK, .38, .013, 1);

            //Set the opacity curve for the volume
            spwf.AddPoint(0 * ScaleVTK, 0);
            spwf.AddPoint(29 * ScaleVTK, 0);
            spwf.AddPoint(30 * ScaleVTK, .2);
            spwf.AddPoint(255 * ScaleVTK, 1);

            //Set the gradient curve for the volume
            gpwf.AddPoint(0 * ScaleVTK, 0);
            gpwf.AddPoint(30 * ScaleVTK, .1);
            gpwf.AddPoint(255 * ScaleVTK, 1);

            vol.GetProperty().SetColor(ctf);
            vol.GetProperty().SetScalarOpacity(spwf);
            vol.GetProperty().SetGradientOpacity(gpwf);

            vol.SetMapper(texMapper);

            //Go through the Graphics Pipeline
            ren1.AddVolume(vol);

            camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            //camera.DebugOn();


            //double cameraDistance = (Bounds[1] - Bounds[0]) * 4;



            camera.SetFocalPoint(0, 0, 0);
            camera.SetPosition(0, 0, 200);
            camera.SetParallelScale(200);
        }
    }
}
