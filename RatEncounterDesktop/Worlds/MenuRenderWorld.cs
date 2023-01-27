using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using RatEncounterDesktop.Render;
using RatEncounterDesktop.Render.UI;

namespace RatEncounterDesktop.Worlds
{
    public class MenuRenderWorld : RenderWorld
    {
        public static int MenuWorldID { get { return 1; } }

        // DECLARE UI OBJECTS
        private static UIObject[] UI_Objects = new UIObject[0];

        // INVENTORY
        private static BackgroundImage bg_colorCycle; 
        private static double bg_colCyc_H = 0; private static double bg_colCyc_dispbottom = 30;
        private static BackgroundImage bg_scrollingBackpacks;

        private static TextLabel TextTest;

        // IMAGE/TEXTURES
        private static Texture2D texture_panelblank;
        private static Texture2D texture_scrollingBackpacks;

        public override void Init()
        {
            MainCamera = new Kirali.Light.Camera(new Kirali.MathR.Vector3(0, 0, 0), Kirali.MathR.Vector3.Zero, 
                GameWindow.DEFAULT_WIDTH, GameWindow.DEFAULT_HEIGHT);

            // LOAD IMAGES
            texture_panelblank         = Texture2D.RegisterNew(ResourcePath + "UI.containter.panel_blank.png", "panel_blank", this);
            texture_scrollingBackpacks = Texture2D.RegisterNew(ResourcePath + "UI.backgrounds.scrolling_backpacks.png", "bg_scrolling_backpacks", this);

            InitUIObjects();
        }

        private void InitUIObjects()
        {
            // INVENTORY
            bg_colorCycle = new BackgroundImage();
            bg_colorCycle.SetBackgroundImage(texture_panelblank);
            bg_colorCycle.MultiTint = new Color[4];
            bg_colorCycle.MultiTint[0] = Color.FromArgb(84, 255, 158);
            bg_colorCycle.MultiTint[1] = Color.FromArgb(84, 255, 158);
            bg_colorCycle.MultiTint[2] = Color.FromArgb(84, 255, 255);
            bg_colorCycle.MultiTint[3] = Color.FromArgb(84, 255, 255);
            AddUIObject(bg_colorCycle);

            bg_scrollingBackpacks = new BackgroundImage();
            bg_scrollingBackpacks.SetBackgroundImage(texture_scrollingBackpacks);
            bg_scrollingBackpacks.SetRotation(-0.6);
            bg_scrollingBackpacks.SetScale(1.3);
            //bg_scrollingBackpacks.SetAlpha(0.6);
            AddUIObject(bg_scrollingBackpacks);

            BackgroundImage mute_colbg = new BackgroundImage();
            mute_colbg.SetBackgroundImage(texture_panelblank);
            mute_colbg.SetAlpha(0.4);
            AddUIObject(mute_colbg);


            TextTest = new TextLabel(0, 40, new Point(20, MainCamera.Height - 50), Color.White, MainCamera);
            TextTest.Text = "THIS is a test, a rat text test";
            AddUIObject(TextTest);
        }

        public override void OnInitComplete()
        {

        }

        public override void Render()
        {
            // render background image
            //BackgroundImage.Render(Camera3D);

            // Update elements
            bg_scrollingBackpacks.Move(new Kirali.MathR.Vector2(0.0013, 0.002) * 0.5);

            double col_sat = 0.8;
            bg_colorCycle.MultiTint[0] = UIObject.ColorFromHSV(bg_colCyc_H, col_sat, 1);
            bg_colorCycle.MultiTint[1] = bg_colorCycle.MultiTint[0];
            bg_colorCycle.MultiTint[2] = UIObject.ColorFromHSV(bg_colCyc_H + bg_colCyc_dispbottom, col_sat, 1);
            bg_colorCycle.MultiTint[3] = bg_colorCycle.MultiTint[2];
            bg_colCyc_H += 0.5;



            // Render UI objects
            if (UI_Objects.Length > 0)
            {
                for (int ix = 0; ix < UI_Objects.Length; ix++) { UI_Objects[ix].Render(MainCamera); }
            }
        }

        public void Tick()
        {

        }

        private static void AddUIObject(UIObject ui_obj)
        {
            if (UI_Objects.Length == 0) { UI_Objects = new UIObject[] { ui_obj }; }
            else
            {
                UIObject[] temp = new UIObject[UI_Objects.Length + 1];
                for (int ix = 0; ix < UI_Objects.Length; ix++)
                {
                    temp[ix] = UI_Objects[ix];
                }
                temp[UI_Objects.Length] = ui_obj;
                UI_Objects = temp;
            }
        }
    }
}
