﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TouhouPrideGameJam4.Character;
using TouhouPrideGameJam4.SO.Item;
using UnityEngine;
using UnityEngine.UI;

namespace TouhouPrideGameJam4.Inventory
{
    /// <summary>
    /// NOT USED ANYMORE
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField]
        private Transform _nameContainer, _descriptionContainer, _quantityContainer, _actionContainer;

        [SerializeField]
        private GameObject _textPrefab, _buttonPrefab;

        private IReadOnlyList<AItemInfo> _items;
        private ACharacter _owner;

        public void UpdateContent(ACharacter owner, IReadOnlyList<AItemInfo> items, ItemType? filter)
        {
            _owner = owner;
            _items = items;

            for (var i = 0; i < _nameContainer.childCount; i++) Destroy(_nameContainer.GetChild(i).gameObject);
            for (var i = 0; i < _descriptionContainer.childCount; i++) Destroy(_descriptionContainer.GetChild(i).gameObject);
            for (var i = 0; i < _quantityContainer.childCount; i++) Destroy(_quantityContainer.GetChild(i).gameObject);
            for (var i = 0; i < _actionContainer.childCount; i++) Destroy(_actionContainer.GetChild(i).gameObject);
            foreach (var item in items.Where(x => filter == null || x.Type == filter.Value))
            {
                Instantiate(_textPrefab, _nameContainer).GetComponent<TMP_Text>().text = item.Name + (item.Type == ItemType.Weapon && _owner.EquippedWeapon == (WeaponInfo)item ? " (Equipped)" : "");
                Instantiate(_textPrefab, _descriptionContainer).GetComponent<TMP_Text>().text = item.Description;
                //Instantiate(_textPrefab, _quantityContainer).GetComponent<TMP_Text>().text = item.Value.ToString();
                var button = Instantiate(_buttonPrefab, _actionContainer);
                button.GetComponentInChildren<TMP_Text>().text = item.ActionName;
                button.GetComponent<Button>().onClick.AddListener(new(() =>
                {
                    item.DoAction(_owner);
                    _owner.ShowItems(this, filter);
                }));
            }
        }
    }
}
