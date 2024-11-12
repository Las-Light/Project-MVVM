using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities.Characters;

namespace NothingBehind.Scripts.Game.State.Root
{
    [Serializable]
    public class GameState
    {
        public int GlobalEntityId;
        public List<CharacterEntity> Characters;
    }
}