using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ControlFreak2;
using DarkTonic.MasterAudio;

namespace HornSpirit {
    public class GameController : MonoBehaviour {
        public enum Type {
            None,
            Default,
            CardDeck,
            Chest,
            Dialogue,
            HeroUpgrade,
            NotificationPanel,
            RelicCardUI
        }

        [SerializeField] WeaponUpgradeCardDeck weaponUpgradeDeck;
        [SerializeField] HeroUpgradeCardDeck heroUpgardeDeck;
        [SerializeField] RelicCardUI relicCardUI;
        Hero hero;
        ItemManager itemManager;
        DialogueManager dialogueManager;
        NotificationPanel notificationPanel;
        Dictionary<Type, ButtonAction> dicButtonAction = new();
        delegate void ButtonAction(bool enable);
        Type currentAction = Type.None;

        public void Init() {
            hero = GameManager.Instance.Hero;
            itemManager = GameManager.Instance.ItemManager;
            dialogueManager = GameManager.Instance.DialogueManager;
            notificationPanel = GameManager.Instance.NotificationPanel;

            InitDictionary();
        }

        void InitDictionary() {
            dicButtonAction.Add(Type.Default, DefaultAction);
            dicButtonAction.Add(Type.CardDeck, CardDeckAction);
            dicButtonAction.Add(Type.Chest, ChestAction);
            dicButtonAction.Add(Type.Dialogue, DialogueAction);
            dicButtonAction.Add(Type.HeroUpgrade, HeroUpgradeAction);
            dicButtonAction.Add(Type.NotificationPanel, NotificationAction);
            dicButtonAction.Add(Type.RelicCardUI, RelicCardAction);
        }

        public void Disable() {
            if (currentAction != Type.None) {
                dicButtonAction[currentAction](false);
                currentAction = Type.None;
            }
        }

        public void EnableAction(Type action) {
            if (currentAction != Type.None) {
                dicButtonAction[currentAction](false);

                if (currentAction == Type.Default) {
                    hero.AttackRelease();
                }
            }

            dicButtonAction[action](true);
            currentAction = action;
        }

        void DefaultAction(bool enable) {
            if (enable) {
                StartCoroutine(nameof(CoDefaultButtons));
            } else {
                StopCoroutine(nameof(CoDefaultButtons));
            }
        }

        void CardDeckAction(bool enable) {
            if (enable) {
                StartCoroutine(nameof(CoCardDeckButtons));
            } else {
                StopCoroutine(nameof(CoCardDeckButtons));
            }
        }

        void ChestAction(bool enable) {
            if (enable) {
                StartCoroutine(nameof(CoChestOpenButton));
            } else {
                StopCoroutine(nameof(CoChestOpenButton));
            }
        }

        void DialogueAction(bool enable) {
            if (enable) {
                StartCoroutine(nameof(CoDialogueButtons));
            } else {
                StopCoroutine(nameof(CoDialogueButtons));
            }
        }

        void HeroUpgradeAction(bool enable) {
            if (enable) {
                StartCoroutine(nameof(CoHeroUpgradeButtons));
            } else {
                StopCoroutine(nameof(CoHeroUpgradeButtons));
            }
        }

        void NotificationAction(bool enable) {
            if (enable) {
                StartCoroutine(nameof(CoNotificationPanelButtons));
            } else {
                StopCoroutine(nameof(CoNotificationPanelButtons));
            }
        }

        void RelicCardAction(bool enable) {
            if (enable) {
                StartCoroutine(nameof(CoRelicCardUIButtons));
            } else {
                StopCoroutine(nameof(CoRelicCardUIButtons));
            }
        }

        IEnumerator CoDefaultButtons() {
            while (true) {
                if (CF2Input.GetButton("Fire1")) {
                    hero.Attack();
                }

                if (CF2Input.GetButtonDown("Fire1")) {
                    hero.AttackPress();
                }

                if (CF2Input.GetButtonUp("Fire1")) {
                    hero.AttackRelease();
                }

                yield return null;
            }
        }

        IEnumerator CoCardDeckButtons() {
            while (true) {
                if (CF2Input.GetButtonDown("LeftFace")) {
                    weaponUpgradeDeck.SelectCard(0);
                }

                if (CF2Input.GetButtonDown("BottomFace")) {
                    weaponUpgradeDeck.SelectCard(1);
                }

                if (CF2Input.GetButtonDown("RightFace")) {
                    weaponUpgradeDeck.SelectCard(2);
                }

                yield return null;
            }
        }

        IEnumerator CoHeroUpgradeButtons() {
            while (true) {
                if (CF2Input.GetButtonDown("TopFace")) {
                    heroUpgardeDeck.SelectCard(0);
                }

                if (CF2Input.GetButtonDown("RightFace")) {
                    heroUpgardeDeck.SelectCard(1);
                }

                if (CF2Input.GetButtonDown("BottomFace")) {
                    heroUpgardeDeck.SelectCard(2);
                }

                yield return null;
            }
        }

        IEnumerator CoChestOpenButton() {
            while (true) {
                if (CF2Input.GetButtonDown("BottomFace")) {
                    itemManager.DoItemAction();
                }

                yield return null;
            }
        }

        IEnumerator CoDialogueButtons() {
            while (true) {
                if (CF2Input.GetButtonDown("BottomFace")) {
                    dialogueManager.DisplayNextSentence();
                    MasterAudio.PlaySound("Click 13");
                }

                yield return null;
            }
        }

        IEnumerator CoNotificationPanelButtons() {
            while (true) {
                if (CF2Input.GetButtonDown("BottomFace")) {
                    notificationPanel.OnConfirmButton();
                }

                yield return null;
            }
        }

        IEnumerator CoRelicCardUIButtons() {
            while (true) {
                if (CF2Input.GetButtonDown("BottomFace")) {
                    relicCardUI.OnSelect();
                }

                yield return null;
            }
        }
    }
}
