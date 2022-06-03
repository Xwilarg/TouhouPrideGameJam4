﻿using TouhouPrideGameJam4.SO.Item;
using UnityEngine;
using UnityEngine.UI;

namespace TouhouPrideGameJam4.UI
{
    public class ShortcutButton : MonoBehaviour
    {
        public void OnClick()
        {
            UIManager.Instance.ShortcutTarget = this;
        }

        /// <summary>
        /// What is actually in the tile (the object the player has set there)
        /// </summary>
        private Image _contentImage;
        /// <summary>
        /// Highlight color indicating if the item is selected
        /// </summary>
        private Image _highlightImage;
        /// <summary>
        /// Item that is contained there
        /// </summary>
        private AItemInfo _content;

        private void Awake()
        {
            _contentImage = transform.GetChild(0).GetComponent<Image>();
            _highlightImage = GetComponent<Image>();
        }

        public void SetHighlight()
        {
            _highlightImage.color = Color.yellow;
        }

        public void ClearHighlight()
        {
            _highlightImage.color = new Color(1f, 1f, 1f, .4f);
        }

        public void ClearColor()
        {
            _contentImage.color = new Color(0f, 0f, 0f, 0f);
        }

        public void SetContent(AItemInfo item)
        {
            if (item != _content)
            {
                UIManager.Instance.ShortcutTarget = null;
            }
            _content = item;
            _contentImage.sprite = item != null ? item.Sprite : null;
            _contentImage.color = item == null ? new Color(0f, 0f, 0f, 0f) : Color.white;
        }
    }
}