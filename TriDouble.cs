namespace _3D_Renderer
{
    class TriDouble
    {
        //Triangle of doubles
        public double[] Verts { get; }
        public TriDouble(double Vert1, double Vert2, double Vert3)
        {
            Verts = new double[] { Vert1, Vert2, Vert3 };
        }
        public double Vert1
        {
            get { return Verts[0]; }
        }
        public double Vert2
        {
            get { return Verts[1]; }
        }
        public double Vert3
        {
            get { return Verts[2]; }
        }
        public double BaryInterp(Bary BarycentricCoord)
        {
            return Vert1 * BarycentricCoord.U + Vert2 * BarycentricCoord.V + Vert3 * BarycentricCoord.W;
        }
    }
}
