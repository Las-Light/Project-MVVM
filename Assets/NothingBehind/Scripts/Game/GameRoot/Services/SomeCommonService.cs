using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.Services
{
    public class SomeCommonService
    {
        // Например провайдер состояния или провайдер настроек, сервис аналитики
        public SomeCommonService()
        {
            Debug.Log(GetType().Name + " has been created");
        }
    }
}