using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class Obstacle : MonoBehaviour {
        [Header("Parameter")]
        [SerializeField] GameObject fracturedObject;

        void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Hero")) {
                fracturedObject.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }
}
