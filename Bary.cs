using System;

namespace _3D_Renderer
{
    struct Bary
    {
        //Barycentric coordinate - each component represents the proportion of area of each of the three sub-triangles of the main one
        //formed when drawing lines joining a point to each vertex of a triangle
        //These are, helpfully, equivalent to linearly interpolating between vertices of a triangle in the x and y directions as the triangle is rasterised
        //(If Vert1 had a property 1U, 0V, 0W, Vert2 0U 1V 0W, Vert3 0U 0V 1W and delta U, delta V and delta W were computed and applied.)
        public double U { get; }
        public double V { get; }
        public double W { get; }
        public Bary(double U, double V, double W)
        {
            this.U = U;
            this.V = V;
            this.W = W;
        }

        public bool Internal()
        {
            return U > 0 && V > 0 && W > 0;
        }

        public Bary Clamp()
        {
            //This method will clamp the UVW coord to one on the edge of the triangle
            //Note that it will not necessarily (and will often not be) the closest point on the edge, but it should be quite close
            //Desmos graph to show here: https://www.desmos.com/calculator/1ro5nl4trk (blue point is clamped using this method)
            double TempU = Math.Max(Math.Min(U, 1), 0);
            double TempV = Math.Max(Math.Min(V, 1), 0);
            double TempW = Math.Max(Math.Min(W, 1), 0);
            double NewU = TempU / (TempU + TempV + TempW);
            double NewV = TempV / (TempU + TempV + TempW);
            double NewW = TempW / (TempU + TempV + TempW);
            return new Bary(NewU, NewV, NewW);
        }

        public double InverseWeight(TriDouble Weights)
        {
            return 1 / (1 / Weights.Vert1 * U +
                                1 / Weights.Vert2 * V +
                                1 / Weights.Vert3 * W);
        }

        public double PerspCorrectedZ(TriVec3D VertPositions)
        {
            return InverseWeight(new TriDouble(Math.Abs(VertPositions.Vert1.Z), Math.Abs(VertPositions.Vert2.Z), Math.Abs(VertPositions.Vert3.Z)));
        }

        public Bary PerspCorrect(TriVec3D VertPositions)
        {
            double PointZ = PerspCorrectedZ(VertPositions);
            return new Bary(U * PointZ / VertPositions.Vert1.Z, V * PointZ / VertPositions.Vert2.Z, W * PointZ / VertPositions.Vert3.Z);
        }

        public Bary PerspCorrect(double PointZ, TriVec3D VertPositions)
        {
            //To correct for perspective, interpolation must be weighted inversely proportionally to the depth at each point
            return new Bary(U * PointZ / VertPositions.Vert1.Z, V * PointZ / VertPositions.Vert2.Z, W * PointZ / VertPositions.Vert3.Z);
        }
    }
}
