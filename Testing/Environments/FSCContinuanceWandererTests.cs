﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CauldronTests
{
    [TestFixture()]
    class FSCContinuanceWandererTests : BaseTest
    {
        protected TurnTakerController fsc { get { return FindEnvironment(); } }

        [Test()]
        public void TestLoadFSC()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            Assert.AreEqual(5, this.GameController.TurnTakerControllers.Count());
        }

        [Test()]
        public void TestFSCDecklist()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            Card borg = GetCard("CombatCyborg");
            AssertIsTarget(borg, 4);
            AssertCardHasKeyword(borg, "time monster", false);

            Card paradox = GetCard("ParadoxIntrusion");
            AssertIsTarget(paradox, 4);
            AssertCardHasKeyword(paradox, "time monster", false);

            Card behemoth = GetCard("PrehistoricBehemoth");
            AssertIsTarget(behemoth, 8);
            AssertCardHasKeyword(behemoth, "time monster", false);

            Card glitch = GetCard("VortexGlitch");
            AssertCardHasKeyword(glitch, "time vortex", false);

            Card interference = GetCard("VortexInterference");
            AssertCardHasKeyword(interference, "time vortex", false);

            Card surge = GetCard("VortexSurge");
            AssertCardHasKeyword(surge, "time vortex", false);
        }

        [Test()]
        public void TestCombatCyborgEndDamageVillain()
        {
            SetupGameController("BaronBlade", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            int mdpHitpoints = mdp.HitPoints ?? default;
            QuickHPStorage(ra, legacy, haka, baron);
            PlayCard("CombatCyborg");
            //At the end of the environment turn, this card deals non-environment the target with the lowest HP (H)-2 projectile damage.
            GoToEndOfTurn(env);
            QuickHPCheck(0, 0, 0, 0);
            Assert.AreEqual(mdpHitpoints - 1, mdp.HitPoints);
        }

        [Test()]
        public void TestCombatCyborgEndDamageHero()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            GoToPlayCardPhase(env);
            QuickHPStorage(ra, legacy, haka, spite);
            PlayCard("CombatCyborg");
            //At the end of the environment turn, this card deals non-environment the target with the lowest HP (H)-2 projectile damage.
            GoToEndOfTurn(env);
            QuickHPCheck(-1, 0, 0, 0);
        }

        [Test()]
        public void TestCombatCyborgEndNotDamageEnvironment()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card intrusion = GetCard("ParadoxIntrusion");
            GoToPlayCardPhase(env);
            PlayCard("CombatCyborg");
            PlayCard(intrusion);
            QuickHPStorage(intrusion);
            //At the end of the environment turn, this card deals non-environment the target with the lowest HP (H)-2 projectile damage.
            GoToEndOfTurn(env);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestCombatCyborgReduceDamage()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card intrusion = GetCard("ParadoxIntrusion");
            Card borg = GetCard("CombatCyborg");
            PlayCard(borg);
            PlayCard(intrusion);
            //Reduce damage dealt to environment targets by 2.
            QuickHPStorage(intrusion);
            DealDamage(ra, intrusion, 3, DamageType.Fire);
            QuickHPCheck(-1);
            //Reduce damage dealt to environment targets by 2.
            QuickHPStorage(borg);
            DealDamage(ra, borg, 3, DamageType.Fire);
            QuickHPCheck(-1);
        }
        [Test()]
        public void TestHeartOfTheWandererDestroySelf()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card heart = GetCard("HeartOfTheWanderer");
            PlayCard(heart);
            //At the end of the environment turn, destroy this card.
            GoToStartOfTurn(env);
            AssertInPlayArea(env, heart);
            GoToEndOfTurn(env);
            AssertInTrash(env, heart);
        }

        [Test()]
        public void TestHeartOfTheWandererDiscard()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card heart = GetCard("HeartOfTheWanderer");
            Card lab = GetCard("LabRaid");
            PutOnDeck(spite, lab);
            DecisionMoveCardDestinations = new MoveCardDestination[] {
                new MoveCardDestination(spite.TurnTaker.Trash),
                new MoveCardDestination(legacy.TurnTaker.Trash),
                new MoveCardDestination(ra.TurnTaker.Trash),
                new MoveCardDestination(haka.TurnTaker.Trash),
                new MoveCardDestination(fsc.TurnTaker.Trash)
            };
            PlayCard(heart);
            //When this card enters play, reveal the top card of each deck in turn order and either discard it or replace it.
            AssertNumberOfCardsInTrash(spite, 1);
            AssertNumberOfCardsInTrash(legacy, 1);
            AssertNumberOfCardsInTrash(ra, 1);
            AssertNumberOfCardsInTrash(haka, 1);
            AssertNumberOfCardsInTrash(fsc, 1);
        }

        [Test()]
        public void TestHeartOfTheWandererReturn()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card heart = GetCard("HeartOfTheWanderer");
            DecisionMoveCardDestinations = new MoveCardDestination[] {
                new MoveCardDestination(spite.TurnTaker.Deck),
                new MoveCardDestination(legacy.TurnTaker.Deck),
                new MoveCardDestination(ra.TurnTaker.Deck),
                new MoveCardDestination(haka.TurnTaker.Deck),
                new MoveCardDestination(fsc.TurnTaker.Deck)
            };
            PlayCard(heart);
            //When this card enters play, reveal the top card of each deck in turn order and either discard it or replace it.
            AssertNumberOfCardsInTrash(spite, 0);
            AssertNumberOfCardsInTrash(legacy, 0);
            AssertNumberOfCardsInTrash(ra, 0);
            AssertNumberOfCardsInTrash(haka, 0);
            AssertNumberOfCardsInTrash(fsc, 0);
        }

        [Test()]
        public void TestHeartOfTheWandererTeamVillainDiscard()
        {
            SetupGameController("ErmineTeam", "Legacy", "BiomancerTeam", "Ra", "FrictionTeam", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card heart = GetCard("HeartOfTheWanderer");
            DecisionMoveCardDestinations = new MoveCardDestination[] {
                new MoveCardDestination(ermineTeam.TurnTaker.Trash),
                new MoveCardDestination(legacy.TurnTaker.Trash),
                new MoveCardDestination(biomancerTeam.TurnTaker.Trash),
                new MoveCardDestination(ra.TurnTaker.Trash),
                new MoveCardDestination(frictionTeam.TurnTaker.Trash),
                new MoveCardDestination(haka.TurnTaker.Trash),
                new MoveCardDestination(fsc.TurnTaker.Trash)
            };
            PlayCard(heart);
            //When this card enters play, reveal the top card of each deck in turn order and either discard it or replace it.
            AssertNumberOfCardsInTrash(ermineTeam, 1);
            AssertNumberOfCardsInTrash(legacy, 1);
            AssertNumberOfCardsInTrash(biomancerTeam, 1);
            AssertNumberOfCardsInTrash(ra, 1);
            AssertNumberOfCardsInTrash(frictionTeam, 1);
            AssertNumberOfCardsInTrash(haka, 1);
            AssertNumberOfCardsInTrash(fsc, 1);
        }

        [Test()]
        public void TestParadoxIntrusionEndTurnDamage0Vortex()
        {
            SetupGameController("Cauldron.FSCContinuanceWanderer", "Spite", "Guise", "Parse", "Haka");
            StartGame();
            GoToPlayCardPhase(env);
            PlayCard("ParadoxIntrusion");
            //At the end of the environment turn, this card deals the hero target with the highest HP {H} energy damage.
            //Then, this card deals X villain targets 2 energy damage each, where x is the number of time vortex cards in the environment trash.
            QuickHPStorage(haka, spite);
            GoToEndOfTurn(env);
            QuickHPCheck(-3, 0);
        }

        [Test()]
        public void TestParadoxIntrusionEndTurnDamage2Vortex()
        {
            SetupGameController("Cauldron.FSCContinuanceWanderer", "LaCapitan", "Guise", "Parse", "Haka");
            StartGame();
            Card boat = GetCardInPlay("LaParadojaMagnifica");
            PutInTrash("VortexSurge", "VortexGlitch");
            GoToPlayCardPhase(env);
            PlayCard("ParadoxIntrusion");
            //At the end of the environment turn, this card deals the hero target with the highest HP {H} energy damage.
            //Then, this card deals X villain targets 2 energy damage each, where x is the number of time vortex cards in the environment trash.
            QuickHPStorage(haka.CharacterCard, capitan.CharacterCard, boat);
            GoToEndOfTurn(env);
            QuickHPCheck(-3, -2, -2);
        }

        [Test()]
        public void TestPrehistoricBehemothEndDamage()
        {
            //This card is immune to damage dealt by targets with less than 10HP.
            SetupGameController("LaCapitan", "Guise", "Parse", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            GoToPlayCardPhase(env);
            PlayCard("PrehistoricBehemoth");
            //At the end of the environment turn, this card deals the {H - 2} hero target 2 melee damage each.
            QuickHPStorage(haka, parse, guise, capitan);
            GoToEndOfTurn(env);
            QuickHPCheck(-2, 0, 0, 0);
        }

        [Test()]
        public void TestPrehistoricBehemothImmune()
        {
            //This card is immune to damage dealt by targets with less than 10HP.
            SetupGameController("LaCapitan", "Guise", "Parse", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card behemoth = GetCard("PrehistoricBehemoth");
            PlayCard(behemoth);
            //Source HP > 10
            QuickHPStorage(behemoth);
            DealDamage(haka, behemoth, 2, DamageType.Melee);
            QuickHPCheck(-2);
            //Source HP < 10
            SetHitPoints(haka, 8);
            QuickHPStorage(behemoth);
            DealDamage(haka, behemoth, 2, DamageType.Melee);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestSuperimposedRealities()
        {
            Assert.IsTrue(false);
        }

        [Test()]
        public void TestTemporalAccelerationDestroySelf()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card accel = GetCard("TemporalAcceleration");
            GoToStartOfTurn(haka);
            PlayCard(accel);
            //At the end of the environment turn, destroy this card.
            GoToStartOfTurn(env);
            AssertInPlayArea(env, accel);
            GoToEndOfTurn(env);
            AssertInTrash(env, accel);
        }

        [Test()]
        public void TestTemporalAccelerationPlayCards()
        {
            SetupGameController("BaronBlade", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card field = GetCard("BacklashField");
            PutOnDeck(baron, field);
            Card ring = GetCard("TheLegacyRing");
            PutOnDeck(legacy, ring);
            Card staff = GetCard("TheStaffOfRa");
            PutOnDeck(ra, staff);
            Card mere = GetCard("Mere");
            PutOnDeck(haka, mere);
            //When this card enters play, play the top card of the villain deck. Then, play the top card of each hero deck in turn order.
            PlayCard(GetCard("TemporalAcceleration"));
            AssertInPlayArea(baron, field);
            AssertInPlayArea(legacy, ring);
            AssertInPlayArea(ra, staff);
            AssertInPlayArea(haka, mere);
        }

        [Test()]
        public void TestTemporalResetDestroySelf()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card reset = GetCard("TemporalReset");
            GoToStartOfTurn(haka);
            PlayCard(reset);
            //At the end of the environment turn, destroy this card.
            GoToStartOfTurn(env);
            AssertInPlayArea(env, reset);
            GoToEndOfTurn(env);
            AssertInTrash(env, reset);
        }

        [Test()]
        public void TestTemporalReset()
        {
            SetupGameController("LaCapitan", "Ra", "Parse", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card pi0 = GetCard("ParadoxIntrusion", 0);
            Card pi1 = GetCard("ParadoxIntrusion", 1);
            PlayCards(pi0, pi1);
            //When this card enters play, destroy all other environment cards. Then shuffle 2 cards from each trash pile back into their deck, and each non-character target regains {H} HP.
            Card reset = GetCard("TemporalReset");
            PlayCard(reset);
            AssertInDeck(env, pi0);
            AssertInDeck(env, pi1);
        }

        [Test()]
        public void TestTemporalReversal()
        {
            SetupGameController("LaCapitan", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            //When this card enters play, place 1 card in play from each other deck back on top of that deck.
            IEnumerable<Card> inDeck = GetCards("Fortitude", "FleshOfTheSunGod", "Mere");
            PlayCards(inDeck);
            IEnumerable<Card> inPlay = GetCards("TheLegacyRing", "TheStaffOfRa", "TaMoko", "Trueshot");
            PlayCards(inPlay);
            Card rev = GetCard("TemporalReversal");
            PlayCard(rev);
            AssertIsInPlay(inPlay);
            AssertInDeck(capitan, GetCard("LaParadojaMagnifica"));
            AssertInDeck(inDeck);
        }

        [Test()]
        public void TestTemporalReversalDestroySelf()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card rev = GetCard("TemporalReversal");
            GoToStartOfTurn(haka);
            PlayCard(rev);
            //At the end of the environment turn, destroy this card.
            GoToStartOfTurn(env);
            AssertInPlayArea(env, rev);
            GoToEndOfTurn(env);
            AssertInTrash(env, rev);
        }

        [Test()]
        public void TestTemporalSlipstream()
        {
            SetupGameController("LaCapitan", "Guise", "Parse", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card slip = GetCard("TemporalSlipstream");
            int guiseTrash = guise.TurnTaker.Trash.NumberOfCards;
            int parseTrash = parse.TurnTaker.Trash.NumberOfCards;
            int hakaTrash = haka.TurnTaker.Trash.NumberOfCards;
            int guiseHand = guise.NumberOfCardsInHand;
            int parseHand = parse.NumberOfCardsInHand;
            int hakaHand = haka.NumberOfCardsInHand;
            QuickHandStorage(guise, parse, haka);
            //When this card enters play, each player discards their hand and draws that many cards.
            PlayCard(slip);
            AssertNumberOfCardsInTrash(guise, guiseHand + guiseTrash);
            AssertNumberOfCardsInTrash(guise, parseHand + parseTrash);
            AssertNumberOfCardsInTrash(guise, hakaHand + hakaTrash);
            QuickHandCheck(0, 0, 0);
        }

        [Test()]
        public void TestTemporalSlipstreamDestroySelf()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card slip = GetCard("TemporalSlipstream");
            GoToStartOfTurn(haka);
            PlayCard(slip);
            //At the end of the environment turn, destroy this card.
            GoToStartOfTurn(env);
            AssertInPlayArea(env, slip);
            GoToEndOfTurn(env);
            AssertInTrash(env, slip);
        }

        [Test()]
        public void TestTimeFreezeNotTriggerPhase()
        {
            Game game = new Game(new string[] { "LaCapitan", "CaptainCosmic", "Parse", "Haka", "Cauldron.FSCContinuanceWanderer" });
            SetupGameController(game);
            StartGame();
            Card slip = GetCard("TimeFreeze");
            Card crest = GetCard("CosmicCrest");
            PlayCard(crest);
            DecisionSelectCard = cosmic.CharacterCard;
            GoToPlayCardPhase(capitan);
            //Play this card next to a hero.
            PlayCard(slip);
            //That hero skips their turns...
            this.RunCoroutine(GameController.EnterNextTurnPhase());
            AssertTurnPhaseDetails(game.ActiveTurnPhase, capitan, Phase.End);
            this.RunCoroutine(GameController.EnterNextTurnPhase());
            AssertTurnPhaseDetails(game.ActiveTurnPhase, parse, Phase.Start);
            //...and targets in their play are are immune to damage.
            QuickHPStorage(cosmic.CharacterCard, crest, parse.CharacterCard);
            DealDamage(capitan, cosmic, 2, DamageType.Melee);
            DealDamage(capitan, crest, 2, DamageType.Melee);
            DealDamage(capitan, parse, 2, DamageType.Melee);
            QuickHPCheck(0, 0, -2);
        }

        [Test()]
        public void TestTimeFreezeTriggerPhase()
        {
            Game game = new Game(new string[] { "LaCapitan", "CaptainCosmic", "Parse", "Haka", "Cauldron.FSCContinuanceWanderer" });
            SetupGameController(game);
            StartGame();
            Card slip = GetCard("TimeFreeze");
            Card crest = GetCard("CosmicCrest");
            PlayCard(crest);
            DecisionSelectCard = cosmic.CharacterCard;
            GoToEndOfTurn(capitan);
            //Play this card next to a hero.
            PlayCard(slip);
            //That hero skips their turns...
            AssertTurnPhaseDetails(game.ActiveTurnPhase, capitan, Phase.End);
            this.RunCoroutine(GameController.EnterNextTurnPhase());
            AssertTurnPhaseDetails(game.ActiveTurnPhase, parse, Phase.Start);
            //...and targets in their play are are immune to damage.
            QuickHPStorage(cosmic.CharacterCard, crest, parse.CharacterCard);
            DealDamage(capitan, cosmic, 2, DamageType.Melee);
            DealDamage(capitan, crest, 2, DamageType.Melee);
            DealDamage(capitan, parse, 2, DamageType.Melee);
            QuickHPCheck(0, 0, -2);
        }

        [Test()]
        public void TestTimeFreezeDestroySelf()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card freeze = GetCard("TimeFreeze");
            GoToStartOfTurn(haka);
            PlayCard(freeze);
            //At the start of the environment turn, destroy this card.
            GoToStartOfTurn(env);
            AssertInTrash(env, freeze);
        }

        [Test()]
        public void TestVortexGlitch() //This Test is known to fail
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card glitch = GetCard("VortexGlitch");
            //Players may not play one-shots.
            PlayCard(glitch);
            GoToPlayCardPhase(legacy);
            PlayCard("Thokk");
            AssertNotInTrash(legacy, "Thokk");
            //Currently the CannotPlayCards function doesn't only prevent certain cards. It's all or nothing
            IEnumerable<Card> cards = GetCards("Fortitude", "TheLegacyRing");
            PlayCards(cards);
            AssertInPlayArea(legacy, cards);
        }

        [Test()]
        public void TestVortexGlitchDestroySelf()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card glitch = GetCard("VortexGlitch");
            GoToStartOfTurn(haka);
            PlayCard(glitch);
            GoToStartOfTurn(env);
            AssertInPlayArea(env, glitch);
            GoToEndOfTurn(env);
            AssertInPlayArea(env, glitch);
            //When another environment card enters play, destroy this card.
            PlayCard("TimeFreeze");
            AssertInTrash(env, glitch);
        }

        [Test()]
        public void TestVortexInterference()
        {
            //Whenever a hero uses a power, destroy 1 hero ongoing or equipment card.
            Assert.IsTrue(false);
        }

        [Test()]
        public void TestVortexInterferenceDestroySelf()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card interference = GetCard("VortexInterference");
            GoToStartOfTurn(haka);
            PlayCard(interference);
            GoToStartOfTurn(env);
            AssertInPlayArea(env, interference);
            GoToEndOfTurn(env);
            AssertInPlayArea(env, interference);
            //When another environment card enters play, destroy this card.
            PlayCard("TimeFreeze");
            AssertInTrash(env, interference);
        }

        [Test()]
        public void TestVortexSurgee()
        {
            //Whenever a hero card is drawn, 1 player must discard a card.
            Assert.IsTrue(false);
        }

        [Test()]
        public void TestVortexSurgeDestroySelf()
        {
            SetupGameController("Spite", "Legacy", "Ra", "Haka", "Cauldron.FSCContinuanceWanderer");
            StartGame();
            Card surge = GetCard("VortexSurge");
            GoToStartOfTurn(haka);
            PlayCard(surge);
            GoToStartOfTurn(env);
            AssertInPlayArea(env, surge);
            GoToEndOfTurn(env);
            AssertInPlayArea(env, surge);
            //When another environment card enters play, destroy this card.
            PlayCard("TimeFreeze");
            AssertInTrash(env, surge);
        }
    }
}
