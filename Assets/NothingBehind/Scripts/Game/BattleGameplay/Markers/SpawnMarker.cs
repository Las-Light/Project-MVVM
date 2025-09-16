using System.Collections.Generic;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Markers
{
    public class SpawnMarker: MonoBehaviour
    {
        public string Id;
        public List<EntityInitialStateSettings> Characters;
        public Vector3 Position;
        public bool IsTriggered;
    }
}