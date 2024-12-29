using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Characters
{
    [Serializable]
    public class CharacterInitialStateSettings
    {
        public string TypeId;
        public CharacterLevelSettings LevelSettings;
        public Vector3 Position;

        public CharacterInitialStateSettings(string typeId, CharacterLevelSettings levelSettings, Vector3 position)
        {
            TypeId = typeId;
            LevelSettings = levelSettings;
            Position = position;
        }
    }
}