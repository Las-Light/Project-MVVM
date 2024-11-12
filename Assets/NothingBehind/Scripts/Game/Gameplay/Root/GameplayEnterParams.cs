using NothingBehind.Scripts.Game.GameRoot;

namespace NothingBehind.Scripts.Game.Gameplay.Root
{
    public class GameplayEnterParams : SceneEnterParams
    {
        public string SaveFileName { get; }
        
        public GameplayEnterParams(string saveFileName) : base(Scenes.GAMEPLAY)
        {
            SaveFileName = saveFileName;
        }
    }
}