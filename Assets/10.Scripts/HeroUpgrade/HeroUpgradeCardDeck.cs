using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class HeroUpgradeCardDeck : MonoBehaviour {
        const int MaxCardCount = 3;

        [Header("Parameter")]
        [SerializeField] HeroUpgradeUI[] cards;
        [SerializeField] float openInterval = 0.15f;

        int cardCount = 0;

        GameController gameController;
        CanvasGroup myCanvasGroup;
        HeroUpgradeManager heroUpgradeManager;

        public void Init() {
            myCanvasGroup = GetComponent<CanvasGroup>();
            myCanvasGroup.alpha = 0;
            myCanvasGroup.blocksRaycasts = false;

            heroUpgradeManager = GameManager.Instance.HeroUpgradeManager;
            gameController = GameManager.Instance.GameController;

            for (int i = 0; i < cards.Length; i++) {
                cards[i].Init(i, this);
            }
        }

        public void ShowDeck(List<HeroUpgrade> upgrades) {
            if (upgrades.Count == 0) {
                return;
            }

            gameController.Disable();

            Time.timeScale = 0;
            myCanvasGroup.alpha = 1;
            myCanvasGroup.blocksRaycasts = true;

            cardCount = Mathf.Min(upgrades.Count, MaxCardCount);
            UpdateCards(upgrades);
            StartCoroutine(CoShowCards());
        }

        void UpdateCards(List<HeroUpgrade> upgrades) {
            for (int i = 0; i < cardCount; i++) {
                cards[i].UpdateUI(upgrades[i]);
            }
        }

        IEnumerator CoShowCards() {
            for (int i = 0; i < cardCount; i++) {
                cards[i].Show();
                yield return new WaitForSecondsRealtime(openInterval);
            }

            yield return new WaitForSecondsRealtime(0.5f);

            for (int i = 0; i < cardCount; i++) {
                cards[i].ShowButtonIcon();
            }

            gameController.EnableAction(GameController.Type.HeroUpgrade);
        }

        public void HideDeck() {
            myCanvasGroup.alpha = 0;
            myCanvasGroup.blocksRaycasts = false;
            Time.timeScale = 1;
        }

        public void DisableDeck() {
            myCanvasGroup.blocksRaycasts = false;
        }

        public void SelectCard(HeroUpgradeUI selectedCard) {
            myCanvasGroup.blocksRaycasts = false;
            gameController.EnableAction(GameController.Type.Default);

            heroUpgradeManager.UseUpgrade(selectedCard.HeroUpgrade);

            foreach (HeroUpgradeUI card in cards) {
                card.Hide();
            }
        }

        public void SelectCard(int index) {
            myCanvasGroup.blocksRaycasts = false;
            gameController.EnableAction(GameController.Type.Default);

            cards[index].OnSelect();
            // heroUpgradeManager.UseUpgrade(index);
        }
    }
}
