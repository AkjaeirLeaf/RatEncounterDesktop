using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RatEncounterDesktop.Render;
using RatEncounterDesktop.Worlds;

namespace RatEncounterDesktop.Languages
{
    public class English_Common : Language
    {
        public English_Common()
        {
            language_internal = "en_us";
            register_id = 0;
            //glyphs_call = ("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789").ToCharArray();
            lettersCanCapitalize = true;
            supportTinyLetters = false;
            lettex_ROWS_COUNT = 5; // letter glyph textures size
            lettex_COLS_COUNT = 14;
            symtex_ROWS_COUNT = 6;
            symtex_COLS_COUNT = 11;

            //load textures!
            Texture2D.RegisterNew(RenderWorld.ResourcePath + "Language.english.glyphset_0.png", 0, this);
            Texture2D.RegisterNew(RenderWorld.ResourcePath + "Language.english.glyphset_1.png", 1, this);
            Texture2D.RegisterNew(RenderWorld.ResourcePath + "Language.english.glyphset_2.png", 2, this);


            //texture_library = new Texture2D[5];
            BuildGlyphsets_Letters("ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÄÆÃÅĀÈÉÊËĒĖĘÌĮĪÍÏÎŁŃÑÕŌØŒÓĠÒÖÔŚŠŪÚÙÜÛŸŽŹŻ");
            BuildGlyphsets_SymbolsNumeric("0123456789-–—•/\\:;()$₽¥€¢£₩&§@\"«»„“”.…,?¿!¡'`‘’[]{}#%‰^*+=≈≠_|~<>");
        }


        public static void RegisterLanguage()
        {
            Content.Register(new English_Common());
        }
    }
}
