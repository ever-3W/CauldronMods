using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Controller.Guise;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.MagnificentMara
{
    public class YouCanDoThatTooCardController : ICanDoThatTooCardController
    {
        public YouCanDoThatTooCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
        }

        public override TurnTakerController AskIfTurnTakerControllerIsReplaced(TurnTakerController ttc, CardSource cardSource)
        {
            var baseTtc = base.AskIfTurnTakerControllerIsReplaced(ttc, cardSource);
            if (baseTtc == base.TurnTakerControllerWithoutReplacements)
            {
                return base.TurnTakerController;
            }
            return baseTtc;
        }
    }
}
