﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.TheWanderingIsle
{
    public class IslandquakeCardController : TheWanderingIsleCardController
    {
        public IslandquakeCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowListOfCards(new LinqCardCriteria((Card c) => c.IsInPlay && this.IsHeroTargetWhoCausedTeryxToGainHpLastRound(c), "hero targets that have caused Teryx to regain HP", useCardsSuffix: false, useCardsPrefix: false, "hero target that has caused Teryx to regain HP", "hero targets that have caused Teryx to regain HP"));
        }

        public override void AddTriggers()
        {
            // At the start of the environment turn, this card deals each target other than Teryx 4 sonic damage.
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, this.DealDamageResponse, TriggerType.DealDamage);
            // Hero targets which caused Teryx to regain HP since the end of the last environment turn are immune to this damage.
            base.AddImmuneToDamageTrigger((DealDamageAction action) =>
                //damage initiated by this card's text, a.k.a. "this damage"
                action.CardSource.Card == base.Card && this.IsHeroTargetWhoCausedTeryxToGainHpLastRound(action.Target));
        }

        private IEnumerator DealDamageResponse(PhaseChangeAction pca)
        {
            //this card deals each target other than Teryx 4 sonic damage
            IEnumerator coroutine;
            coroutine = base.DealDamage(base.Card, (Card c) => c.Identifier != TeryxIdentifier, 4, DamageType.Sonic);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Then, this card is destroyed.
            coroutine = base.GameController.DestroyCard(this.DecisionMaker, base.Card, cardSource: base.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            yield break;
        }

        private bool IsHeroTargetWhoCausedTeryxToGainHpLastRound(Card card)
        {
            return card.IsHero && card.IsTarget &&
                base.GameController.Game.Journal.GainHPEntries()
                        .Any(e => e.Round == this.Game.Round && e.TargetCard.Identifier == TeryxIdentifier && e.SourceCard == card);
        }

    }
}
