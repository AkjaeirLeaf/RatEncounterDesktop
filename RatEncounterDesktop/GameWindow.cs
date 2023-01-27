using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Security.Cryptography;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

using RatEncounterDesktop.Worlds;

namespace RatEncounterDesktop
{
    public class GameWindow : OpenTK.GameWindow
    {
        public static GameWindow ENV_RUNNER;
        public const int DEFAULT_WIDTH  = 360;
        public const int DEFAULT_HEIGHT = 620;

        private static bool render_ready = false;
        public static int CurrentBoundTexture = 0;

        // RENDER WORLDS
        // 0: Debug
        // 1: Main Menu?
        // 2: 
        public static MenuRenderWorld MenuWorld;

        public GameWindow(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
            ENV_RUNNER = this;
            InitOpenGLEvents();
            Content.AllocSetupAll();
            InitRenderWorld();

            render_ready = true;
        }

        private void InitOpenGLEvents()
        {
            // Add events here

            //KeyDown += SQ_KeyDown;
            //KeyPress += SQ_KeyPress;
            //MouseMove += SQ_MouseMove;
            //MouseWheel += SQ_MouseWheel;
            //MouseDown += SQ_MouseDown;
        }

        private void InitRenderWorld()
        {
            /*
            debug_world = new RenderWorld();
            RenderWorld.ErrorImage = new Texture2D("SoliaQuestClassic.Resources.Debug.errorImage.png", "error", debug_world);

            main_menu = new RenderWorld(Width, Height);
            planet_levels = new PlanetWorld();

            planet_levels.ResizeLimits(Width, Height);
            //LevelEditorWindow LEV = new LevelEditorWindow();
            //LEV.Show();

            character_screen = new CharacterWorld();
            */

            MenuWorld = new MenuRenderWorld();
        }

        protected override void OnLoad(EventArgs e)
        {
            //currentState = GameState.LOADING;
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            //GL.ClearColor(0.2f, 0.0f, 0.2f, 1.0f);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthClamp);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.Texture2D);

            base.OnLoad(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState input = Keyboard.GetState();

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Render Here
            if (render_ready)
            {
                MenuWorld.Render();
            }
            

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }

        private int MousePos_X = 0;
        private int MousePos_Y = 0;

        public static int WIN_Height { get { return m_height; } }
        public static int WIN_Width { get { return m_width; } }
        private static int m_height = DEFAULT_HEIGHT;
        private static int m_width = DEFAULT_WIDTH;

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            m_width = Width;
            m_height = Height;
            double scale = 1.0;
            if (((double)Width / DEFAULT_WIDTH) > ((double)Height / DEFAULT_HEIGHT))
                scale *= ((double)Height / DEFAULT_HEIGHT);
            else scale *= ((double)Width / DEFAULT_WIDTH);

            MenuWorld.ResizeLimits(Width, Height);

            base.OnResize(e);
        }
        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            base.OnUnload(e);
        }

    }
}
