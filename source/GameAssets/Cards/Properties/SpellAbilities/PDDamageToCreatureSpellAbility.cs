﻿using AmaruCommon.GameAssets.Cards.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AmaruCommon.GameAssets.Cards.Properties.SpellAbilities
{

    [Serializable]
    public class PDDamageToCreatureSpellAbility : SpellAbility
    {
        public int PDDamage { get; private set; }
        public PDDamageToCreatureSpellAbility(int PDDamage = 3) : base(true,1, Constants.KindOfTarget.CREATURE)
        {
            this.PDDamage = PDDamage;
        }
        public override int Visit(PropertyVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}