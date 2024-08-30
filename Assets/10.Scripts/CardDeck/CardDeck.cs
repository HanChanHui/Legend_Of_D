using System.Collections;
using UnityEngine;

namespace HornSpirit {
    [RequireComponent(typeof(CanvasGroup))]
    public class CardDeck : MonoBehaviour {
        [Header("Parameter")]
        [SerializeField] UpgradeCardUI[] cards;
        [SerializeField] float openInterval = 0.15f;

        GameController gameController;
        CanvasGroup myCanvasGroup;
        int cardCount;

        public void Init() {
            myCanvasGroup = GetComponent<CanvasGroup>();
            myCanvasGroup.alpha = 0;
            myCanvasGroup.blocksRaycasts = false;

            gameController = GameManager.Instance.GameController;

            for (int i = 0; i < cards.Length; i++) {
                cards[i].Init(i, this);
            }
        }

        public void ShowDeck(UpgradeCardMeta[] metas) {
            myCanvasGroup.alpha = 1;
            myCanvasGroup.blocksRaycasts = true;

            UpdateCards(metas);
            StartCoroutine(CoShowCards());
        }

        void UpdateCards(UpgradeCardMeta[] metas) {
            cardCount = metas.Length;

            for (int i = 0; i < cardCount; i++) {
                cards[i].UpdateUI(metas[i]);
            }
        }

        IEnumerator CoShowCards() {
            for (int i = 0; i < cardCount; i++) {
                cards[i].Show();
                yield return new WaitForSeconds(openInterval);
            }

            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < cardCount; i++) {
                cards[i].ShowButtonIcon();
            }

            gameController.EnableAction(GameController.Type.CardDeck);
        }

        public void HideDeck() {
            myCanvasGroup.alpha = 0;
            myCanvasGroup.blocksRaycasts = false;
        }

        public void DisableDeck() {
            myCanvasGroup.blocksRaycasts = false;
        }

        public void SelectCard(UpgradeCardUI selectedCard) {
            gameController.EnableAction(GameController.Type.Default);
            // apply selected card

            foreach (UpgradeCardUI card in cards) {
                card.Hide();
            }
        }

        public void SelectCard(int index) {
            gameController.EnableAction(GameController.Type.Default);
            cards[index].OnSelect();
        }
    }
}
