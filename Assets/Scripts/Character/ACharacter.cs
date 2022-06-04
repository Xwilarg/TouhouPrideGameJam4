﻿using System.Collections.Generic;
using System.Linq;
using TouhouPrideGameJam4.Game;
using TouhouPrideGameJam4.Inventory;
using TouhouPrideGameJam4.SO.Item;
using UnityEngine;

namespace TouhouPrideGameJam4.Character
{
    public class ACharacter : MonoBehaviour
    {
        /// <summary>
        /// Information about the character
        /// </summary>
        [SerializeField]
        private SO.CharacterInfo _info;

        /// <summary>
        /// Items that the character has
        /// </summary>
        protected Dictionary<AItemInfo, int> _items = new();

        /// <summary>
        /// Current health of the character
        /// </summary>
        private int _health;

        /// <summary>
        /// Equipped weapon
        /// </summary>
        protected WeaponInfo _equipedWeapon;

        // Used for smooth movement
        public Vector2 OldPos { set; get; }
        private float _moveTimer = 0f;

        // Position
        private Vector2Int _position;
        public Vector2Int Position
        {
            set
            {
                OldPos = transform.position;
                _moveTimer = 0f;
                _position = value;
            }
            get
            {
                return _position;
            }
        }

        protected void Init()
        {
            _health = _info.BaseHealth;
            _items = _info.StartingItems.ToDictionary(x => x, x => 1);
            _equipedWeapon = (WeaponInfo)_info.StartingItems.FirstOrDefault(x => x.Type == ItemType.Weapon);
            UpdateInventoryDisplay();
        }

        protected void UpdateC()
        {
            _moveTimer += Time.deltaTime * 10f;
            transform.position = Vector2.Lerp(OldPos, Position, Mathf.Clamp01(_moveTimer));
        }

        /// <summary>
        /// Update action bar and inventory display
        /// </summary>
        public virtual void UpdateInventoryDisplay()
        { }

        /// <summary>
        /// Remove an item from the character inventory
        /// </summary>
        /// <param name="item">Item to remove</param>
        public void RemoveItem(AItemInfo item)
        {
            if (_items[item] == 1)
            {
                _items.Remove(item);
                // Our weapon was unequipped, we equip any other one we can
                if (item is WeaponInfo weapon && IsEquipped(weapon))
                {
                    Equip((WeaponInfo)_info.StartingItems.FirstOrDefault(x => x.Type == ItemType.Weapon));
                }
            }
            else
            {
                _items[item]--;
            }
            UpdateInventoryDisplay();
        }

        /// <summary>
        /// Is the character able to attack
        /// </summary>
        public bool CanAttack() => _equipedWeapon != null;

        /// <summary>
        /// Change the currently equipped weapon to the one given in parameter
        /// </summary>
        public void Equip(WeaponInfo weapon)
        {
            _equipedWeapon = weapon;
            UpdateInventoryDisplay();
        }

        /// <summary>
        /// Is the weapon given in parameter the one equipped
        /// </summary>
        public bool IsEquipped(WeaponInfo weapon) => _equipedWeapon == weapon;

        /// <summary>
        /// Show intentory
        /// </summary>
        /// <param name="inventory">Inentory script</param>
        /// <param name="baseFilter">Base filter to apply on items</param>
        public void ShowItems(InventoryUI inventory, ItemType? baseFilter)
        {
            var items = new Dictionary<AItemInfo, int>(_items);
            inventory.UpdateContent(this, items, baseFilter);
        }

        public void TakeDamage(int amount)
        {
            _health -= amount;
            if (_health <= 0)
            {
                TurnManager.Instance.RemoveCharacter(this);
            }
            else if (_health > _info.BaseHealth)
            {
                _health = _info.BaseHealth;
            }

            // Display text with the damage done

            Color color;
            if (amount > 0) color = Color.red;
            else if (amount < 0) color = Color.green;
            else color = Color.yellow;
            TurnManager.Instance.SpawnDamageText(amount, color, Position.x + Random.Range(-.5f, .5f), Position.y + Random.Range(-.5f, .5f));

            TurnManager.Instance.UpdateDebugText();
        }

        public void Attack(ACharacter target)
        {
            target.TakeDamage(_equipedWeapon.Damage);
        }

        public override string ToString()
        {
            return $"{name} - Health: {_health} / {_info.BaseHealth}";
        }
    }
}
