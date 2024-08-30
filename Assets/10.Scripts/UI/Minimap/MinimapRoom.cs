using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class MinimapRoom : MonoBehaviour, IMemoryPool {
        [Header("UI")]
        [SerializeField] SpriteRenderer roomSprite;
        [SerializeField] GameObject playerMark;
        [SerializeField] Color defaultColor;
        [SerializeField] Color clearedColor;
        // [SerializeField] Color visitedColor;

        [Header("Parameters")]
        [SerializeField] string mpGroup;
        [SerializeField] string mpType;

        Transform myTransform;
        bool isVisited;

        public void MPStart() {
            myTransform = transform;
            playerMark.SetActive(false);
        }

        public void Show() {
            if (gameObject.activeSelf == false) {
                gameObject.SetActive(true);
            }
        }

        public void Hide() {
            if (gameObject.activeSelf) {
                gameObject.SetActive(false);
            }
        }

        public void EnablePlayerMark(bool enable) {
            playerMark.SetActive(enable);
        }

        public void SetPosition(Vector3 pos, Transform parent) {
            myTransform.SetParent(parent);
            myTransform.position = pos;
        }

        public void Visit() {
            isVisited = true;
            EnablePlayerMark(true);
        }

        public void Leave() {
            isVisited = false;
            EnablePlayerMark(false);
        }

        public void SetCleared() {
            roomSprite.color = clearedColor;
        }

        public void Reset() {
            isVisited = false;
            // roomSprite.color = defaultColor;
        }

        public void MPDestroy() {
            PoolManager.Instance.RemoveItem(mpGroup, mpType, gameObject);
        }
    }
}
