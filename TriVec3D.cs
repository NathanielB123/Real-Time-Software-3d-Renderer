namespace _3D_Renderer
{
    class TriVec3D
    {
        //Triangle of 3d vectors
        public Vec3D[] Verts { get; }
        public TriVec3D(Vec3D Vert1, Vec3D Vert2, Vec3D Vert3)
        {
            Verts = new Vec3D[] { Vert1, Vert2, Vert3 };
        }

        public Vec3D Vert1
        {
            get { return Verts[0]; }
        }
        public Vec3D Vert2
        {
            get { return Verts[1]; }
        }
        public Vec3D Vert3
        {
            get { return Verts[2]; }
        }

        public Vec3D Edge1
        {
            get { return Vert2 - Vert1; }
        }
        public Vec3D Edge2
        {
            get { return Vert3 - Vert1; }
        }

        public double Area
        {
            get { return Vec3D.CrossProduct(Edge1, Edge2).Magnitude(); }
        }
        public Vec3D BaryInterp(Bary BarycentricCoord)
        {
            return Vert1.ScalarMult(BarycentricCoord.U) + Vert2.ScalarMult(BarycentricCoord.V) + Vert3.ScalarMult(BarycentricCoord.W);
        }

        public TriMat ToTriMat(int Dimensions)
        {
            return new TriMat(Vert1.PositionMatrix(Dimensions), Vert2.PositionMatrix(Dimensions), Vert3.PositionMatrix(Dimensions));
        }

        public TriVec2D ToTriVec2D()
        {
            return new TriVec2D(Vert1.ToVec2D(), Vert2.ToVec2D(), Vert3.ToVec2D());
        }

        public Bary ComputeBaryCoord(Vec3D Point, Vec3D[] SurfaceNormals)
        {
            double U = Vec3D.DotProduct(SurfaceNormals[0], Vec3D.CrossProduct(Point - Vert2, Point - Vert3)) / Area;
            double V = Vec3D.DotProduct(SurfaceNormals[0], Vec3D.CrossProduct(Point - Vert3, Point - Vert1)) / Area;
            double W = 1 - U - V;
            return new Bary(U, V, W);
        }
    }
}
