using System;
using System.Collections.Generic;

namespace _3D_Renderer
{
#nullable enable
    class Scene
    {
        //Contains all objects in the scene including camera, light sources and triangles.
        public List<Mesh> SceneMeshes { get; set; } = new List<Mesh>();
        public List<Light> SceneLights { get; set; } = new List<Light>();
        public Camera SceneCamera { get; set; }
        public Colour AmbientLight { get; set; }
        public CubeMap? SceneSkyBox { get; set; }
        public IPostProcessShader[] PostProcessShaders { get; set; }
        public Scene()
        {
            //SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 0), new Vec3D(0.075, 0.075, 0.075), new Quat(Math.Cos(0.1), Math.Sin(0.1), 0, 0),
            //    new Material[] { new Material("StingLow") },
            //    new ITexelShiftShader[0], new IPixelShader[] { new DiffuseShader(), new MetallicSpecularShader(1) }, new IPixelShader[0], new IDeferredShader[0], "Sting",
            //    SmoothShading: true));
            AmbientLight = new Colour(50, 50, 50);
            //for (int x = -10; x < 10; x += 5)
            //{
            //    for (int y = -10; y < 10; y += 5)
            //    {
            //        SceneMeshes.Add(new Mesh(new Vec3D(x, y, -2), new Quat(1, 0, 0, 0),
            //    new Vec3D[] { new Vec3D(1, 1, 1), new Vec3D(1, 1, -1), new Vec3D(1, -1, 1), new Vec3D(1, -1, -1),
            //                         new Vec3D(-1, 1, 1), new Vec3D(-1, 1, -1), new Vec3D(-1, -1, 1), new Vec3D(-1, -1, -1)},
            //    new Material[] { new Material("Box") },
            //    new IShader[] { new DiffuseShader(), new SpecularShader() },
            //    new Surface[] { new Surface(6, 4, 2, 0, new int[] {0, 1}, new Vec2D(0,0), new Vec2D(256, 0), new Vec2D(0, 256)),
            //    new Surface(0, 2, 4, 0, new int[] { 0, 1 }, new Vec2D(256, 256), new Vec2D(0, 256), new Vec2D(256, 0)),
            //                            new Surface(7, 3, 5, 0, new int[] { 0, 1 }, new Vec2D(0, 0), new Vec2D(256, 0), new Vec2D(0, 256)),
            //                            new Surface(1, 5, 3, 0, new int[] { 0, 1 }, new Vec2D(256, 256), new Vec2D(0, 256), new Vec2D(256, 0)),
            //                            new Surface(2, 0, 3, 0, new int[] { 0, 1 }, new Vec2D(0, 0), new Vec2D(256, 0), new Vec2D(0, 256)),
            //                            new Surface(1, 3, 0, 0, new int[] { 0, 1 }, new Vec2D(256, 256), new Vec2D(0, 256), new Vec2D(256, 0)),
            //                            new Surface(4, 6, 5, 0, new int[] { 0, 1 }, new Vec2D(0, 0), new Vec2D(256, 0), new Vec2D(0, 256)),
            //                            new Surface(7, 5, 6, 0, new int[] { 0, 1 }, new Vec2D(256, 256), new Vec2D(0, 256), new Vec2D(256, 0)),
            //                            new Surface(0, 4, 1, 0, new int[] { 0, 1 }, new Vec2D(0, 0), new Vec2D(256, 0), new Vec2D(0, 256)),
            //                            new Surface(5, 1, 4, 0, new int[] { 0, 1 }, new Vec2D(256, 256), new Vec2D(0, 256), new Vec2D(256, 0)),
            //                            new Surface(2, 3, 6, 0, new int[] { 0, 1 }, new Vec2D(0, 0), new Vec2D(256, 0), new Vec2D(0, 256)),
            //                            new Surface(7, 6, 3, 0, new int[] { 0, 1 }, new Vec2D(256, 256), new Vec2D(0, 256), new Vec2D(256, 0))
            //       }));
            //    }
            //}
            PostProcessShaders = new IPostProcessShader[] { //new EdgeAntiAliasX(0.4, 2),
                //new EdgeAntiAliasY(0.4, 2)
                //new SeparableGaussianDepthBlurX(10, 3, 5),
            //    //new SeparableGaussianDepthBlurY(10, 3, 5),
            //    new SeparableBoxDepthBlurX(5, 5),
            //    new SeparableBoxDepthBlurX(5, 5)
                //new KernelConvolve(new Matrix(new Fast2DArr<double>(new double[,] { { -1, 3,-1} }))),
            //new KernelConvolve(new Matrix(new Fast2DArr<double>(new double[,] { { -1 }, { 3 },{ -1 } })))
            //new SeparableGaussianX(6,1),new SeparableGaussianY(6,1)
            };
            //SceneLights.Add(new DirectionalLight(new Quat(1, 0, 0, 0), new Colour(255, 255, 255), false));
            SceneCamera = new Camera(new Vec3D(0, 0, 0),
                new Quat(1, 0, 0, 0), 90);
        }
        public void DemoSceneProjection()
        {
            PostProcessShaders = new IPostProcessShader[] { };
            AmbientLight = new Colour(255, 255, 255);
            SceneSkyBox = null;
            SceneCamera = new Camera(new Vec3D(0, 0, 0),
                new Quat(1, 0, 0, 0), 90);
            SceneMeshes = new List<Mesh>();
            SceneLights = new List<Light>();
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 3), new Vec3D(0.8, 11, 0.45), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] {new Colour(255,255,255)},1,1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 2), new Vec3D(0.2, 1, 0.2), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(-4.36, 0, -4.35), new Vec3D(-0.83, 0, -1.1), new Vec3D(-4.64, 0, 1.3) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(255, 0, 0) }, 1, 1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 2, 1, 0, 0, 1, 2,BackFaceCull:false)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 2.95), new Vec3D(0.2, 1, 0.2), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(-4.36, 0, -4.35), new Vec3D(-0.83, 0, -1.1), new Vec3D(-4.64, 0, 1.3) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(150, 0, 0) }, 1, 1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 2, 1, 0, 0, 1, 2,BackFaceCull:false)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 1), new Vec3D(0.2, 1, 0.2), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(4.4, 0, -1.86), new Vec3D(2.86, 0, 4.25), new Vec3D(0.75, 0, -3.83) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(0, 255, 0) }, 1, 1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 2, 1, 0, 0, 1, 2,BackFaceCull:false)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 2.95), new Vec3D(0.2, 1, 0.2), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(4.4, 0, -1.86), new Vec3D(2.86, 0, 4.25), new Vec3D(0.75, 0, -3.83) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(0, 150, 0) }, 1, 1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 2, 1, 0, 0, 1, 2,BackFaceCull:false)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 2.5), new Vec3D(0.2, 1, 0.2), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(-2.47, 0, 3.92), new Vec3D(0.84, 0, 3.92), new Vec3D(-1.02, 0, 0.67) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(0, 0, 255) }, 1, 1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 2, 1, 0, 0, 1, 2,BackFaceCull:false)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 2.95), new Vec3D(0.2, 1, 0.2), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(-2.47, 0, 3.92), new Vec3D(0.84, 0, 3.92), new Vec3D(-1.02, 0, 0.67) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(0, 0, 150) }, 1, 1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 2, 1, 0, 0, 1, 2,BackFaceCull:false)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(4, 0, 3), new Vec3D(0.8, 1, 0.45), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(255, 255, 255) }, 1, 1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(4, 0, 2), new Vec3D(0.2, 1, 0.2), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(-4.36, 0, -4.35), new Vec3D(-0.83, 0, -1.1), new Vec3D(-4.64, 0, 1.3) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(255, 0, 0) }, 1, 1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 2, 1, 0, 0, 1, 2,BackFaceCull:false)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(4, 0, 2.95), new Vec3D(0.2, 1, 0.2), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(-4.36, 0, -4.35), new Vec3D(-0.83, 0, -1.1), new Vec3D(-4.64, 0, 1.3) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(150, 0, 0) }, 1, 1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 2, 1, 0, 0, 1, 2,BackFaceCull:false)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(4, 0, 1), new Vec3D(0.2, 1, 0.2), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(4.4, 0, -1.86), new Vec3D(2.86, 0, 4.25), new Vec3D(0.75, 0, -3.83) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(0, 255, 0) }, 1, 1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 2, 1, 0, 0, 1, 2,BackFaceCull:false)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(4, 0, 2.95), new Vec3D(0.1, 1, 0.1), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(4.4, 0, -1.86), new Vec3D(2.86, 0, 4.25), new Vec3D(0.75, 0, -3.83) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(0, 150, 0) }, 1, 1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 2, 1, 0, 0, 1, 2,BackFaceCull:false)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(4, 0, 2.5), new Vec3D(0.2, 1, 0.2), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
               new Vec3D[] { new Vec3D(-2.47, 0, 3.92), new Vec3D(0.84, 0, 3.92), new Vec3D(-1.02, 0, 0.67) },
               new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0) },
               new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(0, 0, 255) }, 1, 1)) },
               new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
               new Surface[] { new Surface(0, 2, 1, 0, 0, 1, 2,BackFaceCull:false)
            }));
            SceneMeshes.Add(new Mesh(new Vec3D(4, 0, 2.95), new Vec3D(0.4, 1, 0.4), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(-2.47, 0, 3.92), new Vec3D(0.84, 0, 3.92), new Vec3D(-1.02, 0, 0.67) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0) },
                new Material[] { new Material(new Arr2D<Colour>(new Colour[] { new Colour(0, 0, 150) }, 1, 1)) },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 2, 1, 0, 0, 1, 2,BackFaceCull:false)
             }));

        }

        public void DemoScenePowerPoint()
        {
            PostProcessShaders = new IPostProcessShader[] { };
            AmbientLight = new Colour(255, 255, 255);
            //SceneSkyBox = new CubeMap("CubeMapLow", Size: 100);
            SceneCamera = new Camera(new Vec3D(0, 0, 0),
                new Quat(1, 0, 0, 0), 90);
            SceneMeshes = new List<Mesh>();
            SceneLights = new List<Light>();
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 3.2), new Vec3D(1.6, 1, 0.9), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material("Slide1") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(8, 0, 3.2), new Vec3D(1.6, 1, 0.9), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material("Slide2") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(8*2, 0, 3.2), new Vec3D(1.6, 1, 0.9), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material("Slide3") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(8*3, 0, 3.2), new Vec3D(1.6, 1, 0.9), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material("Slide4") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(8*4, 0, 3.2), new Vec3D(1.6, 1, 0.9), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material("Slide5") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(8*5, 0, 3.2), new Vec3D(1.6, 1, 0.9), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material("Slide6") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(8 * 5, 4, 3.2), new Vec3D(1.6, 1, 0.9), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material("Slide7") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(8*6, 0, 3.2), new Vec3D(1.6, 1, 0.9), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material("Slide8") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(8*7, 0, 3.2), new Vec3D(1.6, 1, 0.9), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material("Slide9") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(8*8, 0, 3.2), new Vec3D(1.6, 1, 0.9), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material("Slide10") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(8*9, 0, 3.2), new Vec3D(1.6, 1, 0.9), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material("Slide11") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(8*10, 0, 3.2), new Vec3D(1.6, 1, 0.9), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(1, 0), new Vec2D(1, 1), new Vec2D(0, 0), new Vec2D(0, 1) },
                new Material[] { new Material("Slide12") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));

        }

        public void DemoSceneCow()
        {

            PostProcessShaders = new IPostProcessShader[] { };
            AmbientLight = new Colour(100, 100, 100);
            SceneSkyBox = new CubeMap("CubeMap2", Size: 100);
            SceneCamera = new Camera(new Vec3D(-0.48334271009458746, -0.7342830182085532, -1.0485950700057838),
                new Quat(0.9637292693001338, -0.22128907423216201, 0.14918374739640253, 0.0011183168041962626), 90);
            SceneMeshes = new List<Mesh>();
            SceneLights = new List<Light>();
            SceneLights.Add(new DirectionalLight(new Quat(new Vec3D(0, 1, 0), Math.PI / 2), new Colour(255, 255, 255), CastsShadows: false));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 0), new Vec3D(0.5, 0.5, 0.5), new Quat(new Vec3D(1, 0, 0), Math.PI) * new Quat(new Vec3D(0, 1, 0), Math.PI),
                new Material[] { new Material("spot_texture") },
                new ITexelShiftShader[0], new IPixelShader[] { new OverrideSpecularityShader(2), new SchlicksFresnelShader(2), new DiffuseShader(), new DielectricSpecularShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                "spot_triangulated", SmoothShading: true));
        }

        public void DemoSceneThreeLightsAndAScreenSpaceReflection()
        {
            PostProcessShaders = new IPostProcessShader[] { new SimpleAntiAliasX(0.5, 2), new SimpleAntiAliasY(0.5, 2) };
            AmbientLight = new Colour(0, 0, 0);
            SceneSkyBox = null;
            SceneCamera = new Camera(new Vec3D(2.8000754574405997, -1.2785905535087154, -3.5380258390983386),
                new Quat(0.918865955529961, -0.13795949669243132, -0.367876388961866, -0.03632486042341102), 90);
            SceneMeshes = new List<Mesh>();
            SceneLights = new List<Light>();
            SceneLights.Add(new PointLight(new Vec3D(0, 0, -3), new Colour(255, 100, 100), CastsShadows: false, Intensity: 4));
            SceneLights.Add(new PointLight(new Vec3D(3, 0, 3), new Colour(100, 255, 100), CastsShadows: false, Intensity: 6));
            SceneLights.Add(new PointLight(new Vec3D(-3, 0, 3), new Colour(100, 100, 255), CastsShadows: false, Intensity: 6));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 1.5, 0), new Vec3D(1.5, 1, 1.5), new Quat(1, 0, 0, 0),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(0, 0), new Vec2D(1, 0), new Vec2D(0, 1), new Vec2D(1, 1) },
                new Material[] { new Material("White") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DielectricSpecularShader(1) }, new IPixelShader[] { }, new IDeferredShader[] { new ScreenSpaceDielectricReflectionShader(2) },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 0), new Vec3D(0.5, 0.5, 0.5), new Quat(1, 0, 0, 0),
                new Vec3D[] { new Vec3D(2, 2, 2), new Vec3D(2, 2, -2), new Vec3D(2, -2, 2), new Vec3D(2, -2, -2),
                                     new Vec3D(-2, 2, 2), new Vec3D(-2, 2, -2), new Vec3D(-2, -2, 2), new Vec3D(-2, -2, -2)},
                new Vec2D[] { new Vec2D(0, 0), new Vec2D(1, 0), new Vec2D(0, 1), new Vec2D(1, 1) },
                new Material[] { new Material("Box") },
                new ITexelShiftShader[] { new ParallaxMappingWithoutOffsetLimiting(0.1) }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { new DielectricSpecularShader() }, new IDeferredShader[] { },
                new Surface[] { new Surface(6, 4, 2, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(0, 2, 4, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(7, 3, 5, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(1, 5, 3, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(2, 0, 3, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(1, 3, 0, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(4, 6, 5, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(7, 5, 6, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(0, 4, 1, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(5, 1, 4, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(2, 3, 6, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(7, 6, 3, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT)
                   }));

        }

        public void DemoSceneWavyScreenSpaceReflection()
        {
            PostProcessShaders = new IPostProcessShader[] { new SimpleAntiAliasX(0.5, 2), new SimpleAntiAliasY(0.5, 2) };
            AmbientLight = new Colour(0, 0, 0);
            SceneSkyBox = null;
            SceneCamera = new Camera(new Vec3D(2.8000754574405997, -1.2785905535087154, -3.5380258390983386),
                new Quat(0.918865955529961, -0.13795949669243132, -0.367876388961866, -0.03632486042341102), 90);
            SceneMeshes = new List<Mesh>();
            SceneLights = new List<Light>();
            SceneLights.Add(new PointLight(new Vec3D(0, 0, -3), new Colour(255, 100, 100), CastsShadows: false, Intensity: 4));
            SceneLights.Add(new PointLight(new Vec3D(3, 0, 3), new Colour(100, 255, 100), CastsShadows: false, Intensity: 6));
            SceneLights.Add(new PointLight(new Vec3D(-3, 0, 3), new Colour(100, 100, 255), CastsShadows: false, Intensity: 6));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 1.5, 0), new Vec3D(1.5, 1, 1.5), new Quat(1, 0, 0, 0),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(0, 0), new Vec2D(1, 0), new Vec2D(0, 1), new Vec2D(1, 1) },
                new Material[] { new Material("Wavy") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DielectricSpecularShader(1) }, new IPixelShader[] { }, new IDeferredShader[] { new ScreenSpaceDielectricReflectionShader(2) },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 0), new Vec3D(0.5, 0.5, 0.5), new Quat(1, 0, 0, 0),
                new Vec3D[] { new Vec3D(2, 2, 2), new Vec3D(2, 2, -2), new Vec3D(2, -2, 2), new Vec3D(2, -2, -2),
                                     new Vec3D(-2, 2, 2), new Vec3D(-2, 2, -2), new Vec3D(-2, -2, 2), new Vec3D(-2, -2, -2)},
                new Vec2D[] { new Vec2D(0, 0), new Vec2D(1, 0), new Vec2D(0, 1), new Vec2D(1, 1) },
                new Material[] { new Material("Box") },
                new ITexelShiftShader[] { new ParallaxMappingWithoutOffsetLimiting(0.1) }, new IPixelShader[] { new DiffuseShader() }, new IPixelShader[] { new DielectricSpecularShader() }, new IDeferredShader[] { },
                new Surface[] { new Surface(6, 4, 2, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(0, 2, 4, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(7, 3, 5, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(1, 5, 3, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(2, 0, 3, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(1, 3, 0, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(4, 6, 5, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(7, 5, 6, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(0, 4, 1, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(5, 1, 4, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(2, 3, 6, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(7, 6, 3, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT)
                   }));

        }

        public void DemoSceneShadowsAndSubSurfaceScattering()
        {
            PostProcessShaders = new IPostProcessShader[] { };
            AmbientLight = new Colour(20, 20, 20);
            SceneSkyBox = null;
            SceneCamera = new Camera(new Vec3D(-2.5843253650894717, 1E-06, -2.6242996688425224),
                new Quat(0.9335275485554954, 0, 0.35850567092861946, 0), 90);
            SceneMeshes = new List<Mesh>();
            SceneLights = new List<Light>();
            SceneLights.Add(new PointLight(new Vec3D(0, 0, -3), new Colour(255, 255, 255), Intensity: 5, ActiveShadowMaps: new bool[] { false, false, false, false, true, false }, ShadowBias: 0.2));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 3), new Vec3D(1.5, 1.5, 1.5), new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2),
                new Vec3D[] { new Vec3D(2, 0, 2), new Vec3D(2, 0, -2), new Vec3D(-2, 0, 2), new Vec3D(-2, 0, -2) },
                new Vec2D[] { new Vec2D(0, 0), new Vec2D(1, 0), new Vec2D(0, 1), new Vec2D(1, 1) },
                new Material[] { new Material("White") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader(), new DielectricSpecularShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(0, 1, 2, 0, 0, 1, 2),
                                new Surface(3, 2, 1, 0, 3, 2, 1)
             }));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 0), new Vec3D(1, 1, 1), new Quat(Math.Cos(Math.PI / 4), Math.Sin(Math.PI / 4), 0, 0),
                new Material[] { new Material("Brown") },
                new ITexelShiftShader[0], new IPixelShader[] { new DiffuseShader(), new DielectricSpecularShader(0.5), new SubsurfaceScatteringShader(new Colour(100, 0, 0)) }, new IPixelShader[] { }, new IDeferredShader[] { },
                "suzanne", SmoothShading: true, OverrideNormals: false, ReceivesShadows: false));
        }

        public void DemoSceneCubeMap()
        {
            PostProcessShaders = new IPostProcessShader[] { new NonSeparableBokehDepthBlur(2, 5) 
            };
            AmbientLight = new Colour(150, 150, 150);
            SceneSkyBox = new CubeMap("CubeMapLow", Size: 100);
            SceneCamera = new Camera(new Vec3D(-2.888514838374785, -0.20847505060387328, -3.462438119617296),
                new Quat(0.8563979440390279, -0.06488714664476676, 0.5104730125396565, -0.042302755403777925), 90);
            SceneMeshes = new List<Mesh>();
            SceneLights = new List<Light>();
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, -4), new Vec3D(0.25, 0.25, 0.25), new Quat(1, 0, 0, 0),
                new Vec3D[] { new Vec3D(2, 2, 2), new Vec3D(2, 2, -2), new Vec3D(2, -2, 2), new Vec3D(2, -2, -2),
                                     new Vec3D(-2, 2, 2), new Vec3D(-2, 2, -2), new Vec3D(-2, -2, 2), new Vec3D(-2, -2, -2)},
                new Vec2D[] { new Vec2D(0, 0), new Vec2D(1, 0), new Vec2D(0, 1), new Vec2D(1, 1) },
                new Material[] { new Material("Box") },
                new ITexelShiftShader[] { }, new IPixelShader[] { new DiffuseShader(), new DielectricSpecularShader() }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { new Surface(6, 4, 2, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(0, 2, 4, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(7, 3, 5, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(1, 5, 3, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(2, 0, 3, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(1, 3, 0, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(4, 6, 5, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(7, 5, 6, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(0, 4, 1, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(5, 1, 4, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(2, 3, 6, 0, 0, 1, 2,TextureWrappingMode:Surface.REPEAT),
                                    new Surface(7, 6, 3, 0, 3, 2, 1,TextureWrappingMode:Surface.REPEAT)
                   }));
            SceneMeshes.Add(new Mesh(new Vec3D(0, 0, 0), new Vec3D(0.1, 0.1, 0.1), new Quat(Math.Cos(Math.PI / 4), Math.Sin(Math.PI / 4), 0, 0),
                new Material[] { new Material("White") },
                new ITexelShiftShader[0], new IPixelShader[] { new DiffuseShader(), new DielectricCubeMapReflectionShader() }, new IPixelShader[0], new IDeferredShader[] { },
                "Teapot-Low", SmoothShading: true));
            GenerateCubeMaps(100, 100);
        }
        public void GenerateCubeMaps(int CubeMapWidth, int CubeMapHeight)
        {
            for (int MeshNum = 0; MeshNum < SceneMeshes.Count; MeshNum++)
            {
                // Render cube maps for every mesh
                Mesh MeshCopy = SceneMeshes[MeshNum];
                //Replaces the current mesh with an empty one
                SceneMeshes[MeshNum] = new Mesh(new Vec3D(0, 0, 0), new Vec3D(0, 0, 0), new Quat(1, 0, 0, 0),
                new Vec3D[] { },
                new Vec2D[] { },
                new Material[] { },
                new ITexelShiftShader[] { }, new IPixelShader[] { }, new IPixelShader[] { }, new IDeferredShader[] { },
                new Surface[] { });
                Arr2D<Colour> Up;
                Arr2D<Colour> Down;
                Arr2D<Colour> Right;
                Arr2D<Colour> Left;
                Arr2D<Colour> Front;
                Arr2D<Colour> Back;
                int[] Resolution = new int[] { CubeMapWidth + 1, CubeMapHeight + 1 };
                Rasteriser.RenderToTexture(Resolution, new Camera(MeshCopy.Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2), 90), this, out Up, out _);
                Rasteriser.RenderToTexture(Resolution, new Camera(MeshCopy.Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), -Math.PI / 2), 90), this, out Down, out _);
                Rasteriser.RenderToTexture(Resolution, new Camera(MeshCopy.Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(0, 1, 0), -Math.PI / 2), 90), this, out Left, out _);
                Rasteriser.RenderToTexture(Resolution, new Camera(MeshCopy.Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(0, 1, 0), Math.PI / 2), 90), this, out Right, out _);
                Rasteriser.RenderToTexture(Resolution, new Camera(MeshCopy.Position, new Quat(1, 0, 0, 0), 90), this, out Front, out _);
                Rasteriser.RenderToTexture(Resolution, new Camera(MeshCopy.Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(0, 1, 0), Math.PI), 90), this, out Back, out _);
                SceneMeshes[MeshNum] = MeshCopy;
                //Rasteriser misses the edge rows of pixels so they need to be removed to avoid seams
                Up = Up.Resize(CubeMapWidth, CubeMapHeight);
                Down = Down.Resize(CubeMapWidth, CubeMapHeight);
                Right = Right.Resize(CubeMapWidth, CubeMapHeight);
                Left = Left.Resize(CubeMapWidth, CubeMapHeight);
                Front = Front.Resize(CubeMapWidth, CubeMapHeight);
                Back = Back.Resize(CubeMapWidth, CubeMapHeight);
                SceneMeshes[MeshNum].CubeMapReflection = new CubeMap(new Material(Right), new Material(Left), new Material(Up),
                    new Material(Down), new Material(Front), new Material(Back));
            }
        }
        public void GenerateShadowMaps(int ShadowMapWidth, int ShadowMapHeight)
        {
            for (int LightNum = 0; LightNum < SceneLights.Count; LightNum++)
            {
                // Render shadow buffers for every light source
                // Shadows on directional light sources are not supported as they required orthogonal projection (which is not too difficult to 
                // code but I do not have time to add the necessary checks in the rasteriser for it)
                if (!(SceneLights[LightNum] is DirectionalLight))
                {
                    Arr2D<double> Default = new Arr2D<double>(ShadowMapWidth, ShadowMapHeight);
                    Arr2D<double> Up = Default;
                    Arr2D<double> Down = Default;
                    Arr2D<double> Right = Default;
                    Arr2D<double> Left = Default;
                    Arr2D<double> Front = Default;
                    Arr2D<double> Back = Default;
                    int[] Resolution = new int[] { ShadowMapWidth + 1, ShadowMapHeight + 1 };
                    if (SceneLights[LightNum].ActiveShadowMaps[0])
                    {
                        Rasteriser.RenderToTexture(Resolution, new Camera(SceneLights[LightNum].Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(0, 1, 0), Math.PI / 2), 90), this, out Right, Mode: Rasteriser.DEPTH_ONLY);
                    }
                    if (SceneLights[LightNum].ActiveShadowMaps[1])
                    {
                        Rasteriser.RenderToTexture(Resolution, new Camera(SceneLights[LightNum].Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(0, 1, 0), -Math.PI / 2), 90), this, out Left, Mode: Rasteriser.DEPTH_ONLY);
                    }
                    if (SceneLights[LightNum].ActiveShadowMaps[2])
                    {
                        Rasteriser.RenderToTexture(Resolution, new Camera(SceneLights[LightNum].Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2), 90), this, out Up, Mode: Rasteriser.DEPTH_ONLY);
                    }
                    if (SceneLights[LightNum].ActiveShadowMaps[3])
                    {
                        Rasteriser.RenderToTexture(Resolution, new Camera(SceneLights[LightNum].Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), -Math.PI / 2), 90), this, out Down, Mode: Rasteriser.DEPTH_ONLY);
                    }
                    if (SceneLights[LightNum].ActiveShadowMaps[4])
                    {
                        Rasteriser.RenderToTexture(Resolution, new Camera(SceneLights[LightNum].Position, new Quat(1, 0, 0, 0), 90), this, out Front, Mode: Rasteriser.DEPTH_ONLY);
                    }
                    if (SceneLights[LightNum].ActiveShadowMaps[5])
                    {
                        Rasteriser.RenderToTexture(Resolution, new Camera(SceneLights[LightNum].Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(0, 1, 0), Math.PI), 90), this, out Back, Mode: Rasteriser.DEPTH_ONLY);
                    }
                    //Rasteriser misses the edge rows of pixels so they need to be removed to avoid seams
                    Up = Up.Resize(ShadowMapWidth, ShadowMapHeight);
                    Down = Down.Resize(ShadowMapWidth, ShadowMapHeight);
                    Right = Right.Resize(ShadowMapWidth, ShadowMapHeight);
                    Left = Left.Resize(ShadowMapWidth, ShadowMapHeight);
                    Front = Front.Resize(ShadowMapWidth, ShadowMapHeight);
                    Back = Back.Resize(ShadowMapWidth, ShadowMapHeight);
                    SceneLights[LightNum].ShadowMap = new CubeMap(new Material(DisplacementMap: Right), new Material(DisplacementMap: Left), new Material(DisplacementMap: Up),
                        new Material(DisplacementMap: Down), new Material(DisplacementMap: Front), new Material(DisplacementMap: Back));
                    SceneLights[LightNum].ProjectionMatrices = new Matrix[] {
                        new Camera(SceneLights[LightNum].Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(0, 1, 0), Math.PI / 2), 90).GetCameraSpaceTransform(Resolution),
                        new Camera(SceneLights[LightNum].Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(0, 1, 0), -Math.PI / 2), 90).GetCameraSpaceTransform(Resolution),
                        new Camera(SceneLights[LightNum].Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(1, 0, 0), Math.PI / 2), 90).GetCameraSpaceTransform(Resolution),
                        new Camera(SceneLights[LightNum].Position, new Quat(1, 0, 0, 0) *  new Quat(new Vec3D(1, 0, 0), -Math.PI / 2), 90).GetCameraSpaceTransform(Resolution),
                        new Camera(SceneLights[LightNum].Position, new Quat(1, 0, 0, 0), 90).GetCameraSpaceTransform(Resolution),
                        new Camera(SceneLights[LightNum].Position, new Quat(1, 0, 0, 0) * new Quat(new Vec3D(0, 1, 0), Math.PI), 90).GetCameraSpaceTransform(Resolution)};
                }
            }
        }
    }
}
