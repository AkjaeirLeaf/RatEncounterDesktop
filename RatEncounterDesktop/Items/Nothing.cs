using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatEncounterDesktop.Items
{
    public class Nothing : Item
    {
        public Nothing()
        {
            internal_id = "";
            displayName = "";
            description = "";

            itemRarityID = 0;
            maxStackSize = -1;
            canUseItem = false;
            removeWhenZero = false;
        }
    }
}
