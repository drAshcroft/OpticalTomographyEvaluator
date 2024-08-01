using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;

namespace ReconstructCells.Registration
{
    class AlignByRecon : ReconstructNodeTemplate
    {
        int nProjections = 25;
        double scale = .5;

        #region Properties

        #region Set

        public void setNumberOfProjections(int nProjections)
        {
            this.nProjections = nProjections;
        }

        public void setScale(double scale)
        {
            this.scale = scale;
        }

        #endregion

        #region Get


        #endregion

        #endregion

        #region Code

        protected override void RunNodeImpl()
        {
            var library = mPassData.Library;

            //generate an example reconstrunction at half size
            Tomography.RoughRecon roughRecon = new Tomography.RoughRecon();
            roughRecon.setScaleData(true, .5);
            roughRecon.setNumberProjections(25);
            roughRecon.SetInput(mPassData);
            roughRecon.RunNode();


            //now create the forward projections
            Tomography.PseudoSiddonForward psf = new Tomography.PseudoSiddonForward();
            psf.setDensityGrid(roughRecon.getDensityGrid());
            psf.setNumberForwardProjections(60);
            psf.setTargetSize(library[0].Width);
            psf.SetInput(roughRecon.GetOutput());
            psf.RunNode();

            //now align the projections to the recon
            Registration.FineRegistration fr = new Registration.FineRegistration();
            fr.setTargets(psf.getProjections());
            fr.SetInput(psf.GetOutput());
            fr.RunNode();

            mPassData = fr.GetOutput();
        }

        #endregion
    }
}
