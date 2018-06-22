﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Logging;
using AmaruCommon.Exceptions;
using AmaruCommon.Constants;
using AmaruCommon.GameAssets.Cards;
using AmaruCommon.GameAssets.Characters;
using AmaruCommon.GameAssets.Players;
using AmaruCommon.GameAssets.Cards.Properties.Abilities;
using AmaruCommon.GameAssets.Cards.Properties.SpellAbilities;
using System.Linq;
using AmaruCommon.Actions.Targets;

namespace AmaruCommon.GameAssets.Players
{
    public class Player : Loggable
    {
        private int _mana = 0;
        // Properties
        public int Mana { get => _mana; set => _mana = value > AmaruConstants.MAX_MANA ? AmaruConstants.MAX_MANA : value; }
        public int Health { get; set; } = AmaruConstants.INITIAL_PLAYER_HEALTH;
        public bool IsShieldUpProtected { get => Outer.Exists(o => o.Shield == Shield.SHIELDUP || o.Shield == Shield.BOTH); }
        public bool IsShieldMaidenProtected { get => Outer.Exists(o => o.Shield == Shield.SHIELDMAIDEN || o.Shield == Shield.BOTH); }
        public bool IsAlive { get => Health > 0; }
        public bool InnerAttackAllowed { get { return (!PlayedSpell.Any() ? false : PlayedSpell.Exists(ps => ps.Effect is AttackFromInnerSpellAbility)); } }  // Effect of played spell card
        public bool IsImmune { get; set; } = false;
        public bool HasChanged { get; set; } = false;           // Wheter player has changed or not

        private int _initialTurnMana = 0;

        public int ManaPlayedCount { get => this.Mana - _initialTurnMana; }

        // Cards
        public Stack<Card> Deck { get; private set; } = null;
        public LimitedList<Card> Hand { get; private set; } = new LimitedList<Card>(AmaruConstants.HAND_MAX_SIZE);
        public LimitedList<CreatureCard> Inner { get; private set;} = new LimitedList<CreatureCard>(AmaruConstants.INNER_MAX_SIZE);
        public LimitedList<CreatureCard> Outer { get; private set; } = new LimitedList<CreatureCard>(AmaruConstants.OUTER_MAX_SIZE);
        public List<Card> Graveyard { get; private set; } = new List<Card>();
        public List<SpellCard> PlayedSpell { get; private set; } = null;
        private ReadOnlyDictionary<Place, IEnumerable<Card>> _cardDict; 

        // Communication
        public EnemyInfo AsEnemy { get => new EnemyInfo(Character, Health, Mana, Deck.Count, Hand.Count, Inner, Outer, PlayedSpell, IsImmune); }
        public OwnInfo AsOwn { get => new OwnInfo(Character,  Health, Mana, Deck.Count, Hand, Inner, Outer, PlayedSpell, IsImmune); }

        public CharacterEnum Character { get; private set; } = CharacterEnum.INVALID;
         
        public Player(CharacterEnum character, string logger) : base (logger)
        {
            Character = character;
            Deck = new Stack<Card>(DeckFactory.GetDeck(Character));
            // Initializes readonly dict
            _cardDict = new ReadOnlyDictionary<Place, IEnumerable<Card>>(new Dictionary<Place, IEnumerable<Card>>(){
                    {Place.DECK, Deck},
                    {Place.HAND, Hand},
                    {Place.INNER, Inner},
                    {Place.OUTER, Outer},
                    {Place.GRAVEYARD, Graveyard},
                });
        }
        public Player (Player p) : base ( "AiLogger")
        {
            this.Deck = new Stack<Card>(p.Deck);
            this.Hand = new LimitedList<Card>(p.Hand);
            this.Outer = new LimitedList<CreatureCard>(p.Outer);
            this.Inner = new LimitedList<CreatureCard>(p.Inner);
            this.Graveyard = new List<Card>(p.Graveyard);
            this.Character = p.Character;
            // Initializes readonly dict
            _cardDict = new ReadOnlyDictionary<Place, IEnumerable<Card>>(new Dictionary<Place, IEnumerable<Card>>(){
                    {Place.DECK, Deck},
                    {Place.HAND, Hand},
                    {Place.INNER, Inner},
                    {Place.OUTER, Outer},
                    {Place.GRAVEYARD, Graveyard},
                });
        }

        public Card Draw()
        {
            if (Deck.Count == 0)
            {
                Health--;
                return null;
            }
            if (Hand.Count == AmaruConstants.HAND_MAX_SIZE)
                return null;
            Card drawnCard = Deck.Pop();
            Hand.Add(drawnCard);
            return drawnCard;
        }

        public List<Card> Draw(int amount)
        {
            List<Card> drawnCards = new List<Card>();
            for (int i = 0; i < amount; i++)
                drawnCards.Add(this.Draw());
            return drawnCards;
        }

        /// <summary>
        /// Returns Card with given id in given place if exists, null otherwise
        /// </summary>
        /// <param name="id"></param>
        /// <param name="place">Place.DECK not allowed</param>
        /// <returns></returns>
        public Card GetCardFromId(int id, Place place)
        {
            if (place == Place.DECK)
                throw new InvalidSearchLocation();

            Card card = null;

            if(place == Place.INNER || place == Place.OUTER)
                    card = ((List<CreatureCard>)_cardDict[place]).Find(c => c.Id == id);
            if (place == Place.HAND)
                    card = ((List<Card>)_cardDict[place]).Find(c => c.Id == id);

            return card;
        }

        public CreatureCard PlayACreatureFromHand(int id, Place z)
        {
            CreatureCard creature = (CreatureCard) GetCardFromId(id, Place.HAND);
            Mana -= creature.Cost;
            if (z == Place.INNER)
                Inner.Add(creature);
            else
                Outer.Add(creature);
            Hand.Remove(creature);
            creature.Energy++;
            return creature;
        }

        public SpellCard PlayASpellFromHand(int id)
        {
            SpellCard spell = (SpellCard)GetCardFromId(id, Place.HAND);
            Log("Played SpellCard is " + spell == null ? "null" : spell.Name);
            Mana -= spell.Cost;
            // remove this card from hand
            Hand.Remove(spell);
            return spell;
        }

        public CreatureCard MoveACreatureFromPlace(int id, Place z)
        {
            CreatureCard creature;
            if (z == Place.INNER) {
                creature = (CreatureCard) GetCardFromId(id, Place.OUTER);
                Inner.Add(creature);
                Outer.Remove(creature);
            }
            else {
                creature = (CreatureCard)GetCardFromId(id, Place.INNER);
                Outer.Add(creature);
                Inner.Remove(creature);
            }

            return creature;
        }

        public void ResetManaCount()
        {
            _initialTurnMana = this.Mana;
        }
    }
}
