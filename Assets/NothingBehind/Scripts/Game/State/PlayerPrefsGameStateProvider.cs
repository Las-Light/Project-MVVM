using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Root;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State
{
    public class PlayerPrefsGameStateProvider : IGameStateProvider
    {
        private const string GAME_STATE_KEY = nameof(GAME_STATE_KEY);
        
        public GameStateProxy GameState { get; private set; }
        private GameState _gameStateOrigin;
        
        public Observable<GameStateProxy> LoadGameState()
        {
            if (!PlayerPrefs.HasKey(GAME_STATE_KEY))
            {
                GameState = CreateGameStateFromSettings();
                Debug.Log("Game State created from settings" + JsonUtility.ToJson(_gameStateOrigin, true));

                SaveGameState(); // Сохраним дефолтное состояние
            }
            else
            {
                // Загружаем
                var json = PlayerPrefs.GetString(GAME_STATE_KEY);
                _gameStateOrigin = JsonUtility.FromJson<GameState>(json);
                GameState = new GameStateProxy(_gameStateOrigin);
                
                Debug.Log("Game State loaded: " + json);
            }

            return Observable.Return(GameState);
        }

        public Observable<bool> SaveGameState()
        {
            var json = JsonUtility.ToJson(_gameStateOrigin, true);
            PlayerPrefs.SetString(GAME_STATE_KEY, json);

            return Observable.Return(true);
        }

        public Observable<bool> ResetGameState()
        {
            GameState = CreateGameStateFromSettings();
            SaveGameState();

            return Observable.Return(true);
        }
        private GameStateProxy CreateGameStateFromSettings()
        {
            // Состояние по умолчанию из настроек, мы делаем фейк
            _gameStateOrigin = new GameState
            {
                Maps = new List<MapState>()
            };

            return new GameStateProxy(_gameStateOrigin);
        }
    }
}