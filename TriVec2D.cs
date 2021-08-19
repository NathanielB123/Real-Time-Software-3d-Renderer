namespace _3D_Renderer
{
    class TriVec2D
    {
        //Triangle of 2d vectors
        public Vec2D[] Verts { get; }
        public TriVec2D(Vec2D Vert1, Vec2D Vert2, Vec2D Vert3)
        {
            Verts = new Vec2D[] { Vert1, Vert2, Vert3 };
        }
        public Vec2D Vert1
        {
            get { return Verts[0]; }
        }
        public Vec2D Vert2
        {
            get { return Verts[1]; }
        }
        public Vec2D Vert3
        {
            get { return Verts[2]; }
        }

        public Vec2D Edge1
        {
            get { return Vert2 - Vert1; }
        }
        public Vec2D Edge2
        {
            get { return Vert3 - Vert1; }
        }

        public double SignedArea
        {
            get { return Vec2D.PerpDotProd(Edge1, Edge2); }
        }

        public double Area
        {
            get { return Vec2D.AreaBetween(Edge1, Edge2); }
        }

        public Bary ComputeInternalBaryCoord(Vec2D Point)
        {
            double U = Vec2D.AreaBetween(Point - Vert2, Point - Vert3) / Area;
            double V = Vec2D.AreaBetween(Point - Vert3, Point - Vert1) / Area;
            double W = 1 - U - V;
            return new Bary(U, V, W);
        }

        public Bary ComputeBaryCoord(Vec2D Point)
        {
            double U = Vec2D.PerpDotProd(Point - Vert2, Point - Vert3) / SignedArea;
            double V = Vec2D.PerpDotProd(Point - Vert3, Point - Vert1) / SignedArea;
            double W = 1 - U - V;
            return new Bary(U, V, W);
        }

        public Vec2D BaryInterp(Bary BarycentricCoord)
        {
            return Vert1.ScalarMult(BarycentricCoord.U) + Vert2.ScalarMult(BarycentricCoord.V) + Vert3.ScalarMult(BarycentricCoord.W);
        }
    }
}
