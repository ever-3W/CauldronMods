﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CauldronTests
{
    [TestFixture()]
    public class NorthsparTests : BaseTest
    {

        #region NorthsparHelperFunctions

        protected TurnTakerController northspar { get { return FindEnvironment(); } }

        #endregion

        [Test()]
        public void TestNorthsparWorks()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();
            Assert.AreEqual(5, this.GameController.TurnTakerControllers.Count());

        }

        [Test()]
        [Sequential]
        public void DecklistTest_Mercenary_IsMercenary([Values("TakAhab")] string mercenary)
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToPlayCardPhase(northspar);

            Card card = PlayCard(mercenary);
            AssertIsInPlay(card);
            AssertCardHasKeyword(card, "mercenary", false);
        }

        [Test()]
        [Sequential]
        public void DecklistTest_FirstWaypoint_IsFirstWaypoint([Values("MakeshiftShelter")] string firstWaypoint)
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToPlayCardPhase(northspar);

            Card card = PlayCard(firstWaypoint);
            AssertIsInPlay(card);
            AssertCardHasKeyword(card, "first waypoint", false);
        }

        [Test()]
        [Sequential]
        public void DecklistTest_SecondWaypoint_IsSecondWaypoint([Values("SupplyDepot")] string secondWaypoint)
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToPlayCardPhase(northspar);

            //put 1st waypoint in play so no autodestroy
            PlayCard("MakeshiftShelter");

            Card card = PlayCard(secondWaypoint);
            AssertIsInPlay(card);
            AssertCardHasKeyword(card, "second waypoint", false);
        }

        [Test()]
        [Sequential]
        public void DecklistTest_ThirdWaypoint_IsThirdWaypoint([Values("DemolishedCamp", "LandingSite")] string thirdWaypoint)
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToPlayCardPhase(northspar);

            //put 1st and 2nd waypoint in play so no autodestroy
            PlayCard("MakeshiftShelter");
            PlayCard("SupplyDepot");

            Card card = PlayCard(thirdWaypoint);
            AssertIsInPlay(card);
            AssertCardHasKeyword(card, "third waypoint", false);
        }

        [Test()]
        [Sequential]
        public void DecklistTest_MineralDeposit_IsMineralDeposit([Values("AethiumVein")] string mineralDeposit)
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToPlayCardPhase(northspar);

            Card card = PlayCard(mineralDeposit);
            AssertIsInPlay(card);
            AssertCardHasKeyword(card, "mineral deposit", false);
        }

        [Test()]
        [Sequential]
        public void DecklistTest_Frozen_IsFrozen([Values("LostInTheSnow", "RagingBlizzard", "Frostbite", "SnowShrieker", "FrozenSolid","WhatsLeftOfThem", "BitterCold" )] string frozen)
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToPlayCardPhase(northspar);

            Card card = PlayCard(frozen);
            AssertIsInPlay(card);
            AssertCardHasKeyword(card, "frozen", false);
        }

        [Test()]
        [Sequential]
        public void DecklistTest_Creature_IsCreature([Values("AlienFungus", "SnowShrieker")] string creature)
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToPlayCardPhase(northspar);

            Card card = PlayCard(creature);
            AssertIsInPlay(card);
            AssertCardHasKeyword(card, "creature", false);
        }

        [Test()]
        public void TestThirdWaypoint_PlayTopCardAndDestroy_NotIndestructible()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToPlayCardPhase(northspar);
            Card bitter = PutOnDeck("BitterCold");
            Card thirdWaypoint = GetCard("LandingSite");
            //When this card enters play, play the top card of the environment deck and destroy this card. 
            AssertInDeck(bitter);
            PlayCard(thirdWaypoint);
            AssertInPlayArea(northspar, bitter);
            AssertInTrash(thirdWaypoint);
        }

        [Test()]
        public void TestThirdWaypoint_PlayTopCardAndDestroy_Indestructible()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToPlayCardPhase(northspar);
            //put 1st and 2nd waypoint in play so no autodestroy
            PlayCard("MakeshiftShelter");
            PlayCard("SupplyDepot");

            Card bitter = PutOnDeck("BitterCold");
            Card thirdWaypoint = GetCard("LandingSite");

            //When this card enters play, play the top card of the environment deck and destroy this card. 
            AssertInDeck(bitter);
            PlayCard(thirdWaypoint);
            AssertInPlayArea(northspar, bitter);
            AssertInPlayArea(northspar, thirdWaypoint);
        }

        [Test()]
        public void TestThirdWaypoint_OtherThirdWaypointEntersPlay()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToPlayCardPhase(northspar);
            //put 1st and 2nd waypoint in play so no autodestroy
            PlayCard("MakeshiftShelter");
            PlayCard("SupplyDepot");

            Card bitter = PutOnDeck("BitterCold");
            Card thirdWaypoint = GetCard("LandingSite");
            Card otherThirdWaypoint = GetCard("DemolishedCamp");

            //If Demolished Camp/Landing Site would enter play, instead remove it from the game.
            AssertInDeck(otherThirdWaypoint);
            PlayCard(thirdWaypoint);
            PlayCard(otherThirdWaypoint);
            AssertOutOfGame(otherThirdWaypoint);
        }

        [Test()]
        public void TestLandingSiteStartOfTurn_NoTakAhab()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToEndOfTurn(haka);

            //play an equipment to destroy
            Card equipment = PlayCard("Mere");

            //put 1st and 2nd waypoint in play so no autodestroy
            PlayCard("MakeshiftShelter");
            PlayCard("SupplyDepot");

            //stack deck to prevent tak ahab from entering play
            PutOnDeck("BitterCold");
            Card landingSite = GetCard("LandingSite");

            PlayCard(landingSite);
            AssertInPlayArea(northspar, landingSite);

            //At the start of the environment turn, if Tak Ahab is in play, you may destroy 1 hero ongoing or equipment card. If you do, place the top card of any deck beneath Tak Ahab.
            //Tak Ahab is not in play so nothing should be destroyed and no cards should be under
            GoToStartOfTurn(northspar);
            AssertInPlayArea(haka, equipment);
        }

        [Test()]
        public void TestLandingSiteStartOfTurn_TakAhabInPlay_DestroyEquipment()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToEndOfTurn(haka);

            //play an equipment to destroy
            Card equipment = PlayCard("Mere");

            //put 1st and 2nd waypoint in play so no autodestroy
            PlayCard("MakeshiftShelter");
            PlayCard("SupplyDepot");

            //stack deck to prevent tak ahab from entering play
            PutOnDeck("BitterCold");
            Card landingSite = GetCard("LandingSite");

            Card takAhab = PlayCard("TakAhab");
            int numCardsBeneathTakAhab = GetNumberOfCardsUnderCard(takAhab);
            PlayCard(landingSite);
            AssertInPlayArea(northspar, landingSite);

            //At the start of the environment turn, if Tak Ahab is in play, you may destroy 1 hero ongoing or equipment card. If you do, place the top card of any deck beneath Tak Ahab.
            //Tak Ahab is in play, so equipment will be destroyed
            //should be one additional card under takAhab
            GoToStartOfTurn(northspar);
            AssertInTrash(haka, equipment);
            AssertNumberOfCardsUnderCard(takAhab, numCardsBeneathTakAhab + 1);
        }

        [Test()]
        public void TestLandingSiteStartOfTurn_TakAhabInPlay_DestroyOngoing()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToEndOfTurn(haka);

            //play an ongoing to destroy
            Card ongoing = PlayCard("Dominion");

            //put 1st and 2nd waypoint in play so no autodestroy
            PlayCard("MakeshiftShelter");
            PlayCard("SupplyDepot");

            //stack deck to prevent tak ahab from entering play
            PutOnDeck("BitterCold");
            Card landingSite = GetCard("LandingSite");

            Card takAhab = PlayCard("TakAhab");
            int numCardsBeneathTakAhab = GetNumberOfCardsUnderCard(takAhab);
            PlayCard(landingSite);
            AssertInPlayArea(northspar, landingSite);

            //At the start of the environment turn, if Tak Ahab is in play, you may destroy 1 hero ongoing or equipment card. If you do, place the top card of any deck beneath Tak Ahab.
            //Tak Ahab is in play, so ongoing will be destroyed
            //should be one additional card under takAhab
            GoToStartOfTurn(northspar);
            AssertInTrash(haka, ongoing);
            AssertNumberOfCardsUnderCard(takAhab, numCardsBeneathTakAhab + 1);
        }

        [Test()]
        public void TestLandingSiteStartOfTurn_TakAhabInPlay_MoveTopCardOfAnyDeck()
        {
            SetupGameController("KaargraWarfang", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToEndOfTurn(haka);

            //play an ongoing to destroy
            Card ongoing = PlayCard("Dominion");

            //put 1st and 2nd waypoint in play so no autodestroy
            PlayCard("MakeshiftShelter");
            PlayCard("SupplyDepot");

            //stack deck to prevent tak ahab from entering play
            PutOnDeck("BitterCold");
            Card landingSite = GetCard("LandingSite");

            Card takAhab = PlayCard("TakAhab");
            int numCardsBeneathTakAhab = GetNumberOfCardsUnderCard(takAhab);
            PlayCard(landingSite);
            AssertInPlayArea(northspar, landingSite);

            DecisionSelectLocation = new LocationChoice(ra.TurnTaker.Deck);
            Card topCard = ra.TurnTaker.Deck.TopCard;
            //At the start of the environment turn, if Tak Ahab is in play, you may destroy 1 hero ongoing or equipment card. If you do, place the top card of any deck beneath Tak Ahab.
            GoToStartOfTurn(northspar);
            AssertUnderCard(takAhab, topCard);
        }

        [Test()]
        public void TestDemolishedCamp_IncreaseDamage()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToEndOfTurn(haka);
            DestroyCard(GetCardInPlay("MobileDefensePlatform"), haka.CharacterCard);
            //play an equipment to destroy
            Card equipment = PlayCard("Mere");

            //put 1st and 2nd waypoint in play so no autodestroy
            PlayCard("MakeshiftShelter");
            PlayCard("SupplyDepot");

            //stack deck to prevent tak ahab from entering play
            PutOnDeck("BitterCold");
            Card demolishedCamp = GetCard("DemolishedCamp");

            Card takAhab = PlayCard("TakAhab");
            PlayCard(demolishedCamp);
            AssertInPlayArea(northspar, demolishedCamp);

            // Increase damage dealt to Tak Ahab by 1.
            QuickHPStorage(takAhab);
            DealDamage(haka.CharacterCard, takAhab, 2, DamageType.Projectile);
            QuickHPCheck(-3);

            //should only affect tak ahab

            //check villain
            QuickHPStorage(baron);
            DealDamage(haka, baron, 2, DamageType.Projectile);
            QuickHPCheck(-2);

            //check hero
            QuickHPStorage(haka);
            DealDamage(ra, haka, 2, DamageType.Fire);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestDemolishedCamp_RemoveOnDestruction()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            GoToEndOfTurn(haka);
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            //play an equipment to destroy
            Card equipment = PlayCard("Mere");

            //put 1st and 2nd waypoint in play so no autodestroy
            PlayCard("MakeshiftShelter");
            PlayCard("SupplyDepot");

            //stack deck to prevent tak ahab from entering play
            PutOnDeck("BitterCold");
            Card demolishedCamp = GetCard("DemolishedCamp");

            Card takAhab = PlayCard("TakAhab");
            PlayCard(demolishedCamp);
            AssertInPlayArea(northspar, demolishedCamp);

            // If Tak Ahab is destroyed, remove him from the game.
            DestroyCard(takAhab, haka.CharacterCard);
            AssertOutOfGame(takAhab);

            //should only affect Tak Ahab
            DestroyCard(mdp, haka.CharacterCard);
            AssertInTrash(mdp);

        }

        [Test()]
        public void TestMakeshiftShelter_Indestrucible()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            Card makeshift = PlayCard("MakeshiftShelter");
            //This card and Supply Depot are indestructible.
            DestroyCard(makeshift, ra.CharacterCard);
            AssertInPlayArea(northspar, makeshift);
        }

        [Test()]
        public void TestMakeshiftShelter_MakesSupplyDepotIndestructible()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            Card makeshift = PlayCard("MakeshiftShelter");
            Card supply = PlayCard("SupplyDepot");
            //This card and Supply Depot are indestructible.
            DestroyCard(supply, ra.CharacterCard);
            AssertIsInPlay(supply);
        }

        [Test()]
        public void TestMakeshiftShelterStartOfTurn_NoTakAhabInTrash()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            Card makeshift = PlayCard("MakeshiftShelter");
            
            Card bitter = PutInTrash("BitterCold");
            //At the start of the environment turn, you may shuffle a card from the environment trash into the deck. if Tak Ahab is in the environment trash, shuffle him into the deck."
            DecisionYesNo = true;
            QuickShuffleStorage(northspar.TurnTaker.Deck);
            GoToStartOfTurn(northspar);
            AssertInDeck(bitter);
            QuickShuffleCheck(1);
        }

        [Test()]
        public void TestMakeshiftShelterStartOfTurn_WithTakAhabInTrash()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            Card makeshift = PlayCard("MakeshiftShelter");
            Card bitter = PutInTrash("BitterCold");
            Card takAhab = PutInTrash("TakAhab");

            //At the start of the environment turn, you may shuffle a card from the environment trash into the deck. if Tak Ahab is in the environment trash, shuffle him into the deck."
            DecisionYesNo = true;
            QuickShuffleStorage(northspar.TurnTaker.Deck);
            GoToStartOfTurn(northspar);
            AssertInDeck(bitter);
            AssertInDeck(takAhab);
            QuickShuffleCheck(2);
        }

        [Test()]
        public void TestMakeshiftShelterStartOfTurn_WithTakAhabInTrash_OptionalShuffle()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            Card makeshift = PlayCard("MakeshiftShelter");
            Card bitter = PutInTrash("BitterCold");
            Card takAhab = PutInTrash("TakAhab");

            //At the start of the environment turn, you may shuffle a card from the environment trash into the deck. if Tak Ahab is in the environment trash, shuffle him into the deck."
            DecisionYesNo = false;
            QuickShuffleStorage(northspar.TurnTaker.Deck);
            GoToStartOfTurn(northspar);
            AssertInTrash(bitter);
            AssertInDeck(takAhab);
            QuickShuffleCheck(1);
        }


        [Test()]
        public void TestSupplyDepot_MakesThirdWaypointsIndestructible_LandingSite()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            //play makeshift shelter so supply depot doesn't auto blow up
            Card makeshift = PlayCard("MakeshiftShelter");
            Card supply = PlayCard("SupplyDepot");
            Card landing = PlayCard("LandingSite");
            //Third Waypoint cards are indestructible.
            DestroyCard(landing, ra.CharacterCard);
            AssertInPlayArea(northspar, landing);
        }

        [Test()]
        public void TestSupplyDepot_MakesThirdWaypointsIndestructible_DemolishedCamp()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            //play makeshift shelter so supply depot doesn't auto blow up
            Card makeshift = PlayCard("MakeshiftShelter");
            Card supply = PlayCard("SupplyDepot");
            Card demolished = PlayCard("DemolishedCamp");
            //Third Waypoint cards are indestructible.
            DestroyCard(demolished, ra.CharacterCard);
            AssertInPlayArea(northspar, demolished);
        }

        [Test()]
        public void TestSupplyDepot_NoMakeshift()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            //stack deck
            Card bitter = PutOnDeck("BitterCold");

            //When this card enters play, destroy it and play the top card of the environment deck if Makeshift Shelter is not in play. Otherwise place it next to a hero. 
            Card supply = PlayCard("SupplyDepot");
            AssertInPlayArea(northspar, bitter);
            AssertInTrash(supply);
        
        }

        [Test()]
        public void TestSupplyDepot_WithMakeshift()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            //stack deck
            Card bitter = PutOnDeck("BitterCold");

            //play makeshift shelter so supply depot doesn't auto blow up
            Card makeshift = PlayCard("MakeshiftShelter");

            //When this card enters play, destroy it and play the top card of the environment deck if Makeshift Shelter is not in play. Otherwise place it next to a hero. 
            DecisionSelectCard = legacy.CharacterCard;
            Card supply = PlayCard("SupplyDepot");

            AssertNextToCard(supply, legacy.CharacterCard);
            AssertInDeck(bitter);

        }

        [Test()]
        public void TestSupplyDepot_GrantedPower()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            //play makeshift shelter so supply depot doesn't auto blow up
            Card makeshift = PlayCard("MakeshiftShelter");

            //should be next to legacy
            DecisionSelectCard = legacy.CharacterCard;
            Card supply = PlayCard("SupplyDepot");
            AssertNextToCard(supply, legacy.CharacterCard);

            //"Power: this hero deals 1 target 1 fire damage."
            //AssertNumberOfUsablePowers(legacy, 2);
            QuickHPStorage(haka);
            DecisionSelectTarget = haka.CharacterCard;
            DecisionSelectPower = legacy.CharacterCard;
            DecisionSelectPowerIndex = 1;
            bool skipped;
            SelectAndUsePower(legacy, out skipped);
            QuickHPCheck(-1);



        }

        [Test()]
        public void TestTakAhab_NoCardsUnder()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            Card takAhab = PutInTrash("TakAhab");

            Card baronTop = baron.TurnTaker.Deck.TopCard;
            Card raTop = ra.TurnTaker.Deck.TopCard;
            Card legacyTop = legacy.TurnTaker.Deck.TopCard;
            Card hakaTop = haka.TurnTaker.Deck.TopCard;
            Card northsparTop = northspar.TurnTaker.Deck.TopCard;

            // Whenever this card has no cards beneath it, place the top card of each hero deck beneath this one.
            PlayCard(takAhab);
            AssertUnderCard(takAhab, raTop);
            AssertUnderCard(takAhab, hakaTop);
            AssertUnderCard(takAhab, legacyTop);
            AssertOnTopOfDeck(baronTop);
            AssertOnTopOfDeck(northsparTop);

            //grab new top cards  for when we will move all cards from under him later
           baronTop = baron.TurnTaker.Deck.TopCard;
           raTop = ra.TurnTaker.Deck.TopCard;
           legacyTop = legacy.TurnTaker.Deck.TopCard;
           hakaTop = haka.TurnTaker.Deck.TopCard;
           northsparTop = northspar.TurnTaker.Deck.TopCard;

            MoveAllCards(northspar, takAhab.UnderLocation, northspar.TurnTaker.Trash);

            AssertUnderCard(takAhab, raTop);
            AssertUnderCard(takAhab, hakaTop);
            AssertUnderCard(takAhab, legacyTop);

            AssertOnTopOfDeck(baronTop);
            AssertOnTopOfDeck(northsparTop);


        }

        [Test()]
        public void TestTakAhab_EndOfTurnDiscard()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();
            // At the end of the environment turn, discard a card from beneath this one. 
            Card takAhab = PlayCard("TakAhab");
            int numCardsUnderTakAhabBefore = GetNumberOfCardsUnderCard(takAhab);
            GoToEndOfTurn(northspar);
            AssertNumberOfCardsUnderCard(takAhab, numCardsUnderTakAhabBefore - 1);
        }

        [Test()]
        public void TestTakAhab_EndOfTurnDamage_OWaypoints()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();
            // At the end of the environment turn,  discard a card from beneath this one. this card deals the target from that deck with the highest HP X irreducible sonic damage, where X = the number of Waypoints in play plus 2.
            Card takAhab = PlayCard("TakAhab");
            MoveAllCards(northspar, takAhab.UnderLocation, northspar.TurnTaker.Trash, leaveSomeCards: 1);
            Card underCard = takAhab.UnderLocation.TopCard;
            Card underCardOwnerCharacterCard = underCard.Owner.CharacterCard;
            QuickHPStorage(underCardOwnerCharacterCard);
            GoToEndOfTurn(northspar);
            QuickHPCheck(-2);

        }

        [Test()]
        public void TestTakAhab_EndOfTurnDamage_1Waypoint()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            PlayCard("MakeshiftShelter");

            // At the end of the environment turn,  discard a card from beneath this one. this card deals the target from that deck with the highest HP X irreducible sonic damage, where X = the number of Waypoints in play plus 2.
            Card takAhab = PlayCard("TakAhab");
            MoveAllCards(northspar, takAhab.UnderLocation, northspar.TurnTaker.Trash, leaveSomeCards: 1);
            Card underCard = takAhab.UnderLocation.TopCard;
            Card underCardOwnerCharacterCard = underCard.Owner.CharacterCard;
            QuickHPStorage(underCardOwnerCharacterCard);
            GoToEndOfTurn(northspar);
            QuickHPCheck(-3);

        }

        [Test()]
        public void TestTakAhab_EndOfTurnDamage_2Waypoints()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            PlayCard("MakeshiftShelter");
            PlayCard("SupplyDepot");

            // At the end of the environment turn,  discard a card from beneath this one. this card deals the target from that deck with the highest HP X irreducible sonic damage, where X = the number of Waypoints in play plus 2.
            Card takAhab = PlayCard("TakAhab");
            MoveAllCards(northspar, takAhab.UnderLocation, northspar.TurnTaker.Trash, leaveSomeCards: 1);
            Card underCard = takAhab.UnderLocation.TopCard;
            Card underCardOwnerCharacterCard = underCard.Owner.CharacterCard;
            QuickHPStorage(underCardOwnerCharacterCard);
            GoToEndOfTurn(northspar);
            QuickHPCheck(-4);

        }

        [Test()]
        public void TestAethiumVein_DestroyedByHero_TakAhabInPlay()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            Card vein = PlayCard("AethiumVein");
            Card villainTopCard = baron.TurnTaker.Deck.TopCard;
            Card takAhab = PlayCard("TakAhab");
            AssertOnTopOfDeck(villainTopCard);
            //    If this card is destroyed by a hero card and Tak Ahab is in play, place the top card of the villain deck beneath him.",
            
            DestroyCard(vein, ra.CharacterCard);
            AssertUnderCard(takAhab, villainTopCard);
        }

        [Test()]
        public void TestAethiumVein_DestroyedByNonHero_TakAhabInPlay()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            Card vein = PlayCard("AethiumVein");
            Card villainTopCard = baron.TurnTaker.Deck.TopCard;
            Card takAhab = PlayCard("TakAhab");
            AssertOnTopOfDeck(villainTopCard);
            //    If this card is destroyed by a hero card and Tak Ahab is in play, place the top card of the villain deck beneath him.",

            DestroyCard(vein, baron.CharacterCard);
            AssertOnTopOfDeck(villainTopCard);
        }

        [Test()]
        public void TestAethiumVein_DestroyedByHero_TakAhabNotInPlay()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            Card vein = PlayCard("AethiumVein");
            Card villainTopCard = baron.TurnTaker.Deck.TopCard;
            AssertOnTopOfDeck(villainTopCard);
            //  If this card is destroyed by a hero card and Tak Ahab is in play, place the top card of the villain deck beneath him.
            DestroyCard(vein, ra.CharacterCard);
            AssertOnTopOfDeck(villainTopCard);
        }

        [Test()]
        public void TestAethiumVein_StartOfTurn()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Tachyon", "Bunker", "Cauldron.Northspar");
            StartGame();

            Card vein = PlayCard("AethiumVein");
            Card takAhab = PlayCard("TakAhab");
            // At the start of the environment turn, destroy this card and Tak Ahab's end of turn effect acts twice this turn.
            AssertInPlayArea(northspar, vein);
            GoToStartOfTurn(northspar);
            AssertInTrash(vein);
            int numCardsUnderTakAhabBefore = GetNumberOfCardsUnderCard(takAhab);
            GoToEndOfTurn(northspar);
            AssertNumberOfCardsUnderCard(takAhab, numCardsUnderTakAhabBefore - 2);

            //check that it is only for this turn
            numCardsUnderTakAhabBefore = GetNumberOfCardsUnderCard(takAhab);
            GoToEndOfTurn(northspar);
            AssertNumberOfCardsUnderCard(takAhab, numCardsUnderTakAhabBefore - 1);
        }

        [Test()]
        public void TestAethiumVein_StartOfTurn_PlayTakAfter()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Tachyon", "Bunker", "Cauldron.Northspar");
            StartGame();

            Card vein = PlayCard("AethiumVein");
            // At the start of the environment turn, destroy this card and Tak Ahab's end of turn effect acts twice this turn.
            AssertInPlayArea(northspar, vein);
            GoToStartOfTurn(northspar);
            AssertInTrash(vein);
            Card takAhab = PlayCard("TakAhab");
            int numCardsUnderTakAhabBefore = GetNumberOfCardsUnderCard(takAhab);
            GoToEndOfTurn(northspar);
            AssertNumberOfCardsUnderCard(takAhab, numCardsUnderTakAhabBefore - 2);

            //check that it is only for this turn
            numCardsUnderTakAhabBefore = GetNumberOfCardsUnderCard(takAhab);
            GoToEndOfTurn(northspar);
            AssertNumberOfCardsUnderCard(takAhab, numCardsUnderTakAhabBefore - 1);
        }


        [Test()]
        public void TestAlienFungus_FrozenEntersPlay()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Tachyon", "Bunker", "Cauldron.Northspar");
            StartGame();


            Card fungus = PlayCard("AlienFungus");
            Card takAhab = PlayCard("TakAhab");

            //set all targets to have a hitpoint of 1
            SetHitPoints((Card c) => c.IsTarget, 1);

            //Whenever a Frozen card enters play, this card and Tak Ahab each regain 3HP.
            QuickHPStorage(fungus, takAhab, baron.CharacterCard, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard, tachyon.CharacterCard, bunker.CharacterCard);
            PlayCard("Frostbite");
            QuickHPCheck(3, 3, 0, 0, 0, 0, 0, 0);

            //only for frozen
            QuickHPStorage(fungus, takAhab, baron.CharacterCard, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard, tachyon.CharacterCard, bunker.CharacterCard);
            PlayCard("AethiumVein");
            QuickHPCheck(0, 0, 0, 0, 0, 0, 0, 0);
        }

        [Test()]
        public void TestAlienFungus_EndOfTurn()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Tachyon", "Bunker", "Cauldron.Northspar");
            StartGame();

            //destroy mdp so theoretically baron could be dealt damage
            DestroyCard(GetCardInPlay("MobileDefensePlatform"), baron.CharacterCard);

            Card fungus = PlayCard("AlienFungus");
            //At the end of the environment turn this card deals each hero target 2 toxic damage."
            QuickHPStorage(baron, ra, legacy, haka, tachyon, bunker);
            GoToEndOfTurn(northspar);
            QuickHPCheck(0, -2, -2, -2, -2, -2);
        }

        [Test()]
        public void TestBitterCold_EnvironmentCardEntersPlay()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            SetHitPoints(ra.CharacterCard, 20);
            SetHitPoints(legacy.CharacterCard, 25);
            SetHitPoints(haka.CharacterCard, 30);


            Card bitter = PlayCard("BitterCold");
            //Whenever an environment card enters play, this card deals the target with the third highest HP 2 cold damage.
            //3rd highest is legacy
            QuickHPStorage(baron, ra, legacy, haka);
            PlayCard("AethiumVein");
            QuickHPCheck(0, 0, -2, 0);

            //check no damage on hero
            QuickHPUpdate();
            PlayCard("Mere");
            QuickHPCheckZero();

            //check no damage on villain
            QuickHPUpdate();
            PlayCard("MobileDefensePlatform");
            QuickHPCheckZero();

        }

        [Test()]
        public void TestBitterCold_EndOfTurnEachTurn()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            DestroyCard(GetCardInPlay("MobileDefensePlatform"), baron.CharacterCard);

            Card bitter = PlayCard("BitterCold");

            //At the end of each turn, all targets that were dealt cold damage during that turn deal themselves 1 psychic damage.
            GoToPlayCardPhase(ra);
            DealDamage(haka, baron, 5, DamageType.Cold);
            DealDamage(baron, ra, 5, DamageType.Cold);
            QuickHPStorage(baron, ra, legacy, haka);
            GoToEndOfTurn(ra);
            QuickHPCheck(-1, -1, 0, 0);

            GoToPlayCardPhase(northspar);
            DealDamage(haka, legacy, 5, DamageType.Cold);
            DealDamage(ra, haka, 5, DamageType.Fire);
            QuickHPStorage(baron, ra, legacy, haka);
            GoToEndOfTurn(northspar);
            QuickHPCheck(0, 0, -1, 0);
        }

        [Test()]
        public void TestEerieStillness_ImmuneToCold()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            DestroyCard(GetCardInPlay("MobileDefensePlatform"), baron.CharacterCard);

            Card eerie = PlayCard("EerieStillness");
            Card takAhab = PlayCard("TakAhab");
            //All targets are immune to cold damage.

            //check villain targets

            //for cold
            QuickHPStorage(baron);
            DealDamage(ra, baron, 5, DamageType.Cold);
            QuickHPCheckZero();
            //for other
            QuickHPUpdate();
            DealDamage(ra, baron, 5, DamageType.Fire);
            QuickHPCheck(-5);

            //for hero

            //for cold
            QuickHPStorage(haka);
            DealDamage(ra, haka, 5, DamageType.Cold);
            QuickHPCheckZero();
            //for other
            QuickHPUpdate();
            DealDamage(ra, haka, 5, DamageType.Fire);
            QuickHPCheck(-5);

            //for env target

            //for cold
            QuickHPStorage(takAhab);
            DealDamage(ra.CharacterCard, takAhab, 5, DamageType.Cold);
            QuickHPCheckZero();
            //for other
            QuickHPUpdate();
            DealDamage(ra.CharacterCard, takAhab, 5, DamageType.Fire);
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestEerieStillness_StartOfTurnFromTrash()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            DestroyCard(GetCardInPlay("MobileDefensePlatform"), baron.CharacterCard);

            Card eerie = PlayCard("EerieStillness");
            Card makeshift = PutInTrash("MakeshiftShelter");
            //At the start of the environment turn, search the environment deck and trash for a First, Second, or Third Waypoint card and put it into play, then shuffle the deck and destroy this card.
            DecisionSelectCard = makeshift;
            DecisionSelectLocation = new LocationChoice(northspar.TurnTaker.Trash);
            QuickShuffleStorage(northspar);
            GoToStartOfTurn(northspar);
            AssertIsInPlay(makeshift);
            QuickShuffleCheck(1);
            AssertInTrash(eerie);

        }

        [Test()]
        public void TestEerieStillness_StartOfTurnFromDeck()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            DestroyCard(GetCardInPlay("MobileDefensePlatform"), baron.CharacterCard);

            Card eerie = PlayCard("EerieStillness");
            PlayCard("MakeshiftShelter");

            Card depot = PutOnDeck("SupplyDepot");
            //At the start of the environment turn, search the environment deck and trash for a First, Second, or Third Waypoint card and put it into play, then shuffle the deck and destroy this card.
            DecisionSelectCards = new Card[] { depot, haka.CharacterCard };
            DecisionSelectLocation = new LocationChoice(northspar.TurnTaker.Deck);

            QuickShuffleStorage(northspar);
            GoToStartOfTurn(northspar);
            AssertIsInPlay(depot);
            QuickShuffleCheck(1);
            AssertInTrash(eerie);

        }

        [Test()]
        public void TestEerieStillness_StartOfTurnFromDeck_ThirdWaypoint()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.Northspar");
            StartGame();

            DestroyCard(GetCardInPlay("MobileDefensePlatform"), baron.CharacterCard);

            Card eerie = PlayCard("EerieStillness");
            PlayCard("MakeshiftShelter");
            PlayCard("SupplyDepot");

            Card landing = PutOnDeck("LandingSite");
            //At the start of the environment turn, search the environment deck and trash for a First, Second, or Third Waypoint card and put it into play, then shuffle the deck and destroy this card.
            DecisionSelectCard = landing;
            DecisionSelectLocation = new LocationChoice(northspar.TurnTaker.Deck);

            QuickShuffleStorage(northspar);
            GoToStartOfTurn(northspar);
            AssertIsInPlay(landing);
            QuickShuffleCheck(1);
            AssertInTrash(eerie);

        }
        
         [Test()]
        public void TestFrostbite_EndOfTurn()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "AbsoluteZero", "Cauldron.Northspar");
            StartGame();
            SetHitPoints(az, 10);
            //swap villain targets
            DestroyCard(GetCardInPlay("MobileDefensePlatform"), baron.CharacterCard);
            Card battalion = PlayCard("BladeBattalion");

            Card frostbite = PlayCard("Frostbite");
            //play out az null point calibration to make sure it is cold damage
            PlayCard("NullPointCalibrationUnit");
            //At the end of the environment turn, this card deals the {H + 1} non-environment targets with the lowest HP 4 cold damage each and is destroyed.
            //H+1 = 4
            GoToPlayCardPhase(northspar);
            AssertInPlayArea(northspar, frostbite);
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, az.CharacterCard);
            GoToEndOfTurn(northspar);
            QuickHPCheck(0, -4, -4, -4, 4);
            AssertInTrash(frostbite);
        }


    }
}
