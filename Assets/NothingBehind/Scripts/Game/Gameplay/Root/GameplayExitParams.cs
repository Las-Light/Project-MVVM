using NothingBehind.Scripts.Game.MainMenu.Root;

namespace NothingBehind.Scripts.Game.Gameplay.Root
{
    public class GameplayExitParams
    {
        public MainMenuEnterParams MainMenuEnterParams { get; }

        public GameplayExitParams(MainMenuEnterParams mainMenuEnterParams)
        {
            MainMenuEnterParams = mainMenuEnterParams;
        }
    }
}