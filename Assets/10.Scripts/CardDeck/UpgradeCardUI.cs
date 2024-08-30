using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DarkTonic.MasterAudio;
using DG.Tweening;

namespace HornSpirit {
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Button))]
    public class UpgradeCardUI : MonoBehaviour {
        [Header("UI")]
        [SerializeField] Image icon;
        [SerializeField] Image buttonIcon;
        [SerializeField] Image selectEffect;
        [SerializeField] TextMeshProUGUI titleText;

        [Header("Parameter")]
        [SerializeField] float startScale;
        [SerializeField] float endScale = 1.0f;
        [SerializeField] float selectScale = 1.2f;
        [SerializeField] float fadeTime = 0.5f;
        [SerializeField] float fadeoutTime = 0.15f;
        [SerializeField] float selectEffectFadeTime = 0.5f;
        [SerializeField] string showSfx;
        [SerializeField] string selectSfx;

        CardDeck cardDeck;
        UpgradeCardMeta myMeta;
        public UpgradeCardMeta MyMeta { get { return myMeta; } }
        RectTransform myTransform;
        CanvasGroup myCanvasGroup;
        Button myButton;
        int index;
        bool selected;

        public void Init(int index, CardDeck cardDeck) {
            myTransform = GetComponent<RectTransform>();
            myCanvasGroup = GetComponent<CanvasGroup>();
            myButton = GetComponent<Button>();
            myButton.enabled = false;

            myCanvasGroup.alpha = 0;

            this.index = index;
            this.cardDeck = cardDeck;
            HideButtonIcon();
        }

        public void UpdateUI(UpgradeCardMeta meta) {
            myMeta = meta;

            titleText.text = meta.Title;
        }

        public void Show() {
            DOTween.Sequence()
                .OnStart(() => {
                    selected = false;
                    MasterAudio.PlaySound(showSfx);
                    myTransform.localScale = Vector3.one * startScale;
                })
                .Append(myCanvasGroup.DOFade(1, fadeTime))
                .Join(myTransform.DOScale(endScale, fadeTime).SetEase(Ease.OutBounce))
                .AppendCallback(() => {
                    myButton.enabled = true;
                }).SetUpdate(true);
        }

        public void Hide() {
            HideButtonIcon();

            if (selected) {
                SelectedHide();
            } else {
                NormalHide();
            }
        }

        void NormalHide() {
            DOTween.Sequence()
                .Append(myTransform.DOScale(startScale, fadeoutTime).SetEase(Ease.OutBounce))
                .Append(myCanvasGroup.DOFade(0, fadeoutTime)).SetUpdate(true);
        }

        void SelectedHide() {
            DOTween.Sequence()
                .OnStart(() => {
                    cardDeck.DisableDeck();
                    selectEffect.gameObject.SetActive(true);
                    selectEffect.color = Color.white;
                })
                .Append(myTransform.DOScale(selectScale, fadeTime).SetEase(Ease.OutBounce))
                .Join(selectEffect.DOFade(0, selectEffectFadeTime))
                .Append(myCanvasGroup.DOFade(0, fadeoutTime))
                .AppendCallback(() => {
                    cardDeck.HideDeck();
                    selectEffect.gameObject.SetActive(false);
                }).SetUpdate(true);
        }

        public void ShowButtonIcon() {
            buttonIcon.gameObject.SetActive(true);
            buttonIcon.DOFade(1, 0.3f);
        }

        public void HideButtonIcon() {
            buttonIcon.gameObject.SetActive(false);
            buttonIcon.DOFade(0, 0);
        }

        public void OnSelect() {
            MasterAudio.PlaySound(selectSfx);

            selected = true;
            cardDeck.SelectCard(this);
        }
    }
}