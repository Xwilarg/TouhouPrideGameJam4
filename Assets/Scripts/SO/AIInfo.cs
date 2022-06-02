﻿using UnityEngine;

namespace TouhouPrideGameJam4.SO
{
    [CreateAssetMenu(menuName = "ScriptableObject/AIInfo", fileName = "AIInfo")]
    public class AIInfo : ScriptableObject
    {
        [Tooltip("If the enemy is further than this distance from the target, it doesn't go towards him")]
        public int MaxDistanceToMove;
    }
}