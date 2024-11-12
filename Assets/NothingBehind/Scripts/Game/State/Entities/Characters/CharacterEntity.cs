using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Entities.Characters
{
    [Serializable]
    public class CharacterEntity : Entity
    {
        public string TypeID;
        public int Level;
        public Vector3Int Position;
        public float Health;
    }
}