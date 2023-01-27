using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kirali.Framework;

//using RatEncounterDesktop.IO;
//using RatEncounterDesktop.Items;
using RatEncounterDesktop.Languages;
using RatEncounterDesktop.Render;
using RatEncounterDesktop.Worlds;

namespace RatEncounterDesktop
{
    public class Content
    {
        

        public Content()
        {

        }

        /// <summary>
        /// <tooltip>Load and Register Game Content</tooltip>
        /// </summary>
        public static void AllocSetupAll()
        {
            //WriteAllResources();

            //Register Languages
            English_Common.RegisterLanguage();



        }


        public static Language[] languageList = new Language[0];
        public static int Register(Language lang)
        {
            try
            {
                languageList = ArrayHandler.append(languageList, lang);
                return languageList.Length - 1;
            }
            catch
            {
                return -1;
            }
        }
    }
}
