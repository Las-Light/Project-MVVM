using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Equipments
{
    public class EquipmentView: MonoBehaviour
    {
        [SerializeField] private GameObject _backpackSlotPrefab;
        [SerializeField] private GameObject _chestRigSlotPrefab;

        private EquipmentViewModel _equipmentViewModel;
        
        public int OwnerId { get; set; }
        public void Bind(EquipmentViewModel equipmentViewModel)
        {
            _equipmentViewModel = equipmentViewModel;
            // Задаем размер инвентаря в соответствии с размером экрана
            var viewScreenSize = new Vector2(Screen.width / 3, Screen.height);
            transform.parent.GetComponent<RectTransform>().sizeDelta = viewScreenSize;
            GetComponent<RectTransform>().sizeDelta = viewScreenSize;
        }
    }
}