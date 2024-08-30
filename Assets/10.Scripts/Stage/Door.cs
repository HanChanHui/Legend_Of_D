using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class Door : MonoBehaviour {
        [SerializeField] Constants.CardinalPoints direction;
        public Room nextRoom;   // TODO: public -> private
        BoxCollider myCollider;

        public void Init() {
            myCollider = GetComponent<BoxCollider>();
        }

        void OnTriggerEnter(Collider other) {
            if (!other.CompareTag("Hero")) {
                return;
            }

            Lock();
            GameManager.Instance.MoveHeroToNextRoom(nextRoom, direction, Unlock);
        }

        public void Lock() {
            myCollider.enabled = false;
        }

        public void Unlock() {
            myCollider.enabled = true;
        }
    }
}
