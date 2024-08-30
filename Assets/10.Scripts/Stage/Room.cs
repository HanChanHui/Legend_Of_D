using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace HornSpirit {
    public class Room : MonoBehaviour {
        [Header("Properties")]
        [SerializeField] bool isBossRoom;
        [SerializeField] Constants.CardinalPoints doorFlag;
        [SerializeField]
        DirectionDoorDictionary dicDoorObject = new() {
            { Constants.CardinalPoints.North, null },
            { Constants.CardinalPoints.South, null },
            { Constants.CardinalPoints.East, null },
            { Constants.CardinalPoints.West, null }
        };
        [SerializeField]
        DirectionDoorDictionary dicWallObject = new() {
            { Constants.CardinalPoints.North, null },
            { Constants.CardinalPoints.South, null },
            { Constants.CardinalPoints.East, null },
            { Constants.CardinalPoints.West, null }
        };
        [SerializeField] Vector3 center;
        [SerializeField] Vector3Int size;
        [SerializeField] Vector2Int coordinate;

        [Header("AStar Parameters")]
        [SerializeField] LayerMask mask;
        [SerializeField] float diameter;
        [SerializeField] bool unwalkableWhenNoGround;

        [Header("For Prototype")]
        [SerializeField] Transform enemyParent;

        Dictionary<Constants.CardinalPoints, Door> dicDoor = new();
        StageManager stageManager;
        WeaponManager weaponManager;
        int enemyCount;
        bool isCleared;
        public bool IsCleared { get => isCleared; }

        public Vector3 Position => transform.position;
        public Vector3 Center => transform.position + center;
        public bool IsBossRoom { get => isBossRoom; }

        GridGraph myGridGraph;
        MinimapRoom minimapRoom;
        Vector3 minimapRoomPos;
        Transform myTransform;
        public Vector3 MinimapRoomPos { get => minimapRoomPos; }

        public void Init() {
            stageManager = GameManager.Instance.StageManager;
            weaponManager = GameManager.Instance.WeaponManager;
            myTransform = transform;

            InitDoors();
            InitMinimap();
            // CheckEnemy();
            CheckEnemyForPrototype();
            CreateGridGraph();
        }

        void InitDoors() {
            foreach (Constants.CardinalPoints door in System.Enum.GetValues(typeof(Constants.CardinalPoints))) {
                if ((doorFlag & door) == door) {
                    dicDoorObject[door].SetActive(true);
                    dicWallObject[door].SetActive(false);

                    dicDoor.Add(door, dicDoorObject[door].GetComponent<Door>());
                    dicDoor[door].Init();
                }
            }
        }

        void InitMinimap() {
            float space = 3.5f;

            minimapRoomPos = new Vector3(coordinate.x * space, 0, coordinate.y * space);
            CreateMinimapRoom(minimapRoomPos);
        }

        void CheckEnemy() {
            Enemy[] enemies = GetComponentsInChildren<Enemy>();
            enemyCount = enemies.Length;

            foreach (Enemy enemy in enemies) {
                enemy.SetRoom(this);
            }
        }

        void CheckEnemyForPrototype() {
            if (enemyParent == null) {
                isCleared = true;
                return;
            }

            Enemy[] enemies = enemyParent.GetComponentsInChildren<Enemy>();
            enemyCount = enemies.Length;

            foreach (Enemy enemy in enemies) {
                enemy.MPStart();
                enemy.StopObject();
                enemy.ResetHealth();
                enemy.EnableSoulEater();
                enemy.SetRoom(this);
            }
        }

        public Vector3 GetDoorPos(Constants.CardinalPoints direction) {
            return dicDoor[direction].transform.position;
        }

        public void DefeatEnemy() {
            enemyCount--;
            weaponManager.CheckVampireMode();

            if (enemyCount == 0) {
                RoomIsCleared();
            }
        }

        public void AddEnemy() {
            enemyCount++;
        }

        void CreateGridGraph() {
            AstarData data = AstarPath.active.data;
            myGridGraph = data.AddGraph(typeof(GridGraph)) as GridGraph;

            float nodeSize = 1f;
            myGridGraph.center = Center;
            myGridGraph.SetDimensions(size.x, size.z, nodeSize);
            myGridGraph.collision.diameter = diameter;
            myGridGraph.collision.mask = mask;
            myGridGraph.collision.heightCheck = false;
            myGridGraph.collision.unwalkableWhenNoGround = unwalkableWhenNoGround;

            AstarPath.active.Scan();
        }

        public void ScanGridGraph() {
            if (enemyCount <= 0) {  // if room is cleared
                return;
            }

            AstarPath.active.Scan(myGridGraph);
        }

        // for test
        public void StopEnemy() {
            if (enemyParent == null) {
                return;
            }

            foreach (Enemy enemy in enemyParent.GetComponentsInChildren<Enemy>(true)) {
                enemy.StopObject();
            }
        }

        public void StartEnemy() {
            if (isCleared || enemyParent == null) {
                return;
            }

            foreach (Enemy enemy in enemyParent.GetComponentsInChildren<Enemy>(true)) {
                enemy.StartObject();
            }
        }

        void CreateMinimapRoom(Vector3 pos) {
            minimapRoom = PoolManager.Instance.NewItem<MinimapRoom>("Object", "MinimapRoom");
            if (minimapRoom) {
                minimapRoom.Reset();
                minimapRoom.SetPosition(pos, myTransform);
                minimapRoom.Show(); // for prototype
            }
        }

        public void RoomIsCleared() {
            if (isBossRoom) {
                return;
            }

            isCleared = true;
            minimapRoom.SetCleared();
            stageManager.RoomIsCleared();
            UnlockDoors();
        }

        public void VisitRoom() {
            if (isCleared == false) {
                LockDoors();
            }

            if (isBossRoom == false) {
                StartEnemy();
            }

            minimapRoom.Visit();
        }

        public void LeaveRoom() {
            StopEnemy();
            minimapRoom.Leave();
        }

        public void LockDoors() {
            foreach (Door door in dicDoor.Values) {
                door.Lock();
            }
        }

        public void UnlockDoors() {
            foreach (Door door in dicDoor.Values) {
                door.Unlock();
            }
        }
    }
}
