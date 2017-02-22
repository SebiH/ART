using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Core
{
    public class WaitForAvailableCycles : YieldInstruction
    {
        public int Amount;

        public WaitForAvailableCycles()
        {

        }

        public WaitForAvailableCycles(int amount)
        {
            Amount = amount;
        }
    }
}
