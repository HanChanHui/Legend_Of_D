using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarkTonic.MasterAudio;
using DG.Tweening;
using TMPro;

namespace HornSpirit {
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Button))]

    public class HeroUpgradeUI : MonoBehaviour {
        [Header("UI")]
        [SerializeField] Image icon;
        [SerializeField] Image buttonIcon;
        [SerializeField] Image selectEffect;
        [SerializeField] Image gradeFrame;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI descriptionText;

        [Header("Parameter")]
        [SerializeField] float startScale;
        [SerializeField] float endScale = 1.0f;
        [SerializeField] float selectScale = 1.2f;
        [SerializeField] float fadeTime = 0.5f;
        [SerializeField] float fadeoutTime = 0.15f;
        [SerializeField] float selectEffectFadeTime = 0.5f;
        [SerializeField] string showSfx;
        [SerializeField] string selectSfx;

        HeroUpgradeCardDeck myDeck;
        HeroUpgrade heroUpgrade;
        RectTransform myTransform;
        CanvasGroup myCanvasGroup;
        Button myButton;
        int index;
        public int Index { get => index; }
        public HeroUpgrade HeroUpgrade { get => heroUpgrade; }
        bool selected;

        public void Init(int index, HeroUpgradeCardDeck deck) {
            myTransform = GetComponent<RectTransform>();
            myCanvasGroup = GetComponent<CanvasGroup>();
            myButton = GetComponent<Button>();
            myButton.enabled = false;

            myCanvasGroup.alpha = 0;

            this.index = index;
            this.myDeck = deck;
            HideButtonIcon();
        }

        public void UpdateUI(HeroUpgrade heroUpgrade) {
            this.heroUpgrade = heroUpgrade;

            icon.sprite = IconManager.Instance.GetHeroUpgrade(heroUpgrade.ID);
            gradeFrame.sprite = IconManager.Instance.GetHeroUpgradeFrame(heroUpgrade.Grade.ToString());
            titleText.text = heroUpgrade.Name;
            descriptionText.text = heroUpgrade.Description;
        }

        public void Show() {
            DOTween.Sequence()
                .OnStart(() => {
                    selected = false;
                    MasterAudio.PlaySound(showSfx);
                    myTransform.localScale = Vector3.one * startScale;
                })
                .Append(myCanvasGroup.DOFade(1, fadeTime))
                .Join(myTransform.DOScale(endScale, fadeTime))
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
                .Append(myTransform.DOScale(startScale, fadeoutTime))
                .Append(myCanvasGroup.DOFade(0, fadeoutTime)).SetUpdate(true);
        }

        void SelectedHide() {
            DOTween.Sequence()
                .OnStart(() => {
                    myDeck.DisableDeck();
                    selectEffect.gameObject.SetActive(true);
                    selectEffect.color = Color.white;
                })
                .Append(myTransform.DOScale(selectScale, fadeTime))
                .Join(selectEffect.DOFade(0, selectEffectFadeTime))
                .Append(myCanvasGroup.DOFade(0, fadeoutTime))
                .AppendCallback(() => {
                    myDeck.HideDeck();
                    selectEffect.gameObject.SetActive(false);
                }).SetUpdate(true);
        }

        public void ShowButtonIcon() {
            buttonIcon.gameObject.SetActive(true);
            buttonIcon.DOFade(1, 0.3f).SetUpdate(true);
        }

        public void HideButtonIcon() {
            buttonIcon.gameObject.SetActive(false);
            buttonIcon.DOFade(0, 0).SetUpdate(true);
        }

        public void OnSelect() {
            MasterAudio.PlaySound(selectSfx);

            selected = true;
            myDeck.SelectCard(this);
        }
    }
}
