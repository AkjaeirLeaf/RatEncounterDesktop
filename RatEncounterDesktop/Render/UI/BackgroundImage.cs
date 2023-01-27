using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirali.Light;
using Kirali.MathR;

namespace RatEncounterDesktop.Render.UI
{
    public class BackgroundImage : UIObject
    {
        public BackgroundImage()
        {
            draw_object_bounds = new Vector2[]
            {
                new Vector2(-1,  1),
                new Vector2( 1,  1),
                new Vector2( 1, -1),
                new Vector2(-1, -1)
            };
        }

        public void SetBackgroundImage(Texture2D texture)
        {
            object_Textures = new Texture2D[] { texture };
        }

        private Vector2 displace_pos = Vector2.Zero;
        public void Move(Vector2 vec)
        {
            displace_pos += vec;
        }

        private double displace_rot = 0;
        public void SetRotation(double radians)
        {
            displace_rot = radians;
        }

        private double displace_scale = 1;
        public void SetScale(double scale)
        {
            displace_scale = scale;
        }

        private double alpha = 1.0;
        public void SetAlpha(double a)
        {
            alpha = a; Tint = System.Drawing.Color.FromArgb((int)(a * 255), Tint.R, Tint.G, Tint.B);
        }

        public override void Render(Camera MainCamera)
        {
            if (object_Textures.Length > 0)
            {
                //create tile
                double r = (double)GameWindow.WIN_Width / GameWindow.WIN_Height;
                double s = displace_scale;
                //double cos = Math.Cos(displace_rot);
                //double sin = Math.Sin(displace_rot);
                Vector2[] tile_vex = new Vector2[4];
                tile_vex[0] = Vector2.Rotate(new Vector2(0 * r - displace_pos.X, 0  - displace_pos.Y), displace_rot);
                tile_vex[1] = Vector2.Rotate(new Vector2(s * r - displace_pos.X, 0  - displace_pos.Y), displace_rot);
                tile_vex[2] = Vector2.Rotate(new Vector2(s * r - displace_pos.X, s  - displace_pos.Y), displace_rot);
                tile_vex[3] = Vector2.Rotate(new Vector2(0 * r - displace_pos.X, s  - displace_pos.Y), displace_rot);

                TextureTile tee = new TextureTile(tile_vex);


                //draw
                if (MultiTint.Length == 0)
                {
                    object_Textures[0].Draw(draw_object_bounds, Tint, tee);
                }
                else if (MultiTint.Length == 4)
                {
                    object_Textures[0].Draw(draw_object_bounds, MultiTint, tee);
                }
            }
            else
            {
                //UIObject.RenderUIGeometry(MainCamera, Tint, main_panelbody);
            }
        }
    }
}
