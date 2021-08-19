using System;

namespace _3D_Renderer
{
    readonly struct Vec2D
    {
        //2D vector
        public double X { get; }
        public double Y { get; }

        public Vec2D(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public double Magnitude()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        public double RectilinearDist()
        {
            //Also known as taxicab or manhattan distance
            return Math.Abs(X) + Math.Abs(Y);
        }

        public double ChebyshevDist()
        {
            //Also known as chessboard distance
            return Math.Max(Math.Abs(X), Math.Abs(Y));
        }

        public Vec2D ScalarMult(double Factor)
        {
            return new Vec2D(X * Factor, Y * Factor);
        }

        public Vec2D Normalise()
        {
            double Magnitude = this.Magnitude();
            //Potential optmisation - use fast inverse square root
            return new Vec2D(X / Magnitude, Y / Magnitude);
        }

        public static Vec2D Lerp(Vec2D VecA, Vec2D VecB, double LerpFactor)
        {
            return VecA.ScalarMult(1 - LerpFactor) + VecB.ScalarMult(LerpFactor);
        }

        public static double DotProduct(Vec2D VecA, Vec2D VecB)
        {
            return VecA.X * VecB.X + VecA.Y * VecB.Y;
        }

        public static double AngleBetween(Vec2D VecA, Vec2D VecB)
        {
            return DotProduct(VecA, VecB) / (VecA.Magnitude() * VecB.Magnitude());
        }

        public static double AreaBetween(Vec2D VecA, Vec2D VecB)
        {
            return Math.Abs(PerpDotProd(VecA, VecB));
        }
        public static double PerpDotProd(Vec2D VecA, Vec2D VecB)
        {
            //Dot product of VecA and VecB rotated 90 degrees
            //Sign gives if VecB is on the left or right side of VecA
            return VecA.X * VecB.Y - VecA.Y * VecB.X;
        }
        public static Vec2D operator +(Vec2D VecA, Vec2D VecB)
        {
            return new Vec2D(VecA.X + VecB.X, VecA.Y + VecB.Y);
        }

        public static Vec2D operator -(Vec2D VecA, Vec2D VecB)
        {
            return new Vec2D(VecA.X - VecB.X, VecA.Y - VecB.Y);
        }
    }
}
