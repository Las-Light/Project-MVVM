using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using ObservableCollections;

namespace NothingBehind.Scripts.Game.Gameplay.Root.View
{
    public class WorldGameplayRootViewModel
    {
        public readonly IObservableCollection<CharacterViewModel> AllCharacters;

        public WorldGameplayRootViewModel(CharactersService charactersService)
        {
            AllCharacters = charactersService.AllCharacters;
        }
    }
}