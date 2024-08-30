using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CameraCtrl : MonoBehaviour {

    public float defaultVibrationDistance = 0.05f;
    public float defaultVibrationTime = 0.05f;

    private bool isShaking;
    private Transform myTransform;
    private Camera myCamera;
    private float originCameraSize;

    void Awake() {
        myTransform = transform;
        myCamera = GetComponent<Camera>();
        originCameraSize = myCamera.orthographicSize;
    }

    public void DeadVibration() {
        if (isShaking)
            return;

        StartCoroutine(CoShake(0.1f, 0.3f));
    }

    public void BombVibration() {
        if (isShaking) {
            return;
        }

        StartCoroutine(CoShake(0.1f, 0.3f));
    }

    public void Shake(float amount, float duration) {
        if (isShaking) {
            return;
        }

        StartCoroutine(CoShake(amount, duration));
    }

    private IEnumerator CoShake(float amount, float duration) {
        float timer = 0;
        Vector3 originPos = myTransform.localPosition;

        isShaking = true;

        while (timer <= duration) {
            myTransform.localPosition = (Vector3)Random.insideUnitCircle * amount + originPos;

            timer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        myTransform.localPosition = originPos;
        isShaking = false;
    }

    public void ZoomCamera(float ratio, float duration) {
        float target = originCameraSize * ratio;

        myCamera.DOKill();
        myCamera.DOOrthoSize(target, duration).SetEase(Ease.InCubic);
    }

    public void RestoreZoomCamera(float duration) {
        myCamera.DOKill();
        myCamera.DOOrthoSize(originCameraSize, duration).SetEase(Ease.OutCubic);
    }

    public void EnableOrthographic(bool enable) {
        myCamera.orthographic = enable;
    }
}
