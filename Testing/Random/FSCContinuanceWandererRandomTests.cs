﻿using NUnit.Framework;
using System;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using Cauldron;
using System.Collections.Generic;

namespace CauldronTests.Random
{
    [TestFixture()]
    public class FSCContinuanceWandererRandomTests : RandomGameTest
    {
        [Test]
        public void TestFSCContinuanceWanderer_Random()
        {
            GameController gameController = SetupRandomGameController(false,
                availableHeroes: CauldronHeroes,
                availableVillains: CauldronVillains,
                useEnvironment: "Cauldron.FSCContinuanceWanderer");
            RunGame(gameController);
        }

        [Test]
        public void TestFSCContinuanceWanderer_Reasonable()
        {
            GameController gameController = SetupRandomGameController(true,
                availableHeroes: CauldronHeroes,
                availableVillains: CauldronVillains,
                useEnvironment: "Cauldron.FSCContinuanceWanderer");
            RunGame(gameController);
        }
    }
}