using System.Collections;
using DI.Scripts;
using NothingBehind.Scripts.Game.Gameplay.Root;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.MainMenu.Root;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State;
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

            var prefabUIRoot = Resources.Load<UIRootView>("UIRoot");
            _uiRoot = Object.Instantiate(prefabUIRoot);
            Object.DontDestroyOnLoad(_uiRoot.gameObject);
            _rootContainer.RegisterInstance(_uiRoot);

            // Настройки приложения

            var settingsProvider = new SettingsProvider();
            _rootContainer.RegisterInstance<ISettingsProvider>(settingsProvider);

            var gameStateProvider = new PlayerPrefsGameStateProvider();
            _rootContainer.RegisterInstance<IGameStateProvider>(gameStateProvider);

            _rootContainer.RegisterFactory(_ => new SomeCommonService()).AsSingle();
        }

        private async void RunGame()
        {
            await _rootContainer.Resolve<ISettingsProvider>().LoadGameSettings();

#if UNITY_EDITOR
            var sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == Scenes.GAMEPLAY)
            {
                var enterParams =
                    new GameplayEnterParams(
                        "StartFromGameplayScene.save",
                        Scenes.GAMEPLAY); //нужно для того, чтобы можно было стартовать в редакторе со сцены геймплея
                _coroutines.StartCoroutine(LoadingAndStartGameplay(enterParams));
                return;
            }

            if (sceneName == Scenes.MAIN_MENU)
            {
                _coroutines.StartCoroutine(LoadingAndStartMainMenu());
            }

            if (sceneName != Scenes.BOOT)
            {
                return;
            }
#endif

            _coroutines.StartCoroutine(LoadingAndStartMainMenu());
        }

        private IEnumerator LoadingAndStartGameplay(SceneEnterParams enterParams)
        {
            _uiRoot.ShowLoadingScreen();
            _cachedSceneContainer?.Dispose();

            yield return LoadScene(Scenes.BOOT);
            yield return LoadScene(enterParams.MapId);

            yield return new WaitForSeconds(1);

            var isGameStateLoaded = false;
            _rootContainer.Resolve<IGameStateProvider>().LoadGameState(enterParams).Subscribe(_ => isGameStateLoaded = true);
            yield return new WaitUntil(() => isGameStateLoaded);

            var sceneEntryPoint = Object.FindFirstObjectByType<GameplayEntryPoint>();
            var gameplayContainer = _cachedSceneContainer = new DIContainer(_rootContainer);
            sceneEntryPoint.Run(gameplayContainer, enterParams).Subscribe(gameplayExitParams =>
            {
                if (gameplayExitParams.SceneEnterParams.MapId == Scenes.MAIN_MENU)
                {
                    _coroutines.StartCoroutine(LoadingAndStartMainMenu(gameplayExitParams.SceneEnterParams));
                }
                else
                {
                    _coroutines.StartCoroutine(LoadingAndStartGameplay(gameplayExitParams.SceneEnterParams));
                }
            });

            _uiRoot.HideLoadingScreen();
        }

        private IEnumerator LoadingAndStartMainMenu(SceneEnterParams enterParam = null)
        {
            _uiRoot.ShowLoadingScreen();
            _cachedSceneContainer?.Dispose();

            yield return LoadScene(Scenes.BOOT);
            if (enterParam != null)
            {
                yield return LoadScene(enterParam.MapId);
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
                var targetSceneName = mainMenuExitParams.SceneEnterParams.MapId;

                if (targetSceneName == Scenes.GAMEPLAY)
                {
                    _coroutines.StartCoroutine(
                        LoadingAndStartGameplay(mainMenuExitParams.SceneEnterParams));
                }
            });

            _uiRoot.HideLoadingScreen();
        }

        private IEnumerator LoadScene(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
        }
    }
}