using NothingBehind.Scripts.Game.GameRoot.Services;
using UnityEngine;

namespace NothingBehind.Scripts.Game.MainMenu.Services
{
    public class SomeMainMenuService
    {
        private readonly InitialGameStateService _initialGameStateService;

        public SomeMainMenuService(InitialGameStateService initialGameStateService)
        {
            _initialGameStateService = initialGameStateService;
            Debug.Log(GetType().Name + " has been created");
        }


        public void Dispose()
        {
            Debug.Log("Clear all subscriber");
        }
    }
}