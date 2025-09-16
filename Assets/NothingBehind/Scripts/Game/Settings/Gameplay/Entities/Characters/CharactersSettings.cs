using System.Collections.Generic;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Characters
{
    [CreateAssetMenu(fileName = "CharactersSettings", menuName = "Game Settings/Characters/New Characters Settings")]
    public class CharactersSettings :ScriptableObject
    {
        public List<CharacterSettings> Characters;
    }
}