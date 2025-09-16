using System.Collections.Generic;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Storages
{
    [CreateAssetMenu(fileName = "StoragesSettings", menuName = "Game Settings/Storages/New Storages Settings")]
    public class StoragesSettings :ScriptableObject
    {
        public List<StorageSettings> Storages;
    }
}