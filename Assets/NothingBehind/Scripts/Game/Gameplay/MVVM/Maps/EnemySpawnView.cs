using System;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Maps
{
    public class EnemySpawnView : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetLayers;
        private ReactiveProperty<bool> _triggered;
        private EnemySpawnViewModel _viewModel;
        private IDisposable _disposable;

        public void Bind(EnemySpawnViewModel viewModel)
        {
            _viewModel = viewModel;
            _triggered = viewModel.Triggered;
            transform.position = viewModel.Position;
            _disposable = _triggered.Skip(1).Subscribe(value =>
            {
                _triggered.Value = value;
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered.Value)
                return;

            if (((1 << other.gameObject.layer) & _targetLayers) == 0)
                return;
            StartCoroutine(_viewModel.SpawnEnemies()); // Задержка для оптимизации
            //if (_viewModel.TriggeredEnemySpawn().Success) _triggered = true;
        }

        private void OnDestroy()
        {
            _disposable.Dispose();
        }
    }
}