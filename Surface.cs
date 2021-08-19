using System;

namespace _3D_Renderer
{
    class Surface
    {
        //Class used to store each triangle in a mesh

        //Constants
        public const int DISCARD = 0;
        public const int CLAMP = 1;
        public const int REPEAT = 2;
        public const int MIRROR = 3;
        //CLAMPFAST skips the clamping to nearest barycentric coordinate but will cause artifacts with most texel shifting shaders
        public const int CLAMPFAST = 4;

        public const int NONE = 0;
        public const int BILLINEAR = 1;
        //Trillinear filtering is not really necessary and would just put an even greater strain on memory bandwidth
        //public const int TRILLINEAR = 2;

        //VertIDs must be public so mesh can change them when recomputing normals
        public int Vert1ID;
        public int Vert2ID;
        public int Vert3ID;
        private readonly int MaterialID;
        private readonly int Vert1TextureCoordID;
        private readonly int Vert2TextureCoordID;
        private readonly int Vert3TextureCoordID;
        public int Vert1NormalID;
        public int Vert2NormalID;
        public int Vert3NormalID;
        public bool BackFaceCull { get; set; }
        public int TextureWrappingMode { get; set; }
        public int TextureFilteringMode { get; set; }
        public bool EnableTexelAveraging { get; set; }
        public Surface(int Vert1ID, int Vert2ID, int Vert3ID, int MaterialID, int Vert1TextureCoordID, int Vert2TextureCoordID, int Vert3TextureCoordID, bool BackFaceCull = true, int TextureWrappingMode = CLAMP, int TextureFilteringMode = BILLINEAR, bool EnableTexelAveraging = true)
        {
            this.Vert1ID = Vert1ID;
            this.Vert2ID = Vert2ID;
            this.Vert3ID = Vert3ID;
            this.MaterialID = MaterialID;
            this.Vert1TextureCoordID = Vert1TextureCoordID;
            this.Vert2TextureCoordID = Vert2TextureCoordID;
            this.Vert3TextureCoordID = Vert3TextureCoordID;
            Vert1NormalID = -1;
            Vert2NormalID = -1;
            Vert3NormalID = -1;
            this.BackFaceCull = BackFaceCull;
            this.TextureWrappingMode = TextureWrappingMode;
            this.TextureFilteringMode = TextureFilteringMode;
            this.EnableTexelAveraging = EnableTexelAveraging;
        }
        public Surface(int Vert1ID, int Vert2ID, int Vert3ID, int MaterialID, int Vert1TextureCoordID, int Vert2TextureCoordID, int Vert3TextureCoordID, int Vert1NormalID, int Vert2NormalID, int Vert3NormalID, bool BackFaceCull = true, int TextureWrappingMode = CLAMP, int TextureFilteringMode = BILLINEAR, bool EnableTexelAveraging = true)
        {
            this.Vert1ID = Vert1ID;
            this.Vert2ID = Vert2ID;
            this.Vert3ID = Vert3ID;
            this.MaterialID = MaterialID;
            this.Vert1TextureCoordID = Vert1TextureCoordID;
            this.Vert2TextureCoordID = Vert2TextureCoordID;
            this.Vert3TextureCoordID = Vert3TextureCoordID;
            this.Vert1NormalID = Vert1NormalID;
            this.Vert2NormalID = Vert2NormalID;
            this.Vert3NormalID = Vert3NormalID;
            this.BackFaceCull = BackFaceCull;
            this.TextureWrappingMode = TextureWrappingMode;
            this.TextureFilteringMode = TextureFilteringMode;
            this.EnableTexelAveraging = EnableTexelAveraging;
        }

        public TriVec3D Verts(Mesh MasterMesh)
        {
            return new TriVec3D(MasterMesh.Vertices[Vert1ID], MasterMesh.Vertices[Vert2ID], MasterMesh.Vertices[Vert3ID]);
        }

        public bool Contains(int VertID)
        {
            return (Vert1ID == VertID) || (Vert2ID == VertID) || (Vert3ID == VertID);
        }

        public TriVec3D Normals(Mesh MasterMesh)
        {
            if (Vert1NormalID == -1)
            {
                //If vertex normals are not defined specifically, use indexes of vertices
                return new TriVec3D(MasterMesh.VertexNormals[Vert1ID], MasterMesh.VertexNormals[Vert2ID], MasterMesh.VertexNormals[Vert3ID]);
            }
            else
            {
                return new TriVec3D(MasterMesh.VertexNormals[Vert1NormalID], MasterMesh.VertexNormals[Vert2NormalID], MasterMesh.VertexNormals[Vert3NormalID]);
            }
        }

        public Vec3D SurfaceNormal(Mesh MasterMesh, Bary Coord)
        {
            //Converts to actual texture coordinates and then back to barycentric to ensure normals are computed correctly with different texture wrapping modes
            return Normals(MasterMesh).BaryInterp(VertexTextureCoords(MasterMesh).ComputeInternalBaryCoord(TextureSpacePos(MasterMesh, Coord)));
        }

        public Material Material(Mesh MasterMesh)
        {
            return MasterMesh.Materials[MaterialID];
        }

        public Vec2D TextureSpacePos(Mesh MasterMesh, Bary Coord)
        {
            Vec2D TextureSpacePos;
            if (!Coord.Internal())
            {
                if (TextureWrappingMode == DISCARD)
                {
                    return new Vec2D(-1, -1);
                }
                else if (TextureWrappingMode == CLAMP)
                {
                    Coord = Coord.Clamp();
                }
            }
            TextureSpacePos = VertexTextureCoords(MasterMesh).BaryInterp(Coord);
            return TextureSpacePos;
        }

        public TriVec2D VertexTextureCoords(Mesh MasterMesh)
        {
            return new TriVec2D(MasterMesh.TextureCoords[Vert1TextureCoordID], MasterMesh.TextureCoords[Vert2TextureCoordID],
                MasterMesh.TextureCoords[Vert3TextureCoordID]);
        }

        private Vec2D? WrapToUVBase(Vec2D TextureSpacePos)
        {
            if (TextureSpacePos.X < 0 || TextureSpacePos.X >= 1 || TextureSpacePos.Y < 0 || TextureSpacePos.Y >= 1)
            {
                switch (TextureWrappingMode)
                {
                    case DISCARD:
                        return null;
                    case CLAMP:
                    case CLAMPFAST:
                        return new Vec2D(Math.Max(Math.Min(TextureSpacePos.X, 1), 0), Math.Max(Math.Min(TextureSpacePos.Y, 1), 0));
                    case REPEAT:
                        return new Vec2D(Double.Mod(TextureSpacePos.X, 1), Math.Max(Math.Min(TextureSpacePos.Y, 1), 0));
                    case MIRROR:
                        double NewX;
                        double NewY;
                        if (Double.Mod(TextureSpacePos.X, 2) <= 1)
                        {
                            NewX = Double.Mod(TextureSpacePos.X, 1);
                        }
                        else
                        {
                            NewX = Double.Mod(-TextureSpacePos.X, 1);
                        }
                        if (Double.Mod(TextureSpacePos.Y, 2) <= 1)
                        {
                            NewY = Double.Mod(TextureSpacePos.Y, 1);
                        }
                        else
                        {
                            NewY = Double.Mod(-TextureSpacePos.Y, 1);
                        }
                        return new Vec2D(NewX, NewY);
                    default:
                        return null;
                }
            }
            else
            {
                return TextureSpacePos;
            }
        }

        public Vec2D? WrapToUV(Vec2D TextureSpacePos, double TexelWidth, double TexelHeight)
        {
            if (EnableTexelAveraging)
            {
                return WrapToUVBase(TextureSpacePos);
                //Old - causes artifacts when used with billinear filtering - to fix would have to drastically change how texture filtering is handled
                //No offset does lead to slight discontinuities when texel widths and heights change, but it is not hugely obvious and much better than the alternative
                //return WrapToUVBase(TextureSpacePos - new Vec2D(TexelWidth / 2, TexelHeight / 2));
            }
            else
            {
                return WrapToUVBase(TextureSpacePos);
            }
        }

        public Colour Albedo(Mesh MasterMesh, Vec2D? UV, Vec2D TextureSpacePos, double TexelWidth, double TexelHeight)
        {
            if (!(UV == null))
            {
                Vec2D ValidUV = (Vec2D)UV;
                if (TextureFilteringMode == NONE)
                {
                    if (EnableTexelAveraging)
                    {
                        return Material(MasterMesh).AveragedAlbedo(ValidUV, TexelWidth, TexelHeight);
                    }
                    else
                    {
                        return Material(MasterMesh).Albedo(ValidUV);
                    }
                }
                else if (TextureFilteringMode == BILLINEAR)
                {
                    if (EnableTexelAveraging)
                    {
                        double[] LerpFactors = Material(MasterMesh).AlbedoLerpFactors(TextureSpacePos);
                        return Colour.BiLerp(Material(MasterMesh).AveragedAlbedo(ValidUV, TexelWidth, TexelHeight),
                                            Material(MasterMesh).AveragedAlbedo(ValidUV, TexelWidth, TexelHeight, new Vec2D(0, 1)),
                                            Material(MasterMesh).AveragedAlbedo(ValidUV, TexelWidth, TexelHeight, new Vec2D(1, 0)),
                                            Material(MasterMesh).AveragedAlbedo(ValidUV, TexelWidth, TexelHeight, new Vec2D(1, 1)),
                                            LerpFactors[1], LerpFactors[0]);
                    }
                    else
                    {
                        double[] LerpFactors = Material(MasterMesh).AlbedoLerpFactors(TextureSpacePos);
                        return Colour.BiLerp(Material(MasterMesh).Albedo(ValidUV),
                                            Material(MasterMesh).Albedo(ValidUV, new Vec2D(0, 1)),
                                            Material(MasterMesh).Albedo(ValidUV, new Vec2D(1, 0)),
                                            Material(MasterMesh).Albedo(ValidUV, new Vec2D(1, 1)),
                                            LerpFactors[1], LerpFactors[0]);
                    }
                }
            }
            return new Colour(0, 0, 0, 0);
        }

        public double Displacement(Mesh MasterMesh, Vec2D? UV, Vec2D TextureSpacePos, double TexelWidth, double TexelHeight)
        {
            if (!(UV == null))
            {
                Vec2D ValidUV = (Vec2D)UV;
                if (TextureFilteringMode == NONE)
                {
                    if (EnableTexelAveraging)
                    {
                        return Material(MasterMesh).AveragedDisplacement(ValidUV, TexelWidth, TexelHeight);
                    }
                    else
                    {
                        return Material(MasterMesh).Displacement(ValidUV);
                    }
                }
                else if (TextureFilteringMode == BILLINEAR)
                {
                    if (EnableTexelAveraging)
                    {
                        double[] LerpFactors = Material(MasterMesh).DisplacementLerpFactors(TextureSpacePos);
                        return Double.BiLerp(Material(MasterMesh).AveragedDisplacement(ValidUV, TexelWidth, TexelHeight),
                                            Material(MasterMesh).AveragedDisplacement(ValidUV, TexelWidth, TexelHeight, new Vec2D(0, 1)),
                                            Material(MasterMesh).AveragedDisplacement(ValidUV, TexelWidth, TexelHeight, new Vec2D(1, 0)),
                                            Material(MasterMesh).AveragedDisplacement(ValidUV, TexelWidth, TexelHeight, new Vec2D(1, 1)),
                                            LerpFactors[1], LerpFactors[0]);
                    }
                    else
                    {
                        double[] LerpFactors = Material(MasterMesh).DisplacementLerpFactors(TextureSpacePos);
                        return Double.BiLerp(Material(MasterMesh).Displacement(ValidUV),
                                            Material(MasterMesh).Displacement(ValidUV, new Vec2D(0, 1)),
                                            Material(MasterMesh).Displacement(ValidUV, new Vec2D(1, 0)),
                                            Material(MasterMesh).Displacement(ValidUV, new Vec2D(1, 1)),
                                            LerpFactors[1], LerpFactors[0]);
                    }
                }
            }
            return 0;
        }

        public Vec3D Normal(Mesh MasterMesh, Vec2D? UV, Vec2D TextureSpacePos, double TexelWidth, double TexelHeight)
        {
            if (!(UV == null))
            {
                Vec2D ValidUV = (Vec2D)UV;
                if (TextureFilteringMode == NONE)
                {
                    if (EnableTexelAveraging)
                    {
                        return Material(MasterMesh).AveragedNormal(ValidUV, TexelWidth, TexelHeight);
                    }
                    else
                    {
                        return Material(MasterMesh).Normal(ValidUV);
                    }
                }
                else if (TextureFilteringMode == BILLINEAR)
                {
                    if (EnableTexelAveraging)
                    {
                        double[] LerpFactors = Material(MasterMesh).NormalLerpFactors(TextureSpacePos);
                        return Vec3D.BiLerp(Material(MasterMesh).AveragedNormal(ValidUV, TexelWidth, TexelHeight),
                                            Material(MasterMesh).AveragedNormal(ValidUV, TexelWidth, TexelHeight, new Vec2D(0, 1)),
                                            Material(MasterMesh).AveragedNormal(ValidUV, TexelWidth, TexelHeight, new Vec2D(1, 0)),
                                            Material(MasterMesh).AveragedNormal(ValidUV, TexelWidth, TexelHeight, new Vec2D(1, 1)),
                                            LerpFactors[1], LerpFactors[0]);
                    }
                    else
                    {
                        double[] LerpFactors = Material(MasterMesh).NormalLerpFactors(TextureSpacePos);
                        return Vec3D.BiLerp(Material(MasterMesh).Normal(ValidUV),
                                            Material(MasterMesh).Normal(ValidUV, new Vec2D(0, 1)),
                                            Material(MasterMesh).Normal(ValidUV, new Vec2D(1, 0)),
                                            Material(MasterMesh).Normal(ValidUV, new Vec2D(1, 1)),
                                            LerpFactors[1], LerpFactors[0]);
                    }
                }
            }
            return new Vec3D(0, 0, 0);
        }

        public double Specular(Mesh MasterMesh, Vec2D? UV, Vec2D TextureSpacePos, double TexelWidth, double TexelHeight)
        {
            if (!(UV == null))
            {
                Vec2D ValidUV = (Vec2D)UV;
                if (TextureFilteringMode == NONE)
                {
                    if (EnableTexelAveraging)
                    {
                        return Material(MasterMesh).AveragedSpecular(ValidUV, TexelWidth, TexelHeight);
                    }
                    else
                    {
                        return Material(MasterMesh).Specular(ValidUV);
                    }
                }
                else if (TextureFilteringMode == BILLINEAR)
                {
                    if (EnableTexelAveraging)
                    {
                        double[] LerpFactors = Material(MasterMesh).SpecularLerpFactors(TextureSpacePos);
                        return Double.BiLerp(Material(MasterMesh).AveragedSpecular(ValidUV, TexelWidth, TexelHeight),
                                            Material(MasterMesh).AveragedSpecular(ValidUV, TexelWidth, TexelHeight, new Vec2D(0, 1)),
                                            Material(MasterMesh).AveragedSpecular(ValidUV, TexelWidth, TexelHeight, new Vec2D(1, 0)),
                                            Material(MasterMesh).AveragedSpecular(ValidUV, TexelWidth, TexelHeight, new Vec2D(1, 1)),
                                            LerpFactors[1], LerpFactors[0]);
                    }
                    else
                    {
                        double[] LerpFactors = Material(MasterMesh).SpecularLerpFactors(TextureSpacePos);
                        return Double.BiLerp(Material(MasterMesh).Specular(ValidUV),
                                            Material(MasterMesh).Specular(ValidUV, new Vec2D(0, 1)),
                                            Material(MasterMesh).Specular(ValidUV, new Vec2D(1, 0)),
                                            Material(MasterMesh).Specular(ValidUV, new Vec2D(1, 1)),
                                            LerpFactors[1], LerpFactors[0]);
                    }
                }
            }
            return 0;
        }
        public Colour Emissive(Mesh MasterMesh, Vec2D? UV, Vec2D TextureSpacePos, double TexelWidth, double TexelHeight)
        {
            if (!(UV == null))
            {
                Vec2D ValidUV = (Vec2D)UV;
                if (TextureFilteringMode == NONE)
                {
                    if (EnableTexelAveraging)
                    {
                        return Material(MasterMesh).AveragedEmissive(ValidUV, TexelWidth, TexelHeight);
                    }
                    else
                    {
                        return Material(MasterMesh).Emissive(ValidUV);
                    }
                }
                else if (TextureFilteringMode == BILLINEAR)
                {
                    if (EnableTexelAveraging)
                    {
                        double[] LerpFactors = Material(MasterMesh).EmissiveLerpFactors(TextureSpacePos);
                        return Colour.BiLerp(Material(MasterMesh).AveragedEmissive(ValidUV, TexelWidth, TexelHeight),
                                            Material(MasterMesh).AveragedEmissive(ValidUV, TexelWidth, TexelHeight, new Vec2D(0, 1)),
                                            Material(MasterMesh).AveragedEmissive(ValidUV, TexelWidth, TexelHeight, new Vec2D(1, 0)),
                                            Material(MasterMesh).AveragedEmissive(ValidUV, TexelWidth, TexelHeight, new Vec2D(1, 1)),
                                            LerpFactors[1], LerpFactors[0]);
                    }
                    else
                    {
                        double[] LerpFactors = Material(MasterMesh).EmissiveLerpFactors(TextureSpacePos);
                        return Colour.BiLerp(Material(MasterMesh).Emissive(ValidUV),
                                            Material(MasterMesh).Emissive(ValidUV, new Vec2D(0, 1)),
                                            Material(MasterMesh).Emissive(ValidUV, new Vec2D(1, 0)),
                                            Material(MasterMesh).Emissive(ValidUV, new Vec2D(1, 1)),
                                            LerpFactors[1], LerpFactors[0]);
                    }
                }
            }
            return new Colour(0, 0, 0, 0);
        }
    }
}
