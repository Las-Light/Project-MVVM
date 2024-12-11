using System.Collections.Generic;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Root;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State
{
    public class PlayerPrefsGameStateProvider : IGameStateProvider
    {
        private const string GAME_STATE_KEY = nameof(GAME_STATE_KEY);
        private const string GAME_SETTINGS_STATE_KEY = nameof(GAME_SETTINGS_STATE_KEY);

        public GameStateProxy GameState { get; private set; }
        public GameSettingsStateProxy SettingsState { get; private set; }
        
        private GameState _gameStateOrigin;
        private GameSettingsState _gameSettingsStateOrigin;

        public Observable<GameStateProxy> LoadGameState(SceneEnterParams sceneEnterParams)
        {
            if (!PlayerPrefs.HasKey(GAME_STATE_KEY))
            {
                GameState = CreateGameStateFromSettings(sceneEnterParams);
                Debug.Log("Game State created from settings" + JsonUtility.ToJson(_gameStateOrigin, true));

                SaveGameState(); // Сохраним дефолтное состояние
            }
            else
            {
                // Загружаем
                var json = PlayerPrefs.GetString(GAME_STATE_KEY);
                _gameStateOrigin = JsonUtility.FromJson<GameState>(json);

                GameState = new GameStateProxy(_gameStateOrigin);
                // добавил передачу куррентМэпИд
                GameState.CurrentMapId.Value = sceneEnterParams.MapId;

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
                _gameSettingsStateOrigin = JsonUtility.FromJson<GameSettingsState>(json);
                SettingsState = new GameSettingsStateProxy(_gameSettingsStateOrigin);
            }

            return Observable.Return(SettingsState);
        }

        public Observable<bool> SaveGameState()
        {
            var json = JsonUtility.ToJson(_gameStateOrigin, true);
            PlayerPrefs.SetString(GAME_STATE_KEY, json);

            Debug.Log("Save GameState");

            return Observable.Return(true);
        }

        public Observable<bool> SaveSettingsState()
        {
            var json = JsonUtility.ToJson(_gameSettingsStateOrigin, true);
            PlayerPrefs.SetString(GAME_SETTINGS_STATE_KEY, json);

            return Observable.Return(true);
        }

        public Observable<bool> ResetGameState(SceneEnterParams sceneEnterParams)
        {
            GameState = CreateGameStateFromSettings(sceneEnterParams);
            SaveGameState();

            return Observable.Return(true);
        }

        public Observable<GameSettingsStateProxy> ResetSettingsState()
        {
            SettingsState = CreateGameSettingsStateFromSettings();
            SaveSettingsState();

            return Observable.Return(SettingsState);
        }

        private GameStateProxy CreateGameStateFromSettings(SceneEnterParams sceneEnterParams)
        {
            // Состояние по умолчанию из настроек, мы делаем фейк
            _gameStateOrigin = new GameState
            {
                Maps = new List<MapState>(),
                CurrentMapId = sceneEnterParams.MapId,
                Resources = new List<ResourceData>()
                {
                    new (){Amount = 0, ResourceType = ResourceType.SoftCurrency},
                    new (){Amount = 0, ResourceType = ResourceType.HardCurrency}
                }
            };

            var gameState = new GameStateProxy(_gameStateOrigin);
            Debug.Log(gameState.CurrentMapId.Value);

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