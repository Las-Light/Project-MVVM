using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Characters
{
    [CreateAssetMenu(fileName = "CharacterLevelSettings", menuName = "Game Settings/Entities/Characters/New Character Level Settings")]
    public class CharacterLevelSettings : EntityLevelSettings
    {
        [field: SerializeField] public float Health { get; private set; }
    }
}