using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Player
{
    [CreateAssetMenu(fileName = "PlayerLevelSettings", menuName = "Game Settings/Entities/Player/New Player Level Settings")]
    public class PlayerLevelSettings : EntityLevelSettings
    {
        [field: SerializeField] public float Health { get; private set; }
    }
}