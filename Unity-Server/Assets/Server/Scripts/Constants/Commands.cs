using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assambra.Server
{
    public sealed class Commands
    {
        public const String SERVER_READY = "serverReady";
        public const String SERVER_STOP = "serverStop";
        public const String SERVER_PLAYER_SPAWN = "serverPlayerSpawn";

        private Commands() { }
    }
}

