using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    [System.Serializable]
    public class Dialogue {
        public string PortraitName;
        public Constants.DialogueSide Side;
        public string Name;
        [TextArea(3, 10)]
        public string[] Sentences;
    }
}
