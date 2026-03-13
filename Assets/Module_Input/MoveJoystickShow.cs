using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveJoystickShow : MonoBehaviour {
    private ScreenJoystick screenJoystick;
    [Serializable]
    public struct ScreenZonePart {
        public Vector2 range_1;
        public Vector2 range_2;
        public GameObject activePart;
    }
    public List<ScreenZonePart> screenZone_list = new List<ScreenZonePart> ();
    private void Awake () {
        screenJoystick = GetComponent<ScreenJoystick> ();
    }
    private void Start () {
        screenJoystick.OnDragScreenJoystick += OnDragScreenJoystick;
    }
    private void OnDragScreenJoystick (float angle, float distance) {
        foreach (var target in screenZone_list) {
            bool ifActive = false;
            if (target.range_1.x != target.range_1.y) {
                if (angle >= target.range_1.x && angle <= target.range_1.y) { ifActive = true; }
            }
            if (target.range_2.x != target.range_2.y) {
                if (angle <= target.range_2.x && angle >= target.range_2.y) { ifActive = true; }
            }
            target.activePart.gameObject.SetActive (ifActive);
        }
    }
    private void OnDestroy () {
        screenJoystick.OnDragScreenJoystick -= OnDragScreenJoystick;
    }
}