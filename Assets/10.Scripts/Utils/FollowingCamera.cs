using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour {
    private Transform myTransform;
    private Vector3 offset;
    private Transform target;

    public void Init() {
        myTransform = transform;
        target = null;
    }

    public void StartFollowing(Transform target) {
        this.target = target;
    }

    void LateUpdate() {
        if (target) {
            myTransform.position = target.position;
        }
    }
}
