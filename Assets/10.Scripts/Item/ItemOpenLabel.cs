using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class ItemOpenLabel : MonoBehaviour {
        Transform target;
        Vector3 offset;
        RectTransform myTransform;

        void Start() {
            myTransform = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        public void Show(Transform transform, Vector3 offset) {
            gameObject.SetActive(true);
            target = transform;
            this.offset = offset;

            StartCoroutine(nameof(CoFollowTarget));
        }

        public void Hide() {
            StopCoroutine(nameof(CoFollowTarget));
            gameObject.SetActive(false);
        }

        IEnumerator CoFollowTarget() {
            while (true) {
                myTransform.position = Camera.main.WorldToScreenPoint(target.position + offset);

                yield return null;
            }
        }
    }
}
