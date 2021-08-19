using System;

namespace _3D_Renderer
{
    readonly struct Vec3D
    {
        //3D Vector
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public Vec3D(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public double Magnitude()
        {
            return Math.Sqrt(SquareMagnitude());
        }

        public double SquareMagnitude()
        {
            return X * X + Y * Y + Z * Z;
        }

        public Vec3D ScalarMult(double Factor)
        {
            return new Vec3D(X * Factor, Y * Factor, Z * Factor);
        }

        public Vec3D Normalise()
        {
            //Potential optmisation - use fast inverse square root
            double Factor = 1 / Magnitude();
            return ScalarMult(Factor);
        }

        //Converts to Matrix type and allows more than 3 dimensions so 4D matrix can be made for translations (4D shears)
        public Matrix PositionMatrix(int Dimensions)
        {
            Arr2D<double> Temp = new Arr2D<double>(Dimensions, 1);
            Temp[0, 0] = X;
            Temp[1, 0] = Y;
            Temp[2, 0] = Z;
            for (int i = 3; i < Dimensions; i++)
            {
                Temp[i, 0] = 1;
            }
            return new Matrix(Temp);
        }
        public Matrix TranslationMatrix()
        {
            return new Matrix(new Arr2D<double>(new double[,] { {1, 0, 0, -X },
                                           {0, 1, 0, -Y },
                                           {0, 0, 1,  -Z},
                                           {0, 0, 0, 1 } }));
        }

        public Matrix ScaleMatrix()
        {
            return new Matrix(new Arr2D<double>(new double[,] { {X, 0, 0, 0 },
                                           {0, Y, 0, 0 },
                                           {0, 0, Z,  0},
                                           {0, 0, 0, 1 } }));
        }

        public Vec2D ToVec2D()
        {
            return new Vec2D(X, Y);
        }

        public static Vec3D Lerp(Vec3D VecA, Vec3D VecB, double LerpFactor)
        {
            return VecA.ScalarMult(1 - LerpFactor) + VecB.ScalarMult(LerpFactor);
        }
        public static Vec3D BiLerp(Vec3D VecA, Vec3D VecB, Vec3D VecC, Vec3D VecD, double LerpFactorA, double LerpFactorB)
        {
            //Could call Lerp three times but this is faster
            double Temp = LerpFactorA * LerpFactorB;
            return VecA.ScalarMult(Temp - LerpFactorA - LerpFactorB + 1) + VecB.ScalarMult(LerpFactorA - Temp) + VecC.ScalarMult(LerpFactorB - Temp) + VecD.ScalarMult(Temp);
        }

        public static double DotProduct(Vec3D VecA, Vec3D VecB)
        {
            return VecA.X * VecB.X + VecA.Y * VecB.Y + VecA.Z * VecB.Z;
        }

        public static double AngleBetween(Vec3D VecA, Vec3D VecB)
        {
            return DotProduct(VecA, VecB) / (VecA.Magnitude() * VecB.Magnitude());
        }

        public static Vec3D CrossProduct(Vec3D VecA, Vec3D VecB)
        {
            return new Vec3D(VecA.Y * VecB.Z - VecA.Z * VecB.Y,
                VecA.Z * VecB.X - VecA.X * VecB.Z,
                VecA.X * VecB.Y - VecA.Y * VecB.X);
        }

        public static Vec3D operator +(Vec3D VecA, Vec3D VecB)
        {
            return new Vec3D(VecA.X + VecB.X, VecA.Y + VecB.Y, VecA.Z + VecB.Z);
        }

        public static Vec3D operator -(Vec3D VecA, Vec3D VecB)
        {
            return new Vec3D(VecA.X - VecB.X, VecA.Y - VecB.Y, VecA.Z - VecB.Z);
        }
    }
}
