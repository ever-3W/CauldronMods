using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using Cauldron.Impact;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CauldronTests
{
    [TestFixture()]
    public class ImpactTests : BaseTest
    {
        #region ImpactHelperFunctions
        protected HeroTurnTakerController impact { get { return FindHero("Impact"); } }
        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(impact.CharacterCard, 1);
            DealDamage(villain, impact, 2, DamageType.Melee);
        }
        private void AssertHasKeyword(string keyword, IEnumerable<string> identifiers)
        {
            foreach (var id in identifiers)
            {
                var card = GetCard(id);
                AssertCardHasKeyword(card, keyword, false);
            }
        }

        #endregion
        [Test]
        public void TestImpactLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.Impact", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(impact);
            Assert.IsInstanceOf(typeof(ImpactCharacterCardController), impact.CharacterCardController);

            Assert.AreEqual(29, impact.CharacterCard.HitPoints);
        }
        [Test]
        public void TestImpactDecklist()
        {
            SetupGameController("BaronBlade", "Cauldron.Impact", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            AssertHasKeyword("ongoing", new[]
            {
                "DecayingOrbit",
                "GravitationalLensing",
                "GraviticOrb",
                "HurledObstruction",
                "InescapablePull",
                "LocalMicrogravity",
                "Meditate",
                "RepulsionField",
                "SlingshotTrajectory",
                "SpatialFinesse"
            });

            AssertHasKeyword("limited", new[]
            {
                "GravitationalLensing",
                "LocalMicrogravity",
                "RepulsionField"
            });

            AssertHasKeyword("one-shot", new[]
            {
                "AcceleratedCollision",
                "CrushingRift",
                "EscapeVelocity",
                "MassDriver"
            });
        }
        [Test]
        public void TestImpactPowerSimple()
        {
            SetupGameController("BaronBlade", "Cauldron.Impact", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            DestroyCard("MobileDefensePlatform");

            QuickHPStorage(baron);
            UsePower(impact);
            QuickHPCheck(-1);
        }
        [Test]
        public void TestImpactPowerDestroyOngoing()
        {
            SetupGameController("BaronBlade", "Cauldron.Impact", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            DestroyCard("MobileDefensePlatform");

            DecisionYesNo = true;
            QuickHPStorage(baron);
            Card moko = PlayCard("TaMoko");
            UsePower(impact);
            QuickHPCheck(-3);
            AssertInTrash(moko);
            UsePower(impact);
            QuickHPCheck(-1);
        }
        [Test]
        public void TestImpactPowerDestroyOngoingIsOptional()
        {
            SetupGameController("BaronBlade", "Cauldron.Impact", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            DestroyCard("MobileDefensePlatform");

            DecisionYesNo = false;
            QuickHPStorage(baron);
            Card moko = PlayCard("TaMoko");
            UsePower(impact);
            QuickHPCheck(-1);
            AssertIsInPlay(moko);
            UsePower(impact);
            QuickHPCheck(-1);
        }
        [Test]
        public void TestImpactIncap1()
        {
            SetupGameController("BaronBlade", "Cauldron.Impact", "Haka", "Bunker", "TheScholar", "Megalopolis");
            StartGame();
            DestroyCard("MobileDefensePlatform");

            SetupIncap(baron);
            AssertIncapLetsHeroUsePower(impact, 0, haka);
        }
        [Test]
        public void TestImpactIncap2()
        {
            SetupGameController("BaronBlade", "Cauldron.Impact", "Haka", "Ra", "TheScholar", "Megalopolis");
            StartGame();
            DestroyCard("MobileDefensePlatform");
            SetupIncap(baron);
            DecisionSelectCards = new Card[] { haka.CharacterCard, baron.CharacterCard, ra.CharacterCard, baron.CharacterCard };
            PlayCard("TheStaffOfRa");

            QuickHPStorage(baron);
            UseIncapacitatedAbility(impact, 1);
            QuickHPCheck(-1);
            UseIncapacitatedAbility(impact, 1);
            QuickHPCheck(-2);
        }
        [Test]
        public void TestImpactIncap3()
        {
            SetupGameController("BaronBlade", "Cauldron.Impact", "Haka", "Bunker", "TheScholar", "DokThorathCapital");
            StartGame();
            DestroyCard("MobileDefensePlatform");

            Card mil = PlayCard("ThorathianMilitary");
            QuickHPStorage(mil);
            DealDamage(haka, mil, 1, DamageType.Melee);
            QuickHPCheck(0);
            UseIncapacitatedAbility(impact, 2);
            DealDamage(haka, mil, 1, DamageType.Melee);
            QuickHPCheck(-1);

            GoToStartOfTurn(impact);
            DealDamage(haka, mil, 1, DamageType.Melee);
            QuickHPCheck(0);
        }
        [Test]
        public void TestAcceleratedCollision()
        {
            SetupGameController("BaronBlade", "Cauldron.Impact", "Haka", "Bunker", "TheScholar", "DokThorathCapital");
            StartGame();
            DestroyCard("MobileDefensePlatform");

            PutOnDeck(impact, impact.HeroTurnTaker.Hand.Cards);
            Card coll1 = PutInHand("AcceleratedCollision");
            Card coll2 = PutInHand("AcceleratedCollision");
            Card fin = PutInHand("SpatialFinesse");
            Assert.AreNotSame(coll1, coll2, "Somehow managed to get the same card twice");

            DecisionSelectCards = new Card[] { baron.CharacterCard, coll2, baron.CharacterCard, null };
            QuickHPStorage(baron);
            PlayCard(coll1);
            //got hit with 2 copies of Accelerated Collision
            QuickHPCheck(-4);

            AssertInTrash(coll1, coll2);
            //extra play is optional
            AssertInHand(fin);
        }
        [Test]
        public void TestCrushingRift()
        {
            SetupGameController("BaronBlade", "Cauldron.Impact", "Haka", "Bunker", "TheScholar", "DokThorathCapital");
            StartGame();

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            SetHitPoints(mdp, 9);
            QuickHPStorage(mdp);
            PlayCard("CrushingRift");
            QuickHPCheck(-4);
        }
        [Test]
        public void TestCrushingRiftIgnoresImmunity()
        {
            SetupGameController("BaronBlade", "Cauldron.Impact", "Haka", "Bunker", "TheScholar", "RealmOfDiscord");
            StartGame();

            PlayCard("ClaustrophobicDelusion");
            PlayCard("BladeBattalion");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            SetHitPoints(mdp, 9);
            QuickHPStorage(mdp);
            DealDamage(impact, mdp, 1, DamageType.Melee);
            QuickHPCheck(0);
            PlayCard("CrushingRift");
            QuickHPCheck(-4);
        }
        [Test]
        public void TestCrushingRiftIgnoredUndamagedAndCharacter()
        {
            SetupGameController("BaronBlade", "Cauldron.Impact", "Haka", "Bunker", "TheScholar", "TheWraith", "RealmOfDiscord");
            StartGame();

            Card batt = PlayCard("BladeBattalion");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            SetHitPoints(batt, 4);
            SetHitPoints(baron, 20);
            QuickHPStorage(baron.CharacterCard, mdp, batt);
            AssertNoDecision();
            PlayCard("CrushingRift");
            QuickHPCheck(0, 0, -2);
        }
    }
}