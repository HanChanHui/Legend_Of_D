using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    [System.Serializable]
    public class HeroUpgrade {
        public string Name;
        public string ID;
        public string Description;
        public Constants.Grade Grade;
        public int[] Values;
    }
}
