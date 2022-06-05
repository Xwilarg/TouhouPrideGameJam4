﻿using TouhouPrideGameJam4.Character;
using TouhouPrideGameJam4.Inventory;
using UnityEngine;

namespace TouhouPrideGameJam4.SO.Item
{
    [CreateAssetMenu(menuName = "ScriptableObject/Item/WeaponInfo", fileName = "WeaponInfo")]
    public class WeaponInfo : AItemInfo
    {
        public int Damage;

        public override ItemType Type => ItemType.Weapon;
        public StatusType[] HitEffects;
        public override string Description => $"{Damage} damages";

        public override string ActionName => "Equip";

        public override string ActionTooltip => "Set the item as your main weapon";

        public override void DoAction(ACharacter owner)
        {
            owner.Equip(this);
        }
    }
}