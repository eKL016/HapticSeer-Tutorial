﻿#define CS_GO
#define DEBUG_IMG_OUTPUT
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace WPFCaptureSample.ScreenCapture.ImageProcess
{
    class BarBloodIndicatorDetector : ImageProcessBase
    {
        private bool IsStopRunning = false;
        protected override double Clipped_Left
        {
            get
            {
#if CS_GO
                return 0.056;
#endif
            }
        }
        protected override double Clipped_Top
        {
            get
            {
#if CS_GO
                return 0.9787;
#endif
            }
        }
        protected override double Clipped_Right
        {
            get
            {
#if CS_GO
                return 0.097;
#endif
            }
        }
        protected override double Clipped_Bottom
        {
            get
            {
#if CS_GO
                return 0.988;
#endif
            }
        }
        protected override double Scale_Width
        {
            get
            {
                return 1;
            }
        }
        protected override double Scale_Height
        {
            get
            {
                return 1;
            }
        }
        private Mat BackgroundRemovalImage = new Mat();
        
        protected override void ImageHandler(object args)
        {
            MCvScalar scalar = new MCvScalar(0);
            while (!IsStopRunning)
            {
                while (!IsProcessingData)
                    Thread.Sleep(1);
                if (!BackgroundRemovalImage.Size.Equals(Data.Size))
                {
                    BackgroundRemovalImage.Dispose();
                    BackgroundRemovalImage = new Mat(Data.Size, DepthType.Cv8U, 1);
                }
                BackgroundRemovalImage.SetTo(scalar);
#if CS_GO
                ElimateBackgroundWithSolidColor(in Data, ref BackgroundRemovalImage, new Color[] { Color.FromArgb(155, 153, 122), Color.FromArgb(188, 56, 0) }, new uint[] { 0xC0C0C0C0, 0xC0C0C0C0 });
                Console.WriteLine(BarLengthCalc(BackgroundRemovalImage, 4, false));
#endif
#if DEBUG_IMG_OUTPUT
                CvInvoke.Imwrite("O:\\BloodBar_Ori.png", Data);
                CvInvoke.Imwrite("O:\\BloodBar_After.png", BackgroundRemovalImage);
#endif
                IsProcessingData = false;
            }
        }
    }
}
