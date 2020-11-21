﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.TheRam
{
    public class TheRamUtilityCharacterCardController : VillainCharacterCardController
    {
        public TheRamUtilityCharacterCardController(Card card, TurnTakerController ttc) : base(card, ttc)
        {
        }

        protected bool IsUpClose(Card c)
        {
            return c.IsTarget && IsUpClose(c.Owner);
        }

        protected bool IsUpClose(TurnTaker tt)
        {
            return tt.GetCardsWhere(HasUpCloseNextToCard).Count() > 0;
        }

        private bool HasUpCloseNextToCard(Card c)
        {
            if (c != null)
            {
                return c.NextToLocation.Cards.Where((Card nextTo) => nextTo.Identifier == "UpClose").Count() > 0;
            }
            return false;
        }

        protected IEnumerator PlayGivenUpCloseByGivenCard(Card upClose, Card target)
        {
            CardController upCloseController = FindCardController(upClose);
            if (upCloseController is UpCloseCardController)
            {
                IEnumerator play = (upCloseController as UpCloseCardController).PlayBySpecifiedHero(target, GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(play);
                }
                else
                {
                    GameController.ExhaustCoroutine(play);
                }
            }
            yield break;
        }
    }
}
