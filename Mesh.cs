using System;
using System.Collections.Generic;
using System.IO;

namespace _3D_Renderer
{
    class Mesh : SceneObject
    {
        public Vec3D Scale { get; set; }
        public Vec3D[] Vertices { get; }
        public Vec2D[] TextureCoords { get; }
        public Vec3D[] VertexNormals { get; }
        public Surface[] Faces { get; }
        public Material[] Materials { get; set; }
        public ITexelShiftShader[] TexelShiftShaders { get; set; }
        public IPixelShader[] PixelShaders { get; set; }
        public IPixelShader[] DeferredPixelShaders { get; set; }
        public IDeferredShader[] DeferredShaders { get; set; }
        public CubeMap? CubeMapReflection { get; set; }
        public bool SmoothShading { get; set; }
        public bool ReceivesShadows { get; set; }
        // Mesh needs a position, a rotation, Vertices look-up, a shaders look-up, a materials look-up, a faces look-up
        public Mesh(Vec3D Position, Vec3D Scale, Quat Rotation, Vec3D[] Vertices, Vec2D[] TextureCoords, Material[] Materials,
            ITexelShiftShader[] TexelShiftShaders, IPixelShader[] PixelShaders, IPixelShader[] DeferredPixelShaders,
            IDeferredShader[] DeferredShaders, Surface[] Faces, bool SmoothShading = false, bool ReceivesShadows = true)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Rotation = Rotation;
            this.Vertices = Vertices;
            this.TextureCoords = TextureCoords;
            this.Materials = Materials;
            this.TexelShiftShaders = TexelShiftShaders;
            this.PixelShaders = PixelShaders;
            this.DeferredPixelShaders = DeferredPixelShaders;
            this.DeferredShaders = DeferredShaders;
            this.Faces = Faces;
            this.SmoothShading = SmoothShading;
            this.ReceivesShadows = ReceivesShadows;
            //Compute vertex normals using surface normals of each face
            VertexNormals = ComputeVertexNormals();
        }

        private Vec3D[] ComputeVertexNormals()
        {
            Vec3D[] TempVertexNormals = new Vec3D[Vertices.Length];
            for (int VertID = 0; VertID < Vertices.Length; VertID++)
            {
                Vec3D VertexNormal = new Vec3D(0, 0, 0);
                foreach (Surface Face in Faces)
                {
                    if (Face.Vert1ID == VertID)
                    {
                        VertexNormal += Vec3D.CrossProduct(Face.Verts(this).Edge1, Face.Verts(this).Edge2).Normalise();
                        Face.Vert1NormalID = VertID;
                    }
                    else if (Face.Vert2ID == VertID)
                    {
                        VertexNormal += Vec3D.CrossProduct(Face.Verts(this).Edge1, Face.Verts(this).Edge2).Normalise();
                        Face.Vert2NormalID = VertID;
                    }
                    else if (Face.Vert3ID == VertID)
                    {
                        VertexNormal += Vec3D.CrossProduct(Face.Verts(this).Edge1, Face.Verts(this).Edge2).Normalise();
                        Face.Vert3NormalID = VertID;
                    }
                }
                //Normalising this sum of normalised vectors gives the average
                TempVertexNormals[VertID] = VertexNormal.Normalise().ScalarMult(-1);
            }
            return TempVertexNormals;
        }

