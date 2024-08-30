using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace HornSpirit {
    public class EnemyLabel : MonoBehaviour, IMemoryPool {
        [Header("Basic")]
        [SerializeField] string mpType;
        [SerializeField] string mpName;
        [SerializeField] TextMeshProUGUI levelText;

        Vector3 offset;
        RectTransform rectTransform;
        Transform target;
        Slider hpSlider;

        public void MPStart() {
            rectTransform = GetComponent<RectTransform>();
            hpSlider = GetComponentInChildren<Slider>();

            Transform parent = GameObject.Find("EnemyLabels").transform;
            rectTransform.SetParent(parent, false);
        }

        public void Create(Transform target, Vector3 offset, int maxHp) {
            this.target = target;
            this.offset = offset;

            InitLabel(maxHp, maxHp);
            StartCoroutine(nameof(CoFollowTarget));
        }

        public void InitLabel(int hp, int maxHp) {
            hpSlider.maxValue = maxHp;
            hpSlider.value = hp;
        }

        public void UpdateLabel(int hp) {
            hpSlider.value = hp;
        }

        IEnumerator CoFollowTarget() {
            while (true) {
                rectTransform.position = Camera.main.WorldToScreenPoint(target.position + offset);

                yield return null;
            }
        }

        public void MyDestroy() {
            PoolManager.Instance.RemoveItem(mpType, mpName, gameObject);
        }
    }
}
