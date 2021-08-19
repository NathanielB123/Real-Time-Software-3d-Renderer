namespace _3D_Renderer
{
    abstract class Light : SceneObject
    {
#nullable enable
        public bool CastsShadows { get; set; }
        public Colour LightColour { get; set; }
        public double Intensity { get; set; }
        public CubeMap? ShadowMap { get; set; }
        public bool[] ActiveShadowMaps { get; set; }
        public Matrix[]? ProjectionMatrices { get; set; }
        public double ShadowBias { get; set; }
        public Light()
        {
            ActiveShadowMaps = new bool[] { true, true, true, true, true, true };
        }
    }
}
