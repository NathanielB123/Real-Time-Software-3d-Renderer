using System;

namespace _3D_Renderer
{
#nullable enable
    class DeferredShaderStaticMethods
    {
        public static Colour? GetScreenSpaceReflection(Arr2D<Colour> CurrentFrame, Arr2D<double> DepthBuffer, (Arr2D<Vec3D> Position,
            Arr2D<Vec3D> Normal, Arr2D<Colour> Albedo, Arr2D<double> Specular, Arr2D<Colour> Emmissive,
            Arr2D<Mesh> MeshRef) DeferredBuffer, int x, int y, int[] Resolution,
            Camera CameraObj, double MaxReflectDist, double ScreenSpaceStep, double Tolerance, int Iterations, double Strength, double Bias)
        {
            Vec3D PixelPos = DeferredBuffer.Position[x, y];
            Vec3D PixelNormal = DeferredBuffer.Normal[x, y];
            Vec3D CamToPixelVector = (PixelPos - CameraObj.Position).Normalise();
            Vec3D ReflectionVector = CamToPixelVector - PixelNormal.ScalarMult(2 * Vec3D.DotProduct(PixelNormal, CamToPixelVector));
            ReflectionVector = ReflectionVector.Normalise().ScalarMult(MaxReflectDist);
            //Already know screen space pixel maps
            Vec2D ScreenSpaceReflectionStart = new Vec2D(x, y);
            Vec3D ReflectionEnd = PixelPos + ReflectionVector;
            Vec3D CameraSpaceReflectionEnd = Matrix.Multiply(CameraObj.GetCameraSpaceTransform(Resolution), ReflectionEnd.PositionMatrix(4)).ToVec3D();
            if (CameraSpaceReflectionEnd.Z < CameraObj.MinZ)
            {
                Vec3D ReflectionStart = PixelPos;
                Vec3D CameraSpaceReflectionStart = Matrix.Multiply(CameraObj.GetCameraSpaceTransform(Resolution), ReflectionStart.PositionMatrix(4)).ToVec3D();
                Vec3D CameraSpaceReflectionVec = CameraSpaceReflectionEnd - CameraSpaceReflectionStart;
                //As reflection vector at normal distances goes out of screen-space, need to find the position vector of the reflection as it hits MinZ
                CameraSpaceReflectionEnd = CameraSpaceReflectionStart + CameraSpaceReflectionVec.ScalarMult((CameraObj.MinZ - CameraSpaceReflectionStart.Z) /
                            CameraSpaceReflectionVec.Z);
            }
            Vec2D ScreenSpaceReflectionEnd = new Vec2D(CameraSpaceReflectionEnd.X / CameraSpaceReflectionEnd.Z,
                CameraSpaceReflectionEnd.Y / CameraSpaceReflectionEnd.Z);
            ScreenSpaceReflectionEnd += new Vec2D(Resolution[0] / 2, Resolution[1] / 2);
            double StartDepth = DepthBuffer[x, y];
            double EndDepth = CameraSpaceReflectionEnd.Z;
            double MinDepth = StartDepth;
            double MaxDepth = EndDepth;
            // First pass, find approx intersection
            double ScreenSpaceReflectDist = (ScreenSpaceReflectionEnd - ScreenSpaceReflectionStart).RectilinearDist();
            double LerpStepSize = ScreenSpaceStep / ScreenSpaceReflectDist;
            bool ReflectionFound = false;
            Vec2D Prev = ScreenSpaceReflectionStart;
            Vec2D Current = ScreenSpaceReflectionStart;
            double ReflectDepth = StartDepth;
            for (double LerpFactor = 0; LerpFactor <= 1; LerpFactor += LerpStepSize)
            {
                Prev = Current;
                MinDepth = ReflectDepth;
                Current = Vec2D.Lerp(ScreenSpaceReflectionStart, ScreenSpaceReflectionEnd, LerpFactor);
                int ReflectX = (int)Math.Round(Current.X);
                int ReflectY = (int)Math.Round(Current.Y);
                if (ReflectX < 0 || ReflectX > Resolution[0] - 1 || ReflectY < 0 || ReflectY > Resolution[1] - 1)
                {
                    //Reflection has gone outside the bounds of the screen
                    break;
                }
                else
                {
                    double Depth = DepthBuffer[ReflectX, ReflectY];
                    //Perspective correct interpolation
                    ReflectDepth = StartDepth * EndDepth / Double.Lerp(StartDepth, EndDepth, 1 - LerpFactor);
                    if (Depth != 0 && ReflectDepth - Depth > Bias && ReflectDepth - Depth < Tolerance + Bias)
                    {
                        ReflectionFound = true;
                        MaxDepth = ReflectDepth;
                        break;
                    }
                }
            }
            if (!ReflectionFound)
            {
                // No reflection
                return null;
            }
            else
            {
                // Second pass, find more accurate intersection
                Vec2D Min = Prev;
                Vec2D Max = Current;
                double Depth;
                for (int _ = 0; _ < Iterations; _++)
                {
                    Current = Vec2D.Lerp(Min, Max, 0.5);
                    int ReflectX = (int)Math.Round(Current.X);
                    int ReflectY = (int)Math.Round(Current.Y);
                    Depth = DepthBuffer[ReflectX, ReflectY];
                    //Perspective correct interpolation
                    ReflectDepth = MinDepth * MaxDepth / Double.Lerp(MinDepth, MaxDepth, 0.5);
                    if (Depth != 0 && ReflectDepth - Depth > 0 && ReflectDepth - Depth < Tolerance)
                    {
                        Max = Current;
                        MaxDepth = ReflectDepth;
                    }
                    else
                    {
                        Min = Current;
                        MinDepth = ReflectDepth;
                    }
                }
                int FinalX = (int)Math.Round(Max.X);
                int FinalY = (int)Math.Round(Max.Y);
                Depth = DepthBuffer[FinalX, FinalY];
                //Perspective correct interpolation
                ReflectDepth = MaxDepth;
                return CurrentFrame[FinalX, FinalY].ScalarMult(Strength * DeferredBuffer.Specular[x, y]
                    //Multiply by 1 - how close the reflection and pixel to camera vector as in this case it is much more likely the reflection is of something not visible in screenspace
                    * (1 - Math.Max(-Vec3D.DotProduct(CamToPixelVector, ReflectionVector.Normalise()), 0))
                    //Multiply by how 1 - the proportion in tolerance to avoid pixels far away from the actual reflection location being given as high intensity
                    * (1 - (ReflectDepth - Depth) / Tolerance)
                    //Multiply by 1-distance to reflection divided by the maximum distance so reflections fade out with distance instead of abruptly stopping
                    * (1 - (DeferredBuffer.Position[FinalX, FinalY] - PixelPos).Magnitude() / MaxReflectDist)
                    );
            }
        }
    }

    interface IDeferredShader
    {
        public Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> DepthBuffer, (Arr2D<Vec3D> Position,
            Arr2D<Vec3D> Normal, Arr2D<Colour> Albedo, Arr2D<double> Specular, Arr2D<Colour> Emmissive,
            Arr2D<Mesh> MeshRef) DeferredBuffer, int x, int y, int[] Resolution,
            Camera CameraObj)
        {
            return CurrentFrame[x, y];
        }
    }

    class ScreenSpaceDielectricReflectionShader : IDeferredShader
    {
        public double MaxReflectDist { get; set; }
        public double ScreenSpaceStep { get; set; }
        public int Iterations { get; set; }
        public double Tolerance { get; set; }
        public double Strength { get; set; }
        public double Bias { get; set; }
        public ScreenSpaceDielectricReflectionShader(double Strength = 1, double MaxReflectDist = 3, double ScreenSpaceStep = 8, int Iterations = 3, double Tolerance = 0.4, double Bias = 0.001)
        {
            this.MaxReflectDist = MaxReflectDist;
            this.ScreenSpaceStep = ScreenSpaceStep;
            this.Iterations = Iterations;
            this.Tolerance = Tolerance;
            this.Bias = Bias;
            this.Strength = Strength;
        }
        public Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> DepthBuffer, (Arr2D<Vec3D> Position,
            Arr2D<Vec3D> Normal, Arr2D<Colour> Albedo, Arr2D<double> Specular, Arr2D<Colour> Emmissive,
            Arr2D<Mesh> MeshRef) DeferredBuffer, int x, int y, int[] Resolution,
            Camera CameraObj)
        {
            Colour? ReflectCol = DeferredShaderStaticMethods.GetScreenSpaceReflection(CurrentFrame, DepthBuffer, DeferredBuffer,
                x, y, Resolution, CameraObj, MaxReflectDist, ScreenSpaceStep, Tolerance, Iterations, Strength, Bias);
            if (ReflectCol != null)
            {
                return CurrentFrame[x, y] + ReflectCol.GetValueOrDefault();
            }
            else
            {
                return CurrentFrame[x, y];
            }
        }
    }

    class ScreenSpaceDielectricReflectionShaderWithCubeMapFallback : IDeferredShader
    {
        public double MaxReflectDist { get; set; }
        public double ScreenSpaceStep { get; set; }
        public int Iterations { get; set; }
        public double Tolerance { get; set; }
        public double Strength { get; set; }
        public double Bias { get; set; }
        public ScreenSpaceDielectricReflectionShaderWithCubeMapFallback(double Strength = 1, double MaxReflectDist = 4, double ScreenSpaceStep = 8, int Iterations = 3,
            double Tolerance = 0.4, double Bias = 0.001)
        {
            this.MaxReflectDist = MaxReflectDist;
            this.ScreenSpaceStep = ScreenSpaceStep;
            this.Iterations = Iterations;
            this.Tolerance = Tolerance;
            this.Bias = Bias;
            this.Strength = Strength;
        }
        public Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> DepthBuffer, (Arr2D<Vec3D> Position,
            Arr2D<Vec3D> Normal, Arr2D<Colour> Albedo, Arr2D<double> Specular, Arr2D<Colour> Emmissive,
            Arr2D<Mesh> MeshRef) DeferredBuffer, int x, int y, int[] Resolution,
            Camera CameraObj)
        {
            Colour? ReflectCol = DeferredShaderStaticMethods.GetScreenSpaceReflection(CurrentFrame, DepthBuffer, DeferredBuffer,
                x, y, Resolution, CameraObj, MaxReflectDist, ScreenSpaceStep, Tolerance, Iterations, Strength, Bias);
            CubeMap? CubeMapReflection = DeferredBuffer.MeshRef[x, y].CubeMapReflection;
            if (ReflectCol != null)
            {
                return CurrentFrame[x, y] + ReflectCol.GetValueOrDefault();
            }
            else if (CubeMapReflection != null)
            {
                Vec3D PixelToCamVector = (CameraObj.Position - DeferredBuffer.Position[x, y]).Normalise();
                Vec3D ReflectionVector = PixelToCamVector - DeferredBuffer.Normal[x, y].ScalarMult(2 * Vec3D.DotProduct(DeferredBuffer.Normal[x, y], PixelToCamVector));
                Colour ReflectionCol = CubeMapReflection.GetReflectionAlbedo(ReflectionVector);
                return CurrentFrame[x, y] + ReflectionCol.ScalarMult(DeferredBuffer.Specular[x, y] * Strength);
            }
            else
            {
                return CurrentFrame[x, y];
            }
        }
    }

    class ScreenSpaceMetallicReflectionShader : IDeferredShader
    {
        public double MaxReflectDist { get; set; }
        public double ScreenSpaceStep { get; set; }
        public int Iterations { get; set; }
        public double Tolerance { get; set; }
        public double Strength { get; set; }
        public double Bias { get; set; }
        public ScreenSpaceMetallicReflectionShader(double Strength = 1, double MaxReflectDist = 4, double ScreenSpaceStep = 8, int Iterations = 3, double Tolerance = 0.4, double Bias = 0.001)
        {
            this.MaxReflectDist = MaxReflectDist;
            this.ScreenSpaceStep = ScreenSpaceStep;
            this.Iterations = Iterations;
            this.Tolerance = Tolerance;
            this.Bias = Bias;
            this.Strength = Strength;
        }
        public Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> DepthBuffer, (Arr2D<Vec3D> Position,
            Arr2D<Vec3D> Normal, Arr2D<Colour> Albedo, Arr2D<double> Specular, Arr2D<Colour> Emmissive,
            Arr2D<Mesh> MeshRef) DeferredBuffer, int x, int y, int[] Resolution,
            Camera CameraObj)
        {
            Colour? ReflectCol = DeferredShaderStaticMethods.GetScreenSpaceReflection(CurrentFrame, DepthBuffer, DeferredBuffer,
                x, y, Resolution, CameraObj, MaxReflectDist, ScreenSpaceStep, Tolerance, Iterations, Strength, Bias);
            if (ReflectCol != null)
            {
                return CurrentFrame[x, y] + Colour.ComponentWiseProd(DeferredBuffer.Albedo[x, y], ReflectCol.GetValueOrDefault());
            }
            else
            {
                return CurrentFrame[x, y];
            }
        }
    }

    class ScreenSpaceMetallicReflectionShaderWithCubeMapFallback : IDeferredShader
    {
        public double MaxReflectDist { get; set; }
        public double ScreenSpaceStep { get; set; }
        public int Iterations { get; set; }
        public double Tolerance { get; set; }
        public double Strength { get; set; }
        public double Bias { get; set; }
        public ScreenSpaceMetallicReflectionShaderWithCubeMapFallback(double Strength = 1, double MaxReflectDist = 4, double ScreenSpaceStep = 8, int Iterations = 3,
            double Tolerance = 0.4, double Bias = 0.001)
        {
            this.MaxReflectDist = MaxReflectDist;
            this.ScreenSpaceStep = ScreenSpaceStep;
            this.Iterations = Iterations;
            this.Tolerance = Tolerance;
            this.Bias = Bias;
            this.Strength = Strength;
        }
        public Colour PerPixel(Arr2D<Colour> CurrentFrame, Arr2D<double> DepthBuffer, (Arr2D<Vec3D> Position,
            Arr2D<Vec3D> Normal, Arr2D<Colour> Albedo, Arr2D<double> Specular, Arr2D<Colour> Emmissive,
            Arr2D<Mesh> MeshRef) DeferredBuffer, int x, int y, int[] Resolution,
            Camera CameraObj)
        {
            Colour? ReflectCol = DeferredShaderStaticMethods.GetScreenSpaceReflection(CurrentFrame, DepthBuffer, DeferredBuffer,
                x, y, Resolution, CameraObj, MaxReflectDist, ScreenSpaceStep, Tolerance, Iterations, Strength, Bias);
            CubeMap? CubeMapReflection = DeferredBuffer.MeshRef[x, y].CubeMapReflection;
            if (ReflectCol != null)
            {
                return CurrentFrame[x, y] + Colour.ComponentWiseProd(DeferredBuffer.Albedo[x, y], ReflectCol.GetValueOrDefault());
            }
            else if (CubeMapReflection != null)
            {
                Vec3D PixelToCamVector = (CameraObj.Position - DeferredBuffer.Position[x, y]).Normalise();
                Vec3D ReflectionVector = PixelToCamVector - DeferredBuffer.Normal[x, y].ScalarMult(2 * Vec3D.DotProduct(DeferredBuffer.Normal[x, y], PixelToCamVector));
                Colour ReflectionCol = CubeMapReflection.GetReflectionAlbedo(ReflectionVector);
                return CurrentFrame[x, y] + Colour.ComponentWiseProd(DeferredBuffer.Albedo[x, y], ReflectionCol.ScalarMult(DeferredBuffer.Specular[x, y] * Strength));
            }
            else
            {
                return CurrentFrame[x, y];
            }
        }
    }
}
