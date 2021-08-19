using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3D_Renderer
{
    class Renderer
    {
        //Class to be instantiated by any app that wishes to use the renderer
        //I have added some specific properties and methods here for updating the demo scenes, as updating inside the app thread meant that
        //properties such as the camera's position were being changed while frames were being rendered, leading to artifacts and crashes.
        private readonly GUI Window;
        private readonly Task UIThread;

        public List<Scene> Scenes { get; set; }
        public int SceneNum { get; set; }
        private int[] Resolution { get; set; }
        public bool DynamicResScaleX { get; set; }
        public bool DynamicResScaleY { get; set; }
        public int DynamicResTargetMs { get; set; }
        private double[] DynamicRes { get; set; }
        public double DynamicResAggression { get; set; }
        public Vec3D NewCameraPosition { get; set; }
        public Quat NewCameraRotation { get; set; }

        //For FPS counter
        private readonly Stopwatch DebugTimer = new Stopwatch();

        public Renderer(int[] Resolution, bool DynamicResScaleX = true, bool DynamicResScaleY = true, int DynamicResTargetMs = 100, double DynamicResAggression = 0.95)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Window = new GUI();
            UIThread = new Task(() => { Application.Run(Window); });
            UIThread.Start();
            this.Resolution = Resolution;
            this.DynamicResScaleX = DynamicResScaleX;
            this.DynamicResScaleY = DynamicResScaleY;
            this.DynamicResTargetMs = DynamicResTargetMs;
            this.DynamicResAggression = DynamicResAggression;
            DynamicRes = new double[] { 1, 1 };
            //All scenes are loaded in memory - not very efficient, but as data will only be read from one scene at a time, 
            //should not affect memory bandwidth, which is the main performance limiter, that much
            Scenes = new List<Scene> { new Scene(), new Scene(), new Scene(), new Scene(), new Scene() };
            Scenes[0].DemoSceneCubeMap();
            Scenes[1].DemoSceneShadowsAndSubSurfaceScattering();
            Scenes[2].DemoSceneThreeLightsAndAScreenSpaceReflection();
            Scenes[3].DemoSceneWavyScreenSpaceReflection();
            Scenes[4].DemoSceneCow();
            SceneNum = 0;
        }

        public void UpdateWindowResolution()
        {
            Window.ClientSize = new Size(Resolution[0], Resolution[1]);
        }

        public Scene CurrentScene()
        {
            return Scenes[SceneNum];
        }

        public int[] ScaledResolution()
        {
            if (DynamicResScaleX && DynamicResScaleY)
            {
                return new int[] {Math.Max((int)Math.Round(Resolution[0] * DynamicRes[0]),1),
                    Math.Max((int)Math.Round(Resolution[1] * DynamicRes[1]), 1)};
            }
            else if (DynamicResScaleX)
            {
                return new int[] {Math.Max((int)Math.Round(Resolution[0] * DynamicRes[0]),1),
                    Resolution[1]};
            }
            else if (DynamicResScaleY)
            {
                return new int[] {Resolution[0],
                    Math.Max((int)Math.Round(Resolution[1] * DynamicRes[1]), 1)};
            }
            else
            {
                return Resolution;
            }

        }

        public void NextScene()
        {
            if (SceneNum == 4)
            {
                SceneNum = 0;
            }
            else
            {
                SceneNum += 1;
            }
        }

        public void SetScene(int NewSceneNum)
        {
            SceneNum = NewSceneNum;
        }

        public void RenderThread()
        {
            Window.DisplayFrame(CreateFrame(ScaledResolution(), Rasteriser.RenderFrame(ScaledResolution(), CurrentScene())));
            while (true)
            {
                Application.DoEvents();
                DebugTimer.Restart();
                bool RunDeferredPass = false;
                if (SceneNum == 2 || SceneNum == 3)
                {
                    RunDeferredPass = true;
                }
                Window.DisplayFrame(CreateFrame(ScaledResolution(), Rasteriser.RenderFrame(ScaledResolution(), CurrentScene(), RunDeferredPass: RunDeferredPass)));
                //Debug.WriteLine((NewCameraPosition.X+","+ NewCameraPosition.Y + "," + NewCameraPosition.Z, "\n", 
                //    NewCameraRotation.W + "," + NewCameraRotation.X + "," + NewCameraRotation.Y + "," + NewCameraRotation.Z));
                long Ellapsed = DebugTimer.ElapsedMilliseconds;
                Window.UpdateCounter((Math.Round(1000.0 / Ellapsed)).ToString());
                if (DynamicResScaleX || DynamicResScaleY)
                {
                    if (DynamicResTargetMs / Ellapsed > 1 / DynamicResAggression || Ellapsed / DynamicResTargetMs > 1 / DynamicResAggression)
                    {
                        if (DynamicResScaleX && DynamicResScaleY)
                        {
                            double Scale = Math.Sqrt((double)DynamicResTargetMs / Ellapsed);
                            DynamicRes[0] *= Scale;
                            DynamicRes[1] *= Scale;
                        }
                        else if (DynamicResScaleX)
                        {
                            DynamicRes[0] *= (double)DynamicResTargetMs / Ellapsed;
                        }
                        else
                        {
                            //DynamicResScaleY is true
                            DynamicRes[1] *= (double)DynamicResTargetMs / Ellapsed;
                        }
                    }
                }
                if (SceneNum == 0)
                {
                    CurrentScene().SceneMeshes[1].Rotation *= new Quat(new Vec3D(0, 0, 1), Ellapsed * 0.001);
                }
                else if (SceneNum == 1)
                {
                    CurrentScene().SceneMeshes[1].Rotation *= new Quat(new Vec3D(1, 0, 0), Ellapsed * 0.001);
                    CurrentScene().GenerateShadowMaps(200, 200);
                }
                CurrentScene().SceneCamera.Position = NewCameraPosition;
                CurrentScene().SceneCamera.Rotation = NewCameraRotation;
            }
        }

        public static unsafe Bitmap CreateFrame(int[] Resolution, Arr2D<Colour> FrameBuffer)
        {
            //Locks bits so they can be edited directly in DrawPixel() - http://web.archive.org/web/20150227183132/http://bobpowell.net/lockingbits.aspx
            Bitmap Frame = new Bitmap(Resolution[0], Resolution[1], PixelFormat.Format24bppRgb);
            BitmapData EditFrame = Frame.LockBits(new Rectangle(0, 0, Resolution[0], Resolution[1]), ImageLockMode.ReadOnly, Frame.PixelFormat);
            for (int y = 0; y < Resolution[1]; y++)
            {
                byte* RowPointer = (byte*)EditFrame.Scan0 + (y * EditFrame.Stride);
                for (int x = 0; x < Resolution[0]; x++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        RowPointer[x * 3 + i] = (byte)FrameBuffer[x, y][i];
                    }
                }
            }
            Frame.UnlockBits(EditFrame);
            return Frame;
        }
    }
}
