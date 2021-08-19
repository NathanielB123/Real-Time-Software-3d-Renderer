using System;

namespace _3D_Renderer
{
    readonly struct Quat
    {
        //Quaternion
        //Used to store rotations/directions. The magnitude of all components should always be 1. The X, Y and Z components encode an axis and the 
        //magnitude of that vector and the w component encodes an angle.
        public double W { get; }
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public Matrix RotationMatrix { get; }
        public Quat(double W, double X, double Y, double Z)
        {
            this.W = W;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            RotationMatrix = ComputeRotationMatrix(W, X, Y, Z);
        }
        public Quat(Vec3D Axis, double Rotation)
        {
            W = Math.Cos(Rotation / 2);
            X = Axis.X * Math.Sin(Rotation / 2);
            Y = Axis.Y * Math.Sin(Rotation / 2);
            Z = Axis.Z * Math.Sin(Rotation / 2);
            RotationMatrix = ComputeRotationMatrix(W, X, Y, Z);
        }

        public Quat(Vec3D VecA, Vec3D VecB)
        {
            W = Vec3D.DotProduct(VecA, VecB);
            Vec3D CrossProd = Vec3D.CrossProduct(VecA, VecB);
            X = CrossProd.X;
            Y = CrossProd.Y;
            Z = CrossProd.Z;
            RotationMatrix = ComputeRotationMatrix(W, X, Y, Z);
        }

        private static Matrix ComputeRotationMatrix(double W, double X, double Y, double Z)
        {
            // Precomputes the rotation matrix
            return new Matrix(new Arr2D<double>(new double[,] {
                { 1 - 2 * Y * Y - 2 * Z * Z, 2 * X * Y + 2 * W * Z, 2 * X * Z - 2 * W * Y, 0 },
                { 2 * X * Y - 2 * W * Z, 1 - 2 * X * X - 2 * Z * Z, 2 * Y * Z + 2 * W * X, 0 },
                { 2 * X * Z + 2 * W * Y, 2 * Y * Z - 2 * W * X, 1 - 2 * X * X - 2 * Y * Y, 0 },
                { 0, 0, 0, 1 } }));
        }

        public Vec3D ToDirectionVector()
        {
            return new Vec3D(X, Y, Z).Normalise();
        }

        public double Pitch()
        {
            return Math.Atan2(2 * (W * X + Y * Z), 1 - 2 * (X * X + Y * Y));
        }
        public double Yaw()
        {
            return Math.Asin(2 * (W * Y - Z * X));
        }
        public double Roll()
        {
            return Math.Atan2(2 * (W * Z + X * Y), 1 - 2 * (Y * Y + Z * Z));
        }

        public Quat Inverse()
        {
            return new Quat(W, -X, -Y, -Z);
        }

        public static Quat operator *(Quat QuaternionA, Quat QuaternionB)
        {
            return new Quat(
                QuaternionA.W * QuaternionB.W - QuaternionA.X * QuaternionB.X - QuaternionA.Y * QuaternionB.Y - QuaternionA.Z * QuaternionB.Z,
                QuaternionA.W * QuaternionB.X + QuaternionA.X * QuaternionB.W + QuaternionA.Y * QuaternionB.Z - QuaternionA.Z * QuaternionB.Y,
                QuaternionA.W * QuaternionB.Y + QuaternionA.Y * QuaternionB.W + QuaternionA.Z * QuaternionB.X - QuaternionA.X * QuaternionB.Z,
                QuaternionA.W * QuaternionB.Z + QuaternionA.Z * QuaternionB.W + QuaternionA.X * QuaternionB.Y - QuaternionA.Y * QuaternionB.X);
        }
    }
}
