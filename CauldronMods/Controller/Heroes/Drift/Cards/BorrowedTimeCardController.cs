﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.Drift
{
    public class BorrowedTimeCardController : DriftUtilityCardController
    {
        public BorrowedTimeCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            //Select {DriftL} or {DriftR}. Shift that direction up to 3 times. X is the number of times you shifted this way.
            //If you shifted at least {DriftL} this way, X targets regain 2 HP each. If you shifted {DriftR} this way, {Drift} deals X targets 3 radiant damage each.
            IEnumerator coroutine = base.SelectAndPerformFunction(base.HeroTurnTakerController, new Function[] {
                    new Function(base.HeroTurnTakerController, "Drift Left", SelectionType.RemoveTokens, () => this.ShiftResponse(0)),
                    new Function(base.HeroTurnTakerController, "Drift Right", SelectionType.AddTokens, () => this.ShiftResponse(1))
            });
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

        private IEnumerator ShiftResponse(int response)
        {
            //Shift that direction up to 3 times. X is the number of times you shifted this way.
            List<SelectNumberDecision> numberDecision = new List<SelectNumberDecision>();
            IEnumerator coroutine = base.GameController.SelectNumber(base.HeroTurnTakerController, SelectionType.SelectNumeral, 0, 3, storedResults: numberDecision, cardSource: base.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            int selectedNumber = numberDecision.FirstOrDefault().SelectedNumber ?? default;
            for (int i = 0; i < selectedNumber; i++)
            {
                //{DriftL}
                if (response == 0)
                {
                    coroutine = base.ShiftL();
                }
                //{DriftR}
                else
                {
                    coroutine = base.ShiftR();
                }
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            //{DriftL}
            if (response == 0)
            {
                //If you shifted at least {DriftL} this way, X targets regain 2 HP each.
                coroutine = base.GameController.SelectAndGainHP(base.HeroTurnTakerController, 2, numberOfTargets: base.TotalShifts, cardSource: base.GetCardSource());
            }
            //{DriftR}
            else
            {
                //If you shifted {DriftR} this way, {Drift} deals X targets 3 radiant damage each.
                coroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.GetActiveCharacterCard()), 3, DamageType.Radiant, base.TotalShifts, false, selectedNumber, cardSource: base.GetCardSource());
            }
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
