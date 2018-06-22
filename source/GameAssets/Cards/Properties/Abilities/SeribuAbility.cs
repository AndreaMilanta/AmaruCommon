﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AmaruCommon.GameAssets.Cards.Properties.Abilities
{
    [Serializable]
    public class SeribuAbility : Ability
    {
        public SeribuAbility(int cost, int numTarget) : base(cost, numTarget)
        {
        }

        public override int Visit(PropertyVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
