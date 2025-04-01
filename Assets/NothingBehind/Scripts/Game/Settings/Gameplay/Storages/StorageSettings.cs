using NothingBehind.Scripts.Game.State.Entities;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Storages
{
    [CreateAssetMenu(fileName = "StorageSettings", menuName = "Game Settings/Storages/New Storage Settings")]
    public class StorageSettings : ScriptableObject
    {
        public EntityType EntityType;
    }
}