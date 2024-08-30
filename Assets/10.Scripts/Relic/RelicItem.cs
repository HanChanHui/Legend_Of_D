using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class RelicItem : MonoBehaviour, IMemoryPool {
        [SerializeField] string mpGroup;
        [SerializeField] string mpType;
        [SerializeField] Vector3 itemLabelOffset;
        [SerializeField] int relicIndex;        // for prototype
        RelicManager relicManager;
        ItemManager itemManager;
        Transform myTransform;
        DialogueManager dialogueManager;
        ConversationScript conversationScript;

        public void MPStart() {
            relicManager = GameManager.Instance.RelicManager;
            itemManager = GameManager.Instance.ItemManager;
            dialogueManager = GameManager.Instance.DialogueManager;
            conversationScript = GameManager.Instance.ConversationScript;

            myTransform = transform;
        }

        public void Create(Vector3 pos, Quaternion rot) {
            myTransform.SetPositionAndRotation(pos, rot);
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
            RelicMeta relicMeta = relicManager.GetRelicMeta(relicIndex);
            GameManager.Instance.Hero.StopMove();

            itemManager.UnregisterItemAction();

            MyDestroy();
            dialogueManager.StartConversation(conversationScript.GetConversation("AfterBoss"), () => {
                Invoke(nameof(ShowNotification), 1.0f);
                GameSave.Instance.SetFirstConversationIsDone(true);
            });
        }

        public void MyDestroy() {
            PoolManager.Instance.RemoveItem(mpGroup, mpType, gameObject);
        }

        void ShowNotification() {
            GameManager.Instance.NotificationPanel.Show();
        }
    }
}
