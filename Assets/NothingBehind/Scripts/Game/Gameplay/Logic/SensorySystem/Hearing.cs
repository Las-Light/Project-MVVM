using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.EventManager;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.Agent;
using NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.SensorySystem
{
    public class Hearing: MonoBehaviour
    {
        //TODO: Refactoring this class
        private EnemyWorldData _worldData;
        private PatrolManager _patrolManager;
        private EnemyData _data;
        private HumanAgent _aiAgent;

        private void Start()
        {
            _aiAgent = GetComponent<HumanAgent>();
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _patrolManager = GetComponent<PatrolManager>();
        }

        //метод проверяет надо ли ему расследовать подозрительные шумы выстрела, если да то запускает расследование
        public void InvestigateShootingArea(Vector3 soundPos, SoundType soundType)
        {
            if (!_worldData.IsJoinBattle)
            {
                if (!_worldData.IsAttacking)
                {
                    float checkSoundDist = (_data.HeardSoundPosition - soundPos).sqrMagnitude;
                    if (checkSoundDist > 4)
                    {
                        if (soundType == SoundType.Check && (_worldData.BaseRole != BaseRole.CheckFront || _worldData.BaseRole != BaseRole.CheckFlank ||
                                                             _worldData.BaseRole != BaseRole.StayUpAndAimer || _worldData.BaseRole != BaseRole.HearingSound))
                        {
                            return;
                        }

                        _worldData.IsHearingSound = true;
                        _data.HeardSoundPosition = soundPos;
                        _data.SoundType = soundType;
                        _data.SoundDetectionTime = Time.time;
                        GlobalEventManager.SendHearingSound(gameObject);
                        if (_worldData.IsMove)
                        {
                            Debug.Log("Interrupt-Hearing");
                            _aiAgent.Interrupt = true;
                        }
                    }
                }
            }
        }
    }
}