﻿using System.Linq;
using TMPro;
using TouhouPrideGameJam4.Game.Persistency;
using TouhouPrideGameJam4.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TouhouPrideGameJam4.Menu
{
    public class RunMenuManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _shopButtonPrefab;

        [SerializeField]
        private Transform _shopContainer;

        [SerializeField]
        private TMP_Text _energyText;

        public void StartGame()
        {
            SceneManager.LoadScene("Main");
        }

        public void DisplayShop()
        {
            for (int i = 0; i < _shopContainer.childCount; i++) Destroy(_shopContainer.GetChild(i).gameObject);

            AddButton(ItemType.Potion);
            AddButton(ItemType.Spell);
            AddButton(ItemType.Weapon);

            _energyText.text = $"Current energy: {PersistencyManager.Instance.TotalEnergy}";
        }

        private void AddButton(ItemType type)
        {
            const int price = 200;
            if (!PersistencyManager.Instance.BuyableItems.Any(x => x.Item.Type == type))
            {
                return;
            }
            var go = Instantiate(_shopButtonPrefab, _shopContainer);
            if (PersistencyManager.Instance.TotalEnergy < price)
            {
                go.GetComponent<Button>().interactable = false;
            }
            else
            {
                go.GetComponent<Button>().onClick.AddListener(new(() =>
                {
                    PersistencyManager.Instance.TotalEnergy -= price;
                    var list = PersistencyManager.Instance.BuyableItems.Where(x => x.Item.Type == type).ToArray();
                    var item = list[Random.Range(0, list.Length)];
                    PersistencyManager.Instance.UnlockItem(item.Item);
                    if (!PersistencyManager.Instance.BuyableItems.Any(x => x.Item.Type == type))
                    {
                        Destroy(go);
                    }

                }));
            }
            go.GetComponentInChildren<TMP_Text>().text = $"Random {type.ToString().ToLowerInvariant()}\n\n{price} energy";
        }
    }
}