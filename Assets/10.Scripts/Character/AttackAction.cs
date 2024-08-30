using UnityEngine;

namespace HornSpirit {
    [System.Serializable]
    public class AttackAction {
        public string Anim;
        public int StandardFrame;
        public int TotalFrame;
        public int ActionFrame;
        float preDelay;
        float postDelay;
        public float PreDelay { get => preDelay; }
        public float PostDelay { get => postDelay; }

        public void Init() {
            preDelay = (float)ActionFrame / StandardFrame;
            postDelay = (float)(TotalFrame - ActionFrame) / StandardFrame;
        }
    }
}