        public Mesh(Vec3D Position, Vec3D Scale, Quat Rotation, Material[] Materials, ITexelShiftShader[] TexelShiftShaders,
            IPixelShader[] PixelShaders, IPixelShader[] DeferredPixelShaders, IDeferredShader[] DeferredShaders, string FileName,
            bool SmoothShading = false, int TextureWrappingMode = Surface.CLAMP, int TextureFilteringMode = Surface.BILLINEAR, bool EnableTexelAveraging = false,
            bool OverrideNormals = false, bool ReceivesShadows = true)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Rotation = Rotation;
            this.Materials = Materials;
            this.TexelShiftShaders = TexelShiftShaders;
            this.PixelShaders = PixelShaders;
            this.DeferredPixelShaders = DeferredPixelShaders;
            this.DeferredShaders = DeferredShaders;
            this.ReceivesShadows = ReceivesShadows;
            string ObjFile = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"Meshes\", FileName + ".obj"));
            List<Vec3D> TempVertices = new List<Vec3D> { };
            List<Vec3D> TempVertexNormals = new List<Vec3D> { };
            List<Vec2D> TempTextureCoords = new List<Vec2D> { };
            List<Surface> TempFaces = new List<Surface> { };
            foreach (string Line in ObjFile.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] Values = Line.Trim('\n').Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (Values[0] == "v")
                {
                    TempVertices.Add(new Vec3D(double.Parse(Values[1]), double.Parse(Values[2]), double.Parse(Values[3])));
                }
                else if (Values[0] == "vn")
                {
                    TempVertexNormals.Add(new Vec3D(double.Parse(Values[1]), double.Parse(Values[2]), double.Parse(Values[3])));
                }
                else if (Values[0] == "vt")
                {
                    TempTextureCoords.Add(new Vec2D(double.Parse(Values[1]), 1 - double.Parse(Values[2])));
                }
                else if (Values[0] == "f")
                {
                    int Offset = 2;
                    if (Values[Values.Length - 1].Contains('/')) //Offsets for if there are trailing spaces
                    {
                        Offset = 1;
                    }
                    for (int i = 2; i < Values.Length - Offset; i++)
                    {
                        if (Values[1].Split('/', StringSplitOptions.RemoveEmptyEntries).Length == 2)
                        {
                            //Vertex normals not specified
                            OverrideNormals = true;
                            TempFaces.Add(new Surface(int.Parse(Values[1].Split('/', StringSplitOptions.RemoveEmptyEntries)[0]) - 1,
                                int.Parse(Values[i + 1].Split('/', StringSplitOptions.RemoveEmptyEntries)[0]) - 1,
                                int.Parse(Values[i].Split('/', StringSplitOptions.RemoveEmptyEntries)[0]) - 1, 0,
                                int.Parse(Values[1].Split('/', StringSplitOptions.RemoveEmptyEntries)[1]) - 1,
                                int.Parse(Values[i + 1].Split('/', StringSplitOptions.RemoveEmptyEntries)[1]) - 1,
                                int.Parse(Values[i].Split('/', StringSplitOptions.RemoveEmptyEntries)[1]) - 1,
                                0, 0, 0,
                                TextureWrappingMode: TextureWrappingMode, EnableTexelAveraging: EnableTexelAveraging,
                                TextureFilteringMode: TextureFilteringMode));
                        }
                        else if (Values[1].Split('/', StringSplitOptions.RemoveEmptyEntries).Length == 3)
                        {
                            TempFaces.Add(new Surface(int.Parse(Values[1].Split('/', StringSplitOptions.RemoveEmptyEntries)[0]) - 1,
                                int.Parse(Values[i + 1].Split('/', StringSplitOptions.RemoveEmptyEntries)[0]) - 1,
                                int.Parse(Values[i].Split('/', StringSplitOptions.RemoveEmptyEntries)[0]) - 1, 0,
                                int.Parse(Values[1].Split('/', StringSplitOptions.RemoveEmptyEntries)[1]) - 1,
                                int.Parse(Values[i + 1].Split('/', StringSplitOptions.RemoveEmptyEntries)[1]) - 1,
                                int.Parse(Values[i].Split('/', StringSplitOptions.RemoveEmptyEntries)[1]) - 1,
                                int.Parse(Values[1].Split('/', StringSplitOptions.RemoveEmptyEntries)[2]) - 1,
                                int.Parse(Values[i + 1].Split('/', StringSplitOptions.RemoveEmptyEntries)[2]) - 1,
                                int.Parse(Values[i].Split('/', StringSplitOptions.RemoveEmptyEntries)[2]) - 1,
                                TextureWrappingMode: TextureWrappingMode, EnableTexelAveraging: EnableTexelAveraging,
                                TextureFilteringMode: TextureFilteringMode));
                        }
                        else
                        {
                            throw new Exception(".obj file does not specify texture coordinates!");
                        }
                    }
                }
            }
            Vertices = TempVertices.ToArray();
            TextureCoords = TempTextureCoords.ToArray();
            Faces = TempFaces.ToArray();
            this.SmoothShading = SmoothShading;
            if (OverrideNormals)
            {
                VertexNormals = ComputeVertexNormals();
            }
            else
            {
                VertexNormals = TempVertexNormals.ToArray();
            }
        }
    }
}
