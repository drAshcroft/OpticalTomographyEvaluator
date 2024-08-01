//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using ILNumerics;

//namespace ViewAndSegment
//{
//    class HolesOtsu : ILMath
//    {

//        public static Tuple<double[,,], double[,,]> DoHoleOtsu(float[, ,] data)
//        {
//            float[, ,] classes = ImageProcessing.Segmentation.Otsu.MultiOtsu(data, 4);

//            ILNumerics.ILArray<float> d = classes;

//            ILNumerics.ILInArray<double> cyto = todouble((d == 2));
//            ILNumerics.ILInArray<double> nucleous = todouble((d == 3));

//            //ILNumerics.ILMatFile m = new ILNumerics.ILMatFile(cyto);
//            //m.Write(@"c:\temp\thresholds.mat");

//            double[] cytoF = new double[data.LongLength];
//            double[] nucleousF = new double[data.LongLength];

//            cyto.ExportValues(ref cytoF);
//            nucleous.ExportValues(ref nucleousF);

//            double[, ,] cytoFFF = new double[data.GetLength(0), data.GetLength(1), data.GetLength(2)], nucleousFFF = new double[data.GetLength(0), data.GetLength(1), data.GetLength(2)];

//            return new Tuple<double[, ,], double[, ,]>(cytoFFF, nucleousFFF);
//        }
//    }
//}
