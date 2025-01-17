﻿using UnityEngine;

namespace TouhouPrideGameJam4.SO.Character
{
    [CreateAssetMenu(menuName = "ScriptableObject/Character/VNCharacterInfo", fileName = "VNCharacterInfo")]
    public class VNCharacterInfo : ScriptableObject
    {
        public string Key;
        public string Name;
        public Color Color;

        [Header("Expressions")]
        public Sprite NeutralExpression;
        public Sprite JoyfulExpression;
        public Sprite EyesClosedExpression;
        public Sprite SadExpression;
        public Sprite AngryExpression;
        public Sprite SurprisedExpression;
        public Sprite ShyExpression;
    }
}