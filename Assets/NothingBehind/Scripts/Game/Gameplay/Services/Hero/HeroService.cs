using NothingBehind.Scripts.Game.Gameplay.Commands.Hero;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities.Hero;
using NothingBehind.Scripts.Game.State.Root;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Services.Hero
{
    public class HeroService
    {
        public readonly ReactiveProperty<HeroViewModel> HeroViewModel = new();

        private readonly MoveHeroService _moveHeroService;
        private readonly LookHeroService _lookHeroService;
        private readonly GameStateProxy _gameState;
        private readonly ICommandProcessor _cmd;
        private readonly SceneEnterParams _sceneEnterParams;

        public HeroService(MoveHeroService moveHeroService,
            LookHeroService lookHeroService,
            GameStateProxy gameState,
            ICommandProcessor cmd,
            SceneEnterParams sceneEnterParams)
        {
            _moveHeroService = moveHeroService;
            _lookHeroService = lookHeroService;
            _gameState = gameState;
            _cmd = cmd;
            _sceneEnterParams = sceneEnterParams;

            InitialHero();
        }

        public bool UpdateHeroPosOnMap(Vector3 position)
        {
            var command = new CmdUpdateHeroPosOnMap(position);
            var result = _cmd.Process(command);
            return result;
        }

        private void InitialHero()
        {
            InitialPosOnMap();
            var hero = _gameState.Hero;
            CreateHeroViewModel(hero.Value);
        }

        private bool InitialPosOnMap()
        {
            var command = new CmdInitHeroPosOnMap(_sceneEnterParams.TargetMapId);
            var result = _cmd.Process(command);

            return result;
        }

        private void CreateHeroViewModel(HeroProxy heroProxy)
        {
            var viewModel = new HeroViewModel(heroProxy,this, _moveHeroService, _lookHeroService);

            HeroViewModel.Value = viewModel;
        }
    }
}