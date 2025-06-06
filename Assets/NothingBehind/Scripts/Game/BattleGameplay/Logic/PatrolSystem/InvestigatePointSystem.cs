using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.PatrolSystem
{
    public class InvestigatePointSystem: SearchManager
    {
        [Space(10)]
        [Header("Sensitivity hiding spot")]
        [Range(-1, 1)]
        [Tooltip("Lower is a better hiding spot")]
        [SerializeField]
        private float maxSens = -0.2f;
        [Range(-1, 1)]
        [Tooltip("Higher is a better hiding spot")]
        [SerializeField] private float minSens = -1f;
        
        [Header("Distance and height cover props")]
        [Range(0, 10)] [SerializeField] private float minInvestPlaceDistance = 1f;
        [Range(1, 25)] [SerializeField] private float maxInvestPlaceDistance = 6f;
        [Range(0, 10)] [SerializeField] private float minHeardPlaceDistance = 1f;
        [Range(1, 25)] [SerializeField] private float maxHeardPlaceDistance = 20f;
        [Range(0, 5f)] [SerializeField] private float minInvestObstacleHeight = 0.39f;
        [Range(0, 5f)] [SerializeField] private float maxInvestObstacleHeight = 4f;
        [Header("Radius search area")]
        [Range(1, 25)] [SerializeField] private float searchRadiusAreas = 25f;
        public void StartSearchInvestigationPoints()
        {
            Debug.Log("Hearing = " + worldData.IsHearingSound + " " + gameObject);
            Debug.Log("SoundPos = " + data.HeardSoundPosition + " " + gameObject.name);
            Debug.Log("SoundTime = " + data.SoundDetectionTime + " " + gameObject.name);
            Debug.Log("TargetLost = " + worldData.IsTargetLost + " " + gameObject.name);
            Debug.Log("TargetPos = " + data.TargetLastKnownPosition + " " + gameObject.name);
            Debug.Log("TargetTime = " + data.TargetLastKnownDetectionTime + " " + gameObject.name);
            if (worldData.IsHearingSound && data.HeardSoundPosition != Vector3.zero &&
                data.SoundDetectionTime > data.TargetLastKnownDetectionTime)
            {
                Debug.Log("HearingInvest " + gameObject.name);
                SettingsCheckArea(minSens, maxSens, minHeardPlaceDistance, maxHeardPlaceDistance,
                    minInvestObstacleHeight, maxInvestObstacleHeight, searchRadiusAreas);
                CheckAreaAndFindPoints(data.HeardSoundPosition);
            }
            else if (data.TargetLastKnownPosition != Vector3.zero)
            {
                Debug.Log("TargetLostInvest " + gameObject.name);
                SettingsCheckArea(minSens, maxSens, minInvestPlaceDistance, maxInvestPlaceDistance,
                    minInvestObstacleHeight, maxInvestObstacleHeight, searchRadiusAreas);
                CheckAreaAndFindPoints(data.TargetLastKnownPosition);
            }
        }
    }
}