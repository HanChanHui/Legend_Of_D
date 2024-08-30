using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace HornSpirit {
    public class IconManager : MonoBehaviour {
        static IconManager instance = null;
        public static IconManager Instance { get { return instance; } }

        [Header("Icon Parent")]
        [SerializeField] Transform relicParent;
        [SerializeField] Transform relicFrameParent;
        [SerializeField] Transform portraitParent;
        [SerializeField] Transform weaponUpgradeParent;
        [SerializeField] Transform weaponUpgradeFrameParent;
        [SerializeField] Transform heroUpgradeParent;
        [SerializeField] Transform heroUpgradeFrameParent;

        Dictionary<string, Sprite> dicRelic = new();
        Dictionary<string, Sprite> dicRelicFrame = new();
        Dictionary<string, Sprite> dicPortrait = new();
        Dictionary<string, Sprite> dicWeaponUpgrade = new();
        Dictionary<string, Sprite> dicWeaponUpgradeFrame = new();
        Dictionary<string, Sprite> dicHeroUpgrade = new();
        Dictionary<string, Sprite> dicHeroUpgradeFrame = new();

        void Awake() {
            if (instance == null) {
                instance = this;
                Init();
                DontDestroyOnLoad(gameObject);
            } else if (instance != this) {
                Destroy(gameObject);
            }
        }

        void Init() {
            InitImages(dicRelic, relicParent);
            InitImages(dicRelicFrame, relicFrameParent);
            InitImages(dicPortrait, portraitParent);
            InitImages(dicWeaponUpgrade, weaponUpgradeParent);
            InitImages(dicWeaponUpgradeFrame, weaponUpgradeFrameParent);
            InitImages(dicHeroUpgrade, heroUpgradeParent);
            InitImages(dicHeroUpgradeFrame, heroUpgradeFrameParent);
        }

        void InitImages(Dictionary<string, Sprite> dic, Transform parent) {
            Image[] images = parent.GetComponentsInChildren<Image>(true);

            foreach (Image image in images) {
                dic.Add(image.name, image.sprite);
            }
        }

        public Sprite GetRelic(string name) {
            if (dicRelic.ContainsKey(name)) {
                return dicRelic[name];
            }

            return null;
        }

        public Sprite GetRelicFrame(string name) {
            if (dicRelicFrame.ContainsKey(name)) {
                return dicRelicFrame[name];
            }

            return null;
        }

        public Sprite GetPortrait(string name) {
            if (dicPortrait.ContainsKey(name)) {
                return dicPortrait[name];
            }

            return null;
        }

        public Sprite GetWeaponUpgrade(string name) {
            if (dicWeaponUpgrade.ContainsKey(name)) {
                return dicWeaponUpgrade[name];
            }

            return null;
        }

        public Sprite GetWeaponUpgradeFrame(string name) {
            if (dicWeaponUpgradeFrame.ContainsKey(name)) {
                return dicWeaponUpgradeFrame[name];
            }

            return null;
        }

        public Sprite GetHeroUpgrade(string name) {
            if (dicHeroUpgrade.ContainsKey(name)) {
                return dicHeroUpgrade[name];
            }

            return null;
        }

        public Sprite GetHeroUpgradeFrame(string name) {
            if (dicHeroUpgradeFrame.ContainsKey(name)) {
                return dicHeroUpgradeFrame[name];
            }

            return null;
        }
    }
}
