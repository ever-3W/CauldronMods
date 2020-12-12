﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.TheRam
{
    public class TheRamTurnTakerController : TurnTakerController
    {
        public TheRamTurnTakerController(TurnTaker tt, GameController gameController) : base(tt, gameController)
        {
        }

        public override IEnumerator StartGame()
        {
            if (CharacterCardController is TheRamCharacterCardController)
            {
                //"At the start of the game, put {TheRam}'s villain character cards into play, “Mechanical Juggernaut” side up.",

                IEnumerator coroutine = HandleWinters(banish: true);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }

                //"Search the villain deck for all copies of Up Close and put them in the trash. 
                coroutine = MoveUpCloseToTrash();
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }

                //Put Grappling Claw into play. Shuffle the villain deck."
                coroutine = GameController.PlayCard(this, FindCardsWhere((Card c) => c.Identifier == "GrapplingClaw").FirstOrDefault(), cardSource: new CardSource(CharacterCardController));
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }

                var shuffleAction = new ShuffleCardsAction(new CardSource(CharacterCardController), this.TurnTaker.Deck);
                coroutine = GameController.DoAction(shuffleAction);
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
            }
            else
            {
                //"At the start of the game, put {TheRam} and {AdmiralWinters}' villain character cards into play, “Amphibious Dreadnought” and “Dreadnought Pilot” sides up. 

                IEnumerator coroutine = HandleWinters(banish: false);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
                //Search the villain deck for all copies of Up Close and put them in the trash. 
                coroutine = MoveUpCloseToTrash();
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }

                //Put 2 copies of Remote Mortar into play. Shuffle the villain deck."
                coroutine = PutCardsIntoPlay(new LinqCardCriteria((Card c) => c.Identifier == "RemoteMortar"), 2);
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

        private IEnumerator MoveUpCloseToTrash()
        {
            BulkMoveCardsAction upCloseToTrash = new BulkMoveCardsAction(GameController, FindCardsWhere((Card c) => c.Identifier == "UpClose"), TurnTaker.Trash, false, TurnTaker, false, false);
            return GameController.DoAction(upCloseToTrash);
        }

        private IEnumerator HandleWinters(bool banish = true)
        {
            Card winters = FindCardsWhere((Card c) => c.Title == "Admiral Winters").FirstOrDefault();
            if (winters == null || winters.Location != TurnTaker.OffToTheSide)
            {
                yield break;
            }

            Location targetLocation = banish ? TurnTaker.InTheBox : TurnTaker.PlayArea;
            IEnumerator coroutine = GameController.MoveCard(this, winters, targetLocation);
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
    }
}
