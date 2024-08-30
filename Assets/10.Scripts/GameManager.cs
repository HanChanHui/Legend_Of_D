using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using ControlFreak2;
using DarkTonic.MasterAudio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DamageNumbersPro;
using Cinemachine;

namespace HornSpirit {
    public class GameManager : MonoBehaviour {
        static GameManager instance = null;
        public static GameManager Instance { get { return instance; } }

        [Header("Parameter")]
        [SerializeField] Hero hero;
        [SerializeField] Boss boss;
        [SerializeField] GameController gameController;
        [SerializeField] FollowingCamera followingCamera;
        [SerializeField] GamepadManager cf2GamepadManager;

        [Header("UI")]
        [SerializeField] CanvasGroup curtain;
        [SerializeField] float curtainFadeTime = 0.5f;
        [SerializeField] float curtainWaitTime = 0.5f;
        [SerializeField] Slider bossHealthSlider;
        [SerializeField] CanvasGroup bossHealthCG;
        [SerializeField] GameObject mainUIObject;
        [SerializeField] DeadPanel deadPanel;
        [SerializeField] NotificationPanel notificationPanel;
        [SerializeField] DamageNumber damageNumber;
        [SerializeField] DamageNumber criticalDamageNumber;
        [SerializeField] GameObject bossName;
        [SerializeField] float bossNameDelay = 5.0f;
        [SerializeField] float addBossTargetDelay = 2.0f;

        [Header("SFX")]
        [SerializeField] string doorOpenSFX;
        [SerializeField] string doorCloseSFX;

        [Header("Manager")]
        [SerializeField] StageManager stageManager;
        [SerializeField] RelicManager relicManager;
        [SerializeField] WeaponManager weaponManager;
        [SerializeField] DialogueManager dialogueManager;
        [SerializeField] ItemManager itemManager;
        [SerializeField] HeroUpgradeManager heroUpgradeManager;
        [SerializeField] StatManager statManager;

        [Header("Card Deck")]
        [SerializeField] WeaponUpgradeCardDeck weaponUpgradeDeck;
        [SerializeField] HeroUpgradeCardDeck heroUpgradeDeck;

        [Header("Enemy Spawn Test")]
        [SerializeField] Transform[] spawnPoints;

        [Header("Conversion")]
        [SerializeField] ConversationScript conversationScript;

        [Header("Camera")]
        [SerializeField] CinemachineTargetGroup targetGroup;
        [SerializeField] CinemachineVirtualCamera targetGroupCamera;
        [SerializeField] CinemachineBlendListCamera blendListCamera;

        [Header("Test")]
        [SerializeField] bool startConversation = false;
        [SerializeField] bool tutorialMonster = false;

        public Hero Hero { get => hero; }
        public FollowingCamera FollowingCamera { get => followingCamera; }
        public GameController GameController { get => gameController; }
        public WeaponManager WeaponManager { get => weaponManager; }
        public StageManager StageManager { get => stageManager; }
        public RelicManager RelicManager { get => relicManager; }
        public ItemManager ItemManager { get => itemManager; }
        public HeroUpgradeManager HeroUpgradeManager { get => heroUpgradeManager; }
        public DialogueManager DialogueManager { get => dialogueManager; }
        public StatManager StatManager { get => statManager; }

        public ConversationScript ConversationScript { get => conversationScript; }
        public NotificationPanel NotificationPanel { get => notificationPanel; }

        // Camera
        int defaultTargetGroupCameraPriority;
        int defaultBlendListCameraPriority;
        float curtainDelay;

