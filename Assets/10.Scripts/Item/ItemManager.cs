using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HornSpirit {
    public class ItemManager : MonoBehaviour {
        [Header("UI")]
        [SerializeField] ItemOpenLabel itemOpenLabel;
        UnityAction itemAction;
        GameController gameController;

        public void Init() {
            gameController = GameManager.Instance.GameController;
        }

        public void RegisterItemAction(UnityAction action, Transform target, Vector3 offset = default) {
            itemAction = action;
            itemOpenLabel.Show(target, offset);
            gameController.EnableAction(GameController.Type.Chest);
        }

        public void RegisterItemAction(UnityAction action) {
            itemAction = action;
        }

        public void UnregisterItemAction() {
            itemAction = null;
            itemOpenLabel.Hide();
            gameController.EnableAction(GameController.Type.Default);
        }

        public void DoItemAction() {
            itemAction?.Invoke();
        }
    }
}
