using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities.Characters
{
    [Serializable]
    public class CharacterData : Entity
    {
        public int Level;
        public Vector3 Position;
        public float Health;
    }
}