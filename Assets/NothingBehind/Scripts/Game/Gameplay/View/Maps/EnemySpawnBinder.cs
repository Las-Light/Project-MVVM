using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Maps
{
    public class EnemySpawnBinder :MonoBehaviour
    {
        private bool _triggered;
        private  EnemySpawnViewModel _viewModel;

        public void Bind(EnemySpawnViewModel viewModel)
        {
            _viewModel = viewModel;
            transform.position = viewModel.Position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered)
                return;
            
            _viewModel.SpawnEnemies();
            _triggered = true;
        }
    }
}