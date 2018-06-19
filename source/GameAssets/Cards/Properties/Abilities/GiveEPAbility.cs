﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmaruCommon.GameAssets.Cards.Properties.Abilities
{
    [Serializable]
    public class GiveEPAbility : Ability
    {
        public int Ep { get; private set; }
    
        protected GiveEPAbility(int cost, int Ep) : base(cost)
        {
            this.Ep = Ep;
        }

        public override int Visit(IPropertyVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}