using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RatEncounterDesktop
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (GameWindow systemsWindow = new GameWindow(GameWindow.DEFAULT_WIDTH, GameWindow.DEFAULT_HEIGHT, "Rat Encounter : The Game"))
            {
                systemsWindow.Run(60.0);
                systemsWindow.Dispose();
            }
        }
    }
}
