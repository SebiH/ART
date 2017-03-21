using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Core
{
    public class WaitForAvailableTicks : YieldInstruction
    {
        public int Amount;

        public WaitForAvailableTicks()
        {

        }

        public WaitForAvailableTicks(int amount)
        {
            Amount = amount;
        }
    }
}
