using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities.Characters;

namespace NothingBehind.Scripts.Game.State.Maps
{
    [Serializable]
    public class MapState
    {
        public int Id;
        public List<CharacterEntity> Characters;
    }
}