using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class Chest : MonoBehaviour {
        [SerializeField] Vector3 itemLabelOffset;
        [SerializeField] int relicIndex;        // for prototype
        RelicManager relicManager;
        ItemManager itemManager;
        Transform myTransform;
        BoxCollider myCollider;

        bool isOpened;

        void Start() {
            relicManager = GameManager.Instance.RelicManager;
            itemManager = GameManager.Instance.ItemManager;

            myTransform = transform;
            myCollider = GetComponent<BoxCollider>();
        }

        void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Hero")) {
                itemManager.RegisterItemAction(OpenAction, myTransform, itemLabelOffset);
            }
        }

        void OnTriggerExit(Collider other) {
            if (other.CompareTag("Hero")) {
                itemManager.UnregisterItemAction();
            }
        }

        void OpenAction() {
            if (isOpened) {
                return;
            }

            RelicMeta relicMeta = relicManager.GetRelicMeta(relicIndex);
            relicManager.ShowRelicCard(relicMeta);
            itemManager.UnregisterItemAction();

            isOpened = true;
            myCollider.enabled = false;
        }
    }
}
