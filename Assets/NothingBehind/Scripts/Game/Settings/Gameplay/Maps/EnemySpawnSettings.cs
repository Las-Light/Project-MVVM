using NothingBehind.Scripts.Game.State.Maps;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Maps
{
    [CreateAssetMenu(fileName = "EnemySpawnSettings", menuName = "Game Settings/Maps/New Enemy Spawn")]
    public class EnemySpawnSettings : ScriptableObject
    {
        public EnemySpawnData EnemySpawnData;

        public EnemySpawnSettings(EnemySpawnData enemySpawnData)
        {
            EnemySpawnData = enemySpawnData;
        }
    }
}