using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DarkTonic.MasterAudio;
using TMPro;

namespace HornSpirit {
    public class ReloadComponent : MonoBehaviour {
        [Header("Basic")]
        [SerializeField] Image circle;
        [SerializeField] Image fill;
        [SerializeField] Vector3 offset;
        [SerializeField] string startSfx;
        [SerializeField] string endSfx;

        [Header("Ammo")]
        [SerializeField] Image ammoFill;
        [SerializeField] TextMeshProUGUI ammoCount;

        RectTransform rectTransform;
        Vector3 defaultCircleColor;
        Transform target;

        // ammo
        int maxAmmo;

        public void Init() {
            rectTransform = GetComponent<RectTransform>();
            Color.RGBToHSV(circle.color,
                out defaultCircleColor.x, out defaultCircleColor.y, out defaultCircleColor.z);

            target = GameManager.Instance.Hero.transform;
            gameObject.SetActive(false);
        }

        public void Reset() {
            fill.fillAmount = 0;
            circle.color = Color.HSVToRGB(defaultCircleColor.x, defaultCircleColor.y, 0);
            rectTransform.localScale = Vector3.one;

            gameObject.SetActive(true);
        }

        public void SetAmmo(int maxAmmo, int currAmmo) {
            this.maxAmmo = maxAmmo;
            UpdateAmmoCount(currAmmo);
        }

        public void UpdateAmmoCount(int currAmmo) {
            // ammoFill.fillAmount = (float)currAmmo / maxAmmo / 3;
            ammoCount.text = currAmmo.ToString();
        }

        public void Show(float time) {
            Reset();
            StartCoroutine(CoReload(time));
            StartCoroutine(CoFollowTarget());
        }

        void Hide() {
            MasterAudio.PlaySound(endSfx);
            rectTransform.DOScale(0, 0.3f).SetEase(Ease.InBounce);
        }

        IEnumerator CoReload(float time) {
            float percent = 0;
            float rate = 1 / time;

            MasterAudio.PlaySound(startSfx);

            while (percent < 1) {
                percent += rate * Time.deltaTime;
                fill.fillAmount = percent;
                circle.color = Color.HSVToRGB(defaultCircleColor.x, defaultCircleColor.y, percent);

                yield return null;
            }

            Hide();
        }

        IEnumerator CoFollowTarget() {
            while (true) {
                rectTransform.position = Camera.main.WorldToScreenPoint(target.position + offset);
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
