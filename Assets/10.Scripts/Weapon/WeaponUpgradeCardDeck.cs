using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    [RequireComponent(typeof(CanvasGroup))]
    public class WeaponUpgradeCardDeck : MonoBehaviour {
        const int MaxCardCount = 3;

        [Header("Parameter")]
        [SerializeField] WeaponUpgradeCardUI[] cards;
        [SerializeField] float openInterval = 0.15f;

        int cardCount = 0;

        GameController gameController;
        CanvasGroup myCanvasGroup;
        WeaponManager weaponManager;

        public void Init() {
            myCanvasGroup = GetComponent<CanvasGroup>();
            myCanvasGroup.alpha = 0;
            myCanvasGroup.blocksRaycasts = false;

            weaponManager = GameManager.Instance.WeaponManager;
            gameController = GameManager.Instance.GameController;

            for (int i = 0; i < cards.Length; i++) {
                cards[i].Init(i, this);
            }
        }

        public void ShowDeck(List<WeaponUpgrade> upgrades) {
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

        void UpdateCards(List<WeaponUpgrade> upgrades) {
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

            gameController.EnableAction(GameController.Type.CardDeck);
        }

        public void HideDeck() {
            myCanvasGroup.alpha = 0;
            myCanvasGroup.blocksRaycasts = false;
            Time.timeScale = 1;
        }

        public void DisableDeck() {
            myCanvasGroup.blocksRaycasts = false;
        }

        public void SelectCard(WeaponUpgradeCardUI selectedCard) {
            gameController.EnableAction(GameController.Type.Default);
            weaponManager.UseWeaponUpgrade(selectedCard.Index);

            foreach (WeaponUpgradeCardUI card in cards) {
                card.Hide();
            }
        }

        public void SelectCard(int index) {
            gameController.EnableAction(GameController.Type.Default);
            cards[index].OnSelect();
            // weaponManager.UseWeaponUpgrade(index);
        }
    }
}
