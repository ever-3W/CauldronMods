﻿using System;
using System.Collections;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;


namespace Cauldron.Cypher
{
    public class RapidPrototypingCardController : CypherBaseCardController
    {
        //==============================================================
        // Draw 2 cards.
        // Play any number of Augments from your hand.
        //==============================================================

        public static string Identifier = "RapidPrototyping";

        private const int CardsToDraw = 2;

        public RapidPrototypingCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            // Draw 2 cards.
            IEnumerator routine = base.DrawCards(base.HeroTurnTakerController, CardsToDraw);

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(routine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(routine);
            }

            // Play any number of Augments from your hand
            routine = base.GameController.SelectAndPlayCardsFromHand(base.HeroTurnTakerController, 40, 
                false, 0, new LinqCardCriteria(IsAugment));

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(routine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(routine);
            }
        }
    }
}