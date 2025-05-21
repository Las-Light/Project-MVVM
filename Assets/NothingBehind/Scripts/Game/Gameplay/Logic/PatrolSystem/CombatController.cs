using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.EventManager;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem
{
    public class CombatController : MonoBehaviour
    {
        private GameObject _enemyTarget;

        public List<GameObject> unitLostTargets = new List<GameObject>();
        public List<GameObject> unitSeesTarget = new List<GameObject>();
        public List<GameObject> unitHearSound = new List<GameObject>();
        public List<GameObject> unitKnowAboutTarget = new List<GameObject>();
        public List<GameObject> unitAttackTarget = new List<GameObject>();

        private readonly Dictionary<GameObject, UnitCombat> _unitDict = new Dictionary<GameObject, UnitCombat>();

        private void Start()
        {
            GlobalEventManager.OnSeenTarget += UnitSeesTarget;
            GlobalEventManager.OnEnemyKilled += RemoveKilledUnit;
            GlobalEventManager.OnLostTarget += UnitLostTarget;
            GlobalEventManager.OnHearingSound += UnitHearSound;
            GlobalEventManager.OnFoundAboutTarget += UnitKnowAboutTarget;
            GlobalEventManager.OnAttackTarget += UnitAttackTarget;
            GlobalEventManager.OnUpdatePosition += UnitUpdatePosition;
        }

        private void OnDestroy()
        {
            GlobalEventManager.OnSeenTarget -= UnitSeesTarget;
            GlobalEventManager.OnEnemyKilled -= RemoveKilledUnit;
            GlobalEventManager.OnLostTarget -= UnitLostTarget;
            GlobalEventManager.OnHearingSound -= UnitHearSound;
            GlobalEventManager.OnFoundAboutTarget -= UnitKnowAboutTarget;
            GlobalEventManager.OnAttackTarget -= UnitAttackTarget;
        }


        // проверка всех найденных точек укрытий на занятость другим юнитом и удаление их из списка FoundCoverPos в случае занятости
        public void CheckCoverPointsForOccupied(EnemyData unitData, GameObject unit)
        {
            if (_unitDict.Count > 1)
                foreach (var unitCombat in _unitDict)
                    if (unitCombat.Key != unit && unitCombat.Value.UnitData.CurrentCover != null)
                        for (int i = unitData.FoundCoverPos.Count - 1; i >= 0; i--)
                            if ((unitData.FoundCoverPos[i].PointPosition -
                                 unitCombat.Value.UnitData.CurrentCover.PointPosition).sqrMagnitude < 4)
                            {
                                unitData.FoundCoverPos.Remove(unitData.FoundCoverPos[i]);
                            }
        }

        // проверка следующего укрытия на занятость другим юнитом, если не занято возвращает true,
        // если занято то устанавливает флаг HasBeenPassed на данном укрытии и возвращает false
        public bool CheckNextCoverPointForOccupied(GameObject unit, List<Points> patrolPointsList,
            Points currentCoverPoint)
        {
            bool isOccupied = false;
            if (_unitDict.Count > 1)
            {
                foreach (var unitCombat in _unitDict)
                    if (!isOccupied)
                    {
                        if (unitCombat.Key != unit && unitCombat.Value.UnitData.CurrentCover != null)
                            if ((patrolPointsList[currentCoverPoint.NextIndex].PointPosition -
                                 unitCombat.Value.UnitData.CurrentCover.PointPosition).sqrMagnitude < 4)
                                isOccupied = true;
                    }
                    else
                    {
                        break;
                    }

                if (isOccupied)
                {
                    patrolPointsList[currentCoverPoint.NextIndex].HasBeenPassed = true;
                    return false;
                }

                return true;
            }

            return true;
        }

        // проверка точек расследования на расследование их другими юнитами, если они заняты то удаляет их из списка FoundInvestigatePoint
        public void CheckInvestPointForOccupied(EnemyData unitData, GameObject unit)
        {
            if (_unitDict.Count > 1)
                foreach (var unitCombat in _unitDict)
                    if (unitCombat.Key != unit && unitCombat.Value.UnitWorldData.IsFindInvestPoint)
                        for (int i = unitData.FoundInvestigatePoint.Count - 1; i >= 0; i--)
                        {
                            for (int j = 0; j < unitCombat.Value.UnitData.FoundInvestigatePoint.Count; j++)
                            {
                                if ((unitData.FoundInvestigatePoint[i].PointPosition -
                                     unitCombat.Value.UnitData.FoundInvestigatePoint[j].PointPosition).sqrMagnitude < 4)
                                {
                                    unitData.FoundInvestigatePoint[i].HasBeenPassed = true;
                                }
                            }

                            if (unitData.FoundInvestigatePoint[i].HasBeenPassed)
                            {
                                unitData.FoundInvestigatePoint.Remove(unitData.FoundInvestigatePoint[i]);
                            }
                        }
        }

        private void SetPositionAsOccupied(GameObject unit)
        {
            foreach (var unitCombat in _unitDict)
            {
                if (unitCombat.Value.UnitData.OccupiedPosition.ContainsKey(unit))
                {
                    unitCombat.Value.UnitData.OccupiedPosition.Remove(unit);
                    unitCombat.Value.UnitData.OccupiedPosition.Add(unit, _unitDict[unit].UnitData.CurrentMovementPos);
                }
                else
                {
                    unitCombat.Value.UnitData.OccupiedPosition.Add(unit, _unitDict[unit].UnitData.CurrentMovementPos);
                }
            }
        }

        // устанавливает всем юнитам которые учавствуют в атаке номер в очереди по дистанции от цели
        // очередь делится на Melee & Shooter
        private void SetQueryNumber()
        {
            int queueNumberMelee = 0;
            int queueNumberShooter = 0;
            bool isMeleeCombat = false;
            bool isMeleeLost = false;
            GameObject[] attackArray = unitAttackTarget.ToArray();
            GameObject[] investigateArray = unitLostTargets.ToArray();
            Array.Sort(attackArray, ArraySortByDistanceComparer);
            Array.Sort(investigateArray, ArraySortByDistanceComparer);
            if (attackArray.Length > 0)
            {
                for (int i = 0; i < attackArray.Length; i++)
                {
                    if (_unitDict[attackArray[i]].UnitData.WeaponType == WeaponType.Melee)
                    {
                        queueNumberMelee++;
                        isMeleeCombat = true;
                        _unitDict[attackArray[i]].QueueNumber = queueNumberMelee;
                    }

                    if (_unitDict[attackArray[i]].UnitData.WeaponType is not WeaponType.Melee or WeaponType.Unarmed)
                    {
                        queueNumberShooter++;
                        _unitDict[attackArray[i]].QueueNumber = queueNumberShooter;
                    }
                }
            }

            if (isMeleeCombat)
            {
                for (int j = 0; j < attackArray.Length; j++)
                {
                    _unitDict[attackArray[j]].MeeleeExist = true;
                }
            }
            else
            {
                for (int j = 0; j < attackArray.Length; j++)
                {
                    _unitDict[attackArray[j]].MeeleeExist = false;
                }
            }

            if (investigateArray.Length > 0)
            {
                for (int i = 0; i < investigateArray.Length; i++)
                {
                    if (_unitDict[investigateArray[i]].UnitWorldData.InAction)
                        continue;
                    
                    if (_unitDict[investigateArray[i]].UnitData.WeaponType == WeaponType.Melee)
                    {
                        queueNumberMelee++;
                        isMeleeLost = true;
                        _unitDict[investigateArray[i]].QueueNumber = queueNumberMelee;
                    }

                    if (_unitDict[investigateArray[i]].UnitData.WeaponType is not WeaponType.Melee
                        or WeaponType.Unarmed)
                    {
                        queueNumberShooter++;
                        _unitDict[investigateArray[i]].QueueNumber = queueNumberShooter;
                    }
                }
            }

            if (isMeleeLost)
            {
                for (int j = 0; j < investigateArray.Length; j++)
                {
                    _unitDict[investigateArray[j]].MeeleeExist = true;
                }
            }
            else
            {
                for (int j = 0; j < investigateArray.Length; j++)
                {
                    _unitDict[investigateArray[j]].MeeleeExist = false;
                }
            }
        }

        // обновление ролей в QueryAttack & HunterRole при смене юнитом позиции
        private void UnitUpdatePosition(GameObject unit)
        {
            SetPositionAsOccupied(unit);
            SetQueryNumber();
            SetRoleByQueryNumber();
            SetBaseRole(unit);
        }

        // добавление юнита в список учавствующих в атаке; обновление цели для расчета номера в очереди;
        // установление роли в QueryAttack
        private void UnitAttackTarget(GameObject unit)
        {
            if (!unitAttackTarget.Contains(unit)) unitAttackTarget.Add(unit);
            _enemyTarget = _unitDict[unit].UnitData.CurrentEnemy;
            SetQueryNumber();
            SetRoleByQueryNumber();
        }

        // добавление юнита в список узнавших о цели; регистрация юнита в CombatController
        private void UnitKnowAboutTarget(GameObject unit)
        {
            if (!unitKnowAboutTarget.Contains(unit)) unitKnowAboutTarget.Add(unit);
            RegisterUnit(unit);
            _unitDict[unit].UnitWorldData.InAction = false;
            SetBaseRole(unit);
        }

        // добавление юнита в список услышавших звук; регистрация юнита в CombatController
        private void UnitHearSound(GameObject unit)
        {
            if (!unitHearSound.Contains(unit)) unitHearSound.Add(unit);
            RegisterUnit(unit);
            _unitDict[unit].UnitWorldData.InAction = false;
            SetBaseRole(unit);
        }

        // добавление юнита в список потерявших цель; удаление из списка видивших, слышавших и атакующих;
        // пересчет номеров в очереди атаки; установление ролей в QueryAttack & HunterRole;
        private void UnitLostTarget(GameObject unit, GameObject target)
        {
            if (!unitLostTargets.Contains(unit)) unitLostTargets.Add(unit);
            if (unitSeesTarget.Contains(unit)) unitSeesTarget.Remove(unit);
            if (unitHearSound.Contains(unit)) unitHearSound.Remove(unit);
            if (unitAttackTarget.Contains(unit)) unitAttackTarget.Remove(unit);

            SetQueryNumber();
            SetRoleByQueryNumber();
            SetBaseRole(unit);
        }

        // добавление юнита в список видящих цель; удаление из списка потерявших, слышавших и знающих;
        private void UnitSeesTarget(GameObject unit, GameObject target)
        {
            if (!unitSeesTarget.Contains(unit)) unitSeesTarget.Add(unit);
            if (unitLostTargets.Contains(unit)) unitLostTargets.Remove(unit);
            if (unitHearSound.Contains(unit)) unitHearSound.Remove(unit);
            if (unitKnowAboutTarget.Contains(unit)) unitKnowAboutTarget.Remove(unit);
            RegisterUnit(unit);
            SetBaseRole(unit);
        }

        // после смерти юнита производит удаление из словаря _unitDict и всех списков;
        // пересчет номеров оставшихся юнитов в очереди атаки; установление ролей в QueryAttack & HunterRole;
        private void RemoveKilledUnit(GameObject unit)
        {
            _unitDict.Remove(unit);
            if (unitSeesTarget.Contains(unit)) unitSeesTarget.Remove(unit);
            if (unitLostTargets.Contains(unit)) unitLostTargets.Remove(unit);
            if (unitHearSound.Contains(unit)) unitHearSound.Remove(unit);
            if (unitAttackTarget.Contains(unit)) unitAttackTarget.Remove(unit);
            if (unitKnowAboutTarget.Contains(unit)) unitKnowAboutTarget.Remove(unit);
            SetQueryNumber();
            SetRoleByQueryNumber();
        }

        // регистрация юнита в CombatController; установление SetHunterRole;
        private void RegisterUnit(GameObject unit)
        {
            if (!_unitDict.ContainsKey(unit))
            {
                unit.TryGetComponent(out EnemyData unitData);
                unit.TryGetComponent(out EnemyWorldData unitWorldData);
                UnitCombat unitCombat = new UnitCombat(unit, unitData, unitWorldData);
                _unitDict.Add(unit, unitCombat);
            }
        }

        // установление роли в зависимости от номера юнита в очереди;
        // установление флагов состояния в worldData в зависимости от роли; 
        private void SetRoleByQueryNumber()
        {
            if (unitAttackTarget.Count > 0)
            {
                foreach (var unit in unitAttackTarget)
                {
                    if (_unitDict[unit].MeeleeExist)
                    {
                        if (_unitDict[unit].UnitData.WeaponType == WeaponType.Melee)
                        {
                            _unitDict[unit].UnitWorldData.AttackRole =
                                _unitDict[unit].QueueNumber switch
                                {
                                    0 => AttackRole.Noncombatant,
                                    1 => AttackRole.MeleeFirst,
                                    _ => AttackRole.MeleeSecond
                                };
                        }
                    }

                    if (_unitDict[unit].UnitData.WeaponType is not WeaponType.Melee or WeaponType.Unarmed)
                    {
                        if (!_unitDict[unit].MeeleeExist)
                        {
                            _unitDict[unit].UnitWorldData.AttackRole =
                                _unitDict[unit].QueueNumber switch
                                {
                                    0 => AttackRole.Noncombatant,
                                    1 or 3 => AttackRole.ShooterFirst,
                                    2 => AttackRole.ShooterStay,
                                    > 3 => AttackRole.ShooterFirst,
                                    _ => AttackRole.Noncombatant
                                };
                        }
                        else
                            _unitDict[unit].UnitWorldData.AttackRole = AttackRole.ShooterSecond;
                    }

                    _unitDict[unit].UnitWorldData.SetStateFromQuery();
                }
            }

            if (unitLostTargets.Count > 0)
            {
                foreach (var unit in unitLostTargets)
                {
                    if (_unitDict[unit].UnitWorldData.InAction)
                        continue;
                    
                    if (!_unitDict[unit].UnitWorldData.IsCheckArea)
                    {
                        if (_unitDict[unit].MeeleeExist)
                        {
                            if (_unitDict[unit].UnitData.WeaponType == WeaponType.Melee)
                            {
                                _unitDict[unit].UnitWorldData.BaseRole = _unitDict[unit].QueueNumber switch
                                {
                                    0 => BaseRole.Patrol,
                                    1 or 2 or 3 => BaseRole.CheckFront,
                                    4 => BaseRole.CheckFlank,
                                    _ => BaseRole.Patrol
                                };
                            }
                        }

                        if (_unitDict[unit].UnitData.WeaponType is not WeaponType.Melee or WeaponType.Unarmed)
                        {
                            if (!_unitDict[unit].MeeleeExist)
                            {
                                _unitDict[unit].UnitWorldData.BaseRole = _unitDict[unit].QueueNumber switch
                                {
                                    0 => BaseRole.Patrol,
                                    1 => BaseRole.CheckFront,
                                    2 => BaseRole.CheckFlank,
                                    > 2 => BaseRole.StayUpAndAimer,
                                    _ => BaseRole.Patrol
                                };
                            }
                            else
                            {
                                _unitDict[unit].UnitWorldData.BaseRole = _unitDict[unit].QueueNumber switch
                                {
                                    _ => BaseRole.CheckFlank
                                };
                            }
                        }
                    }
                    else
                    {
                        _unitDict[unit].UnitWorldData.BaseRole = _unitDict[unit].QueueNumber switch
                        {
                            _ => BaseRole.Investigator
                        };
                    }

                    _unitDict[unit].UnitWorldData.AttackRole = AttackRole.Noncombatant;
                    _unitDict[unit].UnitWorldData.SetStateFromQuery();
                }
            }
        }

        private void SetBaseRole(GameObject unit)
        {
            if (_unitDict[unit].UnitWorldData.IsCouldSeeEnemy)
            {
                // если юнит видит игрока и у него не оружие ближнего боя и дистанция больше 2 метров
                if (_unitDict[unit].UnitData.EnemyType == TargetType.Player && _unitDict[unit].UnitData.WeaponType != WeaponType.Melee)
                {
                    // он один видит врага
                    if (unitSeesTarget.Count >= 1)
                        if (_unitDict[unit].UnitData.WeaponType != WeaponType.Melee)
                        {
                            _unitDict[unit].UnitWorldData.BaseRole = BaseRole.Shooter;
                        }
                }

                // если юнит видит собаку или зараженного
                if (_unitDict[unit].UnitData.EnemyType is TargetType.Dog or TargetType.Infected && _unitDict[unit].UnitData.SqrtDistanceToTarget >= 2)
                    if (_unitDict[unit].UnitData.WeaponType != WeaponType.Melee)
                        _unitDict[unit].UnitWorldData.BaseRole = BaseRole.StayUpAndAimer;

                // если юнит имеет только оружие ближнего боя или дистанция до врага меньше 2 метров
                // if (unitData.WeaponType == WeaponType.Melee || unitData.SqrtDistanceToTarget < 2)
                //     unitWorldData.BaseRole = BaseRole.Approacher;
            }
            else
            {
                if (_unitDict[unit].UnitWorldData.InAction)
                    return;
                
                if (_unitDict[unit].UnitWorldData.IsFoundAboutTarget && !_unitDict[unit].UnitWorldData.IsNeedStay)
                {
                    _unitDict[unit].UnitWorldData.BaseRole = BaseRole.FoundAboutTarget;
                }

                if (_unitDict[unit].UnitWorldData.IsCheckArea && !_unitDict[unit].UnitWorldData.IsFoundAboutTarget)
                {
                    _unitDict[unit].UnitWorldData.BaseRole = BaseRole.Investigator;
                    Debug.Log("Investigate in CombCtrl");
                }

                if (_unitDict[unit].UnitWorldData.IsHearingSound && !_unitDict[unit].UnitWorldData.IsFoundAboutTarget &&
                    _unitDict[unit].UnitData.SoundDetectionTime > _unitDict[unit].UnitData.TargetLastKnownDetectionTime)
                {
                    if ((_unitDict[unit].UnitData.SoundType == SoundType.Shoot || _unitDict[unit].UnitData.SoundType == SoundType.Notify) &&
                        (_unitDict[unit].UnitData.CurrentMovementPos - _unitDict[unit].UnitData.HeardSoundPosition)
                        .sqrMagnitude > 25 && _unitDict[unit].UnitWorldData.BaseRole != BaseRole.CheckFlank)
                    {
                        _unitDict[unit].UnitWorldData.BaseRole = BaseRole.CheckFlank;
                    }

                    if (_unitDict[unit].UnitData.SoundType == SoundType.Check)
                    {
                        if (_unitDict[unit].UnitWorldData.IsFindPosCheckArea &&
                            (_unitDict[unit].UnitData.CurrentMovementPos - _unitDict[unit].UnitData.HeardSoundPosition)
                            .sqrMagnitude < 25)
                        {
                            Debug.Log("CheckSound in CombCtrl");
                            _unitDict[unit].UnitWorldData.BaseRole = BaseRole.Investigator;
                        }
                    }

                    if (_unitDict[unit].UnitData.SoundType == SoundType.Noise && !_unitDict[unit].UnitWorldData.IsSeeEnemy)
                    {
                        _unitDict[unit].UnitWorldData.BaseRole = BaseRole.Patrol;
                    }

                    // if (_unitDict[unit].UnitWorldData.IsNeedStay &&
                    //     _unitDict[unit].UnitData.SoundType == SoundType.Check &&
                    //     !_unitDict[unit].UnitWorldData.IsSeeEnemy)
                    // {
                    //     _unitDict[unit].UnitWorldData.BaseRole = BaseRole.CheckFlank;
                    // }
                }

                if (_unitDict[unit].UnitWorldData.IsInvestigationEnd)
                {
                    Debug.LogError("Patrol in CombCtrl " );
                    _unitDict[unit].UnitWorldData.BaseRole = BaseRole.Patrol;
                }
            }

            _unitDict[unit].UnitWorldData.SetStateFromRole();
        }

        // сортировка массива по дистанции от игрока
        private int ArraySortByDistanceComparer(GameObject a, GameObject b)
        {
            if (a == null && b != null)
            {
                return 1;
            }
            else if (a != null && b == null)
            {
                return -1;
            }
            else if (a == null && b == null)
            {
                return 0;
            }
            else
            {
                var position = _enemyTarget.transform.position;
                return (position - a.transform.position).sqrMagnitude.CompareTo((position - b.transform.position)
                    .sqrMagnitude);
            }
        }
    }
}