using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem
{
    public class UnitCombat
    {
        public UnitCombat(GameObject unitGameObject, EnemyData unitData, EnemyWorldData unitWorldData)
        {
            UnitGameObject = unitGameObject;
            UnitData = unitData;
            UnitWorldData = unitWorldData;
        }

        public GameObject UnitGameObject { get; }
        public EnemyData UnitData { get; }
        public EnemyWorldData UnitWorldData { get; }
        public int QueueNumber { get; set; }
        public bool MeeleeExist { get; set; }
    }
}