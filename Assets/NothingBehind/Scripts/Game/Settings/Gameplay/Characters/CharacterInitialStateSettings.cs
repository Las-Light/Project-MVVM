using System;
using NothingBehind.Scripts.Game.State.Entities;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Characters
{
    [Serializable]
    public class CharacterInitialStateSettings
    {
        public EntityType EntityType;
        public CharacterLevelSettings LevelSettings;
        public Vector3 Position;

        public CharacterInitialStateSettings(EntityType entityType, CharacterLevelSettings levelSettings, Vector3 position)
        {
            EntityType = entityType;
            LevelSettings = levelSettings;
            Position = position;
        }
    }
}