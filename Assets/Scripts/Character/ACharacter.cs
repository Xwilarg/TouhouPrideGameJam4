﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouPrideGameJam4.Character.Player;
using TouhouPrideGameJam4.Dialog;
using TouhouPrideGameJam4.Game;
using TouhouPrideGameJam4.Game.Persistency;
using TouhouPrideGameJam4.Inventory;
using TouhouPrideGameJam4.Map;
using TouhouPrideGameJam4.SO.Item;
using TouhouPrideGameJam4.Sound;
using TouhouPrideGameJam4.UI;
using UnityEngine;
using static UnityEngine.UIElements.NavigationMoveEvent;

namespace TouhouPrideGameJam4.Character
{
    public class ACharacter : MonoBehaviour
    {
        /// <summary>
        /// Information about the character
        /// </summary>
        [SerializeField]
        protected SO.Character.CharacterInfo _info;

        public SO.Character.CharacterInfo Info => _info;

        /// <summary>
        /// Items that the character has
        /// </summary>
        protected List<AItemInfo> _items = new();

        /// <summary>
        /// Current health of the character
        /// </summary>
        protected int _health;

        /// <summary>
        /// Equipped weapon
        /// </summary>
        private WeaponInfo _equippedWeapon;
        /// <summary>
        /// single-use items won't be removed from inventory on use if true
        /// </summary>
        [SerializeField] public bool hasInfiniteItems = false;
        public WeaponInfo EquippedWeapon
        {
            set
            {
                _equippedWeapon = value;
            }
            get
            {
                return _equippedWeapon != null ? _equippedWeapon : _info.DefaultWeapon;
            }
        }

        protected float _baseHealthMult = 1f;
        private int MaxHealth => Mathf.FloorToInt(_info.BaseHealth * _baseHealthMult);

        private Animator _anim;

        // Used for smooth movement
        public Vector2 OldPos { set; get; }
        private float _moveTimer = 0f;

        protected readonly Dictionary<StatusType, int> _currentEffects = new();

        public Team Team { set; get; }
        public bool IsBoss { set; get; }

        protected bool _didReachPosition = true; // When character is done moving where it needed to

        // Position
        private Vector2Int _position;
        public Vector2Int Position
        {
            set
            {
                _anim?.SetBool("IsWalking", true);
                _anim?.SetTrigger("StartWalking");
                OldPos = transform.position;
                _moveTimer = 0f;
                _position = value;
            }
            get
            {
                return _position;
            }
        }

        private Direction _direction = Direction.Up;
        public Direction Direction
        {
            set
            {
                _direction = value;
                _anim?.SetInteger("Direction", (int)value);
            }
            get => _direction;
        }

        public Vector2Int RelativeDirection => Direction switch
        {
            Direction.Up => Vector2Int.up,
            Direction.Down => Vector2Int.down,
            Direction.Left => Vector2Int.left,
            Direction.Right => Vector2Int.right,
            _ => throw new System.NotImplementedException()
        };

        protected void Init(Team team)
        {
            Team = team;
            _anim = GetComponent<Animator>();
            _health = MaxHealth;
            _items = _info.StartingItems.Where(x => x.Item != null).Select(x => x.Item).ToList();
            EquippedWeapon = (WeaponInfo)_info.StartingItems.FirstOrDefault(x => x.Item != null && x.Item.Type == ItemType.Weapon)?.Item;
            UpdateInventoryDisplay();
        }

        protected void UpdateC()
        {
            if (_moveTimer < 1f)
            {
                _moveTimer += Time.deltaTime * 5f;
                var t = Mathf.Clamp01(_moveTimer);
                transform.position = Vector2.Lerp(OldPos, Position, t);
                _didReachPosition = t == 1f;
                if (_moveTimer >= 1f)
                {
                    _anim?.SetBool("IsWalking", false);
                }
            }
        }

        /// <summary>
        /// Called at the end of a turn
        /// </summary>
        public void EndTurn()
        {
            for (int i = _currentEffects.Keys.Count - 1; i >= 0; i--)
            {
                var key = _currentEffects.Keys.ElementAt(i);
                if (_currentEffects[key] == 1)
                {
                    _currentEffects.Remove(key);
                }
                else
                {
                    _currentEffects[key]--;
                }
            }
            OnStatusChange();
        }

        public void AddStatus(StatusType status, int duration)
        {
            if (_currentEffects.ContainsKey(status))
            {
                _currentEffects[status] += duration;
            }
            else
            {
                _currentEffects.Add(status, duration);
            }
            OnStatusChange();
        }

        public void RemoveStatus(StatusType status)
        {
            _currentEffects.Remove(status);
            OnStatusChange();
        }

        public virtual void OnStatusChange()
        { }

        public virtual StatusType[] CurrentEffects => _currentEffects.Keys.ToArray();

        private bool Has(StatusType status) => CurrentEffects.Contains(status);

        /// <summary>
        /// Update action bar and inventory display
        /// </summary>
        public virtual void UpdateInventoryDisplay()
        { }

        public void AddItem(AItemInfo item)
        {
            _items.Add(item);
            if (_equippedWeapon == null && item is WeaponInfo weapon)
            {
                Equip(weapon);
            }
            UpdateInventoryDisplay();
        }

