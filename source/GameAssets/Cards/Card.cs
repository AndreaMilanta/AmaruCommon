﻿using System;

using AmaruCommon.GameAssets.Players;
using AmaruCommon.GameAssets.Cards.Properties;
using AmaruCommon.GameAssets.Characters;

namespace AmaruCommon.GameAssets.Cards
{
    [Serializable]
    public abstract class Card
    {
        // Static stuff for auto id
        private static int _absoluteId = 0;
        private static int NextId { get => _absoluteId++;}

        public CardEnum CardEnum;
        // Properties
        public Card Original { get => CardEnum.Card; }
        public int Id { get; set; }
        public int Cost { get; set; }
        public string Name { get; private set; }

        protected Card (int id, CardEnum cardEnum, string name, int cost)
        {
            if (id < 0)
                this.Id = NextId;
            else
                this.Id = id;
            this.CardEnum = cardEnum;
            this.Cost = cost;
            this.Name = name;
        }

        public virtual void Visit(PropertyVisitor visitor, CharacterEnum character, CardProperty property = null)
        {
            visitor.Owner = character;
            visitor.OwnerCard = this;
            if (property != null)
                property.Visit(visitor);
        }
    }
}
