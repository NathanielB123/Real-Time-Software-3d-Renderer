using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace _3D_Renderer
{
    readonly struct Material
    {
        //Stores material information
        private readonly Arr2D<Colour> AlbedoMap;
        private readonly Arr2D<int[]> SummedAreaAlbedoMap;
        private readonly Arr2D<double> DisplacementMap;
        private readonly Arr2D<double> SummedAreaDisplacementMap;
        private readonly Arr2D<Vec3D> NormalMap;
        private readonly Arr2D<Vec3D> SummedAreaNormalMap;
        private readonly Arr2D<double> SpecularMap;
        private readonly Arr2D<double> SummedAreaSpecularMap;
        private readonly Arr2D<Colour> EmissiveMap;
        private readonly Arr2D<int[]> SummedAreaEmissiveMap;
        public Material(string FileName)
        {
            //Load textures
            Bitmap Img;
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, @"Textures\", FileName + ".png")))
            {
                Img = new Bitmap(new Bitmap(Image.FromFile(Path.Combine(Environment.CurrentDirectory, @"Textures\", FileName + ".png"))));
                Img = Img.Clone(new Rectangle(0, 0, Img.Width, Img.Height), PixelFormat.Format32bppArgb);
                AlbedoMap = new Arr2D<Colour>(Img.Width, Img.Height);
                for (int y = 0; y < Img.Height; y++)
                {
                    for (int x = 0; x < Img.Width; x++)
                    {
                        AlbedoMap[x, y] = new Colour(Img.GetPixel(x, y).R, Img.GetPixel(x, y).G, Img.GetPixel(x, y).B, Img.GetPixel(x, y).A);
                    }
                }
            }
            else
            {
                AlbedoMap = new Arr2D<Colour>(new Colour[,] { { new Colour(255, 255, 255, 255) } });
            }
            SummedAreaAlbedoMap = CreateSummedAreaAlbedoMap(AlbedoMap);
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, @"Textures\", FileName + "D.png")))
            {
                Img = new Bitmap(new Bitmap(Image.FromFile(Path.Combine(Environment.CurrentDirectory, @"Textures\", FileName + "D.png"))));
                Img = Img.Clone(new Rectangle(0, 0, Img.Width, Img.Height), PixelFormat.Format24bppRgb);
                DisplacementMap = new Arr2D<double>(Img.Width, Img.Height);
                for (int y = 0; y < Img.Height; y++)
                {
                    for (int x = 0; x < Img.Width; x++)
                    {
                        DisplacementMap[x, y] = (double)Img.GetPixel(x, y).R / 255;
                    }
                }
            }
            else
            {
                DisplacementMap = new Arr2D<double>(new double[,] { { 0 } });
            }
            SummedAreaDisplacementMap = CreateSummedAreaDisplacementMap(DisplacementMap);
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, @"Textures\", FileName + "N.png")))
            {
                Img = new Bitmap(new Bitmap(Image.FromFile(Path.Combine(Environment.CurrentDirectory, @"Textures\", FileName + "N.png"))));
                Img = Img.Clone(new Rectangle(0, 0, Img.Width, Img.Height), PixelFormat.Format24bppRgb);
                NormalMap = new Arr2D<Vec3D>(Img.Width, Img.Height);
                for (int y = 0; y < Img.Height; y++)
                {
                    for (int x = 0; x < Img.Width; x++)
                    {
                        NormalMap[x, y] = new Vec3D(((double)Img.GetPixel(x, y).R - 128) / 128,
                            ((double)Img.GetPixel(x, y).G - 128) / 128,
                            ((double)Img.GetPixel(x, y).B - 128) / 128).Normalise();
                    }
                }

            }
            else
            {
                NormalMap = new Arr2D<Vec3D>(new Vec3D[,] { { new Vec3D(0, 0, 1) } });
            }
            SummedAreaNormalMap = CreateSummedAreaNormalMap(NormalMap);
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, @"Textures\", FileName + "S.png")))
            {
                Img = new Bitmap(new Bitmap(Image.FromFile(Path.Combine(Environment.CurrentDirectory, @"Textures\", FileName + "S.png"))));
                Img = Img.Clone(new Rectangle(0, 0, Img.Width, Img.Height), PixelFormat.Format24bppRgb);
                SpecularMap = new Arr2D<double>(Img.Width, Img.Height);
                for (int y = 0; y < Img.Height; y++)
                {
                    for (int x = 0; x < Img.Width; x++)
                    {
                        SpecularMap[x, y] = (double)Img.GetPixel(x, y).R / 255;
                    }
                }
            }
            else
            {
                SpecularMap = new Arr2D<double>(new double[,] { { 0.5 } });
            }
            SummedAreaSpecularMap = CreateSummedAreaSpecularMap(SpecularMap);
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, @"Textures\", FileName + "E.png")))
            {
                Img = new Bitmap(new Bitmap(Image.FromFile(Path.Combine(Environment.CurrentDirectory, @"Textures\", FileName + "E.png"))));
                Img = Img.Clone(new Rectangle(0, 0, Img.Width, Img.Height), PixelFormat.Format24bppRgb);
                EmissiveMap = new Arr2D<Colour>(Img.Width, Img.Height);
                for (int y = 0; y < Img.Height; y++)
                {
                    for (int x = 0; x < Img.Width; x++)
                    {
                        EmissiveMap[x, y] = new Colour(Img.GetPixel(x, y).R, Img.GetPixel(x, y).G, Img.GetPixel(x, y).B, Img.GetPixel(x, y).A);
                    }
                }
            }
            else
            {
                EmissiveMap = new Arr2D<Colour>(new Colour[,] { { new Colour(0, 0, 0) } });
            }
            SummedAreaEmissiveMap = CreateSummedAreaEmissiveMap(EmissiveMap);
        }

        public Material(Arr2D<Colour>? AlbedoMap = null, Arr2D<double>? DisplacementMap = null, Arr2D<Vec3D>? NormalMap = null, Arr2D<double>? SpecularMap = null,
            Arr2D<Colour>? EmissiveMap = null)
        {
            if (AlbedoMap != null)
            {
                this.AlbedoMap = AlbedoMap;
            }
            else
            {
                this.AlbedoMap = new Arr2D<Colour>(new Colour[,] { { new Colour(0, 0, 0) } });
            }
            SummedAreaAlbedoMap = CreateSummedAreaAlbedoMap(this.AlbedoMap);
            if (DisplacementMap != null)
            {
                this.DisplacementMap = DisplacementMap;
            }
            else
            {
                this.DisplacementMap = new Arr2D<double>(new double[,] { { 0 } });
            }
            SummedAreaDisplacementMap = CreateSummedAreaDisplacementMap(this.DisplacementMap);
            if (NormalMap != null)
            {
                this.NormalMap = NormalMap;
            }
            else
            {
                this.NormalMap = new Arr2D<Vec3D>(new Vec3D[,] { { new Vec3D(0, 0, 1) } });
            }
            SummedAreaNormalMap = CreateSummedAreaNormalMap(this.NormalMap);
            if (SpecularMap != null)
            {
                this.SpecularMap = SpecularMap;
            }
            else
            {
                this.SpecularMap = new Arr2D<double>(new double[,] { { 0.5 } });
            }
            SummedAreaSpecularMap = CreateSummedAreaSpecularMap(this.SpecularMap);
            if (EmissiveMap != null)
            {
                this.EmissiveMap = EmissiveMap;
            }
            else
            {
                this.EmissiveMap = new Arr2D<Colour>(new Colour[,] { { new Colour(0, 0, 0) } });
            }
            SummedAreaEmissiveMap = CreateSummedAreaEmissiveMap(this.EmissiveMap);
        }
        public Colour Albedo(Vec2D UV)
        {
            if (double.IsNaN(UV.X) || double.IsNaN(UV.Y))
            {
                UV = new Vec2D(0, 0);
            }
            return AlbedoMap[(int)Math.Max(0, Math.Min(Math.Floor(UV.X * AlbedoMap.Width), AlbedoMap.Width - 1)),
                (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * AlbedoMap.Height), AlbedoMap.Height - 1))];
        }
        public Colour Albedo(Vec2D UV, Vec2D Offset)
        {
            if (double.IsNaN(UV.X) || double.IsNaN(UV.Y))
            {
                UV = new Vec2D(0, 0);
            }
            return AlbedoMap[(int)Math.Max(0, Math.Min(Math.Floor(UV.X * AlbedoMap.Width + Offset.X), AlbedoMap.Width - 1)),
                (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * AlbedoMap.Height + Offset.Y), AlbedoMap.Height - 1))];
        }

        private int[] SummedAreaAlbedo(int[] UV)
        {
            return SummedAreaAlbedoMap[UV[0], UV[1]];
        }

        public double[] AlbedoLerpFactors(Vec2D UV)
        {
            Vec2D TexCoord = new Vec2D(UV.X * AlbedoMap.Width, UV.Y * AlbedoMap.Height);
            return new double[] { TexCoord.X - Math.Floor(TexCoord.X), TexCoord.Y - Math.Floor(TexCoord.Y) };
        }

        public Colour AveragedAlbedo(Vec2D UV, double TexelWidth, double TexelHeight)
        {
            int TexelWidthRounded = (int)Math.Max(Math.Round(TexelWidth * AlbedoMap.Width), 0);
            int TexelHeightRounded = (int)Math.Max(Math.Round(TexelHeight * AlbedoMap.Height), 0);
            if (TexelWidthRounded == 1 && TexelHeightRounded == 1)
            {
                return Albedo(UV);
            }
            else
            {
                int UVX = (int)Math.Max(0, Math.Min(Math.Floor(UV.X * AlbedoMap.Width), AlbedoMap.Width - 1));
                int UVY = (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * AlbedoMap.Height), AlbedoMap.Height - 1));
                return AveragedAlbedo(UVX, UVY, TexelWidthRounded, TexelHeightRounded);
            }
        }
        public Colour AveragedAlbedo(Vec2D UV, double TexelWidth, double TexelHeight, Vec2D Offset)
        {
            int TexelWidthRounded = (int)Math.Max(Math.Round(TexelWidth * AlbedoMap.Width), 0);
            int TexelHeightRounded = (int)Math.Max(Math.Round(TexelHeight * AlbedoMap.Height), 0);
            if (TexelWidthRounded == 1 && TexelHeightRounded == 1)
            {
                return Albedo(UV, Offset);
            }
            else
            {
                int UVX = (int)Math.Max(0, Math.Min(Math.Floor(UV.X * AlbedoMap.Width + Offset.X), AlbedoMap.Width - 1));
                int UVY = (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * AlbedoMap.Height + Offset.Y), AlbedoMap.Height - 1));
                return AveragedAlbedo(UVX, UVY, TexelWidthRounded, TexelHeightRounded);
            }
        }

        private Colour AveragedAlbedo(int UVX, int UVY, int TexelWidth, int TexelHeight)
        {
            int[] UVA = new int[] { UVX, UVY };
            int[] UVB = new int[] { Math.Max(0, Math.Min(UVX + TexelWidth, AlbedoMap.Width - 1)), UVY };
            int[] UVC = new int[] { UVX, Math.Max(0, Math.Min(UVY + TexelHeight, AlbedoMap.Height - 1)) };
            int[] UVD = new int[] { Math.Max(0, Math.Min(UVX + TexelWidth, AlbedoMap.Width - 1)), Math.Max(0, Math.Min(UVY + TexelHeight, AlbedoMap.Height - 1)) };
            int Size = (UVA[0] - UVD[0]) * (UVA[1] - UVD[1]);
            if (Size <= 1)
            {
                return AlbedoMap[UVX, UVY];
            }
            else
            {
                int[] SummedAreaAlbedoA = SummedAreaAlbedo(UVA);
                int[] SummedAreaAlbedoB = SummedAreaAlbedo(UVB);
                int[] SummedAreaAlbedoC = SummedAreaAlbedo(UVC);
                int[] SummedAreaAlbedoD = SummedAreaAlbedo(UVD);
                int[] AlbedoSum = new int[] { SummedAreaAlbedoA[0] + SummedAreaAlbedoD[0] - SummedAreaAlbedoB[0] - SummedAreaAlbedoC[0],
                                             SummedAreaAlbedoA[1] + SummedAreaAlbedoD[1] - SummedAreaAlbedoB[1] - SummedAreaAlbedoC[1],
                                             SummedAreaAlbedoA[2] + SummedAreaAlbedoD[2] - SummedAreaAlbedoB[2] - SummedAreaAlbedoC[2],
                                             SummedAreaAlbedoA[3] + SummedAreaAlbedoD[3] - SummedAreaAlbedoB[3] - SummedAreaAlbedoC[3]};
                return new Colour(AlbedoSum[0] / Size, AlbedoSum[1] / Size, AlbedoSum[2] / Size, AlbedoSum[3] / Size);
            }
        }

        public double Displacement(Vec2D UV)
        {
            return DisplacementMap[(int)Math.Max(0, Math.Min(Math.Floor(UV.X * DisplacementMap.Width), DisplacementMap.Width - 1)),
                (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * DisplacementMap.Height), DisplacementMap.Height - 1))];
        }
        public double Displacement(Vec2D UV, Vec2D Offset)
        {
            return DisplacementMap[(int)Math.Max(0, Math.Min(Math.Floor(UV.X * DisplacementMap.Width + Offset.X), DisplacementMap.Width - 1)),
                (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * DisplacementMap.Height + Offset.Y), DisplacementMap.Height - 1))];
        }
        public double[] DisplacementLerpFactors(Vec2D UV)
        {
            Vec2D TexCoord = new Vec2D(UV.X * DisplacementMap.Width, UV.Y * DisplacementMap.Height);
            return new double[] { TexCoord.X - Math.Floor(TexCoord.X), TexCoord.Y - Math.Floor(TexCoord.Y) };
        }

        private double SummedAreaDisplacement(int[] UV)
        {
            return SummedAreaDisplacementMap[UV[0], UV[1]];
        }

        public double AveragedDisplacement(Vec2D UV, double TexelWidth, double TexelHeight)
        {
            int TexelWidthRounded = (int)Math.Max(Math.Round(TexelWidth * DisplacementMap.Width), 0);
            int TexelHeightRounded = (int)Math.Max(Math.Round(TexelHeight * DisplacementMap.Height), 0);
            if (TexelWidthRounded == 1 && TexelHeightRounded == 1)
            {
                return Displacement(UV);
            }
            else
            {
                int UVX = (int)Math.Max(0, Math.Min(Math.Floor(UV.X * DisplacementMap.Width), DisplacementMap.Width - 1));
                int UVY = (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * DisplacementMap.Height), DisplacementMap.Height - 1));
                return AveragedDisplacement(UVX, UVY, TexelWidthRounded, TexelHeightRounded);
            }
        }
        public double AveragedDisplacement(Vec2D UV, double TexelWidth, double TexelHeight, Vec2D Offset)
        {
            int TexelWidthRounded = (int)Math.Max(Math.Round(TexelWidth * DisplacementMap.Width), 0);
            int TexelHeightRounded = (int)Math.Max(Math.Round(TexelHeight * DisplacementMap.Height), 0);
            if (TexelWidthRounded == 1 && TexelHeightRounded == 1)
            {
                return Displacement(UV, Offset);
            }
            else
            {
                int UVX = (int)Math.Max(0, Math.Min(Math.Floor(UV.X * DisplacementMap.Width + Offset.X), DisplacementMap.Width - 1));
                int UVY = (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * DisplacementMap.Height + Offset.Y), DisplacementMap.Height - 1));
                return AveragedDisplacement(UVX, UVY, TexelWidthRounded, TexelHeightRounded);
            }
        }

        private double AveragedDisplacement(int UVX, int UVY, int TexelWidth, int TexelHeight)
        {
            int[] UVA = new int[] { UVX, UVY };
            int[] UVB = new int[] { Math.Max(0, Math.Min(UVX + TexelWidth, DisplacementMap.Width - 1)), UVY };
            int[] UVC = new int[] { UVX, Math.Max(0, Math.Min(UVY + TexelHeight, DisplacementMap.Height - 1)) };
            int[] UVD = new int[] { Math.Max(0, Math.Min(UVX + TexelWidth, DisplacementMap.Width - 1)), Math.Max(0, Math.Min(UVY + TexelHeight, DisplacementMap.Height - 1)) };
            int Size = (UVA[0] - UVD[0]) * (UVA[1] - UVD[1]);
            if (Size <= 1)
            {
                return DisplacementMap[UVX, UVY];
            }
            else
            {
                double SummedAreaDisplacementA = SummedAreaDisplacement(UVA);
                double SummedAreaDisplacementB = SummedAreaDisplacement(UVB);
                double SummedAreaDisplacementC = SummedAreaDisplacement(UVC);
                double SummedAreaDisplacementD = SummedAreaDisplacement(UVD);
                double DisplacementSum = SummedAreaDisplacementA + SummedAreaDisplacementD - SummedAreaDisplacementB - SummedAreaDisplacementC;
                return DisplacementSum / Size;
            }
        }

        public Vec3D Normal(Vec2D UV)
        {
            return NormalMap[(int)Math.Max(0, Math.Min(Math.Floor(UV.X * NormalMap.Width), NormalMap.Width - 1)),
                (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * NormalMap.Height), NormalMap.Height - 1))];
        }
        public Vec3D Normal(Vec2D UV, Vec2D Offset)
        {
            return NormalMap[(int)Math.Max(0, Math.Min(Math.Floor(UV.X * NormalMap.Width + Offset.X), NormalMap.Width - 1)),
                (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * NormalMap.Height + Offset.Y), NormalMap.Height - 1))];
        }

        public double[] NormalLerpFactors(Vec2D UV)
        {
            Vec2D TexCoord = new Vec2D(UV.X * NormalMap.Width, UV.Y * NormalMap.Height);
            return new double[] { TexCoord.X - Math.Floor(TexCoord.X), TexCoord.Y - Math.Floor(TexCoord.Y) };
        }

        private Vec3D SummedAreaNormal(int[] UV)
        {
            return SummedAreaNormalMap[UV[0], UV[1]];
        }

        public Vec3D AveragedNormal(Vec2D UV, double TexelWidth, double TexelHeight)
        {
            int TexelWidthRounded = (int)Math.Max(Math.Round(TexelWidth * NormalMap.Width), 0);
            int TexelHeightRounded = (int)Math.Max(Math.Round(TexelHeight * NormalMap.Height), 0);
            if (TexelWidthRounded == 1 && TexelHeightRounded == 1)
            {
                return Normal(UV);
            }
            else
            {
                int UVX = (int)Math.Max(0, Math.Min(Math.Floor(UV.X * NormalMap.Width), NormalMap.Width - 1));
                int UVY = (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * NormalMap.Height), NormalMap.Height - 1));
                return AveragedNormal(UVX, UVY, TexelWidthRounded, TexelHeightRounded);
            }
        }
        public Vec3D AveragedNormal(Vec2D UV, double TexelWidth, double TexelHeight, Vec2D Offset)
        {
            int TexelWidthRounded = (int)Math.Max(Math.Round(TexelWidth * NormalMap.Width), 0);
            int TexelHeightRounded = (int)Math.Max(Math.Round(TexelHeight * NormalMap.Height), 0);
            if (TexelWidthRounded == 1 && TexelHeightRounded == 1)
            {
                return Normal(UV, Offset);
            }
            else
            {
                int UVX = (int)Math.Max(0, Math.Min(Math.Floor(UV.X * NormalMap.Width + Offset.X), NormalMap.Width - 1));
                int UVY = (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * NormalMap.Height + Offset.Y), NormalMap.Height - 1));
                return AveragedNormal(UVX, UVY, TexelWidthRounded, TexelHeightRounded);
            }
        }

        private Vec3D AveragedNormal(int UVX, int UVY, int TexelWidth, int TexelHeight)
        {
            int[] UVA = new int[] { UVX, UVY };
            int[] UVB = new int[] { Math.Max(0, Math.Min(UVX + TexelWidth, NormalMap.Width - 1)), UVY };
            int[] UVC = new int[] { UVX, Math.Max(0, Math.Min(UVY + TexelHeight, NormalMap.Height - 1)) };
            int[] UVD = new int[] { Math.Max(0, Math.Min(UVX + TexelWidth, NormalMap.Width - 1)), Math.Max(0, Math.Min(UVY + TexelHeight, NormalMap.Height - 1)) };
            int Size = (UVA[0] - UVD[0]) * (UVA[1] - UVD[1]);
            if (Size <= 1)
            {
                return NormalMap[UVX, UVY];
            }
            else
            {
                Vec3D SummedAreaNormalA = SummedAreaNormal(UVA);
                Vec3D SummedAreaNormalB = SummedAreaNormal(UVB);
                Vec3D SummedAreaNormalC = SummedAreaNormal(UVC);
                Vec3D SummedAreaNormalD = SummedAreaNormal(UVD);
                Vec3D NormalSum = SummedAreaNormalA + SummedAreaNormalD - SummedAreaNormalB - SummedAreaNormalC;
                return NormalSum.ScalarMult(1.0 / Size);
            }
        }

        public double Specular(Vec2D UV)
        {
            return SpecularMap[(int)Math.Max(0, Math.Min(Math.Floor(UV.X * SpecularMap.Width), SpecularMap.Width - 1)),
                (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * SpecularMap.Height), SpecularMap.Height - 1))];
        }
        public double Specular(Vec2D UV, Vec2D Offset)
        {
            return SpecularMap[(int)Math.Max(0, Math.Min(Math.Floor(UV.X * SpecularMap.Width + Offset.X), SpecularMap.Width - 1)),
                (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * SpecularMap.Height + Offset.Y), SpecularMap.Height - 1))];
        }

        public double[] SpecularLerpFactors(Vec2D UV)
        {
            Vec2D TexCoord = new Vec2D(UV.X * SpecularMap.Width, UV.Y * SpecularMap.Height);
            return new double[] { TexCoord.X - Math.Floor(TexCoord.X), TexCoord.Y - Math.Floor(TexCoord.Y) };
        }

        private double SummedAreaSpecular(int[] UV)
        {
            return SummedAreaSpecularMap[UV[0], UV[1]];
        }

        public double AveragedSpecular(Vec2D UV, double TexelWidth, double TexelHeight)
        {
            int TexelWidthRounded = (int)Math.Max(Math.Round(TexelWidth * SpecularMap.Width), 0);
            int TexelHeightRounded = (int)Math.Max(Math.Round(TexelHeight * SpecularMap.Height), 0);
            if (TexelWidthRounded == 1 && TexelHeightRounded == 1)
            {
                return Specular(UV);
            }
            else
            {
                int UVX = (int)Math.Max(0, Math.Min(Math.Floor(UV.X * SpecularMap.Width), SpecularMap.Width - 1));
                int UVY = (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * SpecularMap.Height), SpecularMap.Height - 1));
                return AveragedSpecular(UVX, UVY, TexelWidthRounded, TexelHeightRounded);
            }
        }
        public double AveragedSpecular(Vec2D UV, double TexelWidth, double TexelHeight, Vec2D Offset)
        {
            int TexelWidthRounded = (int)Math.Max(Math.Round(TexelWidth * SpecularMap.Width), 0);
            int TexelHeightRounded = (int)Math.Max(Math.Round(TexelHeight * SpecularMap.Height), 0);
            if (TexelWidthRounded == 1 && TexelHeightRounded == 1)
            {
                return Specular(UV, Offset);
            }
            else
            {
                int UVX = (int)Math.Max(0, Math.Min(Math.Floor(UV.X * SpecularMap.Width + Offset.X), SpecularMap.Width - 1));
                int UVY = (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * SpecularMap.Height + Offset.Y), SpecularMap.Height - 1));
                return AveragedSpecular(UVX, UVY, TexelWidthRounded, TexelHeightRounded);
            }
        }

        private double AveragedSpecular(int UVX, int UVY, int TexelWidth, int TexelHeight)
        {
            int[] UVA = new int[] { UVX, UVY };
            int[] UVB = new int[] { Math.Max(0, Math.Min(UVX + TexelWidth, SpecularMap.Width - 1)), UVY };
            int[] UVC = new int[] { UVX, Math.Max(0, Math.Min(UVY + TexelHeight, SpecularMap.Height - 1)) };
            int[] UVD = new int[] { Math.Max(0, Math.Min(UVX + TexelWidth, SpecularMap.Width - 1)), Math.Max(0, Math.Min(UVY + TexelHeight, SpecularMap.Height - 1)) };
            int Size = (UVA[0] - UVD[0]) * (UVA[1] - UVD[1]);
            if (Size <= 1)
            {
                return SpecularMap[UVX, UVY];
            }
            else
            {
                double SummedAreaSpecularA = SummedAreaSpecular(UVA);
                double SummedAreaSpecularB = SummedAreaSpecular(UVB);
                double SummedAreaSpecularC = SummedAreaSpecular(UVC);
                double SummedAreaSpecularD = SummedAreaSpecular(UVD);
                double SpecularSum = SummedAreaSpecularA + SummedAreaSpecularD - SummedAreaSpecularB - SummedAreaSpecularC;
                return SpecularSum / Size;
            }
        }

        public Colour Emissive(Vec2D UV)
        {
            return EmissiveMap[(int)Math.Max(0, Math.Min(Math.Floor(UV.X * EmissiveMap.Width), EmissiveMap.Width - 1)),
                (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * EmissiveMap.Height), EmissiveMap.Height - 1))];
        }
        public Colour Emissive(Vec2D UV, Vec2D Offset)
        {
            return EmissiveMap[(int)Math.Max(0, Math.Min(Math.Floor(UV.X * EmissiveMap.Width + Offset.X), EmissiveMap.Width - 1)),
                (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * EmissiveMap.Height + Offset.Y), EmissiveMap.Height - 1))];
        }

        public double[] EmissiveLerpFactors(Vec2D UV)
        {
            Vec2D TexCoord = new Vec2D(UV.X * SpecularMap.Width, UV.Y * SpecularMap.Height);
            return new double[] { TexCoord.X - Math.Floor(TexCoord.X), TexCoord.Y - Math.Floor(TexCoord.Y) };
        }

        private int[] SummedAreaEmissive(int[] UV)
        {
            return SummedAreaEmissiveMap[UV[0], UV[1]];
        }

        public Colour AveragedEmissive(Vec2D UV, double TexelWidth, double TexelHeight)
        {
            int TexelWidthRounded = (int)Math.Max(Math.Round(TexelWidth * EmissiveMap.Width), 0);
            int TexelHeightRounded = (int)Math.Max(Math.Round(TexelHeight * EmissiveMap.Height), 0);
            if (TexelWidthRounded == 1 && TexelHeightRounded == 1)
            {
                return Emissive(UV);
            }
            else
            {
                int UVX = (int)Math.Max(0, Math.Min(Math.Floor(UV.X * EmissiveMap.Width), EmissiveMap.Width - 1));
                int UVY = (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * EmissiveMap.Height), EmissiveMap.Height - 1));
                return AveragedEmissive(UVX, UVY, TexelWidthRounded, TexelHeightRounded);
            }
        }
        public Colour AveragedEmissive(Vec2D UV, double TexelWidth, double TexelHeight, Vec2D Offset)
        {
            int TexelWidthRounded = (int)Math.Max(Math.Round(TexelWidth * EmissiveMap.Width), 0);
            int TexelHeightRounded = (int)Math.Max(Math.Round(TexelHeight * EmissiveMap.Height), 0);
            if (TexelWidthRounded == 1 && TexelHeightRounded == 1)
            {
                return Emissive(UV, Offset);
            }
            else
            {
                int UVX = (int)Math.Max(0, Math.Min(Math.Floor(UV.X * EmissiveMap.Width + Offset.X), EmissiveMap.Width - 1));
                int UVY = (int)Math.Max(0, Math.Min(Math.Floor(UV.Y * EmissiveMap.Height + Offset.Y), EmissiveMap.Height - 1));
                return AveragedEmissive(UVX, UVY, TexelWidthRounded, TexelHeightRounded);
            }
        }

        private Colour AveragedEmissive(int UVX, int UVY, int TexelWidth, int TexelHeight)
        {
            int[] UVA = new int[] { UVX, UVY };
            int[] UVB = new int[] { Math.Max(0, Math.Min(UVX + TexelWidth, EmissiveMap.Width - 1)), UVY };
            int[] UVC = new int[] { UVX, Math.Max(0, Math.Min(UVY + TexelHeight, EmissiveMap.Height - 1)) };
            int[] UVD = new int[] { Math.Max(0, Math.Min(UVX + TexelWidth, EmissiveMap.Width - 1)), Math.Max(0, Math.Min(UVY + TexelHeight, EmissiveMap.Height - 1)) };
            int Size = (UVA[0] - UVD[0]) * (UVA[1] - UVD[1]);
            if (Size <= 1)
            {
                return EmissiveMap[UVX, UVY];
            }
            else
            {
                int[] SummedAreaEmissiveA = SummedAreaEmissive(UVA);
                int[] SummedAreaEmissiveB = SummedAreaEmissive(UVB);
                int[] SummedAreaEmissiveC = SummedAreaEmissive(UVC);
                int[] SummedAreaEmissiveD = SummedAreaEmissive(UVD);
                int[] EmissiveSum = new int[] { SummedAreaEmissiveA[0] + SummedAreaEmissiveD[0] - SummedAreaEmissiveB[0] - SummedAreaEmissiveC[0],
                                             SummedAreaEmissiveA[1] + SummedAreaEmissiveD[1] - SummedAreaEmissiveB[1] - SummedAreaEmissiveC[1],
                                             SummedAreaEmissiveA[2] + SummedAreaEmissiveD[2] - SummedAreaEmissiveB[2] - SummedAreaEmissiveC[2],
                                             SummedAreaEmissiveA[3] + SummedAreaEmissiveD[3] - SummedAreaEmissiveB[3] - SummedAreaEmissiveC[3]};
                return new Colour(EmissiveSum[0] / Size, EmissiveSum[1] / Size, EmissiveSum[2] / Size, EmissiveSum[3] / Size);
            }
        }

        private static Arr2D<int[]> CreateSummedAreaAlbedoMap(Arr2D<Colour> AlbedoMap)
        {
            Arr2D<int[]> SummedAreaAlbedo = new Arr2D<int[]>(AlbedoMap.Width, AlbedoMap.Height);
            for (int y = 0; y < SummedAreaAlbedo.Height; y++)
            {
                for (int x = 0; x < SummedAreaAlbedo.Width; x++)
                {
                    int[] Colour1 = new int[] { AlbedoMap[x, y].Red, AlbedoMap[x, y].Green, AlbedoMap[x, y].Blue, AlbedoMap[x, y].Alpha };
                    int[] Colour2 = new int[] { 0, 0, 0, 255 };
                    int[] Colour3 = new int[] { 0, 0, 0, 255 };
                    int[] Colour4 = new int[] { 0, 0, 0, 255 };
                    if (y != 0)
                    {
                        Colour2 = new int[] { SummedAreaAlbedo[x, y - 1][0], SummedAreaAlbedo[x, y - 1][1], SummedAreaAlbedo[x, y - 1][2], SummedAreaAlbedo[x, y - 1][3] };
                    }
                    if (x != 0)
                    {
                        Colour3 = new int[] { SummedAreaAlbedo[x - 1, y][0], SummedAreaAlbedo[x - 1, y][1], SummedAreaAlbedo[x - 1, y][2], SummedAreaAlbedo[x - 1, y][3] };
                    }
                    if (x != 0 && y != 0)
                    {
                        Colour4 = new int[] { SummedAreaAlbedo[x - 1, y - 1][0], SummedAreaAlbedo[x - 1, y - 1][1], SummedAreaAlbedo[x - 1, y - 1][2], SummedAreaAlbedo[x - 1, y - 1][3] };
                    }
                    SummedAreaAlbedo[x, y] = new int[] {Colour1[0]+Colour2[0]+Colour3[0]-Colour4[0],
                                                         Colour1[1]+Colour2[1]+Colour3[1]-Colour4[1],
                                                         Colour1[2]+Colour2[2]+Colour3[2]-Colour4[2],
                                                         Colour1[3]+Colour2[3]+Colour3[3]-Colour4[3]};
                }
            }
            return SummedAreaAlbedo;
        }

        private static Arr2D<double> CreateSummedAreaDisplacementMap(Arr2D<double> DisplacementMap)
        {
            Arr2D<double> SummedAreaDisplacementMap = new Arr2D<double>(DisplacementMap.Width, DisplacementMap.Height);
            for (int y = 0; y < SummedAreaDisplacementMap.Height; y++)
            {
                for (int x = 0; x < SummedAreaDisplacementMap.Width; x++)
                {
                    double Displacement1 = DisplacementMap[x, y];
                    double Displacement2 = 0;
                    double Displacement3 = 0;
                    double Displacement4 = 0;
                    if (y != 0)
                    {
                        Displacement2 = SummedAreaDisplacementMap[x, y - 1];
                    }
                    if (x != 0)
                    {
                        Displacement3 = SummedAreaDisplacementMap[x - 1, y];
                    }
                    if (x != 0 && y != 0)
                    {
                        Displacement4 = SummedAreaDisplacementMap[x - 1, y - 1];
                    }
                    SummedAreaDisplacementMap[x, y] = Displacement1 + Displacement2 + Displacement3 - Displacement4;
                }
            }
            return SummedAreaDisplacementMap;
        }
        private static Arr2D<Vec3D> CreateSummedAreaNormalMap(Arr2D<Vec3D> NormalMap)
        {
            Arr2D<Vec3D> SummedAreaNormalMap = new Arr2D<Vec3D>(NormalMap.Width, NormalMap.Height);
            for (int y = 0; y < SummedAreaNormalMap.Height; y++)
            {
                for (int x = 0; x < SummedAreaNormalMap.Width; x++)
                {
                    Vec3D Normal1 = NormalMap[x, y];
                    Vec3D Normal2 = new Vec3D(0, 0, 1);
                    Vec3D Normal3 = new Vec3D(0, 0, 1);
                    Vec3D Normal4 = new Vec3D(0, 0, 1);
                    if (y != 0)
                    {
                        Normal2 = SummedAreaNormalMap[x, y - 1];
                    }
                    if (x != 0)
                    {
                        Normal3 = SummedAreaNormalMap[x - 1, y];
                    }
                    if (x != 0 && y != 0)
                    {
                        Normal4 = SummedAreaNormalMap[x - 1, y - 1];
                    }
                    SummedAreaNormalMap[x, y] = Normal1 + Normal2 + Normal3 - Normal4;
                }
            }
            return SummedAreaNormalMap;
        }
        private static Arr2D<double> CreateSummedAreaSpecularMap(Arr2D<double> SpecularMap)
        {
            Arr2D<double> SummedAreaSpecularMap = new Arr2D<double>(SpecularMap.Width, SpecularMap.Height);
            for (int y = 0; y < SummedAreaSpecularMap.Height; y++)
            {
                for (int x = 0; x < SummedAreaSpecularMap.Width; x++)
                {
                    double Specular1 = SpecularMap[x, y];
                    double Specular2 = 0;
                    double Specular3 = 0;
                    double Specular4 = 0;
                    if (y != 0)
                    {
                        Specular2 = SummedAreaSpecularMap[x, y - 1];
                    }
                    if (x != 0)
                    {
                        Specular3 = SummedAreaSpecularMap[x - 1, y];
                    }
                    if (x != 0 && y != 0)
                    {
                        Specular4 = SummedAreaSpecularMap[x - 1, y - 1];
                    }
                    SummedAreaSpecularMap[x, y] = Specular1 + Specular2 + Specular3 - Specular4;
                }
            }
            return SummedAreaSpecularMap;
        }
        private static Arr2D<int[]> CreateSummedAreaEmissiveMap(Arr2D<Colour> EmissiveMap)
        {
            Arr2D<int[]> SummedAreaEmissiveMap = new Arr2D<int[]>(EmissiveMap.Width, EmissiveMap.Height);
            for (int y = 0; y < SummedAreaEmissiveMap.Height; y++)
            {
                for (int x = 0; x < SummedAreaEmissiveMap.Width; x++)
                {
                    int[] Colour1 = new int[] { EmissiveMap[x, y].Red, EmissiveMap[x, y].Green, EmissiveMap[x, y].Blue, EmissiveMap[x, y].Alpha };
                    int[] Colour2 = new int[] { 0, 0, 0, 0 };
                    int[] Colour3 = new int[] { 0, 0, 0, 0 };
                    int[] Colour4 = new int[] { 0, 0, 0, 0 };
                    if (y != 0)
                    {
                        Colour2 = new int[] { SummedAreaEmissiveMap[x, y - 1][0], SummedAreaEmissiveMap[x, y - 1][1], SummedAreaEmissiveMap[x, y - 1][2], SummedAreaEmissiveMap[x, y - 1][3] };
                    }
                    if (x != 0)
                    {
                        Colour3 = new int[] { SummedAreaEmissiveMap[x - 1, y][0], SummedAreaEmissiveMap[x - 1, y][1], SummedAreaEmissiveMap[x - 1, y][2], SummedAreaEmissiveMap[x - 1, y][3] };
                    }
                    if (x != 0 && y != 0)
                    {
                        Colour4 = new int[] { SummedAreaEmissiveMap[x - 1, y - 1][0], SummedAreaEmissiveMap[x - 1, y - 1][1], SummedAreaEmissiveMap[x - 1, y - 1][2], SummedAreaEmissiveMap[x - 1, y - 1][3] };
                    }
                    SummedAreaEmissiveMap[x, y] = new int[] {Colour1[0]+Colour2[0]+Colour3[0]-Colour4[0],
                                                         Colour1[1]+Colour2[1]+Colour3[1]-Colour4[1],
                                                         Colour1[2]+Colour2[2]+Colour3[2]-Colour4[2],
                                                         Colour1[3]+Colour2[3]+Colour3[3]-Colour4[3]};
                }
            }
            return SummedAreaEmissiveMap;
        }
    }
}