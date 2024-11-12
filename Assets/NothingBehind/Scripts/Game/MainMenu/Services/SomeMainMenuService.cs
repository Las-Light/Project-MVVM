using NothingBehind.Scripts.Game.GameRoot.Services;
using UnityEngine;

namespace NothingBehind.Scripts.Game.MainMenu.Services
{
    public class SomeMainMenuService
    {
        private readonly SomeCommonService _someCommonService;

        public SomeMainMenuService(SomeCommonService someCommonService)
        {
            _someCommonService = someCommonService;
            Debug.Log(GetType().Name + " has been created");
        }


        public void Dispose()
        {
            Debug.Log("Clear all subscriber");
        }
    }
}