using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class GameData : MonoBehaviour {
        static GameData instance = null;
        public static GameData Instance { get { return instance; } }

        void Awake() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(gameObject);
            } else if (instance != this) {
                Destroy(gameObject);
            }
        }
    }
}
