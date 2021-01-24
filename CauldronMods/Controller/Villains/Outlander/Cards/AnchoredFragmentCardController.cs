﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.Outlander
{
    public class AnchoredFragmentCardController : OutlanderUtilityCardController
    {
        public AnchoredFragmentCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            //When this card enters play, {Outlander} deals the hero target with the highest HP 1 melee damage.
            IEnumerator coroutine = base.DealDamageToHighestHP(base.CharacterCard, 1, (Card c) => c.IsHero, (Card c) => 1, DamageType.Melee);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }

        public override void AddTriggers()
        {
            //At the start of the villain turn, if {Outlander} was not dealt at least {H} times 2 damage in the last round, destroy {H} hero ongoing and/or equipment cards.
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, this.MaybeDestroyCardsResponse, TriggerType.DestroyCard);
        }

        private IEnumerator MaybeDestroyCardsResponse(PhaseChangeAction action)
        {
            //...if {Outlander} was not dealt at least {H} times 2 damage in the last round, 
            if (base.Journal.DealDamageEntriesThisTurn().Where((DealDamageJournalEntry entry) => entry.TargetCard == base.CharacterCard && entry.Amount > 0).Sum((DealDamageJournalEntry entry) => entry.Amount) < base.Game.H * 2)
            {
                //...destroy {H} hero ongoing and/or equipment cards.
                IEnumerator coroutine = base.GameController.SelectAndDestroyCards(base.DecisionMaker, new LinqCardCriteria((Card c) => c.IsHero && (c.IsOngoing || base.IsEquipment(c))), base.Game.H, cardSource: base.GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
            }
            yield break;
        }
    }
}
