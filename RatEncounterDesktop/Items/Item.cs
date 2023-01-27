using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatEncounterDesktop.Items
{
    public partial class Item
    {
        protected string internal_id = "";
        public string Internal { get { return internal_id; } }
        
        protected string displayName = "";
        protected string description = "";
        public string DisplayName { get { return displayName; } }
        public string Description { get { return description; } }

        protected int itemRarityID = 0;
        protected int maxStackSize = -1;
        public int MaxStackSize { get { return maxStackSize; } }
        protected bool canUseItem = false;
        public bool CanUse { get { return canUseItem; } }
        protected bool removeWhenZero = true;

        public Item()
        {

        }

        public virtual Item Self()
        {
            return (Item)this.MemberwiseClone();
        }

        public virtual bool IsEqual(Item item)
        {
            if (item.internal_id == this.internal_id
                && item.displayName == this.displayName
                && item.canUseItem == this.canUseItem
                && item.itemRarityID == this.itemRarityID
                && item.description == this.description
                && item.maxStackSize == this.maxStackSize
                && item.removeWhenZero == this.removeWhenZero)
                return true;
            else
                return false;
        }

        public virtual int UseItem(ItemStack itemStack)
        {

            return 0;
        }

        public virtual object GetItemProperty(string propertyName)
        {
            switch (propertyName) //add more default stuff
            {
                case "displayName":
                    return this.displayName;
                case "internalID":
                    return this.internal_id;
                case "description":
                    return this.description;
                default:
                    return null;
            }
        }

        public virtual int[] GetImages()
        {
            return new int[] { 0 };
        }


        //public Bitmap Image()
        //{
        //    SQItemFamily thisFamily;
        //    if (Content.SQWorlditemFamilyList.TryGetValue(familyID, out thisFamily))
        //    {
        //        return thisFamily.GetItemImage(GetImages()[0]);
        //    }
        //    return new System.Drawing.Bitmap(32, 32);
        //}

    }
}
