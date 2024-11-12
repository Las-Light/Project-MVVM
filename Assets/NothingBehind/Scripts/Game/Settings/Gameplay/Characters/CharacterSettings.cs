using System.Collections.Generic;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Characters
{
    [CreateAssetMenu(fileName = "CharacterSettings", menuName = "Game Settings/Characters/New Character Settings")]
    public class CharacterSettings : ScriptableObject
    {
        public string TypeId;
        public string TitleLID;
        public string DescriptionLID;
        public List<CharacterLevelSettings> LevelSettings;
    }
}