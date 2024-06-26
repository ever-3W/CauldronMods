﻿using NUnit.Framework;
using System;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using System.Reflection;
using Handelabra;
using System.Collections.Generic;

using Cauldron.Oriphel;

namespace CauldronTests
{
    [TestFixture()]
    public class OriphelTests : CauldronBaseTest
    {
        #region OriphelHelperFunctions
        protected DamageType DTM
        {
            get { return DamageType.Melee; }
        }

        protected bool IsGuardian(Card c)
        {
            return GameController.DoesCardContainKeyword(c, "guardian");
        }

        private void CleanupStartingCards()
        {
            MoveCards(oriphel, (Card c) => c.IsInPlay && !c.IsCharacter, oriphel.TurnTaker.Deck, true);
        }
        #endregion

        [Test]
        public void TestOriphelLoads()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(oriphel);
            Assert.IsInstanceOf(typeof(OriphelCharacterCardController), oriphel.CharacterCardController);

            Assert.AreEqual(80, oriphel.CharacterCard.HitPoints);
            Assert.AreEqual("Jade", oriphel.CharacterCard.Title);
            FlipCard(oriphel.CharacterCard);
            Assert.AreEqual("Oriphel", oriphel.CharacterCard.Title);
        }
        [Test]
        public void TestOriphelDecklist()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Megalopolis");

            AssertHasKeyword("guardian", new[]
            {
                "HighAsriel",
                "HighDjaril",
                "HighPhaol",
                "HighTormul"
            });

            AssertHasKeyword("relic", new[]
            {
                "MoonShardkey",
                "SunShardkey",
                "VeilShardkey",
                "WorldShardkey"
            });

            AssertHasKeyword("goon", new[]
            {
                "MejiClanLeader",
                "MejiGuard",
                "MejiNomad",
                "ShardbearerNathaniel"
            });

            AssertHasKeyword("transformation", new[]
            {
                "GrandOriphel",
                "ShardwalkersAwakening"
            });

            AssertHasKeyword("ongoing", new[]
            {
                "GrandOriphel"
            });

            AssertHasKeyword("one-shot", new[]
            {
                "Mirage",
                "Sandstorm",
                "ScrollsOfZephaeren",
                "ShardwalkersAwakening",
                "UmbralJavelin"
            });

            AssertNumberOfCardsInGame((Card c) => c.IsVillain && !c.IsCharacter, 25);
        }
        [Test]
        public void TestShardkeyRevealTriggerPlaysTransformation([Values("Moon", "Sun", "Veil", "World")] string element)
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            Card key = PlayCard(element + "Shardkey");
            Card wake = PutOnDeck("ShardwalkersAwakening");

            GoToStartOfTurn(oriphel);
            AssertInTrash(wake);
        }
        [Test]
        public void TestShardkeyRevealTriggerBottomsNonTransformation([Values("Moon", "Sun", "Veil", "World")] string element)
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            Card goon = PutOnDeck("MejiNomad");
            Card key = PlayCard(element + "Shardkey");
            PutOnDeck(oriphel, goon);

            GoToStartOfTurn(oriphel);
            AssertOnBottomOfDeck(goon);
        }
        [Test]
        public void TestGuardianDestroyTrigger([Values("Asriel", "Djaril", "Phaol", "Tormul")] string name)
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            QuickShuffleStorage(oriphel);
            Card guardian = PlayCard("High" + name);

            //stack deck so we know what Jade's extra play will be
            PutOnDeck("Sandstorm");
            PutOnDeck("WorldShardkey");
            PutOnDeck("Mirage");

            DestroyCard(guardian);
            QuickShuffleCheck(1);
            AssertNumberOfCardsInPlay((Card c) => c.IsRelic && c.IsVillain, 1);
        }
        [Test]
        public void TestGuardianDestroyTriggerNoRelicsLeft([Values("Asriel", "Djaril", "Phaol", "Tormul")] string name)
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            PutInTrash(new string[] { "MoonShardkey", "SunShardkey", "VeilShardkey", "WorldShardkey" });
            QuickShuffleStorage(oriphel);
            Card guardian = PlayCard("High" + name);

            DestroyCard(guardian);
            QuickShuffleCheck(1);
            AssertNumberOfCardsInPlay((Card c) => c.IsRelic && c.IsVillain, 0);
        }
        [Test]
        public void TestOriphelSetup([Values(new string[] { }, new string[] { "Bunker" }, new string[] { "Bunker", "Fanatic" })] string[] extraHeroes)
        {
            var startStrings = new List<string> { "Cauldron.Oriphel", "Legacy", "Ra", "Tempest" };
            startStrings.AddRange(extraHeroes);
            startStrings.Add("Megalopolis");
            int totalHeroes = 3 + extraHeroes.Count();

            SetupGameController(startStrings);
            StartGame();

            AssertNumberOfCardsInPlay((Card c) => IsGuardian(c), totalHeroes - 2);
        }
        [Test]
        public void TestJadePlaysFromRelics()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            Card goon = PutOnDeck("MejiNomad");
            PlayCard("MoonShardkey");

            AssertIsInPlay(goon);
        }
        [Test]
        public void TestJadeTriggerHandlesOngoing()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            Card goon = PutOnDeck("MejiNomad");
            Card grand = PlayCard("GrandOriphel");

            AssertIsInPlay(goon);
            AssertInTrash(grand);
        }
        [Test]
        public void TestJadeAdvanced()
        {
            SetupGameController(new string[] { "Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Megalopolis" }, advanced: true);
            StartGame();
            CleanupStartingCards();

            Card guard = PlayCard("MejiGuard");

            QuickHPStorage(legacy);
            DealDamage(oriphel, legacy, 1, DTM);
            QuickHPCheck(-2);
            DealDamage(guard, legacy, 1, DTM);
            QuickHPCheck(-2);
        }
        [Test]
        public void TestOriphelImmediateFlipResponse()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            PutInTrash(new string[] { "MoonShardkey", "SunShardkey", "VeilShardkey", "WorldShardkey" });
            QuickShuffleStorage(oriphel);

            FlipCard(oriphel);

            AssertNumberOfCardsInTrash(oriphel, 0);
            Assert.AreEqual(oriphel.CharacterCard.Title, "Oriphel");
            QuickShuffleCheck(1);
        }
        [Test]
        public void TestOriphelDamageReduction()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            QuickHPStorage(oriphel, legacy);
            FlipCard(oriphel);

            DealDamage(legacy, oriphel, 2, DTM);
            QuickHPCheck(-1, 0);
            DealDamage(oriphel, legacy, 2, DTM);
            QuickHPCheck(0, -2);
        }
        [Test]
        public void TestOriphelAdvanced()
        {
            SetupGameController(new string[] { "Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Megalopolis" }, advanced: true);
            StartGame();
            CleanupStartingCards();

            FlipCard(oriphel);
            QuickHPStorage(oriphel, legacy);

            DealDamage(legacy, oriphel, 3, DTM);
            QuickHPCheck(-1, 0);
            DealDamage(oriphel, legacy, 3, DTM);
            QuickHPCheck(0, -3);
        }
        [Test]
        public void TestOriphelEndOfTurnDamageHIs3()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            FlipCard(oriphel);

            QuickHPStorage(oriphel, legacy, ra, tempest);
            GoToEndOfTurn();
            QuickHPCheck(0, -2, -2, 0);
        }
        [Test]
        public void TestOriphelEndOfTurnDamageHIs4()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            FlipCard(oriphel);

            QuickHPStorage(oriphel, legacy, ra, tempest, haka);
            GoToEndOfTurn();
            QuickHPCheck(0, -3, 0, 0, -3);
        }
        [Test]
        public void TestOriphelFlipConditionDestruction()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            FlipCard(oriphel);
            PutInTrash("MoonShardkey");
            Card sun = PlayCard("SunShardkey");
            DestroyCard(sun);
            AssertNotFlipped(oriphel);
        }
        [Test]
        public void TestOriphelFlipConditionDiscardFromDeck()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Knyfe", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            FlipCard(oriphel);
            PutInTrash("MoonShardkey");
            PutOnDeck("SunShardkey");

            PlayCard("WreckingUppercut");
            AssertNotFlipped(oriphel);
        }

        [Test]
        public void TestOriphelFlipCondition_Requires3_Challenge()
        {
            SetupGameController(new string[] { "Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Knyfe", "Megalopolis" }, challenge: true);
            StartGame();
            CleanupStartingCards();

            FlipCard(oriphel);
            PutInTrash("MoonShardkey");
            PutInTrash("SunShardkey");
            AssertFlipped(oriphel);

            PutInTrash("WorldShardkey");

            AssertNotFlipped(oriphel);
        }

        [Test]
        public void TestRelicsReduceDamage_Challenge()
        {
            SetupGameController(new string[] { "Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Knyfe", "Megalopolis" }, challenge: true);
            StartGame();
            CleanupStartingCards();

            //Reduce damage dealt to villain relics by 2.
            Card relic = PlayCard("MoonShardkey");

            QuickHPStorage(relic);
            DealDamage(ra, relic, 4, DamageType.Fire);
            QuickHPCheck(-2);

            FlipCard(oriphel);
            QuickHPStorage(relic);
            DealDamage(ra, relic, 3, DamageType.Fire);
            QuickHPCheck(-1);

        }
        [Test]
        public void TestGrandOriphelDestroyCondition()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Knyfe", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            GoToEndOfTurn(knyfe);
            FlipCard(oriphel);

            Card grand = PutOnDeck("GrandOriphel");
            PlayCard("MoonShardkey");
            GoToEndOfTurn(oriphel);
            AssertIsInPlay(grand);

            GoToStartOfTurn(oriphel);
            AssertInTrash(grand);
        }
        [Test]
        public void TestGrandOriphelDamageModifiers()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Knyfe", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            FlipCard(oriphel);

            PlayCard("GrandOriphel");

            QuickHPStorage(oriphel, legacy);
            DealDamage(legacy, oriphel, 3, DTM);
            DealDamage(oriphel, legacy, 3, DTM);

            //Oriphel gets -2 total, 1 from self and 1 extra from Grand
            QuickHPCheck(-1, -4);
        }
        [Test]
        public void TestGrandOriphelEndOfTurnDamage()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            FlipCard(oriphel);

            PlayCard("GrandOriphel");
            PlayCard("TaMoko");

            QuickHPStorage(legacy, ra, tempest, haka);
            GoToEndOfTurn(oriphel);

            //Everyone takes (1+1)x2, for a total of 4
            //Legacy and Haka also take the standard EOT hits for 3+1 each, for a total of 8
            //Haka reduces each hit by 1, for a result of 5
            QuickHPCheck(-8, -4, -4, -5);
        }
        [Test]
        public void TestGrandOriphelNotAppliedToJade()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            FlipCard(oriphel);
            PlayCard("GrandOriphel");
            FlipCard(oriphel);

            QuickHPStorage(oriphel, legacy);
            DealDamage(legacy, oriphel, 2, DTM);
            DealDamage(oriphel, legacy, 2, DTM);

            QuickHPCheck(-2, -2);

            GoToEndOfTurn(oriphel);
            QuickHPCheck(0, 0);
        }
        [Test]
        public void TestHighAsrielDamageTrigger()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            //to make it obvious when and where damage should be going
            SetHitPoints(tempest, 10);
            SetHitPoints(ra, 10);
            SetHitPoints(legacy, 31);
            SetHitPoints(haka, 30);

            PlayCard("HighAsriel");
            QuickHPStorage(legacy, haka);

            //damage to highest HP hero target
            PlayCard("TheLegacyRing");
            QuickHPCheck(-2, 0);

            //even if they didn't play the card
            PlayCard("BlazingTornado");
            QuickHPCheck(0, -2);

            //but not if it is "put into play"
            PutIntoPlay("SurgeOfStrength");
            QuickHPCheck(0, 0);
        }
        [Test]
        public void TestHighDjarilDamageTrigger()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "Tempest", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            PlayCard("HighDjaril");

            //to make it obvious when and where damage should be going
            SetHitPoints(tempest, 10);
            SetHitPoints(ra, 10);
            SetHitPoints(legacy, 31);
            SetHitPoints(haka, 30);

            QuickHPStorage(legacy, haka);
            GoToEndOfTurn(oriphel);
            QuickHPCheck(-4, 0);
            GoToEndOfTurn(oriphel);
            QuickHPCheck(0, -4);
        }
        [Test]
        public void TestHighPhaolDamageTrigger()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            Card phaol = PlayCard("HighPhaol");

            QuickHPStorage(legacy, ra, wraith, haka);

            DealDamage(legacy, oriphel, 1, DTM);
            QuickHPCheck(-3, 0, 0, 0);

            //only once per turn
            DealDamage(ra, oriphel, 1, DTM);
            QuickHPCheckZero();

            //restarts at start of turn
            GoToStartOfTurn(legacy);
            DealDamage(ra, oriphel, 1, DTM);
            QuickHPCheck(0, -3, 0, 0);

            //does not activate on hero-hero damage
            GoToStartOfTurn(wraith);
            DecisionSelectTarget = haka.CharacterCard;
            Card bolt = PlayCard("StunBolt");
            UsePower(bolt);
            QuickHPCheck(0, 0, 0, -1);

            //nor on failed damage
            DealDamage(haka, oriphel, 1, DTM);
            QuickHPCheckZero();

            DecisionSelectTarget = phaol;
            DestroyCard(bolt);
            PlayCard(bolt);
            UsePower(bolt);
            QuickHPCheck(0, 0, -2, 0);
        }
        [Test]
        public void TestHighTormulDamageTrigger()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            PlayCard("HighTormul");

            QuickHPStorage(legacy, ra, wraith, haka);

            GoToStartOfTurn(legacy);
            QuickHPCheck(-2, 0, 0, 0);
            GoToStartOfTurn(ra);
            QuickHPCheck(0, -2, 0, 0);
            GoToStartOfTurn(wraith);
            QuickHPCheck(0, 0, -2, 0);
        }
        [Test]
        public void TestMejiClanLeaderDR()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            var goons = new List<Card> { PlayCard("MejiClanLeader"), PlayCard("MejiGuard"), PlayCard("MejiNomad"), PlayCard("ShardbearerNathaniel") };

            foreach (Card goon in goons)
            {
                QuickHPStorage(goon);
                DealDamage(legacy, goon, 2, DTM);
                QuickHPCheck(-1);
            }

            //only goons
            QuickHPStorage(oriphel, legacy);
            DealDamage(oriphel, legacy, 2, DTM);
            DealDamage(legacy, oriphel, 2, DTM);
            QuickHPCheck(-2, -2);
        }
        [Test]
        public void TestMejiClanLeaderDamage()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            PlayCard("MejiClanLeader");

            QuickHPStorage(legacy, ra, wraith, haka);
            GoToEndOfTurn();
            QuickHPCheck(-3, -3, -3, -3);
        }
        [Test]
        public void TestMejiGuardDamage()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            PlayCard("MejiGuard");
            QuickHPStorage(legacy, ra, wraith, haka);
            DrawCard(ra);
            AssertNoDecision();
            GoToEndOfTurn();
            QuickHPCheck(0, -2, 0, 0);
        }
        [Test]
        public void TestMejiGuardDR([Values("HighAsriel", "HighDjaril", "HighPhaol", "HighTormul")] string name)
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            PlayCard("MejiGuard");
            Card djinn = PlayCard(name);

            QuickHPStorage(djinn);
            DealDamage(ra, djinn, 2, DTM);
            QuickHPCheck(-1);
        }
        [Test]
        public void TestMejiNomadDamage([Values(0, 1, 2, 3, 4)] int numGuardians)
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            //make sure the play-card effect of Asriel doesn't cause a tie
            SetHitPoints(legacy, 20);

            PlayCard("MejiNomad");
            var guardians = FindCardsWhere((Card c) => IsGuardian(c)).ToList();
            Card guardian;
            for (int i = 0; i < numGuardians; i++)
            {
                guardian = guardians[i];
                PlayCard(guardian);
                DecisionSelectTarget = guardian;
                PlayCard("ThroatJab");
            }

            int expectedDamage = numGuardians + 2;
            QuickHPStorage(haka);
            GoToEndOfTurn();
            QuickHPCheck(-expectedDamage);
        }
        [Test]
        public void TestShardbearerNathaniel()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            PlayCard("ShardbearerNathaniel");
            var guardians = FindCardsWhere((Card c) => IsGuardian(c)).ToList();
            QuickHPStorage(legacy);

            foreach (Card guardian in guardians)
            {
                PlayCard(guardian);
                DealDamage(guardian, legacy, 1, DTM);
                QuickHPCheck(-2);
            }

            //should not work on Jade
            DealDamage(oriphel, legacy, 1, DTM);
            QuickHPCheck(-1);

            FlipCard(oriphel);
            DealDamage(oriphel, legacy, 1, DTM);
            QuickHPCheck(-2);
        }
        [Test]
        public void TestMirageDiscards()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            Card javelin = PutOnDeck("UmbralJavelin");
            Card storm = PutOnDeck("Sandstorm");

            PlayCard("Mirage");
            AssertInTrash(javelin, storm);
        }
        [Test]
        public void TestMiragePlaysTargets()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            Card phaol = PutOnDeck("HighPhaol");
            Card meji = PutOnDeck("MejiGuard");

            PlayCard("Mirage");
            AssertIsInPlay(phaol, meji);
        }
        [Test]
        public void TestMirageDamage()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            PlayCard("MejiNomad");
            PlayCard("ShardbearerNathaniel");
            PutOnDeck("MejiGuard");
            PutOnDeck("MejiClanLeader");

            QuickHPStorage(legacy, ra, wraith, haka);
            PlayCard("Mirage");

            //First two hits must go to Haka, next one is a choice between Legacy and Haka, last goes to the other
            QuickHPCheck(-1, 0, 0, -3);
        }
        [Test]
        public void TestSandstorm([Values(0, 1, 2, 3)] int environCards)
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            var safeCards = new string[] { "PoliceBackup", "TrafficPileup", "ImpendingCasualty" };

            for (int i = 0; i < environCards; i++)
            {
                PlayCard(safeCards[i]);
            }

            PutInTrash(oriphel, (Card c) => c.IsTarget && !c.IsCharacter);
            QuickShuffleStorage(oriphel.TurnTaker.Trash);

            PlayCard("Sandstorm");
            AssertNumberOfCardsInPlay(oriphel, environCards + 2);
            QuickShuffleCheck(1);
        }
        [Test]
        public void TestSandstormNotEnoughGoons()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            PutInTrash("HighAsriel", "HighTormul", "MejiGuard");
            PlayCard("PoliceBackup");

            PlayCard("Sandstorm");
            AssertNumberOfCardsInPlay(oriphel, 2);
        }
        [Test]
        public void TestScrollsOfZephaerenShuffleAndNoJadeDamage()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            var guardians = new Card[] { PutInTrash("HighAsriel"), PutInTrash("HighPhaol"), PutInTrash("HighTormul"), PutInTrash("HighDjaril") };

            QuickHPStorage(legacy, ra, wraith, haka);
            QuickShuffleStorage(oriphel.TurnTaker.Deck);
            PlayCard("TakeDown");
            PutIntoPlay("ScrollsOfZephaeren");
            AssertNotInTrash(guardians);
            QuickShuffleCheck(1);
            QuickHPCheck(0, 0, 0, 0);
        }
        [Test]
        public void TestScrollsOfZephaerenDamage()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            FlipCard(oriphel);
            var guardians = new Card[] { PutInTrash("HighAsriel"), PutInTrash("HighPhaol"), PutInTrash("HighTormul"), PutInTrash("HighDjaril") };

            QuickHPStorage(ra);
            QuickShuffleStorage(oriphel.TurnTaker.Deck);
            PlayCard("TakeDown");
            PutIntoPlay("ScrollsOfZephaeren");
            QuickHPCheck(-3);

        }
        [Test]
        public void TestShardwalkersAwakeningJade()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            QuickHPStorage(legacy, ra, wraith, haka);
            AssertNotFlipped(oriphel);
            PlayCard("ShardwalkersAwakening");
            AssertFlipped(oriphel);
            QuickHPCheck(0, 0, 0, 0);
        }
        [Test]
        public void TestShardwalkersAwakeningOriphel()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            QuickHPStorage(legacy, ra, wraith, haka);
            FlipCard(oriphel);
            PlayCard("ShardwalkersAwakening");
            AssertFlipped(oriphel);
            QuickHPCheck(-2, -2, -2, -2);
        }
        [Test]
        public void TestUmbralJavelinJade()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            Card goon = PutOnDeck("MejiGuard");
            Card guardian = PutOnDeck("HighPhaol");
            Card relic = PutOnDeck("MoonShardkey");
            Card transformation = PutOnDeck("GrandOriphel");

            PlayCard("UmbralJavelin");
            AssertIsInPlay(goon, guardian);
            AssertInTrash(relic, transformation);
        }
        [Test]
        public void TestUmbralJavelinOriphel()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            FlipCard(oriphel);
            Card goon = PutOnDeck("MejiGuard");

            var destroyed = new Card[] { PlayCard("SurgeOfStrength"), PlayCard("TheLegacyRing"), PlayCard("TheStaffOfRa"), PlayCard("StunBolt") };
            Card tamoko = PlayCard("TaMoko");

            PlayCard("UmbralJavelin");
            AssertInTrash(destroyed);
            AssertIsInPlay(goon, tamoko);
        }
        [Test]
        public void TestShardkeyMoonDamage()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            QuickHPStorage(haka);
            PutOnDeck("HighAsriel");
            PlayCard("MoonShardkey");
            GoToEndOfTurn();
            QuickHPCheck(-2);
        }
        [Test]
        public void TestShardkeyMoonSourceIsHighest()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            DecisionSelectTarget = oriphel.CharacterCard;
            PlayCard("ThroatJab");
            SetHitPoints(oriphel, 10);

            QuickHPStorage(haka);
            PutOnDeck("HighAsriel");
            PlayCard("MoonShardkey");
            GoToEndOfTurn();
            QuickHPCheck(-2);
        }
        [Test]
        public void TestShardkeySun()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            QuickHPStorage(haka);
            PutOnDeck("HighAsriel");
            PlayCard("SunShardkey");

            QuickHPStorage(legacy, ra, wraith, haka);
            UsePower(haka);
            QuickHPCheck(0, 0, 0, -2);
            UsePower(legacy);
            QuickHPCheck(-3, 0, 0, 0);
        }
        [Test]
        public void TestShardkeyVeil()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            PutOnDeck("HighAsriel");
            PutOnDeck("MejiNomad");
            PutOnDeck("MejiGuard");
            PlayCard("VeilShardkey");

            DestroyCard("MejiGuard");
            AssertIsInPlay("MejiNomad");
            DestroyCard("VeilShardkey");
            AssertNotInPlay("HighAsriel");
        }
        [Test]
        public void TestShardkeyWorldDamage()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            QuickHPStorage(wraith);
            PutOnDeck("HighAsriel");
            PlayCard("WorldShardkey");
            GoToEndOfTurn();
            QuickHPCheck(-2);
        }
        [Test]
        public void TestShardkeyWorldSourceIsLowest()
        {
            SetupGameController("Cauldron.Oriphel", "Legacy", "Ra", "TheWraith", "Haka", "Megalopolis");
            StartGame();
            CleanupStartingCards();

            DecisionSelectTarget = oriphel.CharacterCard;
            PlayCard("ThroatJab");
            SetHitPoints(oriphel, 10);

            QuickHPStorage(wraith);
            PutOnDeck("HighAsriel");
            PlayCard("WorldShardkey");
            GoToEndOfTurn();
            QuickHPCheck(-2);
        }
    }
}
