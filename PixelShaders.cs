using System;

namespace _3D_Renderer
{
    interface IPixelShader
    {
        //Pixel shaders, can be run in as as deferred or forward shaders (although in the current version, deferred shaders can only return
        //a colour, so modifying other pixel properties has no effect). If necessarry, this can be fixed quite trivially.
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {

        }
    }
    class DiffuseShader : IPixelShader
    {
        public double Strength { get; set; }
        public DiffuseShader(double Strength = 1)
        {
            this.Strength = Strength;
        }
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            Colour DiffuseLighting = CurrentScene.AmbientLight;
            foreach (Light LightObj in CurrentScene.SceneLights)
            {
                if (LightObj is PointLight)
                {
                    if (LightObj.ShadowMap != null && LightObj.ProjectionMatrices != null && LightObj.CastsShadows)
                    {
                        int FaceNum;
                        double ShadowMapDepth = LightObj.ShadowMap.GetReflectionDisplacement(LightObj.Position - PixelPos, out FaceNum);
                        Vec3D LightSpaceTransformedPixelPos = Matrix.Multiply(LightObj.ProjectionMatrices[FaceNum], PixelPos.PositionMatrix(4)).ToVec3D();
                        double LightSpaceDepth = LightSpaceTransformedPixelPos.Z;
                        if (ShadowMapDepth != 0 && ShadowMapDepth < LightSpaceDepth - LightObj.ShadowBias && MasterMesh.ReceivesShadows)
                        {
                            //Pixel is in shadow
                            continue;
                        }
                    }
                    //Dot product of two normalised vectors gets how close they are
                    Vec3D LightToPixelVector = PixelPos - LightObj.Position;
                    double DistSquared = LightToPixelVector.SquareMagnitude();
                    LightToPixelVector = LightToPixelVector.Normalise();
                    DiffuseLighting += LightObj.LightColour.ScalarMult(LightObj.Intensity * Strength * Math.Max(Vec3D.DotProduct(PixelNormal, LightToPixelVector),
                        0) / DistSquared);
                }
                else if (LightObj is DirectionalLight)
                {
                    //Directional lights do not decrease in intensity with distance
                    DiffuseLighting += LightObj.LightColour.ScalarMult(LightObj.Intensity * Strength * Math.Max(Vec3D.DotProduct(PixelNormal,
                        LightObj.Rotation.ToDirectionVector()), 0));
                }
            }
            PixelColour += Colour.ComponentWiseProd(PixelAlbedo, DiffuseLighting);
        }
    }
    class DielectricSpecularShader : IPixelShader
    {
        public double Strength { get; set; }
        public DielectricSpecularShader(double Strength = 1)
        {
            this.Strength = Strength;
        }
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            Colour SpecularLighting = new Colour(0, 0, 0);
            Vec3D PixelToCamVector = (CameraObj.Position - PixelPos).Normalise();
            foreach (Light LightObj in CurrentScene.SceneLights)
            {
                if (LightObj is PointLight)
                {
                    if (LightObj.ShadowMap != null && LightObj.ProjectionMatrices != null && LightObj.CastsShadows)
                    {
                        int FaceNum;
                        double ShadowMapDepth = LightObj.ShadowMap.GetReflectionDisplacement(LightObj.Position - PixelPos, out FaceNum);
                        Vec3D LightSpaceTransformedPixelPos = Matrix.Multiply(LightObj.ProjectionMatrices[FaceNum], PixelPos.PositionMatrix(4)).ToVec3D();
                        double LightSpaceDepth = LightSpaceTransformedPixelPos.Z;
                        if (ShadowMapDepth != 0 && ShadowMapDepth < LightSpaceDepth - LightObj.ShadowBias && MasterMesh.ReceivesShadows)
                        {
                            //Pixel is in shadow
                            continue;
                        }
                    }
                    Vec3D LightToPixelVector = PixelPos - LightObj.Position;
                    double DistSquared = LightToPixelVector.SquareMagnitude();
                    LightToPixelVector = LightToPixelVector.Normalise();
                    //Subtracting a vector B multiplied by 2*the dot product of vectors A and B from a vector A gives the reflected vector of A about B
                    //In this context, it finds the reflected light vector over the normal, and so the dot product of it with the camera-to-pixel vector 
                    //finds "closeness" of the reflected light and camera vectors - I.E. the amount that pixel is lit specularly by that light source
                    SpecularLighting += LightObj.LightColour.ScalarMult(LightObj.Intensity * Strength * Math.Max(Vec3D.DotProduct(PixelToCamVector, LightToPixelVector -
                        PixelNormal.ScalarMult(2 * Vec3D.DotProduct(PixelNormal, LightToPixelVector))), 0) / DistSquared);
                }
                else if (LightObj is DirectionalLight)
                {
                    Vec3D LightToPixelVector = LightObj.Rotation.ToDirectionVector();
                    SpecularLighting += LightObj.LightColour.ScalarMult(LightObj.Intensity * Strength * PixelSpecular * Math.Max(Vec3D.DotProduct(PixelToCamVector, LightToPixelVector -
                        PixelNormal.ScalarMult(2 * Vec3D.DotProduct(PixelNormal, LightToPixelVector))), 0));
                }
            }
            PixelColour += SpecularLighting;
        }
    }

    class MetallicSpecularShader : IPixelShader
    {
        //Same as dielectric specular shader but tints the reflection by the albedo
        public double Strength { get; set; }
        public MetallicSpecularShader(double Strength = 1)
        {
            this.Strength = Strength;
        }
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            Colour SpecularLighting = new Colour(0, 0, 0);
            Vec3D PixelToCamVector = (CameraObj.Position - PixelPos).Normalise();
            foreach (Light LightObj in CurrentScene.SceneLights)
            {
                if (LightObj is PointLight)
                {
                    if (LightObj.ShadowMap != null && LightObj.ProjectionMatrices != null && LightObj.CastsShadows)
                    {
                        int FaceNum;
                        double ShadowMapDepth = LightObj.ShadowMap.GetReflectionDisplacement(LightObj.Position - PixelPos, out FaceNum);
                        Vec3D LightSpaceTransformedPixelPos = Matrix.Multiply(LightObj.ProjectionMatrices[FaceNum], PixelPos.PositionMatrix(4)).ToVec3D();
                        double LightSpaceDepth = LightSpaceTransformedPixelPos.Z;
                        if (ShadowMapDepth != 0 && ShadowMapDepth < LightSpaceDepth - LightObj.ShadowBias && MasterMesh.ReceivesShadows)
                        {
                            //Pixel is in shadow
                            continue;
                        }
                    }
                    Vec3D LightToPixelVector = PixelPos - LightObj.Position;
                    double DistSquared = LightToPixelVector.SquareMagnitude();
                    LightToPixelVector = LightToPixelVector.Normalise();
                    //Subtracting a vector B multiplied by 2*the dot product of vectors A and B from a vector A gives the reflected vector of A about B
                    //In this context, it finds the reflected light vector over the normal, and so the dot product of it with the camera-to-pixel vector 
                    //finds "closeness" of the reflected light and camera vectors - I.E. the amount that pixel is lit specularly by that light source
                    SpecularLighting += LightObj.LightColour.ScalarMult(LightObj.Intensity * Strength * PixelSpecular * Math.Max(Vec3D.DotProduct(PixelToCamVector, LightToPixelVector -
                        PixelNormal.ScalarMult(2 * Vec3D.DotProduct(PixelNormal, LightToPixelVector))), 0) / DistSquared);
                }
                else if (LightObj is DirectionalLight)
                {
                    Vec3D LightToPixelVector = LightObj.Rotation.ToDirectionVector();
                    SpecularLighting += LightObj.LightColour.ScalarMult(LightObj.Intensity * Strength * Math.Max(Vec3D.DotProduct(PixelToCamVector, LightToPixelVector -
                        PixelNormal.ScalarMult(2 * Vec3D.DotProduct(PixelNormal, LightToPixelVector))), 0));
                }
            }
            PixelColour += Colour.ComponentWiseProd(PixelAlbedo, SpecularLighting);
        }
    }
    class SubsurfaceScatteringShader : IPixelShader
    {
        public Colour SubSurfaceColour { get; set; }
        public SubsurfaceScatteringShader(Colour? SubSurfaceColour = null)
        {
            if (SubSurfaceColour is null)
            {
                this.SubSurfaceColour = new Colour(255, 0, 0);
            }
            else
            {
                this.SubSurfaceColour = (Colour)SubSurfaceColour;
            }
        }
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            Colour SubsurfaceLighting = new Colour(0, 0, 0);
            foreach (Light LightObj in CurrentScene.SceneLights)
            {
                if (LightObj is PointLight)
                {
                    //Dot product of two normalised vectors gets how close they are
                    Vec3D LightToPixelVector = PixelPos - LightObj.Position;
                    double DistSquared = LightToPixelVector.SquareMagnitude();
                    LightToPixelVector = LightToPixelVector.Normalise();
                    SubsurfaceLighting += SubSurfaceColour.ScalarMult(LightObj.Intensity * (1 - Math.Abs(Vec3D.DotProduct(PixelNormal, LightToPixelVector))) / DistSquared);
                }
                else if (LightObj is DirectionalLight)
                {
                    //Directional lights do not decrease in intensity with distance
                    SubsurfaceLighting += SubSurfaceColour.ScalarMult(LightObj.Intensity * (1 - Math.Abs(Vec3D.DotProduct(PixelNormal, LightObj.Rotation.ToDirectionVector()))));
                }
            }
            PixelColour += SubsurfaceLighting;
        }
    }

    class EmissiveShader : IPixelShader
    {
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            PixelColour += PixelEmissive;
        }
    }

    class AlbedoShader : IPixelShader
    {
        // Just adds albedo to pixel colour - for flat lighting
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            PixelColour += PixelAlbedo;
        }
    }

    class SkyBoxShader : IPixelShader
    {
        //With proper clipping, the cube map can just be rendered with an albedo and appear correct; however, due to the lack of clipping, 
        //a dedicated shader is necessary for it to not look odd at some angles
        //As this does not actually use any rendering information, this shader could actually be run entirely as a post-processing effect 
        //(that checks depth=0)
        //An alternative would be to pre-split the cube map into many smaller triangles, but this was simpler and ensures no warping at the edges
        //rather than it just being reduced
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            if (CurrentScene.SceneSkyBox != null)
            {
                Vec3D DirectionVector = new Vec3D((2 * ScreenX - Resolution[1]) / (CameraObj.ProjScale * Resolution[0]), (2 * ScreenY - Resolution[1]) / (CameraObj.ProjScale * Resolution[0]), 1);
                DirectionVector = new Vec3D(-DirectionVector.X, -DirectionVector.Y, -DirectionVector.Z);
                DirectionVector = Matrix.Multiply(CameraObj.Rotation.Inverse().RotationMatrix, DirectionVector.PositionMatrix(4)).ToVec3D();
                PixelColour += CurrentScene.SceneSkyBox.GetReflectionAlbedo(DirectionVector);
            }
        }
    }

    class SimpleFresnelShader : IPixelShader
    {
        public double Strength { get; set; }
        public SimpleFresnelShader(double Strength = 1)
        {
            this.Strength = Strength;
        }
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            //Fresnel is property that leads to dielectric surfaces being vastly more reflective at low grazing angles
            //This shader is a simple approximation of that effect
            Vec3D PixelToCamVector = (CameraObj.Position - PixelPos).Normalise();
            double Reflectance = Strength * (1 - Math.Abs(Vec3D.DotProduct(PixelToCamVector, PixelNormal)));
            PixelSpecular *= Reflectance;
            PixelAlbedo.ScalarMult(1 - Reflectance);
        }
    }

    class SchlicksFresnelShader : IPixelShader
    {
        public double IndexOfRefractionA { get; set; }
        public double IndexOfRefractionB { get; set; }
        public SchlicksFresnelShader(double IndexOfRefractionA = 1, double IndexOfRefractionB = 1)
        {
            this.IndexOfRefractionA = IndexOfRefractionA;
            this.IndexOfRefractionB = IndexOfRefractionB;
        }
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            //Fresnel is property that leads to dielectric surfaces being vastly more reflective at low grazing angles
            //This shader uses the Schlick's approximation - https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.50.2297&rep=rep1&type=pdf
            Vec3D PixelToCamVector = (CameraObj.Position - PixelPos).Normalise();
            double R = ((IndexOfRefractionA - IndexOfRefractionB) / (IndexOfRefractionA + IndexOfRefractionB)) * ((IndexOfRefractionA - IndexOfRefractionB) / (IndexOfRefractionA + IndexOfRefractionB));
            double OneSubtractCosineTheta = 1 - Math.Abs(Vec3D.DotProduct(PixelToCamVector, PixelNormal));
            double Reflectance = R + (1 - R) * OneSubtractCosineTheta * OneSubtractCosineTheta * OneSubtractCosineTheta * OneSubtractCosineTheta * OneSubtractCosineTheta;
            PixelSpecular *= Reflectance;
            PixelAlbedo.ScalarMult(1 - Reflectance);
        }
    }

    class AccurateFresnelShader : IPixelShader
    {
        public double IndexOfRefractionA { get; set; }
        public double IndexOfRefractionB { get; set; }
        public AccurateFresnelShader(double IndexOfRefractionB, double IndexOfRefractionA = 1)
        {
            this.IndexOfRefractionA = IndexOfRefractionA;
            this.IndexOfRefractionB = IndexOfRefractionB;
        }
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            //Fresnel is property that leads to dielectric surfaces being vastly more reflective at low grazing angles
            //This shader uses the actual fresnel equations
            //Assume light is unpolarized => equal proportion is p-polarised and s-polarised
            Vec3D PixelToCamVector = (CameraObj.Position - PixelPos).Normalise();
            //Sign will always be positive
            double CosineTheta = Math.Abs(Vec3D.DotProduct(PixelToCamVector, PixelNormal));
            double SineTheta = Math.Sqrt(1 - CosineTheta * CosineTheta);
            double CosineAngleOfRefraction = Math.Sqrt(1 - (SineTheta * IndexOfRefractionA / IndexOfRefractionB) * (SineTheta * IndexOfRefractionA / IndexOfRefractionB));
            double Rs = (IndexOfRefractionA * CosineTheta - IndexOfRefractionB * CosineAngleOfRefraction) / (IndexOfRefractionA * CosineTheta + IndexOfRefractionB * CosineAngleOfRefraction);
            Rs = Rs * Rs;
            double Rp = (IndexOfRefractionA * CosineAngleOfRefraction - IndexOfRefractionB * CosineTheta) / (IndexOfRefractionA * CosineAngleOfRefraction + IndexOfRefractionB * CosineTheta);
            Rp = Rp * Rp;
            double Reflectance = (Rs + Rp) / 2;
            PixelSpecular *= Reflectance;
            PixelAlbedo.ScalarMult(1 - Reflectance);
        }
    }

    class OverrideSpecularityShader : IPixelShader
    {
        // A simple shader to override the specularity of the assigned material.
        // Allows for specularity without having to define a specular map as well as specularities greater than 1.
        public double Specularity { get; set; }
        public OverrideSpecularityShader(double Specularity=1)
        {
            this.Specularity = Specularity;
        }
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            PixelSpecular = Specularity;
        }
    }

    class DielectricCubeMapReflectionShader : IPixelShader
    {
        public double Strength { get; set; }
        public int Type { get; set; }
        public DielectricCubeMapReflectionShader(double Strength = 1)
        {
            this.Strength = Strength;
        }
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            if (MasterMesh.CubeMapReflection != null)
            {
                Vec3D PixelToCamVector = (CameraObj.Position - PixelPos).Normalise();
                Vec3D ReflectionVector = PixelToCamVector - PixelNormal.ScalarMult(2 * Vec3D.DotProduct(PixelNormal, PixelToCamVector));
                Colour ReflectionCol = MasterMesh.CubeMapReflection.GetReflectionAlbedo(ReflectionVector);
                PixelColour += ReflectionCol.ScalarMult(PixelSpecular * Strength);
            }
        }
    }

    class MetallicCubeMapReflectionShader : IPixelShader
    {
        //Same as dielectric cube map reflection shader but tints the reflection by the albedo
        public double Strength { get; set; }
        public int Type { get; set; }
        public MetallicCubeMapReflectionShader(double Strength = 1)
        {
            this.Strength = Strength;
        }
        public void PerPixel(Camera CameraObj, Mesh MasterMesh, Scene CurrentScene, int[] Resolution, int ScreenX, int ScreenY,
            ref Colour PixelColour, ref Vec3D PixelPos, ref Vec3D PixelNormal, ref Colour PixelAlbedo, ref double PixelSpecular, ref Colour PixelEmissive)
        {
            if (MasterMesh.CubeMapReflection != null)
            {
                Vec3D PixelToCamVector = (CameraObj.Position - PixelPos).Normalise();
                Vec3D ReflectionVector = PixelToCamVector - PixelNormal.ScalarMult(2 * Vec3D.DotProduct(PixelNormal, PixelToCamVector));
                Colour ReflectionCol = MasterMesh.CubeMapReflection.GetReflectionAlbedo(ReflectionVector);
                PixelColour += Colour.ComponentWiseProd(PixelAlbedo, ReflectionCol.ScalarMult(PixelSpecular * Strength));
            }
        }
    }
}
