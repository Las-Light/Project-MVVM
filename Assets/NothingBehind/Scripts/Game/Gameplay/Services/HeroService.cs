using NothingBehind.Scripts.Game.Gameplay.Commands;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Hero;
using NothingBehind.Scripts.Game.State.Root;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class HeroService
    {
        public readonly ReactiveProperty<HeroViewModel> HeroViewModel = new();

        private readonly GameStateProxy _gameState;
        private readonly ICommandProcessor _cmd;
        private readonly SceneEnterParams _sceneEnterParams;

        public HeroService(GameStateProxy gameState, ICommandProcessor cmd, SceneEnterParams sceneEnterParams)
        {
            _gameState = gameState;
            _cmd = cmd;
            _sceneEnterParams = sceneEnterParams;

            InitialHero();
        }

        private void InitialHero()
        {
            InitialPosOnMap(_cmd, _sceneEnterParams);
            var hero = _gameState.Hero;
            CreateHeroViewModel(hero.Value);
        }

        private bool InitialPosOnMap(ICommandProcessor commandProcessor, SceneEnterParams sceneEnterParams)
        {
            var command = new CmdCreateHero(sceneEnterParams.TargetMapId);
            var result = commandProcessor.Process(command);

            return result;
        }

        private void CreateHeroViewModel(HeroProxy heroProxy)
        {
            var viewModel = new HeroViewModel(heroProxy, this);

            HeroViewModel.Value = viewModel;
        }
    }
}