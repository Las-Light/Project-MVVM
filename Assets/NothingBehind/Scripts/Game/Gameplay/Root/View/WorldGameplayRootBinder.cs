using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Root.View
{
    public class WorldGameplayRootBinder : MonoBehaviour
    {
        private readonly Dictionary<int, CharacterBinder> _createCharactersMap = new();
        private readonly CompositeDisposable _disposables = new();
        private WorldGameplayRootViewModel _viewModel;

        public void Bind(WorldGameplayRootViewModel viewModel)
        {
            _viewModel = viewModel;
            
            foreach (var characterViewModel in viewModel.AllCharacters)
            {
                CreateCharacter(characterViewModel);
            }
            
            _disposables.Add(viewModel.AllCharacters.ObserveAdd()
                .Subscribe(e => CreateCharacter(e.Value)));
            
            _disposables.Add(viewModel.AllCharacters.ObserveRemove()
                .Subscribe(e => DestroyCharacter(e.Value)));
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void CreateCharacter(CharacterViewModel characterViewModel)
        {
            //создает CharacterView
            // для примера:
            var characterLevel = characterViewModel.Level.CurrentValue;
            //
            var characterType = characterViewModel.TypeId;
            var prefabCharacterLevelPath = $"Prefabs/Gameplay/World/Characters/Character_{characterType}_{characterLevel}";
            var characterPrefab = Resources.Load<CharacterBinder>(prefabCharacterLevelPath);
            
            var createdCharacter = Instantiate(characterPrefab);
            createdCharacter.Bind(characterViewModel);

            _createCharactersMap[characterViewModel.CharacterEntityId] = createdCharacter;
        }

        private void DestroyCharacter(CharacterViewModel characterViewModel)
        {
            if (_createCharactersMap.TryGetValue(characterViewModel.CharacterEntityId, out var characterBinder))
            {
                Destroy(characterBinder.gameObject);
                _createCharactersMap.Remove(characterViewModel.CharacterEntityId);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _viewModel.HandleTestInput();
            }
        }
    }
}