using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;
// using I2.Loc;

namespace HornSpirit {
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueManager : MonoBehaviour {
        [Header("UI")]
        [SerializeField] Image leftPortrait;
        [SerializeField] Image rightPortrait;
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI dialogueText;
        [SerializeField] GameObject actionButton;
        [SerializeField] float fadeTime = 0.3f;
        [SerializeField] float textSpeed = 0.025f;

        RectTransform myTransform;
        CanvasGroup myCanvasGroup;
        Queue<string> sentences;
        Queue<Dialogue> dialogues;

        UnityAction callbackAction;
        GameManager gameManager;
        GameController gameController;

        public void Init() {
            myTransform = GetComponent<RectTransform>();
            myCanvasGroup = GetComponent<CanvasGroup>();
            gameManager = GameManager.Instance;
            gameController = gameManager.GameController;

            sentences = new Queue<string>();
            dialogues = new Queue<Dialogue>();
            actionButton.SetActive(false);

            Reset();
        }

        public void StartDialogue(Dialogue dialogue) {
            UpdatePortrait(dialogue);
            nameText.text = dialogue.Name;

            foreach (string sentence in dialogue.Sentences) {
                sentences.Enqueue(sentence);
            }

            DisplayNextSentence();
        }

        public void StartDialogueForLoc(Dialogue dialogue) {
            UpdatePortrait(dialogue);
            // nameText.text = LocalizationManager.GetTranslation(dialogue.Name);
            nameText.text = dialogue.Name;

            foreach (string sentence in dialogue.Sentences) {
                // string locSentence = LocalizationManager.GetTranslation(sentence);
                // sentences.Enqueue(locSentence);
                sentences.Enqueue(sentence);

            }

            DisplayNextSentence(true);
        }

        public void DisplayNextSentence(bool first = false) {
            if (sentences.Count == 0) {
                EndDialogue();
                return;
            }

            string sentence = sentences.Dequeue();
            StopAllCoroutines();
            StartCoroutine(TypeSentence(sentence, first));
        }

        void EndDialogue() {
            if (dialogues.Count == 0) {
                callbackAction?.Invoke();
                Hide();

                return;
            }

            StartDialogueForLoc(dialogues.Dequeue());
        }

        IEnumerator TypeSentence(string sentence, bool first) {
            actionButton.SetActive(false);

            dialogueText.text = "";
            foreach (char letter in sentence.ToCharArray()) {
                dialogueText.text += letter;
                yield return new WaitForSeconds(textSpeed);
            }

            actionButton.SetActive(true);
            if (first) {
                gameController.EnableAction(GameController.Type.Dialogue);
            }
        }

        public void StartConversation(List<Dialogue> dialogueList, UnityAction callbackAction = null) {
            gameManager.EnableMainUI(false);

            this.callbackAction = callbackAction;
            nameText.text = "";
            dialogueText.text = "";

            foreach (Dialogue dialogue in dialogueList) {
                dialogues.Enqueue(dialogue);
            }

            Show();
        }

        public void Show() {
            DOTween.Sequence().OnStart(() => {
                myCanvasGroup.blocksRaycasts = true;
            })
                .Append(myCanvasGroup.DOFade(1, fadeTime).SetEase(Ease.InOutQuad))
                .AppendInterval(0.5f)
                .OnComplete(() => {
                    StartDialogueForLoc(dialogues.Dequeue());
                }).SetUpdate(true);
        }

        void Hide() {
            myCanvasGroup.blocksRaycasts = false;
            myCanvasGroup.DOFade(0, fadeTime).OnComplete(() => {
                gameManager.EnableMainUI(true);
                gameController.EnableAction(GameController.Type.Default);
            }).SetEase(Ease.InOutQuad).SetUpdate(true);

            Reset();
        }

        void Reset() {
            leftPortrait.color = Color.white;
            rightPortrait.color = Color.white;
            callbackAction = null;

            nameText.text = "";
            dialogueText.text = "";
            dialogues.Clear();
            sentences.Clear();
        }

        void UpdatePortrait(Dialogue dialogue) {
            if (dialogue.Side == Constants.DialogueSide.Left) {
                leftPortrait.sprite = IconManager.Instance.GetPortrait(dialogue.PortraitName);
                leftPortrait.color = Color.white;
                rightPortrait.color = Color.gray;
            } else {
                rightPortrait.sprite = IconManager.Instance.GetPortrait(dialogue.PortraitName);
                rightPortrait.color = Color.white;
                leftPortrait.color = Color.gray;
            }
        }
    }
}
