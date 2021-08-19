using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _3D_Renderer
{
    static class Rasteriser
    {
        //Contains methods to handle the majority of rasterisation code and call the various shaders to get a final frame
        //Most methods are static

        //Note on spaces - referenced throughout the code but ESPECIALLY important here
        //World space is 3d space space relative to the scene origin
        //Mesh space is 3d space relative to each mesh's origin
        //Camera space is where the point would be of projected orthographically
        //View space is where the point will be projected on the screen INCLUDING the z-coordinate (where z is equivalent to that of camera space)
        //Screen space is where the point will be projected on the screen EXCLUDING the z-coordinate

        public const double PIXEL_RATIO_STEP = .1;
        public const int DEPTH_ONLY = 0;
        public const int FORWARD = 1;
        public const int DEFERRED = 2;
        //Maximum number of surfaces per mesh to be rendered in parallel
        public static int SurfaceThreadsPerMesh { get; set; } = 8;
        //Maximum number of batches of rows per surface to be rendered in parallel
        public static int RowThreadsPerSurface { get; set; } = 8;
        //Number of rows of pixels required for a new thread
        public static int RowRequirement { get; set; } = 50;
        //For post processing and deferred shading
        public static int ShaderThreadsPerPass { get; set; } = 8;

        public static Arr2D<Colour> RenderFrame(int[] Resolution, Scene CurrentScene, bool RunDeferredPass = true)
        {
            // Check if any deferred shaders are in use
            Arr2D<Colour> FrameBuffer;
            if (RunDeferredPass)
            {
                // Render deferred - required for screen space reflections
                RenderToTexture(Resolution, CurrentScene.SceneCamera, CurrentScene, out FrameBuffer, out _, Mode: DEFERRED);
            }
            else
            {
                RenderToTexture(Resolution, CurrentScene.SceneCamera, CurrentScene, out FrameBuffer, out _, Mode: FORWARD);
            }
            return FrameBuffer;
        }

        public static void RenderToTexture(int[] Resolution, Camera CameraObj, Scene CurrentScene,
            out Arr2D<Colour> FrameBuffer, out Arr2D<double> DepthBuffer, out (Arr2D<Vec3D> Position,
            Arr2D<Vec3D> Normal, Arr2D<Colour> Albedo, Arr2D<double> Specular, Arr2D<Colour> Emmissive,
            Arr2D<Mesh> MeshRef)
            DeferredBuffer, int Mode = DEFERRED)
        {
            Matrix CameraSpaceTransform = CameraObj.GetCameraSpaceTransform(Resolution);
            // Initialise frame buffers
            DepthBuffer = new Arr2D<double>(Resolution[0], Resolution[1]);
            FrameBuffer = new Arr2D<Colour>(0, 0);
            DeferredBuffer = (new Arr2D<Vec3D>(0, 0), new Arr2D<Vec3D>(0, 0), new Arr2D<Colour>(0, 0),
                        new Arr2D<double>(0, 0), new Arr2D<Colour>(0, 0), new Arr2D<Mesh>(0, 0));
            if (Mode != DEPTH_ONLY)
            {
                FrameBuffer = new Arr2D<Colour>(Resolution[0], Resolution[1]);
                if (Mode == DEFERRED)
                {
                    DeferredBuffer = (new Arr2D<Vec3D>(Resolution[0], Resolution[1]),
                        new Arr2D<Vec3D>(Resolution[0], Resolution[1]), new Arr2D<Colour>(Resolution[0], Resolution[1]),
                        new Arr2D<double>(Resolution[0], Resolution[1]), new Arr2D<Colour>(Resolution[0], Resolution[1]),
                        new Arr2D<Mesh>(Resolution[0], Resolution[1]));
                }
            }
            foreach (Mesh MasterMesh in CurrentScene.SceneMeshes)
            {
                // Meshes are rendered in serial
                RenderMesh(Mode, Resolution, CameraSpaceTransform, CameraObj, MasterMesh, CurrentScene, ref FrameBuffer, ref DepthBuffer,
                    ref DeferredBuffer);
            }
            if (Mode != DEPTH_ONLY)
            {
                // Render skybox and perform post-process effects if not using on depth buffer rendering mode, otherwise not necessary
                if (CurrentScene.SceneSkyBox != null)
                {
                    Matrix SkyBoxCameraSpaceTransform = CameraObj.GetCameraSpaceTransform(Resolution);
                    RenderMesh(Mode, Resolution, SkyBoxCameraSpaceTransform, CameraObj, CurrentScene.SceneSkyBox.BoxObject, CurrentScene, ref FrameBuffer,
                        ref DepthBuffer, ref DeferredBuffer);
                }
                if (Mode == DEFERRED)
                {
                    FrameBuffer = DoDeferredShading(FrameBuffer, DepthBuffer, DeferredBuffer, CameraObj, CurrentScene);
                }
                FrameBuffer = DoPostProcessing(CurrentScene.PostProcessShaders, FrameBuffer, DepthBuffer);
            }
        }

        public static void RenderToTexture(int[] Resolution, Camera CameraObj, Scene CurrentScene,
             out Arr2D<Colour> FrameBuffer, out Arr2D<double> DepthBuffer, int Mode = FORWARD)
        {
            // Overload method intended for forward rendering so defferred buffer does not have to be discarded.
            // Note that mode and overload can be mixed and matched, if a mode that does no return a selected buffer is used,
            // that buffer will simply be returned as uninitialised. If vice versa, that buffer will be created, but not returned.
            RenderToTexture(Resolution, CameraObj, CurrentScene, out FrameBuffer, out DepthBuffer, out _, Mode: Mode);
        }

        public static void RenderToTexture(int[] Resolution, Camera CameraObj, Scene CurrentScene,
             out Arr2D<double> DepthBuffer, int Mode = DEPTH_ONLY)
        {
            // Overload method intended for depth buffer rendering so defferred buffer and frame buffer does not have to be discarded.
            // Note that mode and overload can be mixed and matched, if a mode that does no return a selected buffer is used,
            // that buffer will simply be returned as uninitialised. If vice versa, that buffer will be created, but not returned.
            RenderToTexture(Resolution, CameraObj, CurrentScene, out _, out DepthBuffer, out _, Mode: Mode);
        }

        public static Arr2D<Colour> DoPostProcessing(IPostProcessShader[] PostProcessShaders, Arr2D<Colour> OldFrame, Arr2D<double> DepthBuffer)
        {
            //If no post-process shaders, OldFrame is returned
            Arr2D<Colour> NewFrame = OldFrame;
            for (int PostProcessNum = 0; PostProcessNum < PostProcessShaders.Length; PostProcessNum++)
            {
                NewFrame = new Arr2D<Colour>(OldFrame.Width, 0);
                int BatchSize = Math.Max(1, OldFrame.Height / ShaderThreadsPerPass);
                List<Task<Arr2D<Colour>>> PostProcessThreads = new List<Task<Arr2D<Colour>>> { };
                int Height = OldFrame.Height;
                for (int StartY = 0; StartY < OldFrame.Height; StartY += BatchSize)
                {
                    int StartYCopy = StartY;
                    PostProcessThreads.Add(Task<Arr2D<Colour>>.Factory.StartNew(() => { return DoPostProcessBatch(StartYCopy, Math.Min(StartYCopy + BatchSize, OldFrame.Height), PostProcessShaders[PostProcessNum], OldFrame, DepthBuffer); }));
                }
                for (int ThreadNum = 0; ThreadNum < PostProcessThreads.Count; ThreadNum++)
                {
                    NewFrame += PostProcessThreads[ThreadNum].Result;
                }
                OldFrame = NewFrame;
            }
            return NewFrame;
        }

        public static Arr2D<Colour> DoPostProcessBatch(int StartY, int EndY, IPostProcessShader PostProcessShader, Arr2D<Colour> FrameBuffer, Arr2D<double> DepthBuffer)
        {
            Arr2D<Colour> NextFrameChunk = new Arr2D<Colour>(FrameBuffer.Width, EndY - StartY);
            for (int y = StartY; y < Math.Min(EndY, FrameBuffer.Height); y++)
            {
                for (int x = 0; x < FrameBuffer.Width; x++)
                {
                    NextFrameChunk[x, y - StartY] = PostProcessShader.PerPixel(FrameBuffer, DepthBuffer, x, y);
                }
            }
            return NextFrameChunk;
        }

        public static Arr2D<Colour> DoDeferredShading(Arr2D<Colour> OldFrame, Arr2D<double> DepthBuffer, (Arr2D<Vec3D> Position,
            Arr2D<Vec3D> Normal, Arr2D<Colour> Albedo, Arr2D<double> Specular, Arr2D<Colour> Emissive,
            Arr2D<Mesh> MeshRef) DeferredBuffer, Camera CameraObj, Scene CurrentScene)
        {
            //If no post-process shaders, OldFrame is returned
            Arr2D<Colour> NewFrame = OldFrame;
            int Pass = 0;
            //Currently unknown how many deferred passes need to be performed, start with one
            int NumOfPasses = 1;
            while (Pass < NumOfPasses)
            {
                NewFrame = new Arr2D<Colour>(OldFrame.Width, 0);
                int BatchSize = Math.Max(1, OldFrame.Height / ShaderThreadsPerPass);
                List<Task<(int PassesNeeded, Arr2D<Colour> NextFrameChunk)>> DeferredShaderThreads = new List<Task<(int PassesNeeded, Arr2D<Colour> NextFrameChunk)>> { };
                int Height = OldFrame.Height;
                for (int StartY = 0; StartY < OldFrame.Height; StartY += BatchSize)
                {
                    int StartYCopy = StartY;
                    DeferredShaderThreads.Add(Task.Factory.StartNew(() =>
                    {
                        return DoDeferredShadingBatch(StartYCopy, Math.Min(StartYCopy + BatchSize, OldFrame.Height), Pass, OldFrame,
                          DepthBuffer, DeferredBuffer, CameraObj, CurrentScene);
                    }));
                }
                for (int ThreadNum = 0; ThreadNum < DeferredShaderThreads.Count; ThreadNum++)
                {
                    NewFrame += DeferredShaderThreads[ThreadNum].Result.NextFrameChunk;
                    if (Pass == 0)
                    {
                        NumOfPasses = Math.Max(NumOfPasses, DeferredShaderThreads[ThreadNum].Result.PassesNeeded);
                    }
                }
                OldFrame = NewFrame;
                Pass++;
            }
            return NewFrame;
        }

        public static (int PassesNeeded, Arr2D<Colour> NextFrameChunk) DoDeferredShadingBatch(int StartY, int EndY, int Pass, Arr2D<Colour> FrameBuffer,
            Arr2D<double> DepthBuffer, (Arr2D<Vec3D> Position, Arr2D<Vec3D> Normal, Arr2D<Colour> Albedo, Arr2D<double> Specular, Arr2D<Colour> Emissive,
            Arr2D<Mesh> MeshRef) DeferredBuffer, Camera CameraObj, Scene CurrentScene)
        {
            int PassesNeeded = 0;
            Arr2D<Colour> NextFrameChunk = new Arr2D<Colour>(FrameBuffer.Width, EndY - StartY);
            int[] Resolution = new int[] { FrameBuffer.Width, FrameBuffer.Height };
            for (int y = StartY; y < Math.Min(EndY, FrameBuffer.Height); y++)
            {
                for (int x = 0; x < FrameBuffer.Width; x++)
                {
                    Colour PixelColour = FrameBuffer[x, y];
                    if (DeferredBuffer.MeshRef[x, y] != null)
                    {
                        int NumOfShaders = DeferredBuffer.MeshRef[x, y].DeferredPixelShaders.Length + DeferredBuffer.MeshRef[x, y].DeferredShaders.Length;
                        PassesNeeded = Math.Max(PassesNeeded, NumOfShaders);
                        if (Pass < NumOfShaders)
                        {
                            if (Pass < DeferredBuffer.MeshRef[x, y].DeferredPixelShaders.Length)
                            {
                                //Run pixel shader
                                Vec3D PixelPos = DeferredBuffer.Position[x, y];
                                Vec3D PixelNormal = DeferredBuffer.Normal[x, y];
                                Colour PixelAlbedo = DeferredBuffer.Albedo[x, y];
                                double PixelSpecular = DeferredBuffer.Specular[x, y];
                                Colour PixelEmissive = DeferredBuffer.Emissive[x, y];
                                DeferredBuffer.MeshRef[x, y].DeferredPixelShaders[Pass].PerPixel(CameraObj, DeferredBuffer.MeshRef[x, y],
                                    CurrentScene, Resolution, x, y,
                                    ref PixelColour, ref PixelPos, ref PixelNormal,
                                    ref PixelAlbedo, ref PixelSpecular, ref PixelEmissive);
                            }
                            else
                            {
                                PixelColour = DeferredBuffer.MeshRef[x, y].DeferredShaders[Pass -
                                    DeferredBuffer.MeshRef[x, y].DeferredPixelShaders.Length].PerPixel(FrameBuffer, DepthBuffer,
                                    DeferredBuffer, x, y, Resolution, CameraObj);
                            }
                        }
                    }
                    NextFrameChunk[x, y - StartY] = PixelColour;
                }
            }
            return (PassesNeeded, NextFrameChunk);
        }

        public static void RenderMesh(int Mode, int[] Resolution, Matrix CameraSpaceTransform, Camera CameraObj, Mesh MasterMesh, Scene CurrentScene,
            ref Arr2D<Colour> FrameBuffer, ref Arr2D<double> DepthBuffer, ref (Arr2D<Vec3D> Position,
            Arr2D<Vec3D> Normal, Arr2D<Colour> Albedo, Arr2D<double> Specular, Arr2D<Colour> Emissive,
            Arr2D<Mesh> MeshRef) DeferredBuffer)
        {
            Matrix RotationMatrix = MasterMesh.Rotation.Inverse().RotationMatrix;
            //Can swap scaling and rotation, but translation must be last as the former transformations rely on scaling and rotating about the origin in mesh space
            Matrix WorldSpaceTransform = Matrix.Multiply(MasterMesh.Position.ScalarMult(-1).TranslationMatrix(),
                Matrix.Multiply(RotationMatrix, MasterMesh.Scale.ScaleMatrix()));
            List<Task<(JaggedArr<double>[] Depth, JaggedArr<Colour>[] Frame, (JaggedArr<Vec3D> Position, JaggedArr<Vec3D> Normal,
                JaggedArr<Colour> Albedo, JaggedArr<double> Specular, JaggedArr<Colour> Emissive)[] Deferred)>> MeshThreads =
                new List<Task<(JaggedArr<double>[] Depth, JaggedArr<Colour>[] Frame, (JaggedArr<Vec3D> Position,
                JaggedArr<Vec3D> Normal, JaggedArr<Colour> Albedo, JaggedArr<double> Specular, JaggedArr<Colour> Emissive)[] Deferred)>>();
            int BatchSize = Math.Max(1, MasterMesh.Faces.Length / SurfaceThreadsPerMesh);
            int FaceNum;
            Arr2D<double> DepthBufferCopy = DepthBuffer;
            for (FaceNum = 0; FaceNum < MasterMesh.Faces.Length - BatchSize; FaceNum += BatchSize)
            {
                int FaceNumCopy = FaceNum;
                // So depth check short circuiting is possible

                Surface[] Faces = new Surface[BatchSize];
                Array.Copy(MasterMesh.Faces, FaceNum, Faces, 0, BatchSize);
                MeshThreads.Add(Task.Factory.StartNew(() =>
                {
                    return DoDrawSurfaceBatch(Faces, Mode, Resolution, WorldSpaceTransform, CameraSpaceTransform, RotationMatrix, CameraObj,
                        MasterMesh, CurrentScene, DepthBufferCopy);
                }));
            }
            Surface[] Faces2 = new Surface[MasterMesh.Faces.Length - FaceNum];
            Array.Copy(MasterMesh.Faces, FaceNum, Faces2, 0, MasterMesh.Faces.Length - FaceNum);
            MeshThreads.Add(Task.Factory.StartNew(() =>
            {
                return DoDrawSurfaceBatch(Faces2, Mode, Resolution, WorldSpaceTransform, CameraSpaceTransform, RotationMatrix, CameraObj,
                    MasterMesh, CurrentScene, DepthBufferCopy);
            }));
            for (int ThreadNum = 0; ThreadNum < MeshThreads.Count; ThreadNum++)
            {
                for (int SurfaceNum = 0; SurfaceNum < MeshThreads[ThreadNum].Result.Depth.Length; SurfaceNum++)
                {
                    if (MeshThreads[ThreadNum].Result.Depth[SurfaceNum] != null)
                    {
                        for (int RowNum = 0; RowNum < MeshThreads[ThreadNum].Result.Depth[SurfaceNum].Height; RowNum++)
                        {
                            for (int ColNum = 0; ColNum < MeshThreads[ThreadNum].Result.Depth[SurfaceNum].Width(RowNum); ColNum++)
                            {
                                int y = MeshThreads[ThreadNum].Result.Depth[SurfaceNum].StartY + RowNum;
                                int x = MeshThreads[ThreadNum].Result.Depth[SurfaceNum].StartX(RowNum) + ColNum;
                                if (MeshThreads[ThreadNum].Result.Depth[SurfaceNum][x, y] != 0 && (
                                    DepthBuffer[x, y] == 0 || MeshThreads[ThreadNum].Result.Depth[SurfaceNum][x, y] <= DepthBuffer[x, y]))
                                {
                                    //Depth check succeeds
                                    DepthBuffer[x, y] = MeshThreads[ThreadNum].Result.Depth[SurfaceNum][x, y];
                                    if (Mode != DEPTH_ONLY)
                                    {
                                        FrameBuffer[x, y] = MeshThreads[ThreadNum].Result.Frame[SurfaceNum][x, y];
                                        if (Mode == DEFERRED)
                                        {
                                            DeferredBuffer.Position[x, y] = MeshThreads[ThreadNum].Result.Deferred[SurfaceNum].Position[x, y];
                                            DeferredBuffer.Normal[x, y] = MeshThreads[ThreadNum].Result.Deferred[SurfaceNum].Normal[x, y];
                                            DeferredBuffer.Albedo[x, y] = MeshThreads[ThreadNum].Result.Deferred[SurfaceNum].Albedo[x, y];
                                            DeferredBuffer.Specular[x, y] = MeshThreads[ThreadNum].Result.Deferred[SurfaceNum].Specular[x, y];
                                            DeferredBuffer.Emissive[x, y] = MeshThreads[ThreadNum].Result.Deferred[SurfaceNum].Emissive[x, y];
                                            DeferredBuffer.MeshRef[x, y] = MasterMesh;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static (JaggedArr<double>[] Depth, JaggedArr<Colour>[] Frame, (JaggedArr<Vec3D> Position, JaggedArr<Vec3D> Normal,
            JaggedArr<Colour> Albedo, JaggedArr<double> Specular, JaggedArr<Colour> Emissive)[] Deferred) DoDrawSurfaceBatch(Surface[] Faces,
            int Mode, int[] Resolution, Matrix WorldSpaceTransform, Matrix CameraSpaceTransform, Matrix MeshRotationMat, Camera CameraObj,
            Mesh MasterMesh, Scene CurrentScene, Arr2D<double> OldDepthBuffer)
        {
            JaggedArr<double>[] SurfaceDepthBufferEdits = new JaggedArr<double>[Faces.Length];
            JaggedArr<Colour>[] SurfaceFrameBufferEdits = new JaggedArr<Colour>[Faces.Length];
            (JaggedArr<Vec3D> Position, JaggedArr<Vec3D> Normal,
            JaggedArr<Colour> Albedo, JaggedArr<double> Specular, JaggedArr<Colour> Emissive)[] SurfaceDeferredBufferEdits =
            new (JaggedArr<Vec3D> Position, JaggedArr<Vec3D> Normal, JaggedArr<Colour> Albedo, JaggedArr<double> Specular,
            JaggedArr<Colour> Emissive)[Faces.Length];
            for (int FaceNum = 0; FaceNum < Faces.Length; FaceNum++)
            {
                (JaggedArr<double> SurfaceDepth, JaggedArr<Colour> SurfaceFrame, (JaggedArr<Vec3D> Position,
            JaggedArr<Vec3D> Normal, JaggedArr<Colour> Albedo, JaggedArr<double> Specular, JaggedArr<Colour> Emissive) SurfaceDeferred)? BufferEdits =
            DrawSurface(Mode, Resolution, WorldSpaceTransform, CameraSpaceTransform, MeshRotationMat, CameraObj, Faces[FaceNum], MasterMesh,
            CurrentScene, OldDepthBuffer);
                if (BufferEdits != null)
                {
                    SurfaceDepthBufferEdits[FaceNum] = BufferEdits.GetValueOrDefault().SurfaceDepth;
                    if (Mode != DEPTH_ONLY)
                    {
                        SurfaceFrameBufferEdits[FaceNum] = BufferEdits.GetValueOrDefault().SurfaceFrame;
                        if (Mode == DEFERRED)
                        {
                            SurfaceDeferredBufferEdits[FaceNum] = BufferEdits.GetValueOrDefault().SurfaceDeferred;
                        }
                    }
                }
            }
            return (SurfaceDepthBufferEdits, SurfaceFrameBufferEdits, SurfaceDeferredBufferEdits);
        }

        public static Vec3D[] ComputeSurfaceNormals(Surface Face, TriVec3D WorldSpaceVerts, Mesh MasterMesh)
        {
            //Tangent and bitangent must be aligned with the texture
            return new Vec3D[] { Vec3D.CrossProduct(WorldSpaceVerts.Edge1,WorldSpaceVerts.Edge2).Normalise(),
                        new Vec3D(Face.VertexTextureCoords(MasterMesh).Edge2.Y*WorldSpaceVerts.Edge1.X-Face.VertexTextureCoords(MasterMesh).Edge1.Y*WorldSpaceVerts.Edge2.X,
                                  Face.VertexTextureCoords(MasterMesh).Edge2.Y*WorldSpaceVerts.Edge1.Y-Face.VertexTextureCoords(MasterMesh).Edge1.Y*WorldSpaceVerts.Edge2.Y,
                                  Face.VertexTextureCoords(MasterMesh).Edge2.Y*WorldSpaceVerts.Edge1.Z-Face.VertexTextureCoords(MasterMesh).Edge1.Y*WorldSpaceVerts.Edge2.Z).Normalise(),
                        new Vec3D(Face.VertexTextureCoords(MasterMesh).Edge1.X*WorldSpaceVerts.Edge2.X-Face.VertexTextureCoords(MasterMesh).Edge2.X*WorldSpaceVerts.Edge1.X,
                                  Face.VertexTextureCoords(MasterMesh).Edge1.X*WorldSpaceVerts.Edge2.Y-Face.VertexTextureCoords(MasterMesh).Edge2.X*WorldSpaceVerts.Edge1.Y,
                                  Face.VertexTextureCoords(MasterMesh).Edge1.X*WorldSpaceVerts.Edge2.Z-Face.VertexTextureCoords(MasterMesh).Edge2.X*WorldSpaceVerts.Edge1.Z).Normalise()
                    };
        }

        public static TriVec3D[] ComputeVertexNormals(Surface Face, Mesh MasterMesh, Vec3D[] SurfaceNormals, Matrix MeshRotationMatrix)
        {
            // Convertes vertex tangent and bitangent from mesh to world space and returns the normal, tangent and bitangent for each vertex
            TriVec3D BaseVertexNormals = Face.Normals(MasterMesh).ToTriMat(4).Multiply(MeshRotationMatrix).ToTriVec3D();
            BaseVertexNormals = new TriVec3D(BaseVertexNormals.Vert1.ScalarMult(-1),
                BaseVertexNormals.Vert2.ScalarMult(-1),
                BaseVertexNormals.Vert3.ScalarMult(-1));
            Matrix[] NormalRotations = new Matrix[] { new Quat(SurfaceNormals[0], BaseVertexNormals.Vert1).RotationMatrix,
                    new Quat(SurfaceNormals[0], BaseVertexNormals.Vert2).RotationMatrix,
                    new Quat(SurfaceNormals[0], BaseVertexNormals.Vert3).RotationMatrix};
            return new TriVec3D[]{ new TriVec3D(BaseVertexNormals.Vert1, BaseVertexNormals.Vert2,
                                                            BaseVertexNormals.Vert3),
                                    new TriVec3D(Matrix.Multiply(NormalRotations[0],SurfaceNormals[1].PositionMatrix(4)).ToVec3D(),
                                                            Matrix.Multiply(NormalRotations[1],SurfaceNormals[1].PositionMatrix(4)).ToVec3D(),
                                                            Matrix.Multiply(NormalRotations[2],SurfaceNormals[1].PositionMatrix(4)).ToVec3D()),
                                    new TriVec3D(Matrix.Multiply(NormalRotations[0],SurfaceNormals[2].PositionMatrix(4)).ToVec3D(),
                                                            Matrix.Multiply(NormalRotations[1],SurfaceNormals[2].PositionMatrix(4)).ToVec3D(),
                                                            Matrix.Multiply(NormalRotations[2],SurfaceNormals[2].PositionMatrix(4)).ToVec3D())};
        }

        public static TriVec2D ComputeVertexRatios(TriVec3D CameraSpaceVerts, TriVec2D ScreenSpaceVerts, Matrix ViewSpaceTransform, Surface Face, Mesh MasterMesh)
        {
            //Finds the ratios of texels to pixels at the vertices of the triangle
            //Interpolating these gives a good approximation of pixel derivatives
            Vec2D Vert1XStep = Face.VertexTextureCoords(MasterMesh).Vert1 + new Vec2D(PIXEL_RATIO_STEP, 0);
            Vec2D Vert1YStep = Face.VertexTextureCoords(MasterMesh).Vert1 + new Vec2D(0, PIXEL_RATIO_STEP);
            Bary Vert1XStepCoord = Face.VertexTextureCoords(MasterMesh).ComputeBaryCoord(Vert1XStep);
            Bary Vert1YStepCoord = Face.VertexTextureCoords(MasterMesh).ComputeBaryCoord(Vert1YStep);

            Vec2D Vert2XStep = Face.VertexTextureCoords(MasterMesh).Vert2 + new Vec2D(PIXEL_RATIO_STEP, 0);
            Vec2D Vert2YStep = Face.VertexTextureCoords(MasterMesh).Vert2 + new Vec2D(0, PIXEL_RATIO_STEP);
            Bary Vert2XStepCoord = Face.VertexTextureCoords(MasterMesh).ComputeBaryCoord(Vert2XStep);
            Bary Vert2YStepCoord = Face.VertexTextureCoords(MasterMesh).ComputeBaryCoord(Vert2YStep);

            Vec2D Vert3XStep = Face.VertexTextureCoords(MasterMesh).Vert3 + new Vec2D(PIXEL_RATIO_STEP, 0);
            Vec2D Vert3YStep = Face.VertexTextureCoords(MasterMesh).Vert3 + new Vec2D(0, PIXEL_RATIO_STEP);
            Bary Vert3XStepCoord = Face.VertexTextureCoords(MasterMesh).ComputeBaryCoord(Vert3XStep);
            Bary Vert3YStepCoord = Face.VertexTextureCoords(MasterMesh).ComputeBaryCoord(Vert3YStep);

            Vec3D CameraSpaceVert1XStep = CameraSpaceVerts.BaryInterp(Vert1XStepCoord);
            Vec2D ScreenSpaceVert1XStep = Matrix.Multiply(ViewSpaceTransform, CameraSpaceVert1XStep.ScalarMult(1 / CameraSpaceVert1XStep.Z).PositionMatrix(4)).ToVec2D();
            Vec3D CameraSpaceVert1YStep = CameraSpaceVerts.BaryInterp(Vert1YStepCoord);
            Vec2D ScreenSpaceVert1YStep = Matrix.Multiply(ViewSpaceTransform, CameraSpaceVert1YStep.ScalarMult(1 / CameraSpaceVert1YStep.Z).PositionMatrix(4)).ToVec2D();
            Vec2D Vert1Ratio = new Vec2D(PIXEL_RATIO_STEP / (ScreenSpaceVerts.Vert1 - ScreenSpaceVert1XStep).ChebyshevDist(),
                PIXEL_RATIO_STEP / (ScreenSpaceVerts.Vert1 - ScreenSpaceVert1YStep).ChebyshevDist());

            Vec3D CameraSpaceVert2XStep = CameraSpaceVerts.BaryInterp(Vert2XStepCoord);
            Vec2D ScreenSpaceVert2XStep = Matrix.Multiply(ViewSpaceTransform, CameraSpaceVert2XStep.ScalarMult(1 / CameraSpaceVert2XStep.Z).PositionMatrix(4)).ToVec2D();
            Vec3D CameraSpaceVert2YStep = CameraSpaceVerts.BaryInterp(Vert2YStepCoord);
            Vec2D ScreenSpaceVert2YStep = Matrix.Multiply(ViewSpaceTransform, CameraSpaceVert2YStep.ScalarMult(1 / CameraSpaceVert2YStep.Z).PositionMatrix(4)).ToVec2D();
            Vec2D Vert2Ratio = new Vec2D(PIXEL_RATIO_STEP / (ScreenSpaceVerts.Vert2 - ScreenSpaceVert2XStep).ChebyshevDist(),
                PIXEL_RATIO_STEP / (ScreenSpaceVerts.Vert2 - ScreenSpaceVert2YStep).ChebyshevDist());

            Vec3D CameraSpaceVert3XStep = CameraSpaceVerts.BaryInterp(Vert3XStepCoord);
            Vec2D ScreenSpaceVert3XStep = Matrix.Multiply(ViewSpaceTransform, CameraSpaceVert3XStep.ScalarMult(1 / CameraSpaceVert3XStep.Z).PositionMatrix(4)).ToVec2D();
            Vec3D CameraSpaceVert3YStep = CameraSpaceVerts.BaryInterp(Vert3YStepCoord);
            Vec2D ScreenSpaceVert3YStep = Matrix.Multiply(ViewSpaceTransform, CameraSpaceVert3YStep.ScalarMult(1 / CameraSpaceVert3YStep.Z).PositionMatrix(4)).ToVec2D();
            Vec2D Vert3Ratio = new Vec2D(PIXEL_RATIO_STEP / (ScreenSpaceVerts.Vert3 - ScreenSpaceVert3XStep).ChebyshevDist(),
                PIXEL_RATIO_STEP / (ScreenSpaceVerts.Vert3 - ScreenSpaceVert3YStep).ChebyshevDist());
            return new TriVec2D(Vert1Ratio, Vert2Ratio, Vert3Ratio);
        }

        public static (JaggedArr<double> SurfaceDepth, JaggedArr<Colour> SurfaceFrame, (JaggedArr<Vec3D> Position,
            JaggedArr<Vec3D> Normal, JaggedArr<Colour> Albedo, JaggedArr<double> Specular, JaggedArr<Colour> Emissive) SurfaceDeferred)? DrawSurface(
            int Mode, int[] Resolution, Matrix WorldSpaceTransform, Matrix CameraSpaceTransform, Matrix MeshRotationMat,
            Camera CameraObj, Surface Face, Mesh MasterMesh, Scene CurrentScene, Arr2D<double> OldDepthBuffer)
        {
            TriMat MeshSpaceVerts = Face.Verts(MasterMesh).ToTriMat(4);
            TriMat WorldSpaceVerts = MeshSpaceVerts.Multiply(WorldSpaceTransform);
            TriVec3D FinalWorldSpaceVerts = WorldSpaceVerts.ToTriVec3D();
            //Array to store the normal, tangent and bitangent vectors of the surface
            Vec3D[] SurfaceNormals = ComputeSurfaceNormals(Face, FinalWorldSpaceVerts, MasterMesh);
            TriMat CameraSpaceVerts = WorldSpaceVerts.Multiply(CameraSpaceTransform);
            List<int> Behind = new List<int>();
            if (CameraSpaceVerts.Vert1.Values[2, 0] <= CameraObj.MinZ)
            {
                Behind.Add(1);
            }
            if (CameraSpaceVerts.Vert2.Values[2, 0] <= CameraObj.MinZ)
            {
                Behind.Add(2);
            }
            if (CameraSpaceVerts.Vert3.Values[2, 0] <= CameraObj.MinZ)
            {
                Behind.Add(3);
            }
            TriVec3D FinalCameraSpaceVerts = CameraSpaceVerts.ToTriVec3D();
            //Have to divide the x and y components by z to correct for perspective
            //As no near-clipping plane is in use, all 3d triangles are mapped to 2d triangles, even if they should be quadrilaterals
            //With the depth estimation later, it should not be too obvious, and clipping is expensive
            //Pre-splitting up any large surfaces into reasonably small triangles in 3d modelling software should provide better performance
            //than running any clipping algorithm while not looking that much worse
            Vec3D ViewSpaceVert1 = new Vec3D(FinalCameraSpaceVerts.Vert1.X / Math.Max(CameraObj.MinZ, FinalCameraSpaceVerts.Vert1.Z),
                                                          FinalCameraSpaceVerts.Vert1.Y / Math.Max(CameraObj.MinZ, FinalCameraSpaceVerts.Vert1.Z),
                                                          Math.Max(CameraObj.MinZ, FinalCameraSpaceVerts.Vert1.Z));
            Vec3D ViewSpaceVert2 = new Vec3D(FinalCameraSpaceVerts.Vert2.X / Math.Max(CameraObj.MinZ, FinalCameraSpaceVerts.Vert2.Z),
                                                          FinalCameraSpaceVerts.Vert2.Y / Math.Max(CameraObj.MinZ, FinalCameraSpaceVerts.Vert2.Z),
                                                          Math.Max(CameraObj.MinZ, FinalCameraSpaceVerts.Vert2.Z));
            Vec3D ViewSpaceVert3 = new Vec3D(FinalCameraSpaceVerts.Vert3.X / Math.Max(CameraObj.MinZ, FinalCameraSpaceVerts.Vert3.Z),
                                                          FinalCameraSpaceVerts.Vert3.Y / Math.Max(CameraObj.MinZ, FinalCameraSpaceVerts.Vert3.Z),
                                                          Math.Max(CameraObj.MinZ, FinalCameraSpaceVerts.Vert3.Z));
            if (Behind.Count == 3)
            {
                //No vertex of surface is visible so cull
                return null;
            }
            TriMat ViewSpaceVerts = new TriMat(ViewSpaceVert1.PositionMatrix(4),
                                               ViewSpaceVert2.PositionMatrix(4),
                                               ViewSpaceVert3.PositionMatrix(4));
            //Translates to screen space (so 0,0 is at the corner instead of the middle)
            Matrix ViewSpaceTransform = new Matrix(new Arr2D<double>(new double[,] { {1, 0, 0, Resolution[0]/2 },
                                                                                   {0, 1, 0, Resolution[1]/2 },
                                                                                   {0, 0, 1, 0 },
                                                                                   {0, 0, 0, 1 } }));
            ViewSpaceVerts = ViewSpaceVerts.Multiply(ViewSpaceTransform);
            TriVec3D FinalViewSpaceVerts = ViewSpaceVerts.ToTriVec3D();
            //Bounding box culling
            if (Math.Max(Math.Max(FinalViewSpaceVerts.Vert1.X, FinalViewSpaceVerts.Vert2.X), FinalViewSpaceVerts.Vert3.X) < 0)
            {
                //Cull
                return null;
            }
            else if (Math.Min(Math.Min(FinalViewSpaceVerts.Vert1.X, FinalViewSpaceVerts.Vert2.X), FinalViewSpaceVerts.Vert3.X) > Resolution[0] - 1)
            {
                //Cull
                return null;
            }
            else if (Math.Max(Math.Max(FinalViewSpaceVerts.Vert1.Y, FinalViewSpaceVerts.Vert2.Y), FinalViewSpaceVerts.Vert3.Y) < 0)
            {
                //Cull
                return null;
            }
            else if (Math.Min(Math.Min(FinalViewSpaceVerts.Vert1.Y, FinalViewSpaceVerts.Vert2.Y), FinalViewSpaceVerts.Vert3.Y) > Resolution[1] - 1)
            {
                //Cull
                return null;
            }
            TriVec2D FinalScreenSpaceVerts = ViewSpaceVerts.ToTriVec2D();
            //Back face culling
            if (Face.BackFaceCull && FinalScreenSpaceVerts.SignedArea < 0)
            {
                //Cull
                return null;
            }
            TriVec2D VertexRatios = ComputeVertexRatios(FinalCameraSpaceVerts, FinalScreenSpaceVerts, ViewSpaceTransform, Face, MasterMesh);
            TriVec3D[] VertexNormals = Array.Empty<TriVec3D>();
            if (MasterMesh.SmoothShading)
            {
                // Computes vertex normals if smooth shading is being used
                VertexNormals = ComputeVertexNormals(Face, MasterMesh, SurfaceNormals, MeshRotationMat);
            }
            //Sorts vertices to make sure Vert1 is lowest, Vert2 is in the middle and Vert3 the highest
            Vec2D[] TempScreenSpaceVerts = new Vec2D[] { ViewSpaceVerts.Vert1.ToVec2D(), ViewSpaceVerts.Vert2.ToVec2D(),
                ViewSpaceVerts.Vert3.ToVec2D() };
            for (int i = 0; i < 2; i++)
            {
                for (int i2 = 0; i2 < 2 - i; i2++)
                {
                    if (TempScreenSpaceVerts[i2].Y > TempScreenSpaceVerts[i2 + 1].Y)
                    {
                        Vec2D TempScreenSpaceVert = TempScreenSpaceVerts[i2];
                        TempScreenSpaceVerts[i2] = TempScreenSpaceVerts[i2 + 1];
                        TempScreenSpaceVerts[i2 + 1] = TempScreenSpaceVert;
                    }
                }
            }
            TriVec2D ScreenSpaceVerts = new TriVec2D(TempScreenSpaceVerts[0], TempScreenSpaceVerts[1], TempScreenSpaceVerts[2]);
            // Was ceiling for maxY
            int MinY = Math.Max(0, (int)Math.Ceiling(ScreenSpaceVerts.Vert1.Y));
            int MaxY = Math.Min(Resolution[1] - 1, (int)Math.Ceiling(ScreenSpaceVerts.Vert3.Y));
            if (MaxY < MinY)
            {
                //Triangle is off-screen
                return null;
            }
            else if (MinY == MaxY)
            {
                int MinX = (int)Math.Floor(Math.Min(Math.Min(ScreenSpaceVerts.Vert1.X, ScreenSpaceVerts.Vert2.X), ScreenSpaceVerts.Vert3.X));
                int MaxX = (int)Math.Floor(Math.Max(Math.Max(ScreenSpaceVerts.Vert1.X, ScreenSpaceVerts.Vert2.X), ScreenSpaceVerts.Vert3.X));
                if (MinX == MaxX)
                {
                    MaxX += 1;
                }
                MinX = Math.Max(0, MinX);
                MaxX = Math.Min(Resolution[0] - 1, MaxX);
                if (MaxX < MinX)
                {
                    //Triangle is off-screen
                    return null;
                }
                int[] StartXs = new int[] { MinX };
                int[] EndXs = new int[] { MaxX };
                JaggedArr<double> SurfaceDepth = new JaggedArr<double>(MinY, StartXs, EndXs);
                JaggedArr<Colour> SurfaceFrame = new JaggedArr<Colour>(MinY, StartXs, EndXs);
                (JaggedArr<Vec3D> Position, JaggedArr<Vec3D> Normal, JaggedArr<Colour> Albedo, JaggedArr<double> Specular,
                    JaggedArr<Colour> Emissive) SurfaceDeferred = (new JaggedArr<Vec3D>(MinY, StartXs, EndXs),
                    new JaggedArr<Vec3D>(MinY, StartXs, EndXs),
                    new JaggedArr<Colour>(MinY, StartXs, EndXs), new JaggedArr<double>(MinY, StartXs, EndXs),
                    new JaggedArr<Colour>(MinY, StartXs, EndXs));
                DrawSurfaceRow(Mode, MinX, MaxX, MinY, Resolution, FinalViewSpaceVerts, SurfaceNormals, VertexNormals,
                    FinalWorldSpaceVerts, VertexRatios, CameraObj, Face, MasterMesh, CurrentScene, OldDepthBuffer, ref SurfaceDepth, ref SurfaceFrame,
                    ref SurfaceDeferred);
                return (SurfaceDepth, SurfaceFrame, SurfaceDeferred);
            }
            else
            {
                int StartX;
                int EndX;
                int[] StartXs = new int[MaxY - MinY];
                int[] EndXs = new int[MaxY - MinY];
                //Precomputes start and end x coordinates for every row of the triangle
                for (int RowY = MinY; RowY < MaxY; RowY++)
                {
                    if ((ScreenSpaceVerts.Vert1.Y - RowY) / (ScreenSpaceVerts.Vert1.Y - ScreenSpaceVerts.Vert3.Y) < (ScreenSpaceVerts.Vert2.Y - ScreenSpaceVerts.Vert1.Y) / (ScreenSpaceVerts.Vert3.Y - ScreenSpaceVerts.Vert1.Y))
                    {
                        StartX = (int)Math.Floor(ScreenSpaceVerts.Vert1.X * (1 - ((ScreenSpaceVerts.Vert1.Y - RowY) / (ScreenSpaceVerts.Vert1.Y - ScreenSpaceVerts.Vert2.Y))) + ScreenSpaceVerts.Vert2.X * ((ScreenSpaceVerts.Vert1.Y - RowY) / (ScreenSpaceVerts.Vert1.Y - ScreenSpaceVerts.Vert2.Y)));
                    }
                    else
                    {
                        StartX = (int)Math.Floor(ScreenSpaceVerts.Vert2.X * (1 - ((ScreenSpaceVerts.Vert2.Y - RowY) / (ScreenSpaceVerts.Vert2.Y - ScreenSpaceVerts.Vert3.Y))) + ScreenSpaceVerts.Vert3.X * ((ScreenSpaceVerts.Vert2.Y - RowY) / (ScreenSpaceVerts.Vert2.Y - ScreenSpaceVerts.Vert3.Y)));
                    }
                    EndX = (int)Math.Floor(ScreenSpaceVerts.Vert1.X * (1 - ((ScreenSpaceVerts.Vert1.Y - RowY) / (ScreenSpaceVerts.Vert1.Y - ScreenSpaceVerts.Vert3.Y))) + ScreenSpaceVerts.Vert3.X * ((ScreenSpaceVerts.Vert1.Y - RowY) / (ScreenSpaceVerts.Vert1.Y - ScreenSpaceVerts.Vert3.Y)));
                    if (EndX < StartX)
                    {
                        int Temp = StartX;
                        StartX = EndX;
                        EndX = Temp;
                    }
                    else if (StartX == EndX)
                    {
                        EndX += 1;
                    }
                    EndX += 1;
                    StartX = Math.Max(0, StartX);
                    EndX = Math.Min(Resolution[0] - 1, EndX);
                    if (EndX < StartX)
                    {
                        //Row is off-screen
                        StartXs[RowY - MinY] = 0;
                        EndXs[RowY - MinY] = 0;
                    }
                    else
                    {
                        StartXs[RowY - MinY] = StartX;
                        EndXs[RowY - MinY] = EndX;
                    }
                }
                // Initialises surface changes to the buffer
                JaggedArr<double> SurfaceDepth = new JaggedArr<double>(MinY, StartXs, EndXs);
                JaggedArr<Colour> SurfaceFrame = new JaggedArr<Colour>(0, Array.Empty<int>(), Array.Empty<int>());
                (JaggedArr<Vec3D> Position, JaggedArr<Vec3D> Normal, JaggedArr<Colour> Albedo, JaggedArr<double> Specular,
                    JaggedArr<Colour> Emissive) SurfaceDeferred = (new JaggedArr<Vec3D>(0, Array.Empty<int>(), Array.Empty<int>()),
                    new JaggedArr<Vec3D>(0, Array.Empty<int>(), Array.Empty<int>()), new JaggedArr<Colour>(0, Array.Empty<int>(), Array.Empty<int>()),
                    new JaggedArr<double>(0, Array.Empty<int>(), Array.Empty<int>()), new JaggedArr<Colour>(0, Array.Empty<int>(), Array.Empty<int>()));
                if (Mode != DEPTH_ONLY)
                {
                    SurfaceFrame = new JaggedArr<Colour>(MinY, StartXs, EndXs);
                    if (Mode == DEFERRED)
                    {
                        SurfaceDeferred = (new JaggedArr<Vec3D>(MinY, StartXs, EndXs),
                        new JaggedArr<Vec3D>(MinY, StartXs, EndXs), new JaggedArr<Colour>(MinY, StartXs, EndXs),
                        new JaggedArr<double>(MinY, StartXs, EndXs), new JaggedArr<Colour>(MinY, StartXs, EndXs));
                    }
                }

                //Using loads of threads for small rows becomes very inneficient due to overhead in starting a task.
                //int ThreadCount = Math.Min(RowThreadsPerSurface, 1 + Math.Max(1, MaxY - MinY) / 30);
                List<Task<(JaggedArr<double> Depth, JaggedArr<Colour> Frame, (JaggedArr<Vec3D> Position, JaggedArr<Vec3D> Normal,
                    JaggedArr<Colour> Albedo, JaggedArr<double> Specular, JaggedArr<Colour> Emissive) Deferred)>> SurfaceThreads =
                    new List<Task<(JaggedArr<double> Depth, JaggedArr<Colour> Frame, (JaggedArr<Vec3D> Position,
                    JaggedArr<Vec3D> Normal, JaggedArr<Colour> Albedo, JaggedArr<double> Specular, JaggedArr<Colour> Emissive) Deferred)>>();
                int BatchSize = Math.Max(1, Math.Max((MaxY - MinY) / RowThreadsPerSurface, RowRequirement));
                int StartY;
                for (StartY = MinY; StartY < MaxY - BatchSize; StartY += BatchSize)
                {
                    int StartYCopy = StartY;
                    int EndY = Math.Min(StartY + BatchSize, MaxY);
                    // Copies StartXs and StartYs and with just the values needed for the batch
                    int[] StartXsCopy = new int[EndY - StartY];
                    int[] EndXsCopy = new int[EndY - StartY];
                    Array.Copy(StartXs, StartY - MinY, StartXsCopy, 0, EndY - StartY);
                    Array.Copy(EndXs, StartY - MinY, EndXsCopy, 0, EndY - StartY);
                    SurfaceThreads.Add(Task.Factory.StartNew(() =>
                    {
                        return DoSurfaceRowBatch(Mode, Resolution, FinalViewSpaceVerts, SurfaceNormals, VertexNormals, FinalWorldSpaceVerts,
                            VertexRatios, StartYCopy, EndY, StartXsCopy, EndXsCopy, CameraObj, Face, MasterMesh, CurrentScene, OldDepthBuffer);
                    }));
                }
                // Then rasterises whatever the remaining part of the surface is
                int[] StartXsCopy2 = new int[MaxY - StartY];
                int[] EndXsCopy2 = new int[MaxY - StartY];
                Array.Copy(StartXs, StartY - MinY, StartXsCopy2, 0, MaxY - StartY);
                Array.Copy(EndXs, StartY - MinY, EndXsCopy2, 0, MaxY - StartY);
                SurfaceThreads.Add(Task.Factory.StartNew(() =>
                {
                    return DoSurfaceRowBatch(Mode, Resolution, FinalViewSpaceVerts, SurfaceNormals, VertexNormals, FinalWorldSpaceVerts,
                        VertexRatios, StartY, MaxY, StartXsCopy2, EndXsCopy2, CameraObj, Face, MasterMesh, CurrentScene, OldDepthBuffer);
                }));
                for (int ThreadNum = 0; ThreadNum < SurfaceThreads.Count; ThreadNum++)
                {
                    // Get returned data from the rows and combine it into the surface's jagged arrays
                    SurfaceDepth.ReplaceRows(SurfaceThreads[ThreadNum].Result.Depth);
                    if (Mode != DEPTH_ONLY)
                    {
                        SurfaceFrame.ReplaceRows(SurfaceThreads[ThreadNum].Result.Frame);
                    }
                    if (Mode == DEFERRED)
                    {
                        SurfaceDeferred.Position.ReplaceRows(SurfaceThreads[ThreadNum].Result.Deferred.Position);
                        SurfaceDeferred.Normal.ReplaceRows(SurfaceThreads[ThreadNum].Result.Deferred.Normal);
                        SurfaceDeferred.Albedo.ReplaceRows(SurfaceThreads[ThreadNum].Result.Deferred.Albedo);
                        SurfaceDeferred.Specular.ReplaceRows(SurfaceThreads[ThreadNum].Result.Deferred.Specular);
                        SurfaceDeferred.Emissive.ReplaceRows(SurfaceThreads[ThreadNum].Result.Deferred.Emissive);
                    }
                }
                return (SurfaceDepth, SurfaceFrame, SurfaceDeferred);
            }
        }

        public static (JaggedArr<double> Depth, JaggedArr<Colour> Frame, (JaggedArr<Vec3D> Position, JaggedArr<Vec3D> Normal,
            JaggedArr<Colour> Albedo, JaggedArr<double> Specular, JaggedArr<Colour> Emissive) Deferred) DoSurfaceRowBatch(int Mode,
            int[] Resolution, TriVec3D ViewSpaceVerts, Vec3D[] SurfaceNormals, TriVec3D[] VertexNormals, TriVec3D WorldSpaceVerts,
            TriVec2D VertexRatios, int StartY, int EndY, int[] StartXs, int[] EndXs, Camera CameraObj, Surface Face, Mesh MasterMesh,
            Scene CurrentScene, Arr2D<double> OldDepthBuffer)
        {
            JaggedArr<double> BatchDepth = new JaggedArr<double>(StartY, StartXs, EndXs);
            JaggedArr<Colour> BatchFrame = new JaggedArr<Colour>(0, Array.Empty<int>(), Array.Empty<int>());
            (JaggedArr<Vec3D> Position, JaggedArr<Vec3D> Normal, JaggedArr<Colour> Albedo, JaggedArr<double> Specular,
               JaggedArr<Colour> Emissive) BatchDeferred = (new JaggedArr<Vec3D>(0, Array.Empty<int>(), Array.Empty<int>()),
               new JaggedArr<Vec3D>(0, Array.Empty<int>(), Array.Empty<int>()), new JaggedArr<Colour>(0, Array.Empty<int>(), Array.Empty<int>()),
               new JaggedArr<double>(0, Array.Empty<int>(), Array.Empty<int>()), new JaggedArr<Colour>(0, Array.Empty<int>(), Array.Empty<int>()));
            if (Mode != DEPTH_ONLY)
            {
                BatchFrame = new JaggedArr<Colour>(StartY, StartXs, EndXs);
                if (Mode == DEFERRED)
                {
                    BatchDeferred = (new JaggedArr<Vec3D>(StartY, StartXs, EndXs),
                        new JaggedArr<Vec3D>(StartY, StartXs, EndXs), new JaggedArr<Colour>(StartY, StartXs, EndXs),
                        new JaggedArr<double>(StartY, StartXs, EndXs), new JaggedArr<Colour>(StartY, StartXs, EndXs));
                }
            }
            for (int RowNum = 0; RowNum < EndY - StartY; RowNum++)
            {
                DrawSurfaceRow(Mode, StartXs[RowNum], EndXs[RowNum], RowNum + StartY, Resolution, ViewSpaceVerts, SurfaceNormals, VertexNormals,
                    WorldSpaceVerts, VertexRatios, CameraObj, Face, MasterMesh, CurrentScene, OldDepthBuffer, ref BatchDepth, ref BatchFrame,
                    ref BatchDeferred);
            }
            return (BatchDepth, BatchFrame, BatchDeferred);
        }

        public static void DrawSurfaceRow(int Mode, int StartX, int EndX, int RowY, int[] Resolution, TriVec3D ViewSpaceVerts, Vec3D[] SurfaceNormals,
            TriVec3D[] VertexNormals, TriVec3D WorldSpaceVerts, TriVec2D VertexRatios, Camera CameraObj, Surface Face, Mesh MasterMesh,
            Scene CurrentScene, Arr2D<double> OldDepthBuffer,
            ref JaggedArr<double> BatchDepth, ref JaggedArr<Colour> BatchFrame, ref (JaggedArr<Vec3D> Position,
            JaggedArr<Vec3D> Normal, JaggedArr<Colour> Albedo, JaggedArr<double> Specular, JaggedArr<Colour> Emissive) BatchDeferred)
        {
            double CurrentZ;
            TriVec2D ScreenSpaceVerts = ViewSpaceVerts.ToTriVec2D();
            for (int CurrentX = StartX; CurrentX < EndX; CurrentX++)
            {
                Vec2D ScreenSpacePos = new Vec2D(CurrentX, RowY);
                //Barycentric coordinates used to interpolate pixel's z-value
                Bary Coord = ScreenSpaceVerts.ComputeInternalBaryCoord(ScreenSpacePos);
                //Correct for perspective
                CurrentZ = Coord.PerspCorrectedZ(ViewSpaceVerts);
                Vec3D ViewSpacePos = new Vec3D(ScreenSpacePos.X, ScreenSpacePos.Y, CurrentZ);
                if (OldDepthBuffer[(int)Math.Round(ViewSpacePos.X), (int)Math.Round(ViewSpacePos.Y)] != 0 &&
                CurrentZ >= OldDepthBuffer[(int)Math.Round(ViewSpacePos.X), (int)Math.Round(ViewSpacePos.Y)])
                {
                    //Fail depth buffer check
                    continue;
                }
                Coord = Coord.PerspCorrect(CurrentZ, ViewSpaceVerts);
                Vec2D TextureSpacePos = Face.TextureSpacePos(MasterMesh, Coord);
                //Estimate for pixel derivatives
                Vec2D TexelDimensions = VertexRatios.BaryInterp(Coord);
                //If not using billinear filtering, the size of the averaged texel must be doubled
                if (Face.TextureFilteringMode != Surface.BILLINEAR)
                {
                    TexelDimensions = TexelDimensions.ScalarMult(2);
                }
                double TexelWidth = TexelDimensions.X;
                double TexelHeight = TexelDimensions.Y;
                Vec2D? UV = Face.WrapToUV(TextureSpacePos, TexelWidth, TexelHeight);
                if (UV == null)
                {
                    //Texel is discarded
                    continue;
                }
                if (Mode == DEPTH_ONLY)
                {
                    Colour PixelAlbedo = Face.Albedo(MasterMesh, UV, TextureSpacePos, TexelWidth, TexelHeight);
                    if (PixelAlbedo.Alpha != 0)
                    {
                        //Pixel is not fully transparent and so is not discarded
                        BatchDepth[(int)Math.Round(ViewSpacePos.X), (int)Math.Round(ViewSpacePos.Y)] = ViewSpacePos.Z;
                    }
                }
                else
                {
                    ITexelShiftShader[] TexelShiftShaders = MasterMesh.TexelShiftShaders;
                    for (int TexelShiftShaderNum = 0; TexelShiftShaderNum < TexelShiftShaders.Length; TexelShiftShaderNum++)
                    {
                        TexelShiftShaders[TexelShiftShaderNum].PerPixelShade(CameraObj, WorldSpaceVerts, SurfaceNormals, Face, MasterMesh,
                            CurrentScene, TexelWidth, TexelHeight, ref TextureSpacePos, ref UV, ref Coord);
                    }
                    Vec3D PixelPos = WorldSpaceVerts.BaryInterp(Coord);
                    Colour PixelAlbedo = Face.Albedo(MasterMesh, UV, TextureSpacePos, TexelWidth, TexelHeight);
                    if (PixelAlbedo.Alpha == 0)
                    {
                        //Pixel is fully transparent and so can be discarded (note that this will remove pixels that will be shaded later specularly, so transparent materials like glass must have alpha > 0)
                        continue;
                    }
                    Colour PixelColour = new Colour(0, 0, 0);
                    Colour PixelEmissive = Face.Emissive(MasterMesh, UV, TextureSpacePos, TexelWidth, TexelHeight);
                    Vec3D Normal = Face.Normal(MasterMesh, UV, TextureSpacePos, TexelWidth, TexelHeight);
                    Vec3D PixelNormal;
                    if (MasterMesh.SmoothShading)
                    {
                        //If smooth shading is enabled, interpolate between vertex normals
                        PixelNormal = (VertexNormals[0].BaryInterp(Coord).ScalarMult(Normal.Z) +
                            VertexNormals[1].BaryInterp(Coord).ScalarMult(Normal.X) +
                            VertexNormals[2].BaryInterp(Coord).ScalarMult(Normal.Y)).Normalise();
                    }
                    else
                    {
                        //Otherise, use the surface's normals
                        PixelNormal = SurfaceNormals[0].ScalarMult(Normal.Z) +
                            SurfaceNormals[1].ScalarMult(Normal.X) +
                            SurfaceNormals[2].ScalarMult(Normal.Y);
                    }
                    double PixelSpecular = Face.Specular(MasterMesh, UV, TextureSpacePos, TexelWidth, TexelHeight);
                    IPixelShader[] PixelShaders = MasterMesh.PixelShaders;
                    for (int PixelShaderNum = 0; PixelShaderNum < PixelShaders.Length; PixelShaderNum++)
                    {
                        // Runs pixel shaders
                        PixelShaders[PixelShaderNum].PerPixel(CameraObj, MasterMesh, CurrentScene, Resolution, (int)Math.Round(ViewSpacePos.X),
                            (int)Math.Round(ViewSpacePos.Y),
                            ref PixelColour, ref PixelPos, ref PixelNormal,
                            ref PixelAlbedo, ref PixelSpecular, ref PixelEmissive);
                    }
                    BatchFrame[(int)Math.Round(ViewSpacePos.X), (int)Math.Round(ViewSpacePos.Y)] = PixelColour;
                    BatchDepth[(int)Math.Round(ViewSpacePos.X), (int)Math.Round(ViewSpacePos.Y)] = ViewSpacePos.Z;
                    if (Mode == DEFERRED)
                    {
                        BatchDeferred.Position[(int)Math.Round(ViewSpacePos.X), (int)Math.Round(ViewSpacePos.Y)] = PixelPos;
                        BatchDeferred.Normal[(int)Math.Round(ViewSpacePos.X), (int)Math.Round(ViewSpacePos.Y)] = PixelNormal;
                        BatchDeferred.Albedo[(int)Math.Round(ViewSpacePos.X), (int)Math.Round(ViewSpacePos.Y)] = PixelAlbedo;
                        BatchDeferred.Specular[(int)Math.Round(ViewSpacePos.X), (int)Math.Round(ViewSpacePos.Y)] = PixelSpecular;
                        BatchDeferred.Emissive[(int)Math.Round(ViewSpacePos.X), (int)Math.Round(ViewSpacePos.Y)] = PixelEmissive;
                    }
                }
            }
        }
    }
}
