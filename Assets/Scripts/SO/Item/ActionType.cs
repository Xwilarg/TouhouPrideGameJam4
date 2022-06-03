﻿using UnityEngine;

namespace TouhouPrideGameJam4.SO.Item
{
    [CreateAssetMenu(menuName = "ScriptableObject/ActionType", fileName = "ActionType")]
    public class ActionType : ScriptableObject
    {
        public Sprite ActionSprite;
        public AudioClip ActionSound;
    }
}