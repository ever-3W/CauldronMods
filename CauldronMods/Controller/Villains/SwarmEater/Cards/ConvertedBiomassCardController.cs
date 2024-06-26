﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Linq;

namespace Cauldron.SwarmEater
{
    public class ConvertedBiomassCardController : CardController
    {
        public ConvertedBiomassCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            //This card and cards beneath it are indestructible
            base.AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            base.SpecialStringMaker.ShowNumberOfCardsUnderCard(base.Card);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            //This card and cards beneath it are indestructible
            return card == base.Card || card.Location == base.Card.UnderLocation;
        }

        public override void AddTriggers()
        {
            //Whenever {SwarmEater} destroys an environment target, put it beneath this card. Cards beneath this one have no game text.
            base.AddTrigger((DestroyCardAction action) => action.WasCardDestroyed && action.CardToDestroy.Card.IsEnvironmentTarget && action.ResponsibleCard == base.CharacterCard, this.DestroyEnvironmentResponse, TriggerType.MoveCard, TriggerTiming.After);
            
            //At the end of the villain turn, {SwarmEater} regains X times 2 HP, where X is the number of cards beneath this one.
            base.AddEndOfTurnTrigger(tt => tt == base.TurnTaker, this.GainHPResponse, new[] { TriggerType.GainHP });
        }

        private IEnumerator DestroyEnvironmentResponse(DestroyCardAction action)
        {
            //...put it beneath this card.
            action.SetPostDestroyDestination(base.Card.UnderLocation, flipFaceDown: true, cardSource: base.GetCardSource());
            yield break;
        }

        private IEnumerator GainHPResponse(PhaseChangeAction action)
        {
            //...{SwarmEater} regains X times 2 HP, where X is the number of cards beneath this one.
            IEnumerator coroutine = base.GameController.GainHP(base.CharacterCard, NumberOfCardsBeneathThis() * 2, () => NumberOfCardsBeneathThis() * 2, cardSource: base.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private int NumberOfCardsBeneathThis()
        {
            return (from card in base.Card.UnderLocation.Cards
                    select card).Count<Card>();
        }
    }
}