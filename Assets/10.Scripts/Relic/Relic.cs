using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class Relic {
        public enum Type {
            Passive,
            Active
        }

        RelicMeta myMeta;
        int quantity;
        public RelicMeta Meta { get => myMeta; }
        public int Quantity { get => quantity; }

        public void Init(RelicMeta meta) {
            myMeta = meta;
            quantity = 0;
        }

        public void AddQuantity(int value) {
            quantity += value;
        }
    }
}
