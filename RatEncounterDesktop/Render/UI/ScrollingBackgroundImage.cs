using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirali.Light;
using Kirali.MathR;

namespace RatEncounterDesktop.Render.UI
{
    public class ScrollingBackgroundImage : UIObject
    {
        public ScrollingBackgroundImage()
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

        public override void Render(Camera MainCamera)
        {
            
        }
    }
}
