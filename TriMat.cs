namespace _3D_Renderer
{
    class TriMat
    {
        //Triangle of matrices
        public Matrix[] Verts { get; }
        public TriMat(Matrix Vert1, Matrix Vert2, Matrix Vert3)
        {
            Verts = new Matrix[] { Vert1, Vert2, Vert3 };
        }
        public Matrix Vert1
        {
            get { return Verts[0]; }
        }
        public Matrix Vert2
        {
            get { return Verts[1]; }
        }
        public Matrix Vert3
        {
            get { return Verts[2]; }
        }

        public TriMat Multiply(Matrix Transformation)
        {
            return new TriMat(Matrix.Multiply(Transformation, Vert1), Matrix.Multiply(Transformation, Vert2), Matrix.Multiply(Transformation, Vert3));
        }

        public TriVec3D ToTriVec3D()
        {
            return new TriVec3D(Vert1.ToVec3D(), Vert2.ToVec3D(), Vert3.ToVec3D());
        }

        public TriVec2D ToTriVec2D()
        {
            return new TriVec2D(Vert1.ToVec2D(), Vert2.ToVec2D(), Vert3.ToVec2D());
        }
    }
}
