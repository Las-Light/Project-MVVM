using System;
using NothingBehind.Scripts.Game.Gameplay.View.UI;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Characters
{
    public class CharacterBinder : MonoBehaviour
    {
        private bool _triggered;
        private GameplayUIManager _gameplayUIManager;
        private int _ownerId;

        public void Bind(CharacterViewModel viewModel, GameplayUIManager gameplayUIManager)
        {
            transform.position = viewModel.Position.CurrentValue;
            _gameplayUIManager = gameplayUIManager;
            _ownerId = viewModel.CharacterEntityId;
        }

        private void OnTriggerStay(Collider other)
        {
            if (_triggered)
                return;
            other.TryGetComponent<PlayerView>(out var heroView);
            if (heroView!=null)
            {
                if (heroView.IsInteractiveActionPressed())
                {
                    _gameplayUIManager.OpenInventory(_ownerId);
                    _triggered = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _triggered = false;
        }
    }
}