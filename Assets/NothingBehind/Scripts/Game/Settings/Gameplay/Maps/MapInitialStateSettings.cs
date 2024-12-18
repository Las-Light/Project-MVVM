using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Maps
{
    [Serializable]
    public class MapInitialStateSettings
    {
        public List<CharacterInitialStateSettings> Characters;
        public List<MapTransferData> MapTransfers;
    }
}