using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Characters
{
    [Serializable]
    public class CharacterInitialStateSettings
    {
        public string TypeId;
        public int Level;
        public float Health;
        public Vector3Int Position;
    }
}