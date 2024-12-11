using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.State.Root
{
    [Serializable]
    public class GameState
    {
        public int GlobalEntityId;
        public string CurrentMapId;
        public List<MapState> Maps;
        public List<ResourceData> Resources;

        public int CreateEntityId()
        {
            return GlobalEntityId++;
        }
    }
}