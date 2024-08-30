using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public static class Constants {
        [System.Flags]
        public enum CardinalPoints {
            North = 0x00000001,
            East = 0x00000002,
            South = 0x00000004,
            West = 0x00000008
        }

        public enum Grade {
            Common,
            Rare,
            Epic,
            Legendary
        }

        public enum DialogueSide {
            Left,
            Right
        }

        public enum EnemyType {
            Normal,
            Elite,
            Boss
        }

        public static void ShuffleArray<T>(T[] array) {
            int index1, index2;
            T temp;

            for (int i = 0; i < array.Length - 1; i++) {
                index1 = UnityEngine.Random.Range(0, array.Length);
                index2 = UnityEngine.Random.Range(0, array.Length);

                temp = array[index1];
                array[index1] = array[index2];
                array[index2] = temp;
            }
        }

        public static void ShuffleList<T>(List<T> list) {
            int index1, index2;
            T temp;

            for (int i = 0; i < list.Count - 1; i++) {
                index1 = UnityEngine.Random.Range(0, list.Count);
                index2 = UnityEngine.Random.Range(0, list.Count);

                temp = list[index1];
                list[index1] = list[index2];
                list[index2] = temp;
            }
        }

        public const string HeroStatFile = "JSON/HeroStat";
    }
}