        /// <summary>
        /// Remove an item from the character inventory
        /// </summary>
        /// <param name="item">Item to remove</param>
        public void RemoveItem(AItemInfo item)
        {
            _items.Remove(item);
            // Our weapon was unequipped, we equip any other one we can
            if (item is WeaponInfo weapon && weapon == EquippedWeapon)
            {
                Equip((WeaponInfo)_items.FirstOrDefault(x => x.Type == ItemType.Weapon));
            }
            UpdateInventoryDisplay();
        }

        public bool CanDoSomething() => !Has(StatusType.Stunned);
        public bool CanMove() => !Has(StatusType.Frozen) && !Has(StatusType.Binded);

        /// <summary>
        /// Change the currently equipped weapon to the one given in parameter
        /// </summary>
        public void Equip(WeaponInfo weapon)
        {
            EquippedWeapon = weapon;
            UpdateInventoryDisplay();
        }

        /// <summary>
        /// Show inventory
        /// </summary>
        /// <param name="inventory">Inventory script</param>
        /// <param name="baseFilter">Base filter to apply on items</param>
        public void ShowItems(InventoryUI inventory, ItemType? baseFilter)
        {
            inventory.UpdateContent(this, _items, baseFilter);
        }

        public bool IsHealthFull => _health == MaxHealth;

        public virtual void TakeDamage(WeaponInfo weapon, int amount)
        {
            string damageText = null;
            if (Has(StatusType.AvoidUp) && Random.Range(0, 100) < 10)
            {
                amount = 0;
                damageText = "MISS";
            }

            if (weapon != null && weapon.HitEffects != null)
            {
                foreach (var status in weapon.HitEffects)
                {
                    if (Random.Range(0, 100) < status.Chance)
                    {
                        AddStatus(status.Effect, status.TurnCount);
                    }
                }
            }

            if (weapon != null && weapon.SoundOverride != null)
            {
                SoundManager.Instance.PlayAttackClip(weapon.SoundOverride);
            }

            if (amount > 0)
            {
                if (Has(StatusType.Invincible))
                {
                    amount = 0;
                }
                else if (Has(StatusType.DefenseBoosted))
                {
                    amount /= 2;
                }
            }

            _health -= amount;
            if (IsBoss)
            {
                UIManager.Instance.SetBossHealth((float)_health / MaxHealth);
                if (_health <= 0f)
                {
                    _anim.SetTrigger("Die");
                    StoryManager.Instance.EnableEndCinematic();
                    StartCoroutine(WaitAndShowEnding());
                }
            }
            else if (_health <= 0)
            {
                PlayerController.Instance.IncreaseEnergy(Random.Range(Info.MinEnergyOnDeath, Info.MaxEnergyOnDeath + 1));
                if (_info.StartingItems.Any() && !MapManager.Instance.IsAnythingOnFloor(Position.x, Position.y)) // TODO: Put object on the next tile?
                {
                    var items = _info.StartingItems;
                    if (Has(StatusType.LootUp) && Random.Range(0, 100) < 10)
                    {
                        items = items.Where(x => x.Item != null).ToArray(); // Ensure that we will loot something
                    }
                    var sumDrop = _info.StartingItems.Sum(x => x.Weight);
                    var targetWeight = Random.Range(0, sumDrop);
                    var index = 0;
                    do
                    {
                        targetWeight -= _info.StartingItems[index].Weight;
                        index++;
                    } while (targetWeight > 0);
                    index--;
                    var target = _info.StartingItems[index];
                    if (target.Item != null)
                    {
                        MapManager.Instance.SetItemOnFloor(Position.x, Position.y, target.Item);
                        UIManager.Instance.UpdateUIOnNewTile();
                    }
                }
                TurnManager.Instance.RemoveCharacter(this);
            }
            else if (_health > MaxHealth)
            {
                _health = MaxHealth;
            }

            // Display text with the damage done

            Color color;
            if (amount > 0) color = Color.red;
            else if (amount < 0) color = Color.green;
            else color = Color.yellow;
            if (damageText == null)
            {
                damageText = amount.ToString();
            }
            TurnManager.Instance.SpawnDamageText(damageText, color, Position.x + Random.Range(-.5f, .5f), Position.y + Random.Range(-.5f, .5f));
        }

        private IEnumerator WaitAndShowEnding()
        {
            yield return new WaitForSeconds(4f);
            StoryManager.Instance.ProgressIsAvailable(StoryProgress.Ending);
            StoryManager.Instance.ShowShrine();
        }

        public void Attack(ACharacter target)
        {
            target.TakeDamage(EquippedWeapon, EquippedWeapon.Damage * (Has(StatusType.AttackBoosted) ? 2 : 1) * (EquippedWeapon.IsHeal ? -1 : 1));
            if (EquippedWeapon.IsSingleUse && !hasInfiniteItems)
            {
                RemoveItem(EquippedWeapon);
            }
            if (target != null) // Target is not dead
            {
                if (target.EquippedWeapon != null && target.EquippedWeapon.CanCounterAttack)
                {
                    target.Attack(this);
                }
            }
        }

        public override string ToString()
        {
            return $"{name} - Health: {_health} / {MaxHealth}";
        }
    }
}
