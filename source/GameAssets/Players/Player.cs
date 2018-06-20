﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
    public class Player
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
         
        public Player(CharacterEnum character)
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
            return ((List<Card>)_cardDict[place]).Find(c => c.Id == id);
        }

        public CreatureCard PlayACreatureFromHand(int id, Place z)
        {
            // Debug.Log(ManaLeft);
            // Debug.Log(playedCard.CurrentManaCost);
            CreatureCard creature = (CreatureCard) GetCardFromId(id, Place.HAND);
            Mana -= creature.Cost;
            // Debug.Log("Mana Left after played a creature: " + ManaLeft);
            // create a new creature object and add it to Table

            if (z == Place.INNER)
                Inner.Add(creature);
            else
                Outer.Add(creature);
            
            //new PlayACreatureCommand(playedCard, this, z, tablePos, newCreature.UniqueCreatureID).AddToQueue();
            // cause battlecry Effect
            //if (newCreature.effect != null)
            //    newCreature.effect.WhenACreatureIsPlayed();
            // remove this card from hand
            Hand.Remove(creature);

            return creature;
        }

        public SpellCard PlayASpellFromHand(int id, List<Target> targets)
        {
            SpellCard spell = (SpellCard)GetCardFromId(id, Place.HAND);
            Mana -= spell.Cost;
            // cause effect instantly:
            /*if ((spell.Effect != null)
                spell.Effect.Visit(playedCard.ca.specialSpellAmount, target);
            else {
                Debug.LogWarning("No effect found on card " + playedCard.ca.name);
            }
            */
            // no matter what happens, move this card to PlayACardSpot
           
            // remove this card from hand
            Hand.Remove(spell);
            // check if this is a creature or a spell
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