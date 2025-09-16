using System.Collections.Generic;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Characters;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Player;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Storages;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Entities
{
    [CreateAssetMenu(fileName = "EntitiesSettings", menuName = "Game Settings/Entities/New Entities Settings")]
    public class EntitiesSettings : ScriptableObject
    {
        [field: SerializeField] public CharactersSettings CharactersSettings { get; private set; }
        [field: SerializeField] public StoragesSettings StoragesSettings { get; private set; }
        [field: SerializeField] public PlayerSettings PlayerSettings { get; private set; }
    }
}