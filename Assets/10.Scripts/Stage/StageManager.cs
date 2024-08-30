using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

namespace HornSpirit {
    public class StageManager : MonoBehaviour {
        [Header("Parameters")]
        [SerializeField] float doorDistance = 5f;
        [SerializeField] Transform roomParent;
        [SerializeField] Transform minimapCamera;

        HeroUpgradeManager heroUpgradeManager;
        Room currentRoom;
        public Room CurrentRoom { get => currentRoom; }
        Hero hero;

        readonly Dictionary<Constants.CardinalPoints, Vector3> dicCardinalPoints = new() {
            { Constants.CardinalPoints.North, Vector3.forward },
            { Constants.CardinalPoints.East, Vector3.right },
            { Constants.CardinalPoints.South, Vector3.back },
            { Constants.CardinalPoints.West, Vector3.left }
        };

        readonly Dictionary<Constants.CardinalPoints, Constants.CardinalPoints> dicOpposite = new() {
            { Constants.CardinalPoints.North, Constants.CardinalPoints.South },
            { Constants.CardinalPoints.East, Constants.CardinalPoints.West },
            { Constants.CardinalPoints.South, Constants.CardinalPoints.North },
            { Constants.CardinalPoints.West, Constants.CardinalPoints.East }
        };

        List<Room> rooms = new();
        Vector3 originMinimapCameraPos;

        public void Init() {
            heroUpgradeManager = GameManager.Instance.HeroUpgradeManager;
            hero = GameManager.Instance.Hero;

            rooms = new List<Room>(roomParent.GetComponentsInChildren<Room>());
            foreach (Room room in rooms) {
                room.Init();
            }

            currentRoom = rooms[0];
            originMinimapCameraPos = minimapCamera.position;

            currentRoom.VisitRoom();
        }

        public Vector3 GetRoomCardinalPoint(Room room, Constants.CardinalPoints direction) {
            Constants.CardinalPoints opposite = dicOpposite[direction];
            return room.GetDoorPos(opposite) + dicCardinalPoints[direction] * doorDistance;
        }

        Vector3 AdjustDoorPosition(Vector3 origin, Vector3 pos, Constants.CardinalPoints direction) {
            if (direction == Constants.CardinalPoints.North || direction == Constants.CardinalPoints.South) {
                origin.x = pos.x;
            } else {
                origin.z = pos.z;
            }

            return origin;
        }

        public void RoomIsCleared() {
            heroUpgradeManager.ShowDeck();

            Invoke(nameof(DestroyHeroPet), 1.0f);
        }

        void DestroyHeroPet() {
            hero.DestroyPet();
        }

        public void SetCurrentRoom(Room room) {
            currentRoom = room;
            MoveMinimapCamera(room.MinimapRoomPos);
        }

        public void MoveMinimapCamera(Vector3 pos) {
            pos.y = originMinimapCameraPos.y;
            minimapCamera.position = pos;
        }
    }
}
