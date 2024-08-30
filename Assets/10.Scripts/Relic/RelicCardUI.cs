using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DarkTonic.MasterAudio;
using DG.Tweening;

namespace HornSpirit {
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Button))]
    public class RelicCardUI : MonoBehaviour {
        [Header("UI")]
        [SerializeField] Image icon;
        [SerializeField] Image buttonIcon;
        [SerializeField] Image gradeFrame;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI descriptionText;

        [Header("Parameter")]
        [SerializeField] float startScale;
        [SerializeField] float endScale = 1.0f;
        [SerializeField] float selectScale = 1.2f;
        [SerializeField] float fadeTime = 0.5f;
        [SerializeField] float fadeoutTime = 0.15f;
        [SerializeField] string showSfx;
        [SerializeField] string selectSfx;

        RelicManager relicManager;
        GameController gameController;
        ItemManager itemManager;
        RelicMeta myMeta;
        RectTransform myTransform;
        CanvasGroup myCanvasGroup;
        Button myButton;

        UnityAction confirmAction;

        public void Init(RelicManager relicManager) {
            myTransform = GetComponent<RectTransform>();
            myCanvasGroup = GetComponent<CanvasGroup>();
            myButton = GetComponent<Button>();
            myButton.enabled = false;

            myCanvasGroup.alpha = 0;
            myCanvasGroup.blocksRaycasts = false;

            this.relicManager = relicManager;
            itemManager = GameManager.Instance.ItemManager;
            gameController = GameManager.Instance.GameController;
        }

        public void UpdateUI(RelicMeta meta) {
            myMeta = meta;

            icon.sprite = IconManager.Instance.GetRelic(meta.ID);
            gradeFrame.sprite = IconManager.Instance.GetRelicFrame(meta.Grade.ToString());
            titleText.text = meta.Name;
            descriptionText.text = meta.Description;
        }

        public void Show(UnityAction action = null) {
            confirmAction = action;

            DOTween.Sequence()
                .OnStart(() => {
                    gameController.Disable();

                    itemManager.RegisterItemAction(OnSelect);
                    MasterAudio.PlaySound(showSfx);
                    myTransform.localScale = Vector3.one * startScale;
                })
                .Append(myCanvasGroup.DOFade(1, fadeTime))
                .Join(myTransform.DOScale(endScale, fadeTime).SetEase(Ease.OutBounce))
                .AppendInterval(0.5f)
                .AppendCallback(() => {
                    ShowButtonIcon();
                    myButton.enabled = true;
                    myCanvasGroup.blocksRaycasts = true;
                    gameController.EnableAction(GameController.Type.RelicCardUI);
                }).SetUpdate(true);
        }

        public void Hide() {
            myCanvasGroup.blocksRaycasts = false;
            DOTween.Sequence()
                .Append(myTransform.DOScale(selectScale, fadeTime).SetEase(Ease.OutBounce))
                .Append(myCanvasGroup.DOFade(0, fadeoutTime))
                .OnComplete(() => {
                    HideButtonIcon();
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
            itemManager.UnregisterItemAction();
            MasterAudio.PlaySound(selectSfx);

            relicManager.AddRelic(myMeta.ID);
            confirmAction?.Invoke();

            gameController.EnableAction(GameController.Type.Default);

            Hide();
        }
    }
}
