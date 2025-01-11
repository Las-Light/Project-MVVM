using System.Linq;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Characters
{
    public class HeroBinder : MonoBehaviour
    {
        public void Bind(HeroViewModel viewModel)
        {
            var currentPosOnMap = viewModel.PositionOnMaps.First(p => p.MapId == viewModel.CurrentMap.CurrentValue);
            transform.position = currentPosOnMap.Position.Value;
        }
    }
}