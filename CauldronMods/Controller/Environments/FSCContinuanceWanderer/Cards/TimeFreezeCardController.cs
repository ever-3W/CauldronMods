﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cauldron.FSCContinuanceWanderer
{
    public class TimeFreezeCardController : CardController
    {
        //This card is very fragile, test changes carefully.
        // Tracks if we're currently in the middle of a call to AskIfTurnTakerOrderShouldBeChanged,
        // to prevent recursion.
        private bool currentlyChangingTurnOrder = false;

        public TimeFreezeCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.ChangesTurnTakerOrder);
        }
        private TurnTaker FrozenTurnTaker
        {
            get
            {
                if (this.Card.IsInPlayAndHasGameText)
                {
                    var frozenTurnTaker = GetCardThisCardIsNextTo()?.Location.HighestRecursiveLocation.OwnerTurnTaker;
                    if (frozenTurnTaker != null && IsHero(frozenTurnTaker))
                    {
                        return frozenTurnTaker;
                    }
                }
                return null;
            }
        }

        // The algorithm here:
        // - Ensure we are asked for the next turntaker before any other cardcontroller by setting our askpriority very high.
        // - When we get asked, ask every other card controller who they think should go first
        // - If they say the turntaker we've frozen is up next, go back and ask again as if the turn order has just moved on
        // - Return the answer we get.
        // - If we don't return null the engine won't go on to ask anyone else; we've taken full control
        //   of the process while still respecting what other cards think.
        // - This should seamlessly handle a lot of other turn-order-changing cards.
        // - We include protection against recursive calls just in case someone else is doing something similar.

        public override int AskPriority => 100;

        public override TurnTaker AskIfTurnTakerOrderShouldBeChanged(TurnTaker fromTurnTaker, TurnTaker toTurnTaker)
        {
            if (FrozenTurnTaker == null)
                return null;

            // We've recursed; this can only happen if another cardcontroller is doing something similar.
            // We should return null just so everything bottoms out in an answer eventually.
            if (currentlyChangingTurnOrder)
                return null;

            currentlyChangingTurnOrder = true;

            TurnTaker next = fromTurnTaker;
            do
            {
                next = GameController.FindNextAfterTurnTaker(next, cc => cc != this || cc.AskPriority <= AskPriority);
            } while (next == FrozenTurnTaker);

            currentlyChangingTurnOrder = false;

            return next;
        }

        public override void AddTriggers()
        {
            //...and targets in their play are are immune to damage.
            base.AddImmuneToDamageTrigger((DealDamageAction action) => action.Target.Location.HighestRecursiveLocation == GetCardThisCardIsNextTo()?.Location.HighestRecursiveLocation);
            //At the start of the environment turn, destroy this card.
            base.AddStartOfTurnTrigger((TurnTaker turnTaker) => turnTaker == base.TurnTaker, base.DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            //Play this card next to a hero.
            IEnumerator coroutine = base.SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) =>  IsHeroCharacterCard(c) && !c.IsIncapacitatedOrOutOfGame && GameController.IsCardVisibleToCardSource(c, GetCardSource())), storedResults, true, decisionSources);
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
    }
}