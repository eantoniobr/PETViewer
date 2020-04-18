﻿using Common;
using Common.WIP;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;
using static Common.Camera;

namespace GUI
{
    public class Window : GameWindow
    {
        private Shader _ourShader;
        private Model _ourModel;

        // We need an instance of the new camera class so it can manage the view and projection matrix code
        // We also need a boolean set to true to detect whether or not the mouse has been moved for the first time
        // Finally we add the last position of the mouse so we can calculate the mouse offset easily
        private Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastPos;

        private double _time;

        public Window(int width, int height, double updateFrequency, string title) : base(
            new GameWindowSettings {UpdateFrequency = updateFrequency},
            new NativeWindowSettings {Size = new Vector2i {X = width, Y = height}, Title = title}
        ) { }

        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            _ourShader = new Shader("Shaders/shader.vert",
                "Shaders/shader.frag");

            _ourModel = new Model("Resources/test_models/item0_01.pet");

            // We initialize the camera so that it is 3 units back from where the rectangle is
            // and give it the proper aspect ratio
            _camera = new Camera(Vector3.UnitZ * 3)
            {
                AspectRatio = ClientSize.X / (float) ClientSize.Y
            };
            // We make the mouse cursor invisible so we can have proper FPS-camera movement
            CursorVisible = false;
            CursorGrabbed = true;

            base.OnLoad();
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _time += 4.0 * e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _ourShader.Use();

            _ourShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            _ourShader.SetMatrix4("view", _camera.GetViewMatrix());

            // var model = Matrix4.Identity * Matrix4.CreateRotationX((float) MathHelper.DegreesToRadians(time));
            var model = Matrix4.Identity
                        * Matrix4.CreateTranslation(new Vector3(0f, -1.75f, 0f))
                        * Matrix4.CreateScale(.2f);
            _ourShader.SetMatrix4("model", model);

            _ourModel.Draw(_ourShader);

            SwapBuffers();

            base.OnRenderFrame(e);
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // TODO compare commented OnMouseMove
            // if (!IsFocused) // check to see if the window is focused
            // {
            //     return;
            // }

            var input = KeyboardState;

            if (input.IsKeyDown(Key.Escape))
            {
                Close();
            }

            if (input.IsKeyDown(Key.W))
                _camera.ProcessKeyboard(CameraMovement.Forward, (float) e.Time);
            if (input.IsKeyDown(Key.S))
                _camera.ProcessKeyboard(CameraMovement.Backward, (float) e.Time);
            if (input.IsKeyDown(Key.A))
                _camera.ProcessKeyboard(CameraMovement.Left, (float) e.Time);
            if (input.IsKeyDown(Key.D))
                _camera.ProcessKeyboard(CameraMovement.Right, (float) e.Time);
            if (input.IsKeyDown(Key.Space))
                _camera.ProcessKeyboard(CameraMovement.Up, (float) e.Time);
            if (input.IsKeyDown(Key.LShift))
                _camera.ProcessKeyboard(CameraMovement.Down, (float) e.Time);

            // Get the mouse state
            var mouse = MouseState;

            if (_firstMove) // this bool variable is initially set to true
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.ProcessMouseMovement(deltaX, deltaY);
            }

            base.OnUpdateFrame(e);
        }


        // This function's main purpose is to set the mouse position back to the center of the window
        // every time the mouse has moved. So the cursor doesn't end up at the edge of the window where it cannot move
        // further out
        // TODO the window doesn't get "focused" when clicking on it, only after changing between windows, but then the mouse is messed up
        // works when settings CursorGrabbed=true and not checking for focus at all even in keyboard
        // protected override void OnMouseMove(MouseMoveEventArgs e)
        // {
        //     if (IsFocused) // check to see if the window is focused
        //     {
        //         MousePosition = new Vector2(ClientSize.X / 2f, ClientSize.Y / 2f);
        //     }
        //
        //     base.OnMouseMove(e);
        // }


        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _camera.ProcessMouseScroll(e.OffsetY);

            base.OnMouseWheel(e);
        }


        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            _camera.AspectRatio = ClientSize.X / (float) ClientSize.Y;

            base.OnResize(e);
        }


        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            _ourShader.Dispose();

            base.OnUnload();
        }
    }
}