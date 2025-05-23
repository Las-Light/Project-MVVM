using System.Collections;
using System.Linq;
using DI.Scripts;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.Gameplay.Root;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GlobalMap.Root;
using NothingBehind.Scripts.Game.MainMenu.Root;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Utils;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NothingBehind.Scripts.Game.GameRoot
{
    public class GameEntryPoint
    {
        private static GameEntryPoint _instance;
        private Coroutines _coroutines;
        private UIRootView _uiRoot;
        private readonly DIContainer _rootContainer = new();
        private DIContainer _cachedSceneContainer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void AutostartGame()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            _instance = new GameEntryPoint();
            _instance.RunGame();
        }

        private GameEntryPoint()
        {
            _coroutines = new GameObject("[COROUTINES]").AddComponent<Coroutines>();
            Object.DontDestroyOnLoad(_coroutines.gameObject);
            _rootContainer.RegisterInstance(AppConstants.COROUTINES, _coroutines);

            var prefabUIRoot = Resources.Load<UIRootView>("UIRoot");
            _uiRoot = Object.Instantiate(prefabUIRoot);
            Object.DontDestroyOnLoad(_uiRoot.gameObject);
            _rootContainer.RegisterInstance(_uiRoot);

            // Настройки приложения

            var settingsProvider = new SettingsProvider();
            _rootContainer.RegisterInstance<ISettingsProvider>(settingsProvider);
            _rootContainer.RegisterFactory(_ => new InitialGameStateService()).AsSingle();

            var initialGameStateService = _rootContainer.Resolve<InitialGameStateService>();
            var gameStateProvider = new PlayerPrefsGameStateProvider(initialGameStateService);
            _rootContainer.RegisterInstance<IGameStateProvider>(gameStateProvider);
        }

        private async void RunGame()
        {
            var gameSettings = await _rootContainer.Resolve<ISettingsProvider>().LoadGameSettings();

#if UNITY_EDITOR
            var sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == Scenes.GAMEPLAY)
            {
                var enterParams =
                    new GameplayEnterParams(
                        "StartFromGameplayScene.save",
                        MapId.Map_1); //нужно для того, чтобы можно было стартовать в редакторе со сцены геймплея
                _coroutines.StartCoroutine(LoadingAndStartGameplay(gameSettings, enterParams));
                return;
            }

            if (sceneName == Scenes.GAMEPLAY_1)
            {
                var enterParams =
                    new GameplayEnterParams(
                        "StartFromGameplayScene.save",
                        MapId.Map_2); //нужно для того, чтобы можно было стартовать в редакторе со сцены геймплея
                _coroutines.StartCoroutine(LoadingAndStartGameplay(gameSettings, enterParams));
                return;
            }

            if (sceneName == Scenes.GLOBAL_MAP)
            {
                var enterParams =
                    new GlobalMapEnterParams(
                        "StartFromGlobalMapScene.save",
                        MapId.GlobalMap); //нужно для того, чтобы можно было стартовать в редакторе со сцены геймплея
                _coroutines.StartCoroutine(LoadingAndStartGlobalMap(gameSettings, enterParams));
                return;
            }

            if (sceneName == Scenes.MAIN_MENU)
            {
                _coroutines.StartCoroutine(LoadingAndStartMainMenu(gameSettings));
            }

            if (sceneName != Scenes.BOOT)
            {
                return;
            }
#endif

            _coroutines.StartCoroutine(LoadingAndStartMainMenu(gameSettings));
        }

        private IEnumerator LoadingAndStartGameplay(GameSettings gameSettings, SceneEnterParams enterParams)
        {
            _uiRoot.ShowLoadingScreen();
            _cachedSceneContainer?.Dispose();

            yield return LoadScene(Scenes.BOOT);
            yield return LoadScene(GetSceneName(enterParams.TargetMapId));

            //yield return new WaitForSeconds(1);

            var isGameStateLoaded = false;
            _rootContainer.Resolve<IGameStateProvider>().LoadGameState(gameSettings, enterParams)
                .Subscribe(_ => isGameStateLoaded = true);
            yield return new WaitUntil(() => isGameStateLoaded);

            var sceneEntryPoint = Object.FindFirstObjectByType<GameplayEntryPoint>();
            var gameplayContainer = _cachedSceneContainer = new DIContainer(_rootContainer);
            sceneEntryPoint.Run(gameplayContainer, enterParams).Subscribe(gameplayExitParams =>
            {
                var targetSceneName = GetSceneName(gameplayExitParams.SceneEnterParams.TargetMapId);
                if (targetSceneName == Scenes.MAIN_MENU)
                {
                    _coroutines.StartCoroutine(LoadingAndStartMainMenu(gameSettings,
                        gameplayExitParams.SceneEnterParams));
                }
                else if (targetSceneName == Scenes.GLOBAL_MAP)
                {
                    _coroutines.StartCoroutine(LoadingAndStartGlobalMap(gameSettings,
                        gameplayExitParams.SceneEnterParams));
                }
                else
                {
                    _coroutines.StartCoroutine(LoadingAndStartGameplay(gameSettings,
                        gameplayExitParams.SceneEnterParams));
                }
            });

            _uiRoot.HideLoadingScreen();
        }

        private IEnumerator LoadingAndStartGlobalMap(GameSettings gameSettings, SceneEnterParams enterParams)
        {
            _uiRoot.ShowLoadingScreen();
            _cachedSceneContainer?.Dispose();

            yield return LoadScene(Scenes.BOOT);
            yield return LoadScene(GetSceneName(enterParams.TargetMapId));

            //yield return new WaitForSeconds(1);

            var isGameStateLoaded = false;
            _rootContainer.Resolve<IGameStateProvider>().LoadGameState(gameSettings, enterParams)
                .Subscribe(_ => isGameStateLoaded = true);
            yield return new WaitUntil(() => isGameStateLoaded);

            var sceneEntryPoint = Object.FindFirstObjectByType<GlobalMapEntryPoint>();
            var globalMapContainer = _cachedSceneContainer = new DIContainer(_rootContainer);
            sceneEntryPoint.Run(globalMapContainer, enterParams).Subscribe(globalMapExitParams =>
            {
                var targetSceneName = GetSceneName(globalMapExitParams.SceneEnterParams.TargetMapId);
                if (targetSceneName == Scenes.MAIN_MENU)
                {
                    _coroutines.StartCoroutine(LoadingAndStartMainMenu(gameSettings,
                        globalMapExitParams.SceneEnterParams));
                }
                else
                {
                    _coroutines.StartCoroutine(LoadingAndStartGameplay(gameSettings,
                        globalMapExitParams.SceneEnterParams));
                }
            });

            _uiRoot.HideLoadingScreen();
        }

        private IEnumerator LoadingAndStartMainMenu(GameSettings gameSettings, SceneEnterParams enterParam = null)
        {
            _uiRoot.ShowLoadingScreen();
            _cachedSceneContainer?.Dispose();

            yield return LoadScene(Scenes.BOOT);
            if (enterParam != null)
            {
                yield return LoadScene(GetSceneName(enterParam.TargetMapId));
            }
            else
            {
                yield return LoadScene(Scenes.MAIN_MENU);
            }

            yield return new WaitForSeconds(1);

            var sceneEntryPoint = Object.FindFirstObjectByType<MainMenuEntryPoint>();
            var mainMenuContainer = _cachedSceneContainer = new DIContainer(_rootContainer);

            sceneEntryPoint.Run(mainMenuContainer, enterParam).Subscribe(mainMenuExitParams =>
            {
                var targetSceneName = GetSceneName(mainMenuExitParams.SceneEnterParams.TargetMapId);

                if (targetSceneName == Scenes.GAMEPLAY)
                {
                    _coroutines.StartCoroutine(
                        LoadingAndStartGameplay(gameSettings, mainMenuExitParams.SceneEnterParams));
                }
                else if (targetSceneName == Scenes.GLOBAL_MAP)
                {
                    _coroutines.StartCoroutine(LoadingAndStartGlobalMap(gameSettings,
                        mainMenuExitParams.SceneEnterParams));
                }
            });

            _uiRoot.HideLoadingScreen();
        }

        private IEnumerator LoadScene(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
        }

        private string GetSceneName(MapId targetMapId)
        {
            var settingsProvider = _rootContainer.Resolve<ISettingsProvider>();
            var mapSettings = settingsProvider.GameSettings.MapsSettings.Maps.First(m => m.MapId == targetMapId);

            return mapSettings.SceneName;
        }
    }
}