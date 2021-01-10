﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CauldronTests
{
    [TestFixture()]
    public class TheChasmOfAThousandNightsTests : BaseTest
    {

        #region ChasmrHelperFunctions

        protected TurnTakerController chasm { get { return FindEnvironment(); } }
        protected bool IsDjinn(Card card)
        {
            return card.DoKeywordsContain("djinn");
        }
        protected bool IsNature(Card card)
        {
            return card.DoKeywordsContain("nature");
        }

        protected Card chasmCard { get { return FindCardsWhere(c => c.Identifier == "TheChasmOfAThousandNights", realCardsOnly: false).First(); } }

        #endregion

        [Test()]
        public void TestChasmWorks()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights");
            StartGame();
            Assert.AreEqual(5, this.GameController.TurnTakerControllers.Count());

        }

        [Test()]
        [Sequential]
        public void DecklistTest_Nature_IsNature([Values("Noxious", "Vicious", "SurprisinglyAgile", "Thieving", "RatherFriendly", "Malevolent")] string nature)
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights");
            StartGame();

            GoToPlayCardPhase(chasm);
            //play a djinn so we have something to go to
            Card djinn = PlayCard("HighTemoq");
            Card card = GetCard(nature);
            if (!card.IsInPlayAndHasGameText)
            {
                FlipCard(card);
                MoveCard(chasm, card, djinn.BelowLocation);
            }
            AssertIsInPlay(card);
            AssertCardHasKeyword(card, "nature", false);
        }
        [Test()]
        [Sequential]
        public void DecklistTest_Djinn_IsDjinn([Values("GrandAmaraqiel", "HighMhegas", "HighTemoq", "Axion", "Tevael", "Gul")] string djinn
            )
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights");
            StartGame();

            GoToPlayCardPhase(chasm);

            Card card = PlayCard(djinn);

            AssertIsInPlay(card);
            AssertCardHasKeyword(card, "djinn", false);
        }

        [Test()]
        [Sequential]
        public void DecklistTest_NoKeyword_HasNoKeyword([Values("BeyondTheVeil", "IreOfTheDjinn", "CrumblingRuins")] string keywordLess)
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights");
            StartGame();

            GoToPlayCardPhase(chasm);

            Card card = PlayCard(keywordLess);
            AssertIsInPlay(card);
            Assert.IsFalse(card.Definition.Keywords.Any(), $"{card.Title} has keywords when it shouldn't.");
        }
        [Test()]
        public void TestNaturesReturnWhenAbandoned_NextToDestroyed()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights");
            StartGame();

            Card djinn = PlayCard("HighTemoq");
            Card nature = djinn.BelowLocation.TopCard;
            AssertNotFlipped(nature);
            AssertBelowCard(nature, djinn);
            DestroyCard(djinn, baron.CharacterCard);
            AssertUnderCard(chasmCard, nature);
            AssertFlipped(nature);

        }

        [Test()]
        public void TestNaturesReturnWhenAbandoned_NextToReturn()
        {
            SetupGameController("BaronBlade", "Ra", "Legacy/FreedomFiveLegacyCharacter", "Haka", "Cauldron.TheChasmOfAThousandNights");
            StartGame();

            Card djinn = PlayCard("HighTemoq");
            Card nature = djinn.BelowLocation.TopCard;
            AssertNotFlipped(nature);
            AssertBelowCard(nature, djinn);
            DecisionSelectFunction = 0;
            DecisionSelectCard = djinn;
            UsePower(legacy);
            AssertUnderCard(chasmCard, nature);
            AssertFlipped(nature);

        }

        [Test()]
        public void TestAxion()
        {
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights" });
            StartGame();
            DestroyNonCharacterVillainCards();
            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("Axion");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard);
            //At the end of the environment turn, this card deals each villain target 1 melee damage.
            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            GoToEndOfTurn(chasm);
            switch (nature.Identifier)
            {
                case "Noxious":
                    QuickHPCheck(-1, -1, -1, -1, -1);
                    break;
                case "Vicious":
                    QuickHPCheck(-2, -2, 0, 0, 0);
                    break;
                case "RatherFriendly":
                    QuickHPCheck(-2, 0, 0, 0, 0);
                    break;
                case "Malevolent":
                    QuickHPCheck(0, 0, 0, -2, 0);
                    break;
                default:
                    QuickHPCheck(-1, -1, 0, 0, 0);
                    break;
            }

        }

        [Test()]
        public void TestGrandAmaraqiel()
        {
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights" });
            StartGame();
            DestroyNonCharacterVillainCards();
            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("GrandAmaraqiel");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard);
            //At the end of the environment turn, this card deals the 2 non-environment targets with the highest HP 3 cold damage each.
            //baron and legacy
            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            GoToEndOfTurn(chasm);
            switch (nature.Identifier)
            {
                case "Noxious":
                    QuickHPCheck(-3, 0, -1, -4, -1);
                    break;
                case "Vicious":
                    QuickHPCheck(-4, 0, 0, -4, 0);
                    break;
                case "RatherFriendly":
                    QuickHPCheck(-6, 0, 0, 0, 0);
                    break;
                case "Malevolent":
                    QuickHPCheck(0, 0, -3, -3, 0);
                    break;
                default:
                    QuickHPCheck(-3, 0, 0, -3, 0);
                    break;
            }

        }

        [Test()]
        public void TestGul_EndOfTurn()
        {
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights" });
            StartGame();
            DestroyNonCharacterVillainCards();
            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("Gul");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard);
            //At the end of the environment turn, this card deals the non-environment target with the lowest HP 3 infernal damage.
            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            GoToEndOfTurn(chasm);
            switch (nature.Identifier)
            {
                case "Noxious":
                    QuickHPCheck(0, -3, -1, -1, -1);
                    break;
                case "Vicious":
                    QuickHPCheck(0, -4, 0, 0, 0);
                    break;
                case "RatherFriendly":
                    QuickHPCheck(-3, 0, 0, 0, 0);
                    break;
                case "Malevolent":
                    QuickHPCheck(0, 0, 0, -3, 0);
                    break;
                default:
                    QuickHPCheck(0, -3, 0, 0, 0);
                    break;
            }

        }

        [Test()]
        public void TestGul_DestroyTarget()
        {
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights" });
            StartGame();
            DestroyNonCharacterVillainCards();
            SetHitPoints(ra, 10);
            SetHitPoints(legacy, 10);

            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("Gul");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard, djinn);
            //Whenever this card destroys a target, it deals each target other than itself 1 cold damage.
            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            DestroyCard(battalion, djinn);
            switch (nature.Identifier)
            {
                case "Vicious":
                    QuickHPCheck(-2, -2, -2, -2, 0);
                    break;
                case "RatherFriendly":
                    QuickHPCheck(-5, 0, 0, 0, 0);
                    break;
                case "Malevolent":
                    QuickHPCheck(0, 0, 0, -5, 0);
                    break;
                default:
                    QuickHPCheck(-1, -1, -1, -1, 0);
                    break;
            }

        }

        [Test()]
        public void TestHighMhegas()
        {
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Tachyon", "Cauldron.TheChasmOfAThousandNights" });
            StartGame();
            DestroyNonCharacterVillainCards();
            SetHitPoints(tachyon, 5);
            SetHitPoints(ra, 15);
            SetHitPoints(legacy, 10);
            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("HighMhegas");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard, tachyon.CharacterCard);
            //At the end of the environment turn, this card deals the 3 hero targets with the highest HP 2 melee damage each.
            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            GoToEndOfTurn(chasm);
            switch (nature.Identifier)
            {
                case "Noxious":
                    QuickHPCheck(0, 0, -3, -3, -3, -1);
                    break;
                case "Vicious":
                    QuickHPCheck(0, 0, -3, -3, -3, 0);
                    break;
                case "RatherFriendly":
                    QuickHPCheck(-6, 0, 0, 0, 0, 0);
                    break;
                case "Malevolent":
                    QuickHPCheck(0, 0, 0, 0, -6, 0);
                    break;
                default:
                    QuickHPCheck(0, 0, -2, -2, -2, 0);
                    break;
            }

        }

        [Test()]
        public void TestHighTemoq()
        {
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Tachyon", "Cauldron.TheChasmOfAThousandNights" });
            StartGame();
            DestroyNonCharacterVillainCards();
            SetHitPoints(tachyon, 6);
            SetHitPoints(ra, 15);
            SetHitPoints(legacy, 10);
            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("HighTemoq");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard, tachyon.CharacterCard);
            //At the end of the environment turn, this card deals the 2 non-environment targets with the lowest HP 2 fire damage each.
            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            GoToEndOfTurn(chasm);
            switch (nature.Identifier)
            {
                case "Noxious":
                    QuickHPCheck(0, -2, -1, -1, -1, -3);
                    break;
                case "Vicious":
                    QuickHPCheck(0, -3, 0, 0, 0, -3);
                    break;
                case "RatherFriendly":
                    QuickHPCheck(-4, 0, 0, 0, 0, 0);
                    break;
                case "Malevolent":
                    QuickHPCheck(0, 0, 0, 0, -4, 0);
                    break;
                default:
                    QuickHPCheck(0, -2, 0, 0, 0, -2);
                    break;
            }

        }

        [Test()]
        public void TestTevael_EndOfTurn()
        {
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Tachyon", "Cauldron.TheChasmOfAThousandNights" });
            StartGame();
            DestroyNonCharacterVillainCards();
            SetHitPoints(tachyon, 6);
            SetHitPoints(ra, 15);
            SetHitPoints(legacy, 10);
            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("Tevael");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard, tachyon.CharacterCard);
            //At the end of the environment turn, this card deals the non-environment target with the third highest HP 4 projectile damage.
            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            GoToEndOfTurn(chasm);
            switch (nature.Identifier)
            {
                case "Noxious":
                    QuickHPCheck(0, 0, -5, -1, -1, -1);
                    break;
                case "Vicious":
                    QuickHPCheck(0, 0, -5, 0, 0, 0);
                    break;
                case "RatherFriendly":
                    QuickHPCheck(-4, 0, 0, 0, 0, 0);
                    break;
                case "Malevolent":
                    QuickHPCheck(0, 0, 0, 0, -4, 0);
                    break;
                default:
                    QuickHPCheck(0, 0, -4, 0, 0, 0);
                    break;
            }

        }

        [Test()]
        public void TestTevael_Indestructible()
        {
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Tachyon", "Cauldron.TheChasmOfAThousandNights" });
            StartGame();
            DestroyNonCharacterVillainCards();
            
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("Tevael");
            Card nature = djinn.BelowLocation.TopCard;
            DestroyCard(djinn, baron.CharacterCard);
            AssertIsInPlay(djinn);
            DealDamage(baron, djinn, 10000, DamageType.Melee);
            AssertInTrash(djinn);

        }

        [Test()]
        public void TestNoxious()
        {
            //NOTE: This test uses a random seed to guarantee that the nature that goes below Axion is Noxious
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights" }, randomSeed: new int?(-1885072287));
            StartGame();
            DestroyNonCharacterVillainCards();
            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("Axion");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard);
            //At the end of the environment turn, this card deals each villain target 1 melee damage.
            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            GoToEndOfTurn(chasm);
            QuickHPCheck(-1, -1, -1, -1, -1);
        }

        [Test()]
        public void TestVicious()
        {
            //NOTE: This test uses a random seed to guarantee that the nature that goes below Axion is Vicious
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights" }, randomSeed: new int?(-807106660));
            StartGame();
            DestroyNonCharacterVillainCards();
            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("Axion");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard);
            //At the end of the environment turn, this card deals each villain target 1 melee damage.
            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            GoToEndOfTurn(chasm);
            QuickHPCheck(-2, -2, 0, 0, 0);
        }

        [Test()]
        public void TestMalevolent()
        {
            //NOTE: This test uses a random seed to guarantee that the nature that goes below Axion is Malevolent
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights" }, randomSeed: new int?(1181223121));
            StartGame();
            DestroyNonCharacterVillainCards();
            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("Axion");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard);
            //At the end of the environment turn, this card deals each villain target 1 melee damage.
            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            GoToEndOfTurn(chasm);
            QuickHPCheck(0, 0, 0, -2, 0);
        }

        [Test()]
        public void TestSurprisinglyAgile()
        {
            //NOTE: This test uses a random seed to guarantee that the nature that goes below Axion is Surprisingly Agile
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights" }, randomSeed: new int?(-1130753949));
            StartGame();
            DestroyNonCharacterVillainCards();
            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("Axion");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard, djinn);
            //Reduce damage dealt to the target next to this card by 1.
            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            DealDamage(ra, (Card c) => c.IsTarget, 2, DamageType.Fire);
            QuickHPCheck(-2, -2, -2, -2, -2, -1);
        }

        [Test()]
        public void TestRatherFriendly()
        {
            //NOTE: This test uses a random seed to guarantee that the nature that goes below Axion is Rather Friendly
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights" }, randomSeed: new int?(1983606371));
            StartGame();
            DestroyNonCharacterVillainCards();
            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("Axion");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard);
            //At the end of the environment turn, this card deals each villain target 1 melee damage.
            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            GoToEndOfTurn(chasm);
            QuickHPCheck(-2, 0, 0, 0, 0);

        }

        [Test()]
        public void TestThieving()
        {
            //NOTE: This test uses a random seed to guarantee that the nature that goes below Grand Amaraqiel is Thieving
            SetupGameController(new string[] { "BaronBlade", "Ra", "Legacy", "Haka", "Cauldron.TheChasmOfAThousandNights" }, randomSeed: new int?(1599956386));
            StartGame();
            DestroyNonCharacterVillainCards();
            Card battalion = PlayCard("BladeBattalion");
            GoToPlayCardPhase(chasm);
            Card djinn = PlayCard("GrandAmaraqiel");
            Card nature = djinn.BelowLocation.TopCard;
            QuickHPStorage(baron.CharacterCard, battalion, ra.CharacterCard, legacy.CharacterCard, haka.CharacterCard);
            //At the end of the environment turn, this card deals the 2 non-environment targets with the highest HP 3 cold damage each.
            //baron and legacy

            PrintSeparator("Checking Triggers In Play");
            PrintTriggers();
            PrintSeparator("Finished Checking Triggers");
            QuickHandStorage(ra, legacy, haka);
            GoToEndOfTurn(chasm);
            //Whenever the target next to this card deals damage to a hero, that hero must discard a card.
            QuickHandCheck(0, -1, 0);
        }
    }

}
