using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class HeroUpgradeManager : MonoBehaviour {
        [Header("UI")]
        [SerializeField] HeroUpgradeCardDeck myDeck;

        delegate void UpgradeAction(HeroUpgrade upgrade);
        Dictionary<string, UpgradeAction> dicUpgradeActions = new();
        Dictionary<string, HeroUpgrade> dicUpgrades = new();
        List<HeroUpgrade> upgrades;
        List<string> upgradeIDs = new();

        Hero hero;
        StatManager statManager;

        public void Init() {
            hero = GameManager.Instance.Hero;
            statManager = GameManager.Instance.StatManager;

            LoadData();
            InitActions();
        }

        void LoadData() {
            string path = "JSON/HeroUpgrade";
            TextAsset textAsset = Resources.Load<TextAsset>(path);

            if (textAsset) {
                upgrades = JsonHelper.FromJsonList<HeroUpgrade>(textAsset.text);

                for (int i = 0; i < upgrades.Count; i++) {
                    string key = $"{upgrades[i].ID}-{upgrades[i].Grade}";
                    if (!dicUpgrades.ContainsKey(key)) {
                        dicUpgrades.Add(key, upgrades[i]);
                    }
                }
            }
        }

        void InitActions() {
            dicUpgradeActions.Add("HeroUpgrade_01", Action_ShootingBoost);
            dicUpgradeActions.Add("HeroUpgrade_02", Action_AgileFootwork);
            dicUpgradeActions.Add("HeroUpgrade_03", Action_RapidFire);
            dicUpgradeActions.Add("HeroUpgrade_04", Action_PrecisionAim);
            dicUpgradeActions.Add("HeroUpgrade_05", Action_DeadlyStrike);
            dicUpgradeActions.Add("HeroUpgrade_06", Action_Agility);
            dicUpgradeActions.Add("HeroUpgrade_07", Action_IronHeart);
            // dicUpgradeActions.Add("HeroUpgrade_08", Action_PerfectBalance);

            upgradeIDs.AddRange(dicUpgradeActions.Keys);
        }

        public void ShowDeck() {
            ShuffleUpgrades();

            List<HeroUpgrade> upgrades = new();

            for (int i = 0; i < 3; i++) {
                string id = upgradeIDs[i];
                // temp set grade
                int grade = 0;
                if (Random.Range(0, 100) < 10) {    // normal: 90%, rare: 10%
                    grade = 1;
                }

                string key = $"{id}-{(Constants.Grade)grade}";
                upgrades.Add(dicUpgrades[key]);
            }

            myDeck.ShowDeck(upgrades);
        }

        public void UseUpgrade(HeroUpgrade heroUpgrade) {
            string id = heroUpgrade.ID;

            if (dicUpgradeActions.ContainsKey(id)) {
                dicUpgradeActions[id](heroUpgrade);
            }
        }

        public void ShuffleUpgrades() {
            Constants.ShuffleList(upgradeIDs);
        }

        void Action_ShootingBoost(HeroUpgrade upgrade) {
            statManager.AddPowerByRate(upgrade.Values[0]);
        }

        void Action_AgileFootwork(HeroUpgrade upgrade) {
            statManager.AddHeroSpeedByRate(upgrade.Values[0]);
        }

        void Action_RapidFire(HeroUpgrade upgrade) {
            hero.AddRPMbyRate(upgrade.Values[0]);
        }

        void Action_PrecisionAim(HeroUpgrade upgrade) {
            statManager.AddCriticalRate(upgrade.Values[0]);
        }

        void Action_DeadlyStrike(HeroUpgrade upgrade) {
            statManager.AddCriticalDamageByRate(upgrade.Values[0]);
        }

        void Action_Agility(HeroUpgrade upgrade) {
            statManager.AddEvasionRate(upgrade.Values[0]);
        }

        void Action_IronHeart(HeroUpgrade upgrade) {
            statManager.AddMaxHealth(upgrade.Values[0]);
        }

        void Action_PerfectBalance(HeroUpgrade upgrade) {
            Debug.Log("Action_PerfectBalance");
        }
    }
}
