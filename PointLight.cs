namespace _3D_Renderer
{
    class PointLight : Light
    {
        public PointLight(Vec3D Position, Colour LightColour, double Intensity = 3, bool CastsShadows = true, double ShadowBias = 0.2, bool[]? ActiveShadowMaps = null)
        {
            this.Position = Position;
            //Point lights have no rotation
            Rotation = new Quat(1, 0, 0, 0);
            this.LightColour = LightColour;
            this.Intensity = Intensity;
            this.CastsShadows = CastsShadows;
            this.ShadowBias = ShadowBias;
            if (ActiveShadowMaps != null)
            {
                this.ActiveShadowMaps = ActiveShadowMaps;
            }
            else
            {
                this.ActiveShadowMaps = new bool[] { true, true, true, true, true, true };
            }
        }
    }
}
