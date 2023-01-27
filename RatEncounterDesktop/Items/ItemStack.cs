using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatEncounterDesktop.Items
{
    public class ItemStack
    {
        private Item stackOf;
        private int stackCount = 0;
        private bool isFloating = false;
        private bool isEmpty = true;
        public bool IsFloatingStack { get { return isFloating; } set { isFloating = value; } }
        public bool IsEmptyStack { get { return isEmpty; } }
        public bool IsFullStack { get { if (stackCount < stackOf.MaxStackSize) { return false; } else if (stackCount >= stackOf.MaxStackSize) { return true; } return true; } }

        public Item StackItem { get { return stackOf; } }
        public int StackSize { get { return stackCount; } }
        public int StackMaxSize { get { return stackOf.MaxStackSize; } }

        public ItemStack()
        {
            stackOf = new Items.Nothing();
            isEmpty = true;
            stackCount = -1;
        }
        public static ItemStack Empty { get { return new ItemStack(); } }

        public ItemStack(Item item, int count, out ItemStack extra, bool floating = false)
        {
            stackOf = item.Self();
            isFloating = floating;

            if (count > 0 && count <= item.MaxStackSize)
            {
                stackCount = count;
                isEmpty = false;
                extra = Empty;
            }
            else if (count <= 0)
            {
                this.DisposeOf();
                extra = Empty;
            }
            else if (count > item.MaxStackSize && !floating)
            {
                stackCount = item.MaxStackSize;
                extra = new ItemStack(item, count - item.MaxStackSize, out _, floating);
            }
            else
            {
                extra = Empty;
            }
        }

        public void Replace(ItemStack stackAdd)
        {
            ItemStack stack = stackAdd.Grab();

            stackOf = stack.StackItem.Self();
            isFloating = false;

            if (stack.StackSize > 0 && stack.StackSize <= stack.StackItem.MaxStackSize)
            {
                stackCount = stack.StackSize;
                stack.DisposeOf();
            }
            else if (stack.StackSize <= 0 || stack.isEmpty)
            {
                this.DisposeOf();
                stack.DisposeOf();
            }
            else if (stack.StackSize > stack.StackItem.MaxStackSize && !stack.isFloating)
            {
                stackCount = stack.stackOf.MaxStackSize;
                ItemStack extra = new ItemStack(stack.StackItem, stack.StackSize - stackOf.MaxStackSize, out _, true);
                stack = extra;
            }
            else
            {

            }
            if (stackCount > 0) { isEmpty = false; } else { isEmpty = true; }
        }
        public void Combine(ItemStack stackAdd, out ItemStack extras)
        {
            ItemStack stack = stackAdd.Grab();

            if (stack.stackOf.IsEqual(this.stackOf))
            {
                isFloating = false; //? is this required?

                if (stack.StackSize > 0 && stack.StackSize <= stack.StackItem.MaxStackSize - this.stackCount)
                {
                    stackCount += stack.StackSize;
                    stack.DisposeOf();
                    extras = Empty;
                }
                else if (stack.StackSize <= 0 || stack.isEmpty)
                {
                    stack.DisposeOf();
                    extras = Empty;
                }
                else if (stack.StackSize > stack.StackItem.MaxStackSize - stackCount && !stack.isFloating)
                {
                    ItemStack extra = new ItemStack(stack.StackItem, stack.StackSize - (stackOf.MaxStackSize - stackCount), out _, true);
                    stackCount = stack.stackOf.MaxStackSize;
                    extras = extra.Grab();
                }
                else
                {
                    extras = new ItemStack();
                }
            }
            else { extras = stack; }//return whole stack because different items cannot be combined.
            if (stackCount > 0) { isEmpty = false; } else { isEmpty = true; }
        }
        public static ItemStack Combine(ItemStack stack1, ItemStack stack2)
        {
            ItemStack stack3;
            if (stack1.stackOf.IsEqual(stack2.stackOf))
            {
                stack3 = new ItemStack(stack1.stackOf, stack1.StackSize + stack2.StackSize, out _, true);
                stack1.DisposeOf();
                stack2.DisposeOf();
                if (stack3.StackSize > 0) { stack3.isEmpty = false; } else { stack3.isEmpty = true; }
                return stack3;
            }
            if (stack1.StackSize > 0) { stack1.isEmpty = false; } else { stack1.isEmpty = true; }
            if (stack2.StackSize > 0) { stack2.isEmpty = false; } else { stack2.isEmpty = true; }
            return new ItemStack();
        }

        /// <summary>
        /// <tooltip>Set the itemstack to mode "float," meaning the item max value is not capped.</tooltip>
        /// </summary>
        public void Float()
        {
            isFloating = true;
        }
        /// <summary>
        /// <tooltip>Limits the item cap of the stack, setting it back to the max. be sure to collect overflow of items not fit.</tooltip>
        /// </summary>
        /// <param name="extra"></param>
        public void Sink(out ItemStack extra)
        {
            if (stackCount <= stackOf.MaxStackSize)
            {
                //no need for extra
                extra = Empty;
                isFloating = false;
            }
            else
            {
                int overflow = stackCount - stackOf.MaxStackSize;
                extra = new ItemStack(stackOf, overflow, out _, true);
                stackCount = stackOf.MaxStackSize;
                isFloating = false;
            }
            if (stackCount == 0)
            {
                DisposeOf();
            }
        }

        //decreasing stack contents
        public int Decrease(int count, bool allOrNothing = true)
        {
            if (stackCount != -1 && stackOf.Internal != "")
            {
                if (canDecr(count))
                {
                    decr(count);
                    if (stackCount == 0) { DisposeOf(); }
                    return 1;
                }
                else
                {
                    if (allOrNothing)
                    {
                        return 0;
                    }
                    else
                    {
                        int i;
                        for (i = 0; i < count; i++)
                        {
                            if (!decr()) { break; }
                            if (stackCount == 0) { DisposeOf(); }
                        }
                        return i;
                    }
                }
            }
            else
            {
                return -8;
            }
        }
        private bool decr()
        {
            if (stackCount > 0) { stackCount--; return true; }
            else { return false; }
        }
        private bool decr(int count)
        {
            if (canDecr(count)) { stackCount -= count; return true; }
            else { return false; }
        }
        public bool canDecr(int count)
        {
            if (stackCount >= count) { return true; }
            else { return false; }
        }


        //increasing stack contents
        public int Increase(int count, bool allOrNothing = true)
        {
            if (stackCount != -1 && stackOf.Internal != "")
            {
                if (canIncr(count))
                {
                    incr(count);
                    return 1;
                }
                else
                {
                    if (allOrNothing)
                    {
                        return 0;
                    }
                    else
                    {
                        int i;
                        for (i = 0; i < count; i++)
                        {
                            if (!incr()) { break; }
                        }
                        return i;
                    }
                }
            }
            else
            {
                return -8;
            }
        }
        private bool incr()
        {
            if (stackCount > 0) { stackCount++; return true; }
            else { return false; }
        }
        private bool incr(int count)
        {
            if (canIncr(count)) { stackCount += count; return true; }
            else { return false; }
        }
        public bool canIncr(int count)
        {
            if (stackCount <= StackMaxSize - count) { return true; }
            else { return false; }
        }

        public ItemStack Grab()
        {
            ItemStack grabbedStack = (ItemStack)this.MemberwiseClone();
            this.DisposeOf();
            return grabbedStack;
        }


        public void DisposeOf()
        {
            stackOf = new Items.Nothing();
            stackCount = -1;
        }
    }
}