        void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Destroy(gameObject);
            }
        }

        void Start() {
            defaultTargetGroupCameraPriority = targetGroupCamera.Priority;
            defaultBlendListCameraPriority = blendListCamera.Priority;

            conversationScript.Init();

            // Manager
            PoolManager.Instance.Init();
            stageManager.Init();
            relicManager.Init();
            weaponManager.Init();
            dialogueManager.Init();
            itemManager.Init();
            heroUpgradeManager.Init();
            statManager.Init();

            // Card Deck
            weaponUpgradeDeck.Init();
            heroUpgradeDeck.Init();
            deadPanel.Init();
            notificationPanel.Init();

            bossHealthCG.alpha = 0;

            hero.Init();
            gameController.Init();

            // start prototype
            DOTween.Sequence()
                .OnStart(() => {
                    curtain.alpha = 1;
                })
                .Append(curtain.DOFade(0, curtainFadeTime))
                .AppendInterval(1.0f)
                .AppendCallback(() => {
                    if (startConversation && GameSave.Instance.FirstConversationIsDone == false) {
                        dialogueManager.StartConversation(conversationScript.GetConversation("First"), StartGame);
                    } else {
                        StartGame();
                    }
                });
        }

        void StartGame() {
            hero.StartMove();
            gameController.EnableAction(GameController.Type.Default);

            if (tutorialMonster) {
                StartCoroutine(CoSpawnPrototypeEnemy());
            }
        }

        IEnumerator CoSpawnPrototypeEnemy() {
            yield return new WaitForSeconds(1.0f);

            for (int i = 0; i < 3; i++) {
                OnSpawnEnemy();
                yield return new WaitForSeconds(0.5f);
            }
        }

        public void OnSpawnEnemy() {
            int index = Random.Range(0, spawnPoints.Length);
            spawnPoints[index].GetPositionAndRotation(out Vector3 pos, out Quaternion rot);
            Enemy enemy = PoolManager.Instance.NewItem<Enemy>("Enemy", "Goblin-Plunderer");
            if (enemy) {
                enemy.Create(pos, rot);
                stageManager.CurrentRoom.AddEnemy();
                enemy.transform.SetParent(stageManager.CurrentRoom.transform);  // for test
                enemy.SetRoom(stageManager.CurrentRoom);
            }
        }

        public void ShowCurtain(UnityAction action, UnityAction postAction, bool startMove = true) {
            DOTween.Sequence().OnStart(() => curtain.gameObject.SetActive(true))
                .AppendCallback(() => MasterAudio.PlaySound(doorOpenSFX))
                .Append(curtain.DOFade(1, curtainFadeTime))
                .AppendCallback(() => action())
                .AppendInterval(curtainWaitTime)
                .AppendCallback(() => MasterAudio.PlaySound(doorCloseSFX))
                .AppendInterval(curtainDelay)
                .Append(curtain.DOFade(0, curtainFadeTime))
                .OnComplete(() => {
                    curtain.gameObject.SetActive(false);
                    postAction();

                    if (startMove) {
                        hero.StartMove();
                    }
                });
        }

        public void ShowCurtainForBoss(UnityAction action, Room room) {
            DOTween.Sequence().OnStart(() => curtain.gameObject.SetActive(true))
                .Append(curtain.DOFade(1, curtainFadeTime))
                .AppendCallback(() => action())
                .AppendInterval(curtainWaitTime)
                .Append(curtain.DOFade(0, curtainFadeTime))
                .AppendInterval(1.0f)
                .OnComplete(() => {
                    curtain.gameObject.SetActive(false);

                    dialogueManager.StartConversation(conversationScript.GetConversation("BeforeBoss"), () => {
                        MasterAudio.StartPlaylist("Boss");
                        room.StartEnemy();
                        Invoke(nameof(HeroPetOwl), 2f);
                        hero.StartMove();
                        EnableTargetGroupCamera(true);
                    });
                });
        }

        public void MoveHeroToNextRoom(Room nextRoom, Constants.CardinalPoints direction, UnityAction action = null) {
            bool heroStartMove = true;

            if (nextRoom.IsBossRoom) {
                heroStartMove = false;
            }

            hero.StopMove();

            curtainDelay = 0;
            if (nextRoom.IsBossRoom) {
                curtainDelay = 1.5f;
                MasterAudio.StopPlaylist();
            }

            ShowCurtain(() => {
                stageManager.CurrentRoom.LeaveRoom();
                stageManager.SetCurrentRoom(nextRoom);

                nextRoom.VisitRoom();
                hero.transform.position = stageManager.GetRoomCardinalPoint(nextRoom, direction);

                // if (nextRoom.IsBossRoom && nextRoom.IsCleared == false) {
                if (nextRoom.IsBossRoom) {
                    // StartCoroutine(CoStartDialogueBeforeBoss(nextRoom));
                    StartBossCutScene(nextRoom);
                } else {
                    if (!nextRoom.IsCleared) {
                        Invoke(nameof(HeroPetOwl), 2f);
                    }
                }

                // TODO: for prototype
                // relicManager.ApplyBersekerNecklace();   // just for test
            }, action, heroStartMove);
        }

        void HeroPetOwl() {     // for prototype
            hero.SpawnPet();
        }

        IEnumerator CoStartDialogueBeforeBoss(Room room) {
            yield return new WaitForSeconds(bossNameDelay);
            MasterAudio.PlaySound("pixelbomb_01");
            bossName.SetActive(true);

            yield return new WaitForSeconds(3.0f);

            ShowCurtainForBoss(() => {
                bossName.SetActive(false);
                boss.HideCloudEffect();
                EnableBlendListCamera(false);
            }, room);

            // bossName.SetActive(false);
            // EnableBlendListCamera(false);
            // yield return new WaitForSeconds(2.0f);

            // dialogueManager.StartConversation(conversationScript.GetConversation("BeforeBoss"), () => {
            //     MasterAudio.StartPlaylist("Boss");
            //     room.StartEnemy();
            //     Invoke(nameof(HeroPetOwl), 2f);
            //     hero.StartMove();
            //     EnableTargetGroupCamera(true);
            // });
        }

        public void OnTestUpgradeDeck() {
            weaponManager.ShowDeck();
        }

        public void OnTestConversion() {
            dialogueManager.StartConversation(conversationScript.GetConversation("First"));
        }

        public void OnTestAttackHero() {
            hero.TakeDamage(1900);
        }

        public void OnTestPetOwl() {
            PetOwl owl = PoolManager.Instance.NewItem<PetOwl>("Object", "PetOwl");
            if (owl) {
                owl.Create(0, 10);
            }
        }

        public void OnTestRelicItem() {
            RelicItem relicItem = PoolManager.Instance.NewItem<RelicItem>("Object", "RelicItem");
            if (relicItem) {
                relicItem.Create(hero.transform.position, Quaternion.identity);
            }
        }

        public void OnStartPlayList(string playlistName) {
            MasterAudio.StartPlaylist(playlistName);
        }

        public void OnStopPlayList(string playlistName) {
            MasterAudio.StopAllPlaylists();
        }

        public void ShowBossHealthSlider(int health, int maxHealth) {
            bossHealthSlider.maxValue = maxHealth;
            bossHealthSlider.value = health;

            bossHealthCG.DOFade(1, 0.5f);
        }

        public void HideBossHealthSlider() {
            bossHealthCG.DOFade(0, 0.5f);
        }

        public void UpdateBossHealthSlider(int health) {
            bossHealthSlider.value = health;
        }

        public void EnableMainUI(bool enable) {
            mainUIObject.SetActive(enable);
        }

        public void ShowDeadPanel() {
            deadPanel.Show();
        }

        public void GoToTitle() {
            curtain.DOFade(1, curtainFadeTime).OnComplete(() => {
                StartCoroutine(CoLoadScene("Title"));
            });
        }

        IEnumerator CoLoadScene(string sceneName) {
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;

            while (!async.isDone) {
                if (async.progress >= 0.9f) {
                    async.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        public void SpawnEnemyDamageNumber(Vector3 position, int damage, bool critical = false) {
            if (critical) {
                criticalDamageNumber.Spawn(position, damage);
            } else {
                damageNumber.Spawn(position, damage);
            }
        }

        public void AddTargetToGroup(Transform target, float weight, float radius) {
            targetGroup.AddMember(target, weight, radius);
        }

        public void EnableTargetGroupCamera(bool enable) {
            if (enable) {
                targetGroupCamera.Priority = 20;
            } else {
                targetGroupCamera.Priority = defaultTargetGroupCameraPriority;
            }
        }

        public void EnableBlendListCamera(bool enable) {
            if (enable) {
                blendListCamera.Priority = 20;
            } else {
                blendListCamera.Priority = defaultBlendListCameraPriority;
            }
        }

        public void OnTestEnableController(bool enable) {
            EnableController(enable);
        }

        public void EnableController(bool enable) {
            cf2GamepadManager.enabled = enable;
        }

        public void StartBossCutScene(Room room) {
            EnableMainUI(false);
            EnableBlendListCamera(true);
            boss.gameObject.SetActive(true);
            boss.ShowCloudEffect();
            Invoke(nameof(PlayBossSound), 1.0f);

            StartCoroutine(nameof(CoStartDialogueBeforeBoss), room);
        }

        void PlayBossSound() {
            MasterAudio.PlaySound("Logo 1 (Short & Lite)");
        }

        public void OnShowHeroUpgradeDeck() {
            heroUpgradeManager.ShowDeck();
        }
    }
}
