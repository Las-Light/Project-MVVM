using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Characters
{
    [Serializable]
    public class CharacterInitialStateSettings
    {
        public string TypeId;
        public CharacterLevelSettings LevelSettings;
        public Vector3Int Position;
    }
}