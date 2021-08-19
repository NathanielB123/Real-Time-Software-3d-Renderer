using System;

namespace _3D_Renderer
{
    class CubeMap
    {
        public Mesh BoxObject;
        public CubeMap(Material Right, Material Left, Material Up, Material Down, Material Front, Material Back, int Size = 1)
        {
            //Used for skyboxes and reflections
            BoxObject = new Mesh(new Vec3D(0, 0, 0), new Vec3D(Size, Size, Size), new Quat(1, 0, 0, 0),
                new Vec3D[] { new Vec3D(2, 2, 2), new Vec3D(2, 2, -2), new Vec3D(2, -2, 2), new Vec3D(2, -2, -2),
                             new Vec3D(-2, 2, 2), new Vec3D(-2, 2, -2), new Vec3D(-2, -2, 2), new Vec3D(-2, -2, -2)},
                new Vec2D[] { new Vec2D(0, 0), new Vec2D(1, 0), new Vec2D(0, 1), new Vec2D(1, 1) },
                new Material[] { Right, Left, Up, Down, Front, Back },
                Array.Empty<ITexelShiftShader>(), new IPixelShader[] { new SkyBoxShader() }, Array.Empty<IPixelShader>(), Array.Empty<IDeferredShader>(),
                new Surface[] { new Surface(6, 4, 2, 0, 0, 1, 2),
                                    new Surface(0, 2, 4, 0, 3, 2, 1),
                                    new Surface(7, 3, 5, 0, 0, 1, 2),
                                    new Surface(1, 5, 3, 0, 3, 2, 1),
                                    new Surface(2, 0, 3, 0, 0, 1, 2),
                                    new Surface(1, 3, 0, 0, 3, 2, 1),
                                    new Surface(4, 6, 5, 0, 0, 1, 2),
                                    new Surface(7, 5, 6, 0, 3, 2, 1),
                                    new Surface(0, 4, 1, 0, 0, 1, 2),
                                    new Surface(5, 1, 4, 0, 3, 2, 1),
                                    new Surface(2, 3, 6, 0, 0, 1, 2),
                                    new Surface(7, 6, 3, 0, 3, 2, 1)}, false);
        }

        public CubeMap(string MaterialName, int Size = 1, int TextureFilteringMode = Surface.NONE, bool EnableTexelAveraging = false)
        {
            Material Up = new Material(MaterialName + "U");
            Material Down = new Material(MaterialName + "D");
            Material Left = new Material(MaterialName + "L");
            Material Right = new Material(MaterialName + "R");
            Material Front = new Material(MaterialName + "F");
            Material Back = new Material(MaterialName + "B");
            BoxObject = new Mesh(new Vec3D(0, 0, 0), new Vec3D(Size, Size, Size), new Quat(1, 0, 0, 0),
                new Vec3D[] { new Vec3D(1, 1, 1), new Vec3D(1, 1, -1), new Vec3D(1, -1, 1), new Vec3D(1, -1, -1),
                             new Vec3D(-1, 1, 1), new Vec3D(-1, 1, -1), new Vec3D(-1, -1, 1), new Vec3D(-1, -1, -1)},
                new Vec2D[] { new Vec2D(0, 0), new Vec2D(1, 0), new Vec2D(0, 1), new Vec2D(1, 1) },
                new Material[] { Right, Left, Up, Down, Front, Back },
                Array.Empty<ITexelShiftShader>(), new IPixelShader[] { new SkyBoxShader() }, Array.Empty<IPixelShader>(), Array.Empty<IDeferredShader>(),
                new Surface[] { new Surface(6, 2, 4, 4, 0, 1, 2,TextureWrappingMode:Surface.CLAMP,TextureFilteringMode:TextureFilteringMode,EnableTexelAveraging:EnableTexelAveraging),
                                    new Surface(0, 4, 2, 4, 3, 2,1,TextureWrappingMode:Surface.CLAMP,TextureFilteringMode:TextureFilteringMode,EnableTexelAveraging:EnableTexelAveraging),
                                    new Surface(3, 7, 1, 5, 0, 1, 2,TextureWrappingMode:Surface.CLAMP,TextureFilteringMode:TextureFilteringMode,EnableTexelAveraging:EnableTexelAveraging),
                                    new Surface(5, 1, 7, 5, 3, 2, 1,TextureWrappingMode:Surface.CLAMP,TextureFilteringMode:TextureFilteringMode,EnableTexelAveraging:EnableTexelAveraging),
                                    new Surface(2, 3, 0, 0, 0, 1, 2,TextureWrappingMode:Surface.CLAMP,TextureFilteringMode:TextureFilteringMode,EnableTexelAveraging:EnableTexelAveraging),
                                    new Surface(1, 0, 3, 0, 3, 2, 1,TextureWrappingMode:Surface.CLAMP,TextureFilteringMode:TextureFilteringMode,EnableTexelAveraging:EnableTexelAveraging),
                                    new Surface(7, 6, 5, 1, 0, 1, 2,TextureWrappingMode:Surface.CLAMP,TextureFilteringMode:TextureFilteringMode,EnableTexelAveraging:EnableTexelAveraging),
                                    new Surface(4, 5, 6, 1, 3, 2, 1,TextureWrappingMode:Surface.CLAMP,TextureFilteringMode:TextureFilteringMode,EnableTexelAveraging:EnableTexelAveraging),
                                    new Surface(4, 0, 5, 3, 0, 1, 2,TextureWrappingMode:Surface.CLAMP,TextureFilteringMode:TextureFilteringMode,EnableTexelAveraging:EnableTexelAveraging),
                                    new Surface(1, 5, 0, 3, 3, 2, 1,TextureWrappingMode:Surface.CLAMP,TextureFilteringMode:TextureFilteringMode,EnableTexelAveraging:EnableTexelAveraging),
                                    new Surface(7, 3, 6, 2, 0, 1, 2,TextureWrappingMode:Surface.CLAMP,TextureFilteringMode:TextureFilteringMode,EnableTexelAveraging:EnableTexelAveraging),
                                    new Surface(2, 6, 3, 2, 3, 2, 1,TextureWrappingMode:Surface.CLAMP,TextureFilteringMode:TextureFilteringMode,EnableTexelAveraging:EnableTexelAveraging)}, false);
        }

        private (int CubeMapMaterialNum, Vec2D CubeMapCoord) GetReflectionCoord(Vec3D DirectionVec)
        {
            DirectionVec = DirectionVec.Normalise();
            int CubeMapMaterialNum;
            Vec2D CubeMapCoord;
            if (Math.Abs(DirectionVec.X) > Math.Abs(DirectionVec.Y) && Math.Abs(DirectionVec.X) > Math.Abs(DirectionVec.Z))
            {
                if (DirectionVec.X > 0)
                {
                    CubeMapCoord = new Vec2D(-DirectionVec.Z, -DirectionVec.Y).ScalarMult(1 / (2 * Math.Abs(DirectionVec.X))) + new Vec2D(0.5, 0.5);
                    CubeMapMaterialNum = 1;
                }
                else
                {
                    CubeMapCoord = new Vec2D(DirectionVec.Z, -DirectionVec.Y).ScalarMult(1 / (2 * Math.Abs(DirectionVec.X))) + new Vec2D(0.5, 0.5);
                    CubeMapMaterialNum = 0;
                }
            }
            else if (Math.Abs(DirectionVec.Y) > Math.Abs(DirectionVec.X) && Math.Abs(DirectionVec.Y) > Math.Abs(DirectionVec.Z))
            {
                if (DirectionVec.Y > 0)
                {
                    CubeMapCoord = new Vec2D(-DirectionVec.X, -DirectionVec.Z).ScalarMult(1 / (2 * Math.Abs(DirectionVec.Y))) + new Vec2D(0.5, 0.5);
                    CubeMapMaterialNum = 2;
                }
                else
                {
                    CubeMapCoord = new Vec2D(-DirectionVec.X, DirectionVec.Z).ScalarMult(1 / (2 * Math.Abs(DirectionVec.Y))) + new Vec2D(0.5, 0.5);
                    CubeMapMaterialNum = 3;
                }
            }
            else
            {
                if (DirectionVec.Z > 0)
                {
                    CubeMapCoord = new Vec2D(DirectionVec.X, -DirectionVec.Y).ScalarMult(1 / (2 * Math.Abs(DirectionVec.Z))) + new Vec2D(0.5, 0.5);
                    CubeMapMaterialNum = 5;
                }
                else
                {
                    CubeMapCoord = new Vec2D(-DirectionVec.X, -DirectionVec.Y).ScalarMult(1 / (2 * Math.Abs(DirectionVec.Z))) + new Vec2D(0.5, 0.5);
                    CubeMapMaterialNum = 4;
                }
            }
            return (CubeMapMaterialNum, CubeMapCoord);
        }

        public Colour GetReflectionAlbedo(Vec3D DirectionVec)
        {
            (int CubeMapMaterialNum, Vec2D CubeMapCoord) ReflectPos = GetReflectionCoord(DirectionVec);
            return BoxObject.Materials[ReflectPos.CubeMapMaterialNum].Albedo(ReflectPos.CubeMapCoord);
        }

        public double GetReflectionDisplacement(Vec3D DirectionVec, out int CubeMapMaterialNum)
        {
            (int CubeMapMaterialNum, Vec2D CubeMapCoord) ReflectPos = GetReflectionCoord(DirectionVec);
            // Required for the pixel shader to know which projection matrix to use
            CubeMapMaterialNum = ReflectPos.CubeMapMaterialNum;
            return BoxObject.Materials[ReflectPos.CubeMapMaterialNum].Displacement(ReflectPos.CubeMapCoord);
        }
    }
}
