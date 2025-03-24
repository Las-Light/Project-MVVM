using Newtonsoft.Json;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.Settings;
using NothingBehind.Scripts.Game.State.Root;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State
{
    public class PlayerPrefsGameStateProvider : IGameStateProvider
    {
        private readonly InitialGameStateService _initialGameStateService;
        private const string GAME_STATE_KEY = nameof(GAME_STATE_KEY);
        private const string GAME_SETTINGS_STATE_KEY = nameof(GAME_SETTINGS_STATE_KEY);

        public GameStateProxy GameState { get; private set; }
        public GameSettingsStateProxy SettingsState { get; private set; }

        private GameState _gameStateOrigin;
        private GameSettingsState _gameSettingsStateOrigin;

        public PlayerPrefsGameStateProvider(InitialGameStateService initialGameStateService)
        {
            _initialGameStateService = initialGameStateService;
        }

        public Observable<GameStateProxy> LoadGameState(GameSettings gameSettings, SceneEnterParams sceneEnterParams)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            
            if (!PlayerPrefs.HasKey(GAME_STATE_KEY))
            {
                GameState = CreateGameStateFromSettings(gameSettings, sceneEnterParams);
                Debug.Log("Game State created from settings" + JsonConvert.SerializeObject(_gameStateOrigin, Formatting.Indented));

                SaveGameState(); // Сохраним дефолтное состояние
            }
            else
            {
                // Загружаем
                var json = PlayerPrefs.GetString(GAME_STATE_KEY);
                _gameStateOrigin = JsonConvert.DeserializeObject<GameState>(json);

                GameState = new GameStateProxy(_gameStateOrigin);
                // добавил передачу куррентМэпИд
                GameState.CurrentMapId.Value = sceneEnterParams.TargetMapId;

                // выдает в лог джейсон с оригинальным стейтом который был сохранен
                Debug.Log("Game State loaded: " + json);
            }

            return Observable.Return(GameState);
        }

        public Observable<GameSettingsStateProxy> LoadSettingsState()
        {
            if (!PlayerPrefs.HasKey(GAME_SETTINGS_STATE_KEY))
            {
                SettingsState = CreateGameSettingsStateFromSettings();

                SaveSettingsState(); // Сохраним дефолтное состояние
            }
            else
            {
                // Загружаем
                var json = PlayerPrefs.GetString(GAME_SETTINGS_STATE_KEY);
                _gameSettingsStateOrigin = JsonConvert.DeserializeObject<GameSettingsState>(json);
                SettingsState = new GameSettingsStateProxy(_gameSettingsStateOrigin);
            }

            return Observable.Return(SettingsState);
        }

        public Observable<bool> SaveGameState()
        {
            var json = JsonConvert.SerializeObject(_gameStateOrigin, Formatting.Indented);
            PlayerPrefs.SetString(GAME_STATE_KEY, json);

            //Debug.Log("Save GameState");

            return Observable.Return(true);
        }

        public Observable<bool> SaveSettingsState()
        {
            var json = JsonConvert.SerializeObject(_gameSettingsStateOrigin, Formatting.Indented);
            PlayerPrefs.SetString(GAME_SETTINGS_STATE_KEY, json);

            return Observable.Return(true);
        }

        public Observable<bool> ResetGameState(GameSettings gameSettings, SceneEnterParams sceneEnterParams)
        {
            GameState = CreateGameStateFromSettings(gameSettings, sceneEnterParams);
            SaveGameState();

            return Observable.Return(true);
        }

        public Observable<GameSettingsStateProxy> ResetSettingsState()
        {
            SettingsState = CreateGameSettingsStateFromSettings();
            SaveSettingsState();

            return Observable.Return(SettingsState);
        }

        private GameStateProxy CreateGameStateFromSettings(GameSettings gameSettings, SceneEnterParams sceneEnterParams)
        {
            // Состояние по умолчанию из настроек
            _gameStateOrigin = _initialGameStateService.CreateGameState(gameSettings, sceneEnterParams);

            var gameState = new GameStateProxy(_gameStateOrigin);

            return gameState;
        }

        private GameSettingsStateProxy CreateGameSettingsStateFromSettings()
        {
            // Состояние по умолчанию из настроек, мы делаем фейк
            _gameSettingsStateOrigin = new GameSettingsState()
            {
                MusicVolume = 8,
                SFXVolume = 8
            };

            return new GameSettingsStateProxy(_gameSettingsStateOrigin);
        }
    }
}