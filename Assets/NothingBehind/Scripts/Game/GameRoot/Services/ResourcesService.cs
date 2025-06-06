using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.GameRoot.Commands.ResourcesCommands;
using NothingBehind.Scripts.Game.GameRoot.MVVM.GameResources;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.Services
{
    public class ResourcesService
    {
        public readonly ObservableList<ResourceViewModel> Resources = new();

        private readonly Dictionary<ResourceType, ResourceViewModel> _resourcesMap = new();
        private readonly ICommandProcessor _cmd;
        private CompositeDisposable _disposables = new();

        public ResourcesService(IObservableCollection<Resource> resources, ICommandProcessor cmd)
        {
            _cmd = cmd;

            foreach (var resource in resources)
            {
                CreateResourceViewModel(resource);
            }
            resources.ObserveAdd().Subscribe(e => CreateResourceViewModel(e.Value)).AddTo(_disposables);
            resources.ObserveRemove().Subscribe(e => RemoveResourceViewModel(e.Value)).AddTo(_disposables);
        }

        public void UpdateResourcesService(IObservableCollection<Resource> newResource)
        {
            // Очищаем данные и отписываемся от старой коллекции
            Resources.Clear();
            _resourcesMap.Clear();
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
            
            foreach (var resource in newResource)
            {
                CreateResourceViewModel(resource);
            }
            
            newResource.ObserveAdd().Subscribe(e => CreateResourceViewModel(e.Value)).AddTo(_disposables);
            newResource.ObserveRemove().Subscribe(e => RemoveResourceViewModel(e.Value)).AddTo(_disposables);
        }

        public CommandResult AddResources(ResourceType resourceType, int amount)
        {
            var command = new CmdResourcesAdd(resourceType, amount);

            return _cmd.Process(command);
        }

        public CommandResult TrySpendResources(ResourceType resourceType, int amount)
        {
            var command = new CmdResourcesSpend(resourceType, amount);

            return _cmd.Process(command);
        }

        public bool IsEnoughResources(ResourceType resourceType, int amount)
        {
            if (_resourcesMap.TryGetValue(resourceType, out var resourceViewModel))
            {
                return resourceViewModel.Amount.CurrentValue >= amount;
            }

            return false;
        }

        public Observable<int> ObserveResource(ResourceType resourceType)
        {
            if (_resourcesMap.TryGetValue(resourceType, out var resourceViewModel))
            {
                return resourceViewModel.Amount;
            }

            throw new Exception($"Resource of type {resourceType} doesn't exist");
        }

        private void CreateResourceViewModel(Resource resource)
        {
            var resourceViewModel = new ResourceViewModel(resource);
            _resourcesMap[resource.ResourceType] = resourceViewModel;

            Resources.Add(resourceViewModel);
        }

        private void RemoveResourceViewModel(Resource resource)
        {
            if (_resourcesMap.TryGetValue(resource.ResourceType, out var resourceViewModel))
            {
                Resources.Remove(resourceViewModel);
                _resourcesMap.Remove(resource.ResourceType);
            }
        }
    }
}