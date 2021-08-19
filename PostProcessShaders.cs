using System;

namespace _3D_Renderer
{
    interface IPostProcessShader
    {
        //Post-processing shaders that do not require the deferred buffer
        public virtual Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> DepthBuffer, int x, int y)
        {
            return CurrentFrame[x, y];
        }
    }
    class KernelConvolve : IPostProcessShader
    {
        public Matrix Kernel { get; protected set; }
        public double KernelFactor { get; protected set; }
        public KernelConvolve(Matrix Kernel)
        {
            this.Kernel = Kernel;
            KernelFactor = 1 / KernelSum();
        }

        public KernelConvolve(Matrix Kernel, double KernelFactor)
        {
            this.Kernel = Kernel;
            this.KernelFactor = KernelFactor;
        }
        protected KernelConvolve()
        {

        }
        public double KernelSum()
        {
            double Sum = 0;
            for (int y = 0; y < Kernel.Height(); y++)
            {
                for (int x = 0; x < Kernel.Width(); x++)
                {
                    Sum += Kernel.Values[x, y];
                }
            }
            return Sum;
        }

        public virtual Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> ZBuffer, int ScreenX, int ScreenY)
        {
            double Red = 0;
            double Blue = 0;
            double Green = 0;
            for (int KernelY = 0; KernelY < Kernel.Height(); KernelY++)
            {
                int YCoord = ScreenY + KernelY - Kernel.Height() / 2;
                for (int KernelX = 0; KernelX < Kernel.Width(); KernelX++)
                {
                    int XCoord = ScreenX + KernelX - Kernel.Width() / 2;
                    if (XCoord < 0 || XCoord >= CurrentFrame.Width - 1 ||
                        YCoord < 0 || YCoord >= CurrentFrame.Height - 1)
                    {
                        //Stretches edges - uses screen X and Y if out of bounds
                        Colour Pixel = CurrentFrame[ScreenX, ScreenY];
                        double Factor = Kernel.Values[KernelX, KernelY];
                        Red += Pixel.Red * Factor;
                        Green += Pixel.Green * Factor;
                        Blue += Pixel.Blue * Factor;
                    }
                    else
                    {
                        Colour Pixel = CurrentFrame[XCoord, YCoord];
                        double Factor = Kernel.Values[KernelX, KernelY];
                        Red += Pixel.Red * Factor;
                        Green += Pixel.Green * Factor;
                        Blue += Pixel.Blue * Factor;
                    }
                }
            }
            Red *= KernelFactor;
            Green *= KernelFactor;
            Blue *= KernelFactor;
            return new Colour((int)Math.Round(Math.Abs(Red)), (int)Math.Round(Math.Abs(Green)), (int)Math.Round(Math.Abs(Blue)));
        }
    }
    class SeparableBoxBlurX : KernelConvolve
    {
        public SeparableBoxBlurX(int Size)
        {
            Kernel = new Matrix(new Arr2D<double>(Size, 1));
            for (int i = 0; i < Size; i++)
            {
                Kernel.Values[i, 0] = 1;
            }
            KernelFactor = 1 / KernelSum();
        }
    }
    class SeparableBoxBlurY : KernelConvolve
    {
        public SeparableBoxBlurY(int Size)
        {
            Kernel = new Matrix(new Arr2D<double>(1, Size));
            for (int i = 0; i < Size; i++)
            {
                Kernel.Values[0, i] = 1;
            }
            KernelFactor = 1 / KernelSum();
        }
    }
    class SeparableBoxDepthBlurX : SeparableBoxBlurX
    {
        public double DepthThreshold { get; set; }
        public SeparableBoxDepthBlurX(int Size, int DepthThreshold) : base(Size)
        {
            this.DepthThreshold = DepthThreshold;
        }
        public override Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> ZBuffer, int ScreenX, int ScreenY)
        {
            if (ZBuffer[ScreenX, ScreenY] > DepthThreshold)
            {
                return base.PerPixel(CurrentFrame, ZBuffer, ScreenX, ScreenY);
            }
            else
            {
                return CurrentFrame[ScreenX, ScreenY];
            }
        }
    }
    class SeparableBoxDepthBlurY : SeparableBoxBlurY
    {
        public double DepthThreshold { get; set; }
        public SeparableBoxDepthBlurY(int Size, int DepthThreshold) : base(Size)
        {
            this.DepthThreshold = DepthThreshold;
        }
        public override Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> ZBuffer, int ScreenX, int ScreenY)
        {
            if (ZBuffer[ScreenX, ScreenY] > DepthThreshold)
            {
                return base.PerPixel(CurrentFrame, ZBuffer, ScreenX, ScreenY);
            }
            else
            {
                return CurrentFrame[ScreenX, ScreenY];
            }
        }
    }

    class NonSeparableBokehBlur : KernelConvolve
    {
        public NonSeparableBokehBlur(int Size)
        {
            Kernel = new Matrix(new Arr2D<double>(Size, Size));
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    if (Math.Sqrt(x * x + y * y) <= Size)
                    {
                        Kernel.Values[x, y] = 1;
                    }
                    else
                    {
                        Kernel.Values[x, y] = 0;
                    }
                }
            }
            KernelFactor = 1 / KernelSum();
        }
    }
    class NonSeparableBokehDepthBlur : NonSeparableBokehBlur
    {
        public double DepthThreshold { get; set; }
        public NonSeparableBokehDepthBlur(int Size, int DepthThreshold) : base(Size)
        {
            this.DepthThreshold = DepthThreshold;
        }
        public override Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> ZBuffer, int ScreenX, int ScreenY)
        {
            if (ZBuffer[ScreenX, ScreenY] > DepthThreshold)
            {
                return base.PerPixel(CurrentFrame, ZBuffer, ScreenX, ScreenY);
            }
            else
            {
                return CurrentFrame[ScreenX, ScreenY];
            }
        }
    }
    class SeparableGaussianBlurX : KernelConvolve
    {
        public SeparableGaussianBlurX(int Size, double SD)
        {
            Kernel = new Matrix(new Arr2D<double>(Size, 1));
            for (int i = 0; i < Size; i++)
            {
                Kernel.Values[i, 0] = Math.Exp(-((i - (Size - 1) / 2) * (i - (Size - 1) / 2)) / (2 * SD * SD));
            }
            KernelFactor = 1 / KernelSum();
        }
    }
    class SeparableGaussianBlurY : KernelConvolve
    {
        public SeparableGaussianBlurY(int Size, double SD)
        {
            Kernel = new Matrix(new Arr2D<double>(1, Size));
            for (int i = 0; i < Size; i++)
            {
                Kernel.Values[0, i] = Math.Exp(-((i - (Size - 1) / 2) * (i - (Size - 1) / 2)) / (2 * SD * SD));
            }
            KernelFactor = 1 / KernelSum();
        }
    }
    class SeparableGaussianDepthBlurX : SeparableGaussianBlurX
    {
        public double DepthThreshold { get; set; }
        public SeparableGaussianDepthBlurX(int Size, double SD, int DepthThreshold) : base(Size, SD)
        {
            this.DepthThreshold = DepthThreshold;
        }
        public override Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> ZBuffer, int ScreenX, int ScreenY)
        {
            if (ZBuffer[ScreenX, ScreenY] > DepthThreshold)
            {
                return base.PerPixel(CurrentFrame, ZBuffer, ScreenX, ScreenY);
            }
            else
            {
                return CurrentFrame[ScreenX, ScreenY];
            }
        }
    }

    class SeparableGaussianDepthBlurY : SeparableGaussianBlurY
    {
        public double DepthThreshold { get; set; }
        public SeparableGaussianDepthBlurY(int Size, double SD, int DepthThreshold) : base(Size, SD)
        {
            this.DepthThreshold = DepthThreshold;
        }
        public override Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> ZBuffer, int ScreenX, int ScreenY)
        {
            if (ZBuffer[ScreenX, ScreenY] > DepthThreshold)
            {
                return base.PerPixel(CurrentFrame, ZBuffer, ScreenX, ScreenY);
            }
            else
            {
                return CurrentFrame[ScreenX, ScreenY];
            }
        }
    }

    class SimpleAntiAliasX : SeparableBoxBlurX
    {
        public double Threshold { get; set; }
        private readonly KernelConvolve Sobel = new KernelConvolve(new Matrix(new Arr2D<double>(
            new double[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } })), 1);
        public SimpleAntiAliasX(double Threshold, int Size) : base(Size)
        {
            this.Threshold = Threshold;
        }
        public override Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> ZBuffer, int ScreenX, int ScreenY)
        {
            Colour Edge = Sobel.PerPixel(CurrentFrame, ZBuffer, ScreenX, ScreenY);
            double EdgeFactor = (Edge.Red + Edge.Green + Edge.Blue) / (255.0 * 3);
            if (EdgeFactor > Threshold)
            {
                return base.PerPixel(CurrentFrame, ZBuffer, ScreenX, ScreenY);
            }
            else
            {
                return CurrentFrame[ScreenX, ScreenY];
            }
        }
    }

    class SimpleAntiAliasY : SeparableBoxBlurY
    {
        public double Threshold { get; set; }
        private readonly KernelConvolve Sobel = new KernelConvolve(new Matrix(new Arr2D<double>(
            new double[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } })), 1);
        public SimpleAntiAliasY(double Threshold, int Size) : base(Size)
        {
            this.Threshold = Threshold;
        }
        public override Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> ZBuffer, int ScreenX, int ScreenY)
        {
            Colour Edge = Sobel.PerPixel(CurrentFrame, ZBuffer, ScreenX, ScreenY);
            double EdgeFactor = (Edge.Red + Edge.Green + Edge.Blue) / (255.0 * 3);
            if (EdgeFactor > Threshold)
            {
                return base.PerPixel(CurrentFrame, ZBuffer, ScreenX, ScreenY);
            }
            else
            {
                return CurrentFrame[ScreenX, ScreenY];
            }
        }
    }

    class Tint : IPostProcessShader
    {
        public Colour TintColour { get; set; }
        public Tint(Colour TintColour)
        {
            this.TintColour = TintColour;
        }

        public Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> ZBuffer, int ScreenX, int ScreenY)
        {
            return Colour.ComponentWiseProd(CurrentFrame[ScreenX, ScreenY], TintColour);
        }
    }

    class Brighten : IPostProcessShader
    {
        public double Strength { get; set; }
        public Brighten(double Strength=0)
        {
            this.Strength = Strength;
        }

        public Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> ZBuffer, int ScreenX, int ScreenY)
        {
            return CurrentFrame[ScreenX, ScreenY] + new Colour(255, 255, 255).ScalarMult(Math.Abs(Strength));
        }
    }

    class Darken : IPostProcessShader
    {
        public double Strength { get; set; }
        public Darken(double Strength = 0)
        {
            this.Strength = Strength;
        }

        public Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> ZBuffer, int ScreenX, int ScreenY)
        {
            return CurrentFrame[ScreenX, ScreenY] - new Colour(255, 255, 255).ScalarMult(Math.Abs(Strength));
        }
    }
}
