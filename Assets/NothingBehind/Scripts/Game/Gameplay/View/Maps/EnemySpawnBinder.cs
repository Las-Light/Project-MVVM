using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Maps
{
    public class EnemySpawnBinder : MonoBehaviour
    {
        private bool _triggered;
        private EnemySpawnViewModel _viewModel;

        public void Bind(EnemySpawnViewModel viewModel)
        {
            _viewModel = viewModel;
            _triggered = viewModel.Triggered.Value;
            transform.position = viewModel.Position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered)
                return;

            _viewModel.SpawnEnemies();
            if (_viewModel.TriggeredEnemySpawn())
            {
                _triggered = true;
            }
        }

        private void OnDestroy()
        {
            _viewModel.Triggered.Dispose();
        }
    }
}