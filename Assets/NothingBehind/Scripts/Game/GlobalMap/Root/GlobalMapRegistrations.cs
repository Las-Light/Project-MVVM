using DI.Scripts;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.GameRoot;
using R3;

namespace NothingBehind.Scripts.Game.GlobalMap.Root
{
    public class GlobalMapRegistrations
    {
        public static void Register(DIContainer container, SceneEnterParams enterParams)
        {
            container.RegisterInstance(AppConstants.EXIT_SCENE_REQUEST_TAG, new Subject<GlobalMapExitParams>());
        }
    }
}