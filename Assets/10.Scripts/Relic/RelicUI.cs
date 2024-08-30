using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HornSpirit {
    public class RelicUI : MonoBehaviour {
        [Header("UI")]
        [SerializeField] Image icon;
        [SerializeField] TextMeshProUGUI quantityText;

        public void UpdateUI(Relic relic) {
            icon.sprite = IconManager.Instance.GetRelic(relic.Meta.ID);
            UpdateQuantity(relic.Quantity);
        }

        public void UpdateQuantity(int quantity) {
            quantityText.text = quantity.ToString();
        }
    }
}
