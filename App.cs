using _3D_Renderer;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace _Application_Namespace
{
    class App
    {
        private readonly Stopwatch FrameTimer = new Stopwatch();
        private double AppMillisecondTarget = 1000.0 / 60;
        private readonly Renderer RendererRef = new Renderer(new int[] { 640, 360 });
        private readonly Task RenderThread;

        public int MoveX = 0;
        public int MoveY = 0;
        public int MoveZ = 0;
        public int TurnX = 0;
        public int TurnY = 0;
        public int TurnZ = 0;

        public Vec3D NewCameraPosition;
        public Quat NewCameraRotation;
        public App()
        {
            InputHandler.MasterApp = this;
            //RenderThread = new Task(() => { AppLogic(); });
            //RenderThread.Start();
            //RendererRef.RenderThread();
            //Above or below chooses to run either the app or the renderer on the main thread.
            RenderThread = new Task(() => { RendererRef.RenderThread(); });
            RenderThread.Start();
            AppLogic();
        }
        public void AppLogic()
        {
            NewCameraPosition = RendererRef.CurrentScene().SceneCamera.Position;
            NewCameraRotation = RendererRef.CurrentScene().SceneCamera.Rotation;
            while (true)
            {
                FrameTimer.Restart();
                Quat InvRot = RendererRef.CurrentScene().SceneCamera.Rotation.Inverse();
                Matrix InvRotMat = InvRot.RotationMatrix;
                NewCameraPosition += Matrix.Multiply(InvRotMat,
                    new Vec3D(0, 0, MoveZ * 0.001 * AppMillisecondTarget).PositionMatrix(4)).ToVec3D();
                NewCameraPosition += Matrix.Multiply(InvRotMat,
                    new Vec3D(MoveX * 0.001 * AppMillisecondTarget, 0, 0).PositionMatrix(4)).ToVec3D();
                NewCameraPosition += Matrix.Multiply(InvRotMat,
                    new Vec3D(0, MoveY * 0.001 * AppMillisecondTarget, 0).PositionMatrix(4)).ToVec3D();
                NewCameraRotation *= new Quat(Math.Cos(TurnX * 0.0005 * AppMillisecondTarget), Math.Sin(TurnX * 0.0005 * AppMillisecondTarget), 0, 0);
                NewCameraRotation *= new Quat(Math.Cos(TurnY * 0.0005 * AppMillisecondTarget), 0, Math.Sin(TurnY * 0.0005 * AppMillisecondTarget), 0);
                NewCameraRotation *= new Quat(Math.Cos(TurnZ * 0.0005 * AppMillisecondTarget), 0, 0, Math.Sin(TurnZ * 0.0005 * AppMillisecondTarget));
                RendererRef.NewCameraPosition = NewCameraPosition;
                RendererRef.NewCameraRotation = NewCameraRotation;

                if (FrameTimer.ElapsedMilliseconds < AppMillisecondTarget)
                {
                    Thread.Sleep(Math.Max((int)Math.Round(AppMillisecondTarget - FrameTimer.ElapsedMilliseconds), 0));
                }
            }
        }

        public void KeyDown(string KeyValue)
        {
            if (KeyValue == "W")
            {
                if (MoveZ == 0)
                {
                    MoveZ = 1;
                }
            }
            else if (KeyValue == "S")
            {
                if (MoveZ == 0)
                {
                    MoveZ = -1;
                }
            }
            if (KeyValue == "A")
            {
                if (MoveX == 0)
                {
                    MoveX = -1;
                }
            }
            else if (KeyValue == "D")
            {
                if (MoveX == 0)
                {
                    MoveX = 1;
                }
            }
            if (KeyValue == "E")
            {
                if (MoveY == 0)
                {
                    MoveY = -1;
                }
            }
            else if (KeyValue == "Q")
            {
                if (MoveY == 0)
                {
                    MoveY = 1;
                }
            }
            if (KeyValue == "Right")
            {
                if (TurnY == 0)
                {
                    TurnY = 1;
                }
            }
            else if (KeyValue == "Left")
            {
                if (TurnY == 0)
                {
                    TurnY = -1;
                }
            }
            if (KeyValue == "Up")
            {
                if (TurnX == 0)
                {
                    TurnX = 1;
                }
            }
            else if (KeyValue == "Down")
            {
                if (TurnX == 0)
                {
                    TurnX = -1;
                }
            }
            if (KeyValue == "Z")
            {
                if (TurnZ == 0)
                {
                    TurnZ = -1;
                }
            }
            else if (KeyValue == "X")
            {
                if (TurnZ == 0)
                {
                    TurnZ = 1;
                }
            }
            if (KeyValue == "Tab")
            {
                RendererRef.NextScene();
                NewCameraPosition = RendererRef.CurrentScene().SceneCamera.Position;
                NewCameraRotation = RendererRef.CurrentScene().SceneCamera.Rotation;
            }
            else if (KeyValue == "T")
            {
                if (RendererRef.DynamicResScaleX || RendererRef.DynamicResScaleY)
                {
                    RendererRef.DynamicResScaleX = false;
                    RendererRef.DynamicResScaleY = false;
                }
                else
                {
                    RendererRef.DynamicResScaleX = true;
                    RendererRef.DynamicResScaleY = true;
                }
            }
            else if (KeyValue == "D1")
            {
                RendererRef.DynamicResTargetMs = 200;
            }
            else if (KeyValue == "D2")
            {
                RendererRef.DynamicResTargetMs = 125;
            }
            else if (KeyValue == "D3")
            {
                RendererRef.DynamicResTargetMs = 100;
            }
            else if (KeyValue == "D4")
            {
                RendererRef.DynamicResTargetMs = 67;
            }
            else if (KeyValue == "D5")
            {
                RendererRef.DynamicResTargetMs = 50;
            }
            else if (KeyValue == "D6")
            {
                RendererRef.DynamicResTargetMs = 33;
            }
        }

        public void KeyUp(int KeyValue)
        {
            if (KeyValue == 87)
            {
                MoveZ = 0;
            }
            else if (KeyValue == 83)
            {
                MoveZ = 0;
            }
            if (KeyValue == 65)
            {
                MoveX = 0;
            }
            else if (KeyValue == 68)
            {
                MoveX = 0;
            }
            if (KeyValue == 69)
            {
                MoveY = 0;
            }
            else if (KeyValue == 81)
            {
                MoveY = 0;
            }
            if (KeyValue == 39)
            {
                TurnY = 0;
            }
            else if (KeyValue == 37)
            {
                TurnY = 0;
            }
            if (KeyValue == 38)
            {
                TurnX = 0;
            }
            else if (KeyValue == 40)
            {
                TurnX = 0;
            }
            if (KeyValue == 90)
            {
                TurnZ = 0;
            }
            else if (KeyValue == 88)
            {
                TurnZ = 0;
            }
        }
    }
}
