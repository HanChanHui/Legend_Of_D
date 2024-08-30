using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class GameSave : MonoBehaviour {
        static GameSave instance = null;
        public static GameSave Instance { get => instance; }

        void Awake() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        bool firstConversationIsDone;
        public bool FirstConversationIsDone { get => firstConversationIsDone; }

        public void SetFirstConversationIsDone(bool value) {
            firstConversationIsDone = value;
        }
    }
}
