using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.Drift
{
    public abstract class DualShiftTrackUtilityCardController : ShiftTrackUtilityCardController
    {
        protected DualShiftTrackUtilityCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(() => $"{this.GetInactiveCharacterCard().AlternateTitleOrTitle} is the inactive {{Drift}}; she is at position {this.InactiveCharacterPosition()}.").Condition = () => GetActiveCharacterCard() != null;
            base.SpecialStringMaker.ShowIfElseSpecialString(() => this.HasTrackAbilityBeenActivated(), () => "Drift has changed the active character this turn.", () => "Drift has not changed the active character this turn.", showInEffectsList: () => true).Condition = () => GetActiveCharacterCard() != null;
        }

        protected enum CustomMode
        {
            ManageAskToSwapMain,
            ManageAskToSwapFromShift,
            ManageAskToSwapFromDamage,
            ManageAskToSwapFromPlay,
            ManageAskToSwapFromPower,

            AskToSwapFromShift, // this is handled separately
            AskToSwapFromDamage,
            AskToSwapFromPlay,
            AskToSwapFromPower
        }

        protected enum SwapCondition
        {
            SwapFromShift,
            SwapFromDamage,
            SwapFromPlay,
            SwapFromPower
        }

        private static readonly Dictionary<SwapCondition, string> EnableSwapConditionKeys = new Dictionary<SwapCondition, string>
        {
            { SwapCondition.SwapFromShift, "AskToSwapFromShift" },
            { SwapCondition.SwapFromDamage, "AskToSwapFromDamage" },
            { SwapCondition.SwapFromPlay, "AskToSwapFromPlay" },
            { SwapCondition.SwapFromPower, "AskToSwapFromPower" }
        };

        private static readonly Dictionary<SwapCondition, CustomMode> SwapConditionManagePromptKeys = new Dictionary<SwapCondition, CustomMode>
        {
            { SwapCondition.SwapFromShift, CustomMode.ManageAskToSwapFromShift },
            { SwapCondition.SwapFromDamage, CustomMode.ManageAskToSwapFromDamage },
            { SwapCondition.SwapFromPlay, CustomMode.ManageAskToSwapFromPlay },
            { SwapCondition.SwapFromPower, CustomMode.ManageAskToSwapFromPower }
        };

        private static readonly Dictionary<SwapCondition, CustomMode> SwapConditionDisplayPromptKeys = new Dictionary<SwapCondition, CustomMode>
        {
            { SwapCondition.SwapFromShift, CustomMode.AskToSwapFromShift },
            { SwapCondition.SwapFromDamage, CustomMode.AskToSwapFromDamage },
            { SwapCondition.SwapFromPlay, CustomMode.AskToSwapFromPlay },
            { SwapCondition.SwapFromPower, CustomMode.AskToSwapFromPower }
        };

        private Card customTextCardBeingPlayed { get; set; }
        private string customTextPowerDescriptor { get; set; }
        private Card customTextCardWithPower { get; set; }

        private CustomMode customMode { get; set; }

        protected const string DriftPosition = "DriftPosition";
        protected const string OncePerTurn = "DualShiftAbilityOncePerTurn";
        public override bool AllowFastCoroutinesDuringPretend => false;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // Once per turn when Drift would shift, play a card, use a power, or be dealt damage, you may do the following in order
            //1. Place your active character on your current shift track space.
            //2. Place the shift token on your inactive character's shift track space.
            //3. Switch which character is active.

            //shift trigger happens in the actual shifting logic
            base.AddTrigger<DealDamageAction>((DealDamageAction action) => Game.HasGameStarted && !this.HasTrackAbilityBeenActivated() && action.Target == GetActiveCharacterCard(), this.TrackResponse, TriggerType.ModifyTokens, TriggerTiming.Before);

            string[] noResponseOnPlayIdentifiers = new string[] { "ShiftTrack", "FutureDrift", "PastDrift" };
            base.AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cpa) => cpa.CardEnteringPlay != null && !this.HasTrackAbilityBeenActivated() && cpa.CardEnteringPlay.Owner == TurnTakerControllerWithoutReplacements.TurnTaker && noResponseOnPlayIdentifiers.All(id => !cpa.CardEnteringPlay.Identifier.Contains(id)), this.TrackResponse, TriggerType.ModifyTokens, TriggerTiming.Before);

            //since both character card powers shift Drift, the player would be prompted anyway so there's no reason to prompt early
            string[] noResponseOnPowerIdentifiers = new string[] { "FutureDrift", "PastDrift" };

            base.AddTrigger<UsePowerAction>((UsePowerAction upa) => upa.Power.CardSource.Card != null && !this.HasTrackAbilityBeenActivated() && upa.Power.CardSource.Card.Owner == TurnTakerControllerWithoutReplacements.TurnTaker && noResponseOnPowerIdentifiers.All(id => !upa.Power.CardSource.Card.Identifier.Contains(id)), this.TrackResponse, TriggerType.ModifyTokens, TriggerTiming.Before);

            base.AddStartOfTurnTrigger(tt => tt == base.TurnTaker, StartOfTurnResponse, TriggerType.Other);
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine = this.ManageAskToSwapSettings();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public IEnumerator StartOfTurnResponse (PhaseChangeAction pca)
        {
            IEnumerator coroutine = this.ManageAskToSwapSettings();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }
        private void SetSwapPromptConditionEnabled(SwapCondition swapCondition, bool value)
        {
            Log.Debug($"CC = {GameController.FindCard("DriftCharacter").Identifier}, Card = {Card.Identifier}, swapCondition = {swapCondition}, Journal.GetCardPropertiesBoolean(Card, EnableSwapConditionKeys[swapCondition]) is {Journal.GetCardPropertiesBoolean(Card, EnableSwapConditionKeys[swapCondition])}");
            GameController.AddCardPropertyJournalEntry(GameController.FindCard("DriftCharacter"), EnableSwapConditionKeys[swapCondition], boolValue: value);
        }

        private bool IsSwapPromptConditionEnabled(SwapCondition swapCondition)
        {
            Log.Debug($"CC = {GameController.FindCard("DriftCharacter").Identifier}, Card = {Card.Identifier}, swapCondition = {swapCondition}, Journal.GetCardPropertiesBoolean(Card, EnableSwapConditionKeys[swapCondition]) is {Journal.GetCardPropertiesBoolean(Card, EnableSwapConditionKeys[swapCondition])}");
            return Journal.GetCardPropertiesBoolean(GameController.FindCard("DriftCharacter"), EnableSwapConditionKeys[swapCondition]) != false;
        }

        public bool ReceiveSwapPromptsWhenShifting()
        {
            return IsSwapPromptConditionEnabled(SwapCondition.SwapFromShift);
        }

        private IEnumerator ManageAskToSwapSetting(SwapCondition swapCondition)
        {
            customMode = SwapConditionManagePromptKeys[swapCondition];
            List<YesNoCardDecision> manageDecision = new List<YesNoCardDecision>();
            YesNoCardDecision decision = new YesNoCardDecision(GameController, HeroTurnTakerController, SelectionType.Custom, Card, action: null, associatedCards: null, cardSource: GetCardSource());
            //decision.ExtraInfo = () => $"{GetInactiveCharacterCard().Title} is at position {InactiveCharacterPosition()}";
            manageDecision.Add(decision);
            IEnumerator coroutine = GameController.MakeDecisionAction(decision);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            SetSwapPromptConditionEnabled(swapCondition, base.DidPlayerAnswerYes(manageDecision.FirstOrDefault()));
        }

        public IEnumerator ManageAskToSwapSettings()
        {
            customMode = CustomMode.ManageAskToSwapMain;
            List<SelectWordDecision> storedResults = new List<SelectWordDecision>();
            string optionKeep = "Keep my current settings";
            string optionManange = "Manage settings";
            string optionDisableAll = "Never ask to swap character cards";
            string optionEnableAll = "Always ask to swap character cards";
            List<string> options = new List<string> { optionKeep, optionManange, optionDisableAll, optionEnableAll };
            SelectWordDecision decision = new SelectWordDecision(GameController, HeroTurnTakerController, SelectionType.Custom, options, cardSource: GetCardSource(), associatedCards: GetShiftTrack().ToEnumerable());
            decision.ExtraInfo = () => "This takes effect until the start of Drift's next turn.";
            storedResults.Add(decision);
            IEnumerator coroutine = GameController.MakeDecisionAction(decision);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (!DidSelectWord(storedResults))
            {
                // do nothing and return
                yield break;
            }
            string selectedWord = GetSelectedWord(storedResults);
            if (selectedWord == optionKeep)
            {
                // do nothing and return
                yield break;
            }
            else if (selectedWord == optionManange)
            {
                foreach (SwapCondition swapCondition in Enum.GetValues(typeof(SwapCondition)))
                {
                    var settingCoroutine = this.ManageAskToSwapSetting(swapCondition);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(settingCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(settingCoroutine);
                    }
                }
            }
            else if (selectedWord == optionDisableAll)
            {
                foreach (SwapCondition swapCondition in Enum.GetValues(typeof(SwapCondition)))
                {
                    SetSwapPromptConditionEnabled(swapCondition, false);
                }
            }
            else
            {
                foreach (SwapCondition swapCondition in Enum.GetValues(typeof(SwapCondition)))
                {
                    SetSwapPromptConditionEnabled(swapCondition, true);
                }
            }
        }

        public bool HasTrackAbilityBeenActivated()
        {
            IEnumerable<CardPropertiesJournalEntry> trackEntries = base.Journal.CardPropertiesEntries((CardPropertiesJournalEntry entry) => entry.Key == OncePerTurn && entry.Card.SharedIdentifier == ShiftTrack && entry.TurnIndex == base.Game.TurnIndex);
            return trackEntries.Any();
        }

        private IEnumerator TrackResponse(GameAction action)
        {
            SwapCondition swapCondition;
            List<YesNoCardDecision> switchDecision = new List<YesNoCardDecision>();
            if (action is CardEntersPlayAction cpa)
            {
                swapCondition = SwapCondition.SwapFromPlay;
                customTextCardBeingPlayed = cpa.CardEnteringPlay;
            }
            else if (action is UsePowerAction upa)
            {
                swapCondition = SwapCondition.SwapFromPower;
                if (upa.Power.Description.StartsWith(Past))
                {
                    customTextPowerDescriptor = $"{{{Past}}} ";
                }
                else if (upa.Power.Description.StartsWith(Future))
                {
                    customTextPowerDescriptor = $"{{{Future}}} ";
                }
                else
                {
                    customTextPowerDescriptor = "";
                }
                customTextCardWithPower = upa.Power.CardSource.Card;
            }
            else
            {
                swapCondition = SwapCondition.SwapFromDamage;
            }

            if (IsSwapPromptConditionEnabled(swapCondition))
            {
                customMode = SwapConditionDisplayPromptKeys[swapCondition];
                YesNoCardDecision decision = new YesNoCardDecision(GameController, HeroTurnTakerController, SelectionType.Custom, Card, action: action is DealDamageAction ? action : null, associatedCards: GetInactiveCharacterCard().ToEnumerable(), cardSource: GetCardSource());
                decision.ExtraInfo = () => $"{GetInactiveCharacterCard().Title} is at position {InactiveCharacterPosition()}";
                switchDecision.Add(decision);
                IEnumerator coroutine = GameController.MakeDecisionAction(decision);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                if (base.DidPlayerAnswerYes(switchDecision.FirstOrDefault()))
                {
                    if(action.IsPretend)
                    { 
                        yield break;
                    }
                    coroutine = SwapActiveDrift();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    if (action is DealDamageAction dealDamageAction)
                    {
                        coroutine = RedirectDamage(dealDamageAction, TargetType.SelectTarget, c => c == GetActiveCharacterCard());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                    }
                }
            }
        }

        public IEnumerator SwapActiveDrift()
        {
            IEnumerator coroutine = null;

            //put in a escape hatch if a swap has happened this turn after selecting to swap
            if (this.HasTrackAbilityBeenActivated())
            {
                coroutine = GameController.SendMessageAction("Drift has already swapped characters this turn...", Priority.Medium, GetCardSource(), showCardSource: true);
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

            //Once per turn you may do the following in order:
            base.SetCardPropertyToTrueIfRealAction(OncePerTurn);
            IEnumerable<CardPropertiesJournalEntry> trackEntries = base.Journal.CardPropertiesEntries((CardPropertiesJournalEntry entry) => entry.Key == OncePerTurn && entry.Card.SharedIdentifier == ShiftTrack);

            int inactivePosition = this.InactiveCharacterPosition();
            base.SetCardProperty(DriftPosition + inactivePosition, false);

            //1. Place your active character on your current shift track space.
            base.SetCardProperty(DriftPosition + this.CurrentShiftPosition(), true);

            //2. Place the shift token on your inactive character's shift track space.
            if (this.CurrentShiftPosition() < inactivePosition)
            {
                AddTokensToPoolAction addTokensAction = new AddTokensToPoolAction(GetCardSource(), GetShiftPool(), inactivePosition - CurrentShiftPosition());
                addTokensAction.AllowTriggersToRespond = false;
                addTokensAction.CanBeCancelled = false;
                coroutine = DoAction(addTokensAction);
            }
            else if (this.CurrentShiftPosition() > inactivePosition)
            {
                RemoveTokensFromPoolAction removeTokensAction = new RemoveTokensFromPoolAction(GetCardSource(), GetShiftPool(), CurrentShiftPosition() - inactivePosition);
                removeTokensAction.AllowTriggersToRespond = false;
                removeTokensAction.CanBeCancelled = false;
                coroutine = DoAction(removeTokensAction);
            }

            if (coroutine != null)
            {
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                //Switch visual
                coroutine = base.GameController.SwitchCards(this.GetShiftTrack(), base.FindCard(Dual + ShiftTrack + this.CurrentShiftPosition(), false), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

            }
            
            Card switchFromDrift = GetActiveCharacterCard();
            Card switchToDrift = GetInactiveCharacterCard(inactivePosition);
            //3. Switch which character is active.
            coroutine = GameController.SwitchCards(switchFromDrift, switchToDrift, ignoreHitPoints: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            yield return DoNothing();
        }

        public Card GetInactiveCharacterCard(int inactivePosition = 0)
        {
            inactivePosition = inactivePosition == 0 ? InactiveCharacterPosition() : inactivePosition;
            string activeBaseIdentifier = GetActiveCharacterCard().Identifier.Replace("Red", "").Replace("Blue", "");
            string desiredIdentifier;
            if(activeBaseIdentifier.Contains("Past"))
            {
                desiredIdentifier = activeBaseIdentifier.Replace("Past", "Future");
            } else
            {
                desiredIdentifier = activeBaseIdentifier.Replace("Future", "Past");
            }
            return base.FindCardsWhere(new LinqCardCriteria((Card c) => c.Location == base.TurnTaker.OffToTheSide && c.Owner == base.TurnTaker && c.Identifier == desiredIdentifier)).FirstOrDefault();
        }

        public int InactiveCharacterPosition()
        {
            int inactivePosition = 0;
            string[] inactivePositions = new string[] {
                DriftPosition + 1,
                DriftPosition + 2,
                DriftPosition + 3,
                DriftPosition + 4
            };
            //Get all entries of any of the positions being toggled
            IEnumerable<CardPropertiesJournalEntry> positionEntries = base.Journal.CardPropertiesEntries((CardPropertiesJournalEntry entry) => entry.Card.SharedIdentifier == ShiftTrack && inactivePositions.Contains(entry.Key));

            //If a position is set to true then there is an odd number of entries (true), if the position is set to false then there is an even number of entries (true + false)
            if (positionEntries.Where((CardPropertiesJournalEntry entry) => entry.Key == DriftPosition + 1).Count() % 2 == 1)
            {
                inactivePosition = 1;
            }
            if (positionEntries.Where((CardPropertiesJournalEntry entry) => entry.Key == DriftPosition + 2).Count() % 2 == 1)
            {
                inactivePosition = 2;
            }
            if (positionEntries.Where((CardPropertiesJournalEntry entry) => entry.Key == DriftPosition + 3).Count() % 2 == 1)
            {
                inactivePosition = 3;
            }
            if (positionEntries.Where((CardPropertiesJournalEntry entry) => entry.Key == DriftPosition + 4).Count() % 2 == 1)
            {
                inactivePosition = 4;
            }
            return inactivePosition;
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {

            if (customMode == CustomMode.ManageAskToSwapMain)
            {
                return new CustomDecisionText
                (
                    "When would you like to receive swap prompts?",
                    "When should they receive swap prompts?",
                    "Vote for when they should receive swap prompts", 
                    "receive swap prompts"
                );
            }

            if (customMode == CustomMode.ManageAskToSwapFromShift)
            {
                return new CustomDecisionText
                (
                    "Would you like swap prompts when moving on the Shift Track?", 
                    "Should they receive swap prompts when moving on the Shift Track?", 
                    "Vote for if they should receive swap prompts when moving on the Shift Track", 
                    "receive swap prompts when moving on the Shift Track"
                );
            }

            if (customMode == CustomMode.ManageAskToSwapFromDamage)
            {
                return new CustomDecisionText
                (
                    "Would you like swap prompts when you would be dealt damage?", 
                    "Should they receive swap prompts when they would be dealt damage?", 
                    "Vote for if they should receive swap prompts when they would be dealt damage", 
                    "receive swap prompts when they would be dealt damage"
                );
            }

            if (customMode == CustomMode.ManageAskToSwapFromPlay)
            {
                return new CustomDecisionText
                (
                    "Would you like swap prompts when you play a card?", 
                    "Should they receive swap prompts when they play a card?", 
                    "Vote for if they should receive swap prompts when they play a card", 
                    "receive swap prompts when playing a card"
                );
            }

            if (customMode == CustomMode.ManageAskToSwapFromPower)
            {
                return new CustomDecisionText
                (
                    "Would you like swap prompts when you use a power?", 
                    "Should they receive swap prompts when they use a power?", 
                    "Vote for if they should receive swap prompts when they use a power", 
                    "receive swap prompts when using"
                );
            }

            if (customMode == CustomMode.AskToSwapFromDamage)
            {
                return new CustomDecisionText("Do you want to switch character cards?", "Should they switch character cards?", "Vote for if they should switch character cards?", "switching character cards");
            }

            if(customMode == CustomMode.AskToSwapFromPlay)
            {
                return new CustomDecisionText($"Do you want to switch character cards before playing [b]{customTextCardBeingPlayed.Title}[/b]?", $"Should they switch character cards before playing [b]{customTextCardBeingPlayed.Title}[/b]?", $"Vote for if they should switch character cards before playing [b]{customTextCardBeingPlayed.Title}[/b]?", $"switching character cards before playing [b]{customTextCardBeingPlayed.Title}[/b]");
            }

            if(customMode == CustomMode.AskToSwapFromPower)
            {
                return new CustomDecisionText($"Do you want to switch character cards before using the {customTextPowerDescriptor}power on [b]{customTextCardWithPower.Title}[/b]?", $"Should they switch character cards before using the {customTextPowerDescriptor}power on [b]{customTextCardWithPower.Title}[/b]?", $"Vote for if they should switch character cardsbefore using the {customTextPowerDescriptor}power on [b]{customTextCardWithPower.Title}[/b]", $"switching character cards before using the {customTextPowerDescriptor}power on [b]{customTextCardWithPower.Title}[/b]");
            }

            return base.GetCustomDecisionText(decision);
        }

        
    }
}
