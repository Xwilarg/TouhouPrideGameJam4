﻿using System.Collections.Generic;
using System.Linq;
using TMPro;
using TouhouPrideGameJam4.Character;
using TouhouPrideGameJam4.Inventory;
using TouhouPrideGameJam4.SO;
using UnityEngine;
using UnityEngine.UI;

namespace TouhouPrideGameJam4.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField]
        private GameObject _statusPrefab;

        [SerializeField]
        private Transform _statusContainer;

        [SerializeField]
        private StatusInfo[] _status;

        private void Awake()
        {
            Instance = this;
            _source = GetComponent<AudioSource>();
            _baseHealth = _healthBar.rectTransform.sizeDelta.x;
        }

        public void SetHealth(float value)
        {
            _healthBar.rectTransform.sizeDelta = new Vector2(value * _baseHealth, _healthBar.rectTransform.sizeDelta.y);
        }

        /// <summary>
        /// Remove tint that indicate that an element is selected
        /// </summary>
        public void ResetHighlight()
        {
            foreach (var btn in ShortcutInventory)
            {
                btn.ClearHighlight();
            }
        }

        public void UseCurrent()
        {
            if (_shortcutTarget != null && !_shortcutTarget.IsEmpty)
            {
                _shortcutTarget.Use();
            }
            else
            {
                PlaySound(ClipNone);
            }
        }

        public void UpdateStatus(IReadOnlyDictionary<StatusType, int> effects)
        {
            for (int i = 0; i < _statusContainer.childCount; i++) Destroy(_statusContainer.GetChild(i).gameObject);

            foreach (var e in effects)
            {
                var go = Instantiate(_statusPrefab, _statusContainer);
                go.GetComponent<Image>().sprite = _status.FirstOrDefault(x => x.Effect == e.Key).Sprite;
                go.GetComponentInChildren<TMP_Text>().text = e.Value.ToString();
            }
        }

        /// <summary>
        /// Element currently selected in the action bar
        /// </summary>
        public ShortcutButton ShortcutTarget
        {
            set
            {
                ResetHighlight();
                if (value == null)
                {
                    ShortcutAction.sprite = ActionNone;
                }
                else
                {
                    value.SetHighlight();
                    ShortcutAction.sprite = value.IsEmpty ? ActionNone : value.ActionSprite;
                }
                _shortcutTarget = value;
            }
            get => _shortcutTarget;
        }

        public void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                _source.PlayOneShot(clip);
            }
        }

        private float _baseHealth;
        [SerializeField]
        private Image _healthBar;

        private ShortcutButton _shortcutTarget = null;

        public Image ShortcutEquipped;
        public ShortcutButton[] ShortcutInventory;
        public Image ShortcutAction;
        public Sprite ActionNone;
        public AudioClip ClipNone;

        public Tooltip Tooptip;

        private AudioSource _source;
    }
}
