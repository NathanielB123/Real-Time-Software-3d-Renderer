namespace _3D_Renderer
{
    interface ITexelShiftShader
    {
        //Shader that takes inputs of texel attributes and changes the texel coordinate, required for parallax mapping but also can be used for 
        //some other shader techniques such as approximating refraction
        public void PerPixelShade(Camera CameraObj, TriVec3D WorldSpaceVerts, Vec3D[] SurfaceNormals, Surface Face,
            Mesh MasterMesh, Scene CurrentScene, double TexelWidth, double TexelHeight, ref Vec2D TextureSpacePos, ref Vec2D? UV, ref Bary Coord)
        {

        }
    }

    class ParallaxMappingWithOffsetLimiting : ITexelShiftShader
    {
        public double Strength { get; set; }
        public ParallaxMappingWithOffsetLimiting(double Strength = 0.1)
        {
            this.Strength = Strength;
        }

        public void PerPixelShade(Camera CameraObj, TriVec3D WorldSpaceVerts, Vec3D[] SurfaceNormals, Surface Face,
            Mesh MasterMesh, Scene CurrentScene, double TexelWidth, double TexelHeight, ref Vec2D TextureSpacePos, ref Vec2D? UV, ref Bary Coord)
        {
            Vec3D WorldSpacePos = WorldSpaceVerts.BaryInterp(Coord);
            double Offset = Face.Displacement(MasterMesh, UV, TextureSpacePos, TexelWidth, TexelHeight) - 0.5;
            Vec3D PixelToCamVector = (WorldSpacePos - CameraObj.Position).Normalise();
            double DotProd = Vec3D.DotProduct(PixelToCamVector, SurfaceNormals[0]);
            Vec3D MappedViewVec = SurfaceNormals[0].ScalarMult(DotProd);
            Vec3D WorldSpaceOffset = (PixelToCamVector - MappedViewVec).Normalise();
            double DotProd2 = Vec3D.DotProduct(WorldSpaceOffset, PixelToCamVector);
            WorldSpacePos -= WorldSpaceOffset.ScalarMult(Strength * Offset * DotProd2);
            Coord = WorldSpaceVerts.ComputeBaryCoord(WorldSpacePos, SurfaceNormals);
            TextureSpacePos = Face.TextureSpacePos(MasterMesh, Coord);
            UV = Face.WrapToUV(TextureSpacePos, TexelWidth, TexelHeight);
        }
    }

    class ParallaxMappingWithoutOffsetLimiting : ITexelShiftShader
    {
        public double Strength { get; set; }
        public ParallaxMappingWithoutOffsetLimiting(double Strength = 0.1)
        {
            this.Strength = Strength;
        }

        public void PerPixelShade(Camera CameraObj, TriVec3D WorldSpaceVerts, Vec3D[] SurfaceNormals, Surface Face,
            Mesh MasterMesh, Scene CurrentScene, double TexelWidth, double TexelHeight, ref Vec2D TextureSpacePos, ref Vec2D? UV, ref Bary Coord)
        {
            Vec3D WorldSpacePos = WorldSpaceVerts.BaryInterp(Coord);
            double Offset = Face.Displacement(MasterMesh, UV, TextureSpacePos, TexelWidth, TexelHeight) - 0.5;
            Vec3D PixelToCamVector = (WorldSpacePos - CameraObj.Position).Normalise();
            double DotProd = Vec3D.DotProduct(PixelToCamVector, SurfaceNormals[0]);
            Vec3D MappedViewVec = SurfaceNormals[0].ScalarMult(DotProd);
            Vec3D WorldSpaceOffset = (PixelToCamVector - MappedViewVec).Normalise();
            Vec3D CameraSpacePixelPos = Matrix.Multiply(CameraObj.GetCameraSpaceTransform(new int[] { 1, 1 }), WorldSpacePos.PositionMatrix(4)).ToVec3D();
            CameraSpacePixelPos = CameraSpacePixelPos.Normalise();
            double DotProd2 = Vec3D.DotProduct(WorldSpaceOffset, PixelToCamVector);
            WorldSpacePos -= WorldSpaceOffset.ScalarMult(Strength * Offset * DotProd2 / CameraSpacePixelPos.Z);
            Coord = WorldSpaceVerts.ComputeBaryCoord(WorldSpacePos, SurfaceNormals);
            TextureSpacePos = Face.TextureSpacePos(MasterMesh, Coord);
            UV = Face.WrapToUV(TextureSpacePos, TexelWidth, TexelHeight);
        }
    }
}
