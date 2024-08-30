using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace HornSpirit {
    public class DeadPanel : MonoBehaviour {
        [Header("UI")]
        [SerializeField] GameObject confirmButton;
        [SerializeField] float startDelay = 2f;
        [SerializeField] float fadeTime = 0.5f;
        [SerializeField] float bgFadeTime = 5f;
        [SerializeField] float afterDelay = 1f;
        [SerializeField] CanvasGroup myCanvasGroup;
        [SerializeField] Image background;

        public void Init() {
            myCanvasGroup.alpha = 0;
            myCanvasGroup.blocksRaycasts = false;
        }

        public void Show() {
            gameObject.SetActive(true);

            DOTween.Sequence()
                .AppendInterval(startDelay)
                .Append(myCanvasGroup.DOFade(1f, fadeTime))
                .Join(background.DOFade(1f, bgFadeTime))
                .AppendInterval(afterDelay)
                .AppendCallback(() => {
                    GameManager.Instance.GoToTitle();
                });
        }
    }
}
