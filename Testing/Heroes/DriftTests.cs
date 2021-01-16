﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using Cauldron.Drift;

namespace CauldronTests
{
    [TestFixture()]
    public class DriftTests : BaseTest
    {
        protected HeroTurnTakerController drift { get { return FindHero("Drift"); } }

        protected const string AttenuationField = "AttenuationField";
        protected const string BorrowedTime = "BorrowedTime";
        protected const string DanceOfTheDragons = "DanceOfTheDragons";
        protected const string DestroyersAdagio = "DestroyersAdagio";
        protected const string DriftStep = "DriftStep";
        protected const string FutureFocus = "FutureFocus";
        protected const string ImposedSynchronization = "ImposedSynchronization";
        protected const string KnightsHeritage = "KnightsHeritage";
        protected const string MakeEverySecondCount = "MakeEverySecondCount";
        protected const string OutOfSync = "OutOfSync";
        protected const string PastFocus = "PastFocus";
        protected const string ResourcefulDaydreamer = "ResourcefulDaydreamer";
        protected const string Sabershard = "Sabershard";
        protected const string ThrowingShard = "ThrowingShard";
        protected const string TransitionShock = "TransitionShock";

        protected const string ShiftTrack = "ShiftTrack";

        public int CurrentShiftPosition()
        {
            return this.GetShiftPool().CurrentValue;
        }

        public TokenPool GetShiftPool()
        {
            return this.GetShiftTrack().FindTokenPool("ShiftPool");
        }

        public Card GetShiftTrack()
        {
            return base.FindCardsWhere((Card c) => c.SharedIdentifier == ShiftTrack && c.IsInPlayAndHasGameText, false).FirstOrDefault();
        }

        private void AssertHasKeyword(string keyword, IEnumerable<string> identifiers)
        {
            foreach (var id in identifiers)
            {
                var card = GetCard(id);
                AssertCardHasKeyword(card, keyword, false);
            }
        }

        [Test()]
        [Order(0)]
        public void TestDriftLoad()
        {
            SetupGameController("BaronBlade", "Cauldron.Drift", "Haka", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(drift);
            Assert.IsInstanceOf(typeof(DriftCharacterCardController), drift.CharacterCardController);

            foreach (var card in drift.HeroTurnTaker.GetAllCards())
            {
                var cc = GetCardController(card);
                Assert.IsTrue(cc.GetType() != typeof(CardController), $"{card.Identifier} is does not have a CardController");
            }

            Assert.AreEqual(26, drift.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestDriftDecklist()
        {
            SetupGameController("BaronBlade", "Cauldron.Drift", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            AssertHasKeyword("focus", new[]
            {
                FutureFocus,
                PastFocus
            });

            AssertHasKeyword("one-shot", new[]
            {
                AttenuationField,
                BorrowedTime,
                DanceOfTheDragons,
                DestroyersAdagio,
                DriftStep,
                ImposedSynchronization,
                ResourcefulDaydreamer
            });

            AssertHasKeyword("ongoing", new[]
            {
                FutureFocus,
                KnightsHeritage,
                MakeEverySecondCount,
                OutOfSync,
                PastFocus,
                Sabershard,
                ThrowingShard,
                TransitionShock
            });

            AssertHasKeyword("limited", new[]
            {
                MakeEverySecondCount,
                OutOfSync,
                TransitionShock
            });
        }

        [Test()]
        public void TestDriftCharacter_InnatePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Drift", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            SetHitPoints(drift, 17);
            //Shift {DriftLL}, {DriftL}, {DriftR}, {DriftRR}. Drift regains 1 HP.

            //Shift Right Twice
            DecisionSelectFunction = 3;
            int shiftPosition = CurrentShiftPosition();
            QuickHPStorage(drift);
            UsePower(drift);
            QuickHPCheck(1);
            Assert.AreEqual(shiftPosition + 2, CurrentShiftPosition());

            //Shift Right
            DecisionSelectFunction = 2;
            shiftPosition = CurrentShiftPosition();
            QuickHPStorage(drift);
            UsePower(drift);
            QuickHPCheck(1);
            Assert.AreEqual(shiftPosition + 1, CurrentShiftPosition());

            //Shift Left
            DecisionSelectFunction = 1;
            shiftPosition = CurrentShiftPosition();
            QuickHPStorage(drift);
            UsePower(drift);
            QuickHPCheck(1);
            Assert.AreEqual(shiftPosition - 1, CurrentShiftPosition());

            //Shift Left Twice
            DecisionSelectFunction = 0;
            shiftPosition = CurrentShiftPosition();
            QuickHPStorage(drift);
            UsePower(drift);
            QuickHPCheck(1);
            Assert.AreEqual(shiftPosition - 2, CurrentShiftPosition());
        }

        [Test()]
        public void TestDriftCharacter_Incap0()
        {
            SetupGameController("Apostate", "Cauldron.Drift", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            DestroyCard(drift);
            //One hero may use a power now.

            //Haka deals 2 damage
            DecisionSelectTurnTaker = haka.TurnTaker;
            QuickHPStorage(apostate);
            UseIncapacitatedAbility(drift, 0);
            QuickHPCheck(-2);

            //Bunker draws 1
            DecisionSelectTurnTaker = bunker.TurnTaker;
            QuickHandStorage(bunker);
            UseIncapacitatedAbility(drift, 0);
            QuickHandCheck(1);

            SetHitPoints(scholar, 17);
            //Scholar heals 1
            DecisionSelectTurnTaker = scholar.TurnTaker;
            QuickHPStorage(scholar);
            UseIncapacitatedAbility(drift, 0);
            QuickHPCheck(1);
        }

        [Test()]
        public void TestDriftCharacter_Incap1()
        {
            SetupGameController("Apostate", "Cauldron.Drift", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            DestroyCard(drift);
            //One target regains 1 HP and deals another target 1 radiant damage.

            DecisionSelectCards = new Card[] { haka.CharacterCard, apostate.CharacterCard };

            SetHitPoints(haka, 17);
            QuickHPStorage(haka, apostate);
            UseIncapacitatedAbility(drift, 1);
            QuickHPCheck(1, -1);
        }

        [Test()]
        public void TestDriftCharacter_Incap2()
        {
            SetupGameController("Apostate", "Cauldron.Drift", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            DestroyCard(drift);
            GoToStartOfTurn(drift);
            //One player may discard a one-shot. If they do, they may draw 2 cards.
            var a = base.GameController.FindTurnTakersWhere(tt => !tt.IsIncapacitatedOrOutOfGame);

            Card elbow = PutInHand("ElbowSmash");
            QuickHandStorage(haka);
            UseIncapacitatedAbility(drift, 2);
            //Discard 1, Draw 2
            QuickHandCheck(1);

            DiscardAllCards(haka, bunker, scholar);
            QuickHandStorage(haka, bunker, scholar);
            UseIncapacitatedAbility(drift, 2);
            //With no discard, no draw
            QuickHandCheckZero();
        }

        [Test()]
        [Sequential]
        public void TestShiftTrackSetup([Values(1, 2, 3, 4)] int decision)
        {
            SetupGameController("BaronBlade", "Cauldron.Drift", "Haka", "Bunker", "TheScholar", "Megalopolis");
            Card track = FindCardsWhere((Card c) => c.Identifier == ShiftTrack + decision, false).FirstOrDefault();
            DecisionSelectCard = track;
            StartGame();

            Assert.AreEqual(decision, CurrentShiftPosition());
            AssertIsInPlay(track);
        }

        [Test()]
        public void TestAttenuationField_Past()
        {
            SetupGameController("BaronBlade", "Cauldron.Drift", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            Card atten = PutInHand(AttenuationField);
            Card mono = PlayCard("PlummetingMonorail");
            Card field = PlayCard("BacklashField");

            //Draw a card.
            QuickHandStorage(drift);
            PlayCard(atten);
            //Play -1, Draw +1
            QuickHandCheck(0);

            //{DriftPast} Destroy 1 environment card.
            //{DriftFuture} Destroy 1 ongoing card.
            AssertInTrash(mono);
            AssertIsInPlay(field);
        }

        [Test()]
        public void TestAttenuationField_Future()
        {
            SetupGameController("BaronBlade", "Cauldron.Drift", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            Card atten = PutInHand(AttenuationField);
            Card mono = PlayCard("PlummetingMonorail");
            Card field = PlayCard("BacklashField");
            DecisionSelectFunction = 3;
            UsePower(drift);

            //Draw a card.
            QuickHandStorage(drift);
            PlayCard(atten);
            //Play -1, Draw +1
            QuickHandCheck(0);

            //{DriftPast} Destroy 1 environment card.
            //{DriftFuture} Destroy 1 ongoing card.
            AssertInTrash(field);
            AssertIsInPlay(mono);
        }

        [Test()]
        [Sequential]
        public void TestBorrowedTime_ShiftL([Values(0, 1, 2, 3)] int shiftAmount)
        {
            SetupGameController("BaronBlade", "Cauldron.Drift", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();

            DecisionSelectFunction = 3;
            UsePower(drift);
            UsePower(drift);

            SetHitPoints(baron, 17);
            SetHitPoints(drift, 17);
            SetHitPoints(haka, 17);

            DecisionSelectFunction = 0;
            DecisionSelectNumber = shiftAmount;
            int?[] hpChange = { 0, 0, 0 };
            for (int i = 0; i < shiftAmount; i++)
            {
                hpChange[i] = 2;
            }

            //Select {DriftL} or {DriftR}. Shift that direction up to 3 times. X is the number of times you shifted this way.
            //If you shifted at least {DriftL} this way, X targets regain 2 HP each.
            QuickHPStorage(baron, drift, haka);
            PlayCard(BorrowedTime);
            QuickHPCheck(hpChange);
        }

        [Test()]
        [Sequential]
        public void TestBorrowedTime_ShiftR([Values(0, 1, 2, 3)] int shiftAmount)
        {
            SetupGameController("BaronBlade", "Cauldron.Drift", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();

            SetHitPoints(baron, 17);
            SetHitPoints(drift, 17);
            SetHitPoints(haka, 17);

            DecisionSelectFunction = 1;
            DecisionSelectNumber = shiftAmount;
            int?[] hpChange = { 0, 0, 0 };
            for (int i = 0; i < shiftAmount; i++)
            {
                hpChange[i] = -3;
            }

            //Select {DriftL} or {DriftR}. Shift that direction up to 3 times. X is the number of times you shifted this way.
            //If you shifted {DriftR} this way, {Drift} deals X targets 3 radiant damage each.
            QuickHPStorage(baron, drift, haka);
            PlayCard(BorrowedTime);
            QuickHPCheck(hpChange);
        }
    }
}
