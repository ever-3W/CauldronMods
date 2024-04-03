using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.MagnificentMara
{

    public class YouCanDoThatTooCardController : CardController
    {
        private Power _powerChosen;

        private ITrigger _makeDecisionTrigger;

        private Dictionary<Power, CardSource> _cardSources;

        public YouCanDoThatTooCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            AddThisCardControllerToList(CardControllerListType.ReplacesCards);
            AddThisCardControllerToList(CardControllerListType.ReplacesTurnTakerController);
            AddThisCardControllerToList(CardControllerListType.ReplacesCardSource);
            _cardSources = new Dictionary<Power, CardSource>();
            _makeDecisionTrigger = AddTrigger((MakeDecisionAction d) => d.Decision is UsePowerDecision && d.Decision.CardSource.CardController == this, PowerChosenResponse, new TriggerType[1] { TriggerType.Hidden }, TriggerTiming.After);
            AddInhibitorException((GameAction gc) => false);
            IEnumerator coroutine = base.GameController.SelectAndUsePower(DecisionMaker, optional: false, (Power p) => p.CardController.Card.IsHeroCharacterCard && !p.IsContributionFromCardSource, 1, eliminateUsedPowers: true, null, showMessage: false, allowAnyHeroPower: true, allowReplacements: true, canBeCancelled: true, null, forceDecision: true, allowOutOfPlayPower: false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            RemoveTrigger(_makeDecisionTrigger);
            if (_powerChosen != null && _powerChosen.CardSource != null)
            {
                _powerChosen.CardSource.CardController.OverrideAllowReplacements = null;
            }
            _powerChosen = null;
            _cardSources = null;
            RemoveThisCardControllerFromList(CardControllerListType.ReplacesCards);
            RemoveThisCardControllerFromList(CardControllerListType.ReplacesTurnTakerController);
            RemoveThisCardControllerFromList(CardControllerListType.ReplacesCardSource);
            RemoveInhibitorException();
        }

        private IEnumerator PowerChosenResponse(MakeDecisionAction d)
        {
            if (d != null)
            {
                _powerChosen = (d.Decision as UsePowerDecision).SelectedPower;
                if (_powerChosen != null)
                {
                    _powerChosen.TurnTakerController = base.TurnTakerController;
                    if (_powerChosen.CardSource != null)
                    {
                        _powerChosen.CardSource.AddAssociatedCardSource(GetCardSource());
                        _powerChosen.CardSource.CardController.OverrideAllowReplacements = true;
                    }
                }
            }
            yield return null;
        }

        public override Card AskIfCardIsReplaced(Card card, CardSource cardSource)
        {
            if (_powerChosen != null && card.IsHeroCharacterCard && cardSource.AllowReplacements)
            {
                CardController cardController = _powerChosen.CardController;
                IEnumerable<CardController> source = cardSource.CardSourceChain.Select((CardSource cs) => cs.CardController);
                bool flag = _powerChosen == cardSource.PowerSource || (cardSource.PowerSource == null && _powerChosen.CardController.CardWithoutReplacements == cardSource.CardController.CardWithoutReplacements);
                if (source.Contains(cardController) && source.Contains(this) && flag && card == cardSource.CardController.CardWithoutReplacements)
                {
                    return base.CharacterCard;
                }
            }
            return null;
        }

        public override TurnTakerController AskIfTurnTakerControllerIsReplaced(TurnTakerController ttc, CardSource cardSource)
        {
            if (_powerChosen != null && cardSource.AllowReplacements)
            {
                _ = _powerChosen.CardController.CardWithoutReplacements;
                TurnTakerController turnTakerControllerWithoutReplacements = _powerChosen.CardController.TurnTakerControllerWithoutReplacements;
                if (ttc == turnTakerControllerWithoutReplacements)
                {
                    CardController cardController = _powerChosen.CardController;
                    IEnumerable<CardController> source = cardSource.CardSourceChain.Select((CardSource cs) => cs.CardController);
                    bool flag = _powerChosen == cardSource.PowerSource || (cardSource.PowerSource == null && _powerChosen.CardController.CardWithoutReplacements == cardSource.CardController.CardWithoutReplacements);
                    if (source.Contains(cardController) && source.Contains(this) && flag)
                    {
                        // this line is the only difference between this class and Controller.Guise.ICanDoThatTooController
                        return base.TurnTakerController;
                    }
                }
            }
            return null;
        }

        public override CardSource AskIfCardSourceIsReplaced(CardSource cardSource, GameAction gameAction = null, ITrigger trigger = null)
        {
            if (_powerChosen != null && cardSource.AllowReplacements && _powerChosen.CardSource.CardController.CardWithoutReplacements == cardSource.CardController.CardWithoutReplacements)
            {
                cardSource.AddAssociatedCardSource(GetCardSource());
                return cardSource;
            }
            return null;
        }

        public override void PrepareToUsePower(Power power)
        {
            base.PrepareToUsePower(power);
            if (ShouldAssociateThisCard(power))
            {
                _cardSources.Add(power, GetCardSource());
                power.CardController.AddAssociatedCardSource(_cardSources[power]);
            }
        }

        public override void FinishUsingPower(Power power)
        {
            base.FinishUsingPower(power);
            if (ShouldAssociateThisCard(power))
            {
                power.CardController.RemoveAssociatedCardSource(_cardSources[power]);
                _cardSources.Remove(power);
            }
        }

        private bool ShouldAssociateThisCard(Power power)
        {
            if (_powerChosen.CardController == power.CardController)
            {
                return power.CardSource != null;
            }
            return false;
        }
    }

}
