using System;

namespace _3D_Renderer
{
    class Camera : SceneObject
    {
        public double Fov { get; set; }
        public double ProjScale { get; set; }
        public double MinZ { get; set; }
        public double MaxZ { get; set; }
        public Camera(Vec3D Position, Quat Rotation, double Fov, double MinZ = 0.001, double MaxZ = 100.0)
        {
            //Avoids potential divides by 0 by slightly offsetting the camera position
            this.Position = Position + new Vec3D(0, 0.000001, 0);
            this.Rotation = Rotation;
            this.Fov = Fov;
            ProjScale = 1 / Math.Tan(Math.PI * Fov / 360);
            this.MinZ = MinZ;
            this.MaxZ = MaxZ;
        }

        public Matrix GetCameraSpaceTransform(int[] Resolution, bool IgnorePos = false, bool IgnoreRot = false)
        {
            Matrix PosMat = new Matrix(new Arr2D<double>(new double[,]{ {1, 0, 0, 0 },
                {0, 1, 0, 0 },
                {0, 0, 1, 0 },
                {0, 0, 0, 1 } }));
            if (!IgnorePos)
            {
                PosMat = Position.TranslationMatrix();
            }
            Matrix RotMat = new Matrix(new Arr2D<double>(new double[,]{ {1, 0, 0, 0 },
                {0, 1, 0, 0 },
                {0, 0, 1, 0 },
                {0, 0, 0, 1 } }));
            if (!IgnoreRot)
            {
                RotMat = Rotation.RotationMatrix;
            }
            return Matrix.Multiply(
                   new Matrix(new Arr2D<double>(new double[,] { {ProjScale * Resolution[0]/2, 0 , 0, 0 },
                                           {0,ProjScale * Resolution[0]/2, 0, 0 },
                                           {0, 0, 1, 0 },
                                           {0, 0, 0, 1 } })),
                   Matrix.Multiply(RotMat, PosMat));

        }
    }
}
