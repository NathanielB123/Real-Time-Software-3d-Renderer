namespace _3D_Renderer
{
    class DirectionalLight : Light
    {
        public DirectionalLight(Quat Rotation, Colour LightColour, double Intensity = 1, bool CastsShadows = true, double ShadowBias = 0.05)
        {
            //Directional light sources have no position
            Position = new Vec3D(0, 0, 0);
            this.Rotation = Rotation;
            this.LightColour = LightColour;
            this.Intensity = Intensity;
            this.CastsShadows = CastsShadows;
            this.ShadowBias = ShadowBias;
            this.ActiveShadowMaps = new bool[] { false, false, false, false, true, false };
        }
    }
}
