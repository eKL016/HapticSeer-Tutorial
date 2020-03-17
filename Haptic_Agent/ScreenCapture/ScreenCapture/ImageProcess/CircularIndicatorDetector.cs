﻿#define CS_GO
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFCaptureSample.ScreenCapture.ImageProcess
{
    class CircularIndicatorDetector : ImageProcessBase
    {
        private double CircularInnerSizeFractionRelatedToClippedSize = 137 / 231f;
        protected override double Clipped_Left
        {
            get
            {
#if CS_GO
                return 0.5 - 0.06;
#endif
                return 0;
            }
        }
        protected override double Clipped_Top
        {
            get
            {
#if CS_GO
                return 0.5 - 0.1065;
#endif
                return 0;
            }
        }
        protected override double Clipped_Right
        {
            get
            {
#if CS_GO
                return 0.5 + 0.06;
#endif
                return 1;
            }
        }
        protected override double Clipped_Bottom
        {
            get
            {
#if CS_GO
                return 0.5 + 0.1065;
#endif
                return 1f;
            }
        }
        protected override double Scale_Width
        {
            get
            {
                return 1f;
            }
        }
        protected override double Scale_Height
        {
            get
            {
                return 1f;
            }
        }

        private volatile bool IsStopRunning = false;
        public CircularIndicatorDetector(double CircularInnerSizeFractionRelatedToClippedSize)
            : base(true, true)
        {
            this.CircularInnerSizeFractionRelatedToClippedSize = CircularInnerSizeFractionRelatedToClippedSize;
        }
        ~CircularIndicatorDetector()
        {
            IsStopRunning = true;
        }

        private unsafe bool IsRedImpulse(byte* Now, byte* Last)
        {
            bool IsRedImpulse = (Last[2] < 127) && (Now[2] > 200);
            if (!IsRedImpulse)
                return false;
            if (Now[0] > 170 || Now[1] > 170)   //Filter White/meaningful color
                return false;
            return true;
        }
        private double GetAngleByCircleModel(int x, int y, int Width, int Height)
        {
            x -= Width / 2;
            y -= Height / 2;
            y *= -1; //Coordinate Changes
            double Angle = Math.Atan2(y, x) / Math.PI * 180;
            if (Angle < 0)
                Angle += 360;
            return Angle;
        }
        protected override void ImageHandler(object args)
        {
            Mat LastFrame = new Mat();
            Mat RedChannelImg = new Mat();
            Mat Kernel = new Mat(4, 4, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
            unsafe
            {
                byte* KernelPtr = (byte*)Kernel.DataPointer;
                for (int i = 0; i < 16; ++i)
                    KernelPtr[i] = 255;
            }
            int InnerWidth, InnerHeight;
            int InnerStartHeight = 0, InnerStopHeight = 0, InnerStartWidth = 0, InnerStopWidth = 0;
            int[] Counter = new int[360];
            int DEBUGNUM = 0;
            while (!IsStopRunning)
            {
                while (!IsProcessingData)
                    Thread.Sleep(1);
                if (!LastFrame.Size.Equals(Data.Size))
                {
                    LastFrame.Dispose();
                    RedChannelImg.Dispose();
                    LastFrame = new Mat(Data.Rows, Data.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
                    RedChannelImg = new Mat(Data.Rows, Data.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                    InnerWidth = (int)(Data.Width * CircularInnerSizeFractionRelatedToClippedSize);
                    InnerHeight = (int)(Data.Height * CircularInnerSizeFractionRelatedToClippedSize);
                    InnerStartHeight = (Data.Height - InnerHeight) / 2;
                    InnerStartWidth = (Data.Width - InnerWidth) / 2;
                    InnerStopHeight = Data.Height - InnerStartHeight;
                    InnerStopWidth = Data.Width - InnerStartWidth;
                    Data.Save("O:\\Output.png");
                    goto WaitForNextFrame;
                }
                Array.Clear(Counter, 0, 360);
                
                unsafe
                {
                    byte* CurrentPtr = (byte*)Data.DataPointer;//ARGB
                    byte* PastPtr = (byte*)LastFrame.DataPointer;
                    byte* RedImgPtr = (byte*)RedChannelImg.DataPointer;
                    int Offset = 0;
                    bool DEBUGBOOL = false;
                    for (int y = 0; y < Data.Height; ++y)
                    {
                        for (int x = 0; x < Data.Width; ++x)
                        {
                            RedImgPtr[Offset >> 2] = 0;
                            if (InnerStartHeight >= y && InnerStopHeight <= y && InnerStartWidth >= x && InnerStopWidth <= x)
                            {
                                Offset += 4;
                                continue;
                            }
                            if (IsRedImpulse(&CurrentPtr[Offset], &PastPtr[Offset]) && CurrentPtr[Offset + 2] > 200)
                            {
                                RedImgPtr[Offset >> 2] = 255;   //Set As White
                                double Angle = GetAngleByCircleModel(x, y, Data.Width, Data.Height);
                                Angle %= 360;
                                Counter[(int)Angle]++;
                                DEBUGBOOL = true;
                            }
                            Offset += 4;
                        }
                    }
                    if (DEBUGBOOL)
                    {

                        CvInvoke.MorphologyEx(RedChannelImg, RedChannelImg, Emgu.CV.CvEnum.MorphOp.Open, Kernel, new System.Drawing.Point(0, 0), 2, Emgu.CV.CvEnum.BorderType.Default, new Emgu.CV.Structure.MCvScalar(0, 0, 0));
                        RedChannelImg.Save("O:\\RedChannel" + DEBUGNUM++ + ".png");
                    }
                }

                long AvgAngle = 0, AngleCounter = 0;
                for (int i = 0; i < 180; ++i)
                {
                    if (Counter[i] < 10) continue;
                    AngleCounter += Counter[i];
                    AvgAngle += Counter[i] * i;
                }
                for (int i = 180; i < 360; ++i)
                {
                    if (Counter[i] < 10) continue;
                    AngleCounter += Counter[i];
                    AvgAngle += Counter[i] * (i - 360);
                }
                if (AngleCounter > 0)
                {
                    double _Angle = AvgAngle / (double)AngleCounter;
                    if (_Angle < 0)
                        _Angle += 360;
                    Console.WriteLine("Avg Angle: " + _Angle);
                }
            WaitForNextFrame:
                Mat temp = Data;
                Data = LastFrame;
                LastFrame = temp;
                IsProcessingData = false;
            }
        }
    }
}