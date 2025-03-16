using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities.Characters
{
    [Serializable]
    public class CharacterData : Entity
    {
        public string TypeId;
        public int Level;
        public Vector3 Position;
        public float Health;
    }
}