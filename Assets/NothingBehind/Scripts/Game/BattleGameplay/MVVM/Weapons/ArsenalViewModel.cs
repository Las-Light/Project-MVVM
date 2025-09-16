using System;
using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.BattleGameplay.Commands.ArsenalCommands;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Equipments;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.Settings.Gameplay.Weapons;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.AmmoItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.MagazinesItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.WeaponItems;
using NothingBehind.Scripts.Game.State.Weapons;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using NothingBehind.Scripts.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.Weapons
{
    public class ArsenalViewModel : IDisposable
    {
        public int OwnerId { get; }
        public EntityType OwnerType { get; }
        public IReadOnlyObservableDictionary<SlotType, Item> EquipmentItems { get; }
        public IReadOnlyObservableDictionary<InventoryGridViewModel, ObservableList<AmmoItem>> AllAmmo => _allAmmo;


        public IReadOnlyObservableDictionary<InventoryGridViewModel, ObservableList<MagazinesItem>> AllMagazines =>
            _allMagazines;

        public IReadOnlyObservableList<WeaponViewModel> AllWeaponViewModels => _allWeaponViewModels;
        public Dictionary<int, WeaponViewModel> WeaponViewModelMap => _weaponViewModelMap;
        public ReactiveProperty<SlotType> CurrentWeaponSlot { get; } = new();
        public ReactiveProperty<WeaponViewModel> CurrentWeapon { get; } = new();

        private readonly ObservableList<WeaponViewModel> _allWeaponViewModels = new();
        private readonly Dictionary<int, WeaponViewModel> _weaponViewModelMap = new();
        private readonly Dictionary<WeaponViewModel, Weapon> _weaponsMap = new();
        private readonly Dictionary<WeaponName, WeaponSettings> _weaponSettingsMap = new();

        private readonly ObservableDictionary<InventoryGridViewModel, ObservableList<AmmoItem>> _allAmmo = new();


        private readonly ObservableDictionary<InventoryGridViewModel, ObservableList<MagazinesItem>> _allMagazines =
            new();

        private readonly Arsenal _arsenal;
        private readonly ICommandProcessor _commandProcessor;
        private readonly CompositeDisposable _disposables = new();


        public ArsenalViewModel(Arsenal arsenal,
            EquipmentViewModel equipmentViewModel,
            InventoryViewModel inventoryViewModel,
            ICommandProcessor commandProcessor,
            WeaponsSettings weaponsSettings)
        {
            _arsenal = arsenal;
            _commandProcessor = commandProcessor;
            OwnerId = arsenal.OwnerId;
            OwnerType = arsenal.OwnerType;

            //Устанавливаем курентСлот из даты
            CurrentWeaponSlot.OnNext(arsenal.CurrentWeaponSlot.Value);

            foreach (var weaponConfig in weaponsSettings.WeaponConfigs)
            {
                _weaponSettingsMap[weaponConfig.WeaponName] = weaponConfig;
            }

            // Добавляем в арсенал оружие которое экипировано
            EquipmentItems = equipmentViewModel.AllEquippedItems;
            foreach (var equippedItem in EquipmentItems)
            {
                if (equippedItem.Key is SlotType.Weapon1 or SlotType.Weapon2)
                {
                    if (equippedItem.Value is WeaponItem weaponItem)
                    {
                        var weapon =
                            arsenal.Weapons.FirstOrDefault(weapon => weapon.Id == weaponItem.Id);
                        if (weapon == null)
                        {
                            AddWeaponToArsenal(arsenal.OwnerType, weaponItem.Weapon);
                        }
                    }
                }
            }

            // Проверяем какие в инвентаре есть патроны и магазины полсе чего добавляем их в арсенал с указанием в какой сетке лежал предмет 

            foreach (var inventoryGrid in inventoryViewModel.AllInventoryGrids)
            {
                var magazinesCollection = new List<MagazinesItem>();
                var ammoCollection = new List<AmmoItem>();
                foreach (var item in inventoryGrid.ItemsMap)
                {
                    if (item.Value is MagazinesItem magazinesItem)
                    {
                        magazinesCollection.Add(magazinesItem);
                    }

                    if (item.Value is AmmoItem ammoItem)
                    {
                        ammoCollection.Add(ammoItem);
                    }
                }

                _allMagazines.Add(inventoryGrid, new ObservableList<MagazinesItem>());
                _allMagazines[inventoryGrid].AddRange(magazinesCollection);
                _allAmmo.Add(inventoryGrid, new ObservableList<AmmoItem>());
                _allAmmo[inventoryGrid].AddRange(ammoCollection);

                //тут же подписываемся на добавление и удаление предметов из сетки
                inventoryGrid.ItemsMap.ObserveRemove().Subscribe(e =>
                {
                    var removedItem = e.Value.Value;
                    if (removedItem is MagazinesItem magazinesItem)
                    {
                        _allMagazines[inventoryGrid].Remove(magazinesItem);
                    }

                    if (removedItem is AmmoItem ammoItem)
                    {
                        _allAmmo[inventoryGrid].Remove(ammoItem);
                    }
                }).AddTo(_disposables);
                inventoryGrid.ItemsMap.ObserveAdd().Subscribe(e =>
                {
                    var addedItem = e.Value.Value;
                    if (addedItem is MagazinesItem magazinesItem)
                    {
                        _allMagazines[inventoryGrid].Add(magazinesItem);
                    }

                    if (addedItem is AmmoItem ammoItem)
                    {
                        _allAmmo[inventoryGrid].Add(ammoItem);
                    }
                }).AddTo(_disposables);
            }

            //создаем вью-модели Weapon (в том числе Unarmed, которая добавлена при инициализации)
            foreach (var weapon in arsenal.Weapons)
            {
                var viewModel = CreateWeaponViewModel(weapon, this);
                _weaponsMap[viewModel] = weapon;
            }

            EquipmentItems.ObserveRemove().Subscribe(e =>
            {
                var removedItem = e.Value.Value;
                if (removedItem is WeaponItem weaponItem)
                {
                    RemoveWeaponFromArsenal(arsenal.OwnerType, weaponItem.Id);
                }
            }).AddTo(_disposables);

            EquipmentItems.ObserveAdd().Subscribe(e =>
            {
                var addedItem = e.Value;
                if (addedItem.Key is SlotType.Weapon1 or SlotType.Weapon2)
                {
                    if (addedItem.Value is WeaponItem weaponItem)
                    {
                        var weapon =
                            arsenal.Weapons.FirstOrDefault(weapon => weapon.Id == weaponItem.Id);
                        if (weapon == null)
                        {
                            AddWeaponToArsenal(arsenal.OwnerType, weaponItem.Weapon);
                        }
                    }
                }
            }).AddTo(_disposables);

            arsenal.Weapons.ObserveRemove().Subscribe(e =>
            {
                var removedWeapon = e.Value;
                var viewModel = RemoveWeaponViewModel(removedWeapon);
                _weaponsMap.Remove(viewModel);
            }).AddTo(_disposables);

            arsenal.Weapons.ObserveAdd().Subscribe(e =>
            {
                var addedWeapon = e.Value;
                var viewModel = CreateWeaponViewModel(addedWeapon, this);
                _weaponsMap[viewModel] = addedWeapon;
            }).AddTo(_disposables);

            //Подписываемся на курентСлот и передаем его в дату для сохранения стейта (важно скипать 1 событие,
            // иначе событие зарекурситься
            CurrentWeaponSlot.Skip(1).Subscribe(slot => { arsenal.CurrentWeaponSlot.OnNext(slot); })
                .AddTo(_disposables);
        }

        private CommandResult AddWeaponToArsenal(EntityType ownerType, Weapon weapon)
        {
            var command = new CmdAddWeaponToArsenal(ownerType, OwnerId, weapon);
            var result = _commandProcessor.Process(command);

            return result;
        }

        private CommandResult RemoveWeaponFromArsenal(EntityType ownerType, int weaponItemId)
        {
            var command = new CmdRemoveWeaponFromArsenal(ownerType, OwnerId, weaponItemId);
            var result = _commandProcessor.Process(command);

            return result;
        }

        private WeaponViewModel CreateWeaponViewModel(Weapon weapon, ArsenalViewModel arsenalViewModel)
        {
            var weaponSettings = _weaponSettingsMap[weapon.WeaponName];
            var weaponViewModel = new WeaponViewModel(weapon, weaponSettings, arsenalViewModel);
            _allWeaponViewModels.Add(weaponViewModel);
            _weaponViewModelMap[weapon.Id] = weaponViewModel;

            return weaponViewModel;
        }

        private WeaponViewModel RemoveWeaponViewModel(Weapon weapon)
        {
            if (!_weaponViewModelMap.TryGetValue(weapon.Id, out var viewModel))
            {
                throw new Exception($"WeaponViewModel with Id {weapon.Id} not found!");
            }

            _allWeaponViewModels.Remove(viewModel);
            _weaponViewModelMap.Remove(weapon.Id);

            return viewModel;
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}