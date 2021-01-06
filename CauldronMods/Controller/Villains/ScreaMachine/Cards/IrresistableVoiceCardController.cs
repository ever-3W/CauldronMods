﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.ScreaMachine
{
    public class IrresistibleVoiceCardController : ScreaMachineBandCardController
    {
        public IrresistibleVoiceCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController, ScreaMachineBandmate.Value.Valentine)
        {
        }

        protected override IEnumerator ActivateBandAbility()
        {
            throw new NotImplementedException();
        }
    }
}
