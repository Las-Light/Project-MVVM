using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.Animation;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.WeaponSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Agent
{
    public class HumanAgent: AIAgent
    {
        private EnemyWorldData _worldData;
        private AnimatorController _humanAnimatorController;

        private void Start()
        {
            _worldData = GetComponent<EnemyWorldData>();
            _humanAnimatorController = GetComponent<AnimatorController>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet)
            {
                _humanAnimatorController.Hit(Random.Range(1, 4));
            }
        }

        public override HashSet<KeyValuePair<string, object>> GetWorldState()
        {
            HashSet<KeyValuePair<string, object>> worldData = new HashSet<KeyValuePair<string, object>>();
        
            worldData.Add(new KeyValuePair<string, object>("canAttack", (this._worldData.IsCanAttack)));
            worldData.Add(new KeyValuePair<string, object>("hasNeedPatrol", (this._worldData.IsNeedPatrol)));
            worldData.Add(new KeyValuePair<string, object>("needCover", (this._worldData.IsNeedCover)));
            worldData.Add(new KeyValuePair<string, object>("hasCover", (this._worldData.IsHaveCover)));
            worldData.Add(new KeyValuePair<string, object>("needChangeCover", (this._worldData.IsNeedChangeCover)));
            worldData.Add(new KeyValuePair<string, object>("needCheckArea", (this._worldData.IsNeedCheckArea)));
            worldData.Add(new KeyValuePair<string, object>("moveToCheckArea", (this._worldData.IsFindPosCheckArea)));
            worldData.Add(new KeyValuePair<string, object>("findCover", (this._worldData.IsFindCover)));
            worldData.Add(new KeyValuePair<string, object>("needSit", (this._worldData.IsNeedSit)));
            worldData.Add(new KeyValuePair<string, object>("wait", (this._worldData.IsNeedWait)));
            worldData.Add(new KeyValuePair<string, object>("needClosely", (this._worldData.IsNeedClosely)));
            worldData.Add(new KeyValuePair<string, object>("needRotate", (this._worldData.IsNeedRotate)));
            worldData.Add(new KeyValuePair<string, object>("enemyClosely", (this._worldData.IsTargetClosely)));
            worldData.Add(new KeyValuePair<string, object>("shouldBeInvestigating", (this._worldData.IsNeedInvestigation)));
            worldData.Add(new KeyValuePair<string, object>("findInvestPoint", (this._worldData.IsFindInvestPoint)));
            worldData.Add(new KeyValuePair<string, object>("needMoveShootPos", (this._worldData.IsFindFireLine || this._worldData.IsFindShootPos)));
            worldData.Add(new KeyValuePair<string, object>("needFireLine", (this._worldData.IsNeedFireLine)));
            worldData.Add(new KeyValuePair<string, object>("needFindShootPos", (this._worldData.IsNeedShootPos)));
            worldData.Add(new KeyValuePair<string, object>("voice", (this._worldData.IsNeedVoice)));
            worldData.Add(new KeyValuePair<string, object>("foundAboutTarget", (this._worldData.IsFoundAboutTarget)));
        
            return worldData;
        }

        public override HashSet<KeyValuePair<string, object>> CreateGoalState()
        {
            HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();

            goal.Add(new KeyValuePair<string, object>("enemyDamage", true));
            goal.Add(new KeyValuePair<string, object>("enemyClosely", true));
            goal.Add(new KeyValuePair<string, object>("hasCover", true));
            goal.Add(new KeyValuePair<string, object>("secureArea", true));
            goal.Add(new KeyValuePair<string, object>("voice", false));
            return goal;
        }
    }
}