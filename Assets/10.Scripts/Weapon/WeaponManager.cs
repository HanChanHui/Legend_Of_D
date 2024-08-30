using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HornSpirit {
    public class WeaponManager : MonoBehaviour {
        [Header("Weapon UI")]
        [SerializeField] ReloadComponent reloadComponent;
        [SerializeField] WeaponUpgradeCardDeck myDeck;

        [Header("Test Weapon")]
        [SerializeField] Weapon[] weapons;

        delegate void WeaponAction(WeaponUpgrade upgrade);
        Dictionary<string, WeaponAction> dicWeaponActions = new();
        Dictionary<string, WeaponUpgrade> dicWeaponUpgrades = new();
        List<WeaponUpgrade> weaponUpgrades;
        List<WeaponUpgrade> actualWeaponUpgrades;
        public List<WeaponUpgrade> ActualWeaponUpgrades { get => actualWeaponUpgrades; }

        Hero hero;
        bool vampireMode;
        int vampireValue;
        public bool VampireMode { get => vampireMode; }
        public int VampireValue { get => vampireValue; }
        bool extraBossDamage;
        float extraBossDamageValue;

        public bool ExtraBossDamage { get => extraBossDamage; }
        public float ExtraBossDamageValue { get => extraBossDamageValue; }

        Dictionary<string, Weapon> dicWeapon = new();
        Weapon equippedWeapon;
        public Weapon EquippedWeapon { get => equippedWeapon; }

        public void Init() {
            hero = GameManager.Instance.Hero;
            reloadComponent.Init();

            LoadData();
            InitActions();

            // equip a weapon for test
            foreach (Weapon weapon in weapons) {
                weapon.Init();
                dicWeapon.Add(weapon.WeaponName, weapon);
            }

            EquipWeapon("Handgun");
        }

        void LoadData() {
            string path = "JSON/WeaponUpgrade";
            TextAsset textAsset = Resources.Load<TextAsset>(path);

            if (textAsset) {
                weaponUpgrades = JsonHelper.FromJsonList<WeaponUpgrade>(textAsset.text);

                for (int i = 0; i < weaponUpgrades.Count; i++) {
                    dicWeaponUpgrades.Add(weaponUpgrades[i].ID, weaponUpgrades[i]);
                }

                actualWeaponUpgrades = new List<WeaponUpgrade>(weaponUpgrades); // for test
            }
        }

        void InitActions() {
            dicWeaponActions.Add("WeaponUpgrade_01", WeaponAction_MultiBullet);
            dicWeaponActions.Add("WeaponUpgrade_02", WeaponAction_Vampire);
            dicWeaponActions.Add("WeaponUpgrade_03", WeaponAction_Executioner);
            dicWeaponActions.Add("WeaponUpgrade_04", WeaponAction_ExtraMagazine);
            dicWeaponActions.Add("WeaponUpgrade_05", WeaponAction_FocusedFire);
        }

        public void ShowDeck() {
            ShuffleWeaponUpgrades();
            myDeck.ShowDeck(actualWeaponUpgrades);
        }

        public void UseWeaponUpgrade(string id) {
            if (dicWeaponActions.ContainsKey(id)) {
                dicWeaponActions[id](dicWeaponUpgrades[id]);
            }
        }

        public void EquipWeapon(string weaponName) {
            if (dicWeapon.ContainsKey(weaponName)) {
                equippedWeapon = dicWeapon[weaponName];
            } else {
                Debug.LogError("No weapon named " + weaponName);
            }
        }

        public void UseWeaponUpgrade(int index) {
            WeaponUpgrade upgrade = actualWeaponUpgrades[index];

            UseWeaponUpgrade(upgrade.ID);
            // actualWeaponUpgrades.RemoveAt(index);
        }

        public void ShuffleWeaponUpgrades() {
            Constants.ShuffleList(actualWeaponUpgrades);
        }

        public void Reload() {
            reloadComponent.Show(equippedWeapon.ReloadTime);
        }

        public void UpdateMagazine(int magazine) {
            reloadComponent.UpdateAmmoCount(magazine);
        }

        public void CheckVampireMode() {
            if (vampireMode) {
                hero.AddHealth(vampireValue);
            }
        }

        void WeaponAction_MultiBullet(WeaponUpgrade upgrade) {
            equippedWeapon.AddBullet();
            int power = Mathf.CeilToInt(equippedWeapon.Power * (float)upgrade.Values[1] / 100);
            equippedWeapon.SetPower(power);
        }

        void WeaponAction_Vampire(WeaponUpgrade upgrade) {
            vampireMode = true;
            vampireValue += upgrade.Values[0];
        }

        void WeaponAction_Executioner(WeaponUpgrade upgrade) {
            extraBossDamage = true;
            extraBossDamageValue = 1 + upgrade.Values[0] / 100;
        }

        void WeaponAction_ExtraMagazine(WeaponUpgrade upgrade) {
            int originMagazine = equippedWeapon.Magazine;
            int extraMagazine = Mathf.Max(1, originMagazine * upgrade.Values[0] / 100);

            equippedWeapon.SetMagazine(originMagazine + extraMagazine);
        }

        void WeaponAction_FocusedFire(WeaponUpgrade upgrade) {
            int originMagazine = equippedWeapon.Magazine;

            equippedWeapon.SetMagazine(Mathf.Max(originMagazine / 2, 1));
            int power = Mathf.CeilToInt(equippedWeapon.Power * (1 + (float)upgrade.Values[0] / 100));
            equippedWeapon.SetPower(power);
        }
    }
}
