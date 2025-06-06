using System.Collections.Generic;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Markers
{
    public class SpawnMarker: MonoBehaviour
    {
        public string Id;
        public List<CharacterInitialStateSettings> Characters;
        public Vector3 Position;
        public bool IsTriggered;
    }
}