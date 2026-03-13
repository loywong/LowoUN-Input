using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class HiJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler {
    protected Vector2 axisValue { private set; get; }
    public Vector2 pAxisValue => axisValue;
    // public Vector2 _AxisValue { set => this.AxisValue = value; }
    public void SetAxisValue (Vector2 v) {
        axisValue = v;
    }
    protected float DragDistance;

    protected Vector2 startPoint;

    public bool isStartDragingEnabled = false; // 等价于 按下
    protected bool isPointerDown => touchCount > 0;
    public bool isDraging { private set; get; }
    public Action OnDragingRelease;
    private int curPointerId;
    private void Set_IsDraging_False () {
        isDraging = false;
        OnDragingRelease?.Invoke ();
    }

    private void OnEnable () {
        // isDraging = false;
        Set_IsDraging_False ();
    }

    [SerializeField] float dragMaxLength = 120;
    [SerializeField] float dragMinLength = 10;
    float dragSpeedFaction = 1f;
    protected void SetDragSpeedFaction (float sf) {
        // LLog.Error ($"TEMP SetDragSpeedFaction dragSpeedFaction:{sf}");
        dragSpeedFaction = sf;
    }

    // float curDragdelta;
    public void OnDrag (PointerEventData eventData) {
        if (!isWork) {
            // Debug.Log ($"HiJoystick OnDrag() isWork false, touchCount:{touchCount}");
            // isDraging = false;
            Set_IsDraging_False ();
            return;
        }

        if (!isStartDragingEnabled) {
            // Debug.Log ($"HiJoystick OnDrag() isStartDraging false, touchCount:{touchCount}");
            // isDraging = false;
            Set_IsDraging_False ();
            return;
        }

        // MARK 当多指触摸的情况下，保持原有的操作数据，不允许受新触摸操作的干扰 
        if (curPointerId != eventData.pointerId) {
            // Debug.Log ($"HiJoystick OnPointerDown() isWork true, touchCount:{touchCount}, isStartDraging:{isStartDragingEnabled}");
            return;
        }

        var vec = (eventData.position - startPoint);
        var dir = vec.normalized;
        DragDistance = vec.magnitude;

        // if (DragDistance < dragMinLength) {
        //     Debug.Log ($"DragDistance < dragMinLength");
        //     isDraging = false;
        //     return;
        // }

        if (DragDistance > dragMaxLength) DragDistance = dragMaxLength;
        axisValue = dir * (DragDistance / dragMaxLength) * dragSpeedFaction;
        isDraging = true;
        OnDraging ();
    }

    public void OnPointerDown (PointerEventData eventData) {
        // Debug.LogError("OnPointerDown");
        // Log.Trace ("input", $"RightJoystick OnPointerDown() isStartDraging:{isStartDraging}");
        if (!isWork) {
            Debug.Log ($"HiJoystick OnPointerDown() isWork false, touchCount:{touchCount}, isDraging:{isDraging}");
            return;
        }

        // 当短暂脱手，视为不脱手
        if (isSimulatePressed) {
            isSimulatePressed = false;
            curPointerId = eventData.pointerId;
            Debug.Log ("[Editor临时] 当短暂脱手，视为不脱手,继续使用之前的摇杆数据");
            return;
        }

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        if (eventData.button == PointerEventData.InputButton.Right) {
            Debug.Log ("eventData.button == PointerEventData.InputButton.Right");
            return;
        }
#endif

        if (!DownCount ()) {
            // Debug.Log ($"HiJoystick OnPointerDown() isWork true, touchCount:{touchCount}, isStartDraging:{isStartDragingEnabled}");
            return;
        }

        curPointerId = eventData.pointerId;
        isStartDragingEnabled = true;
        startPoint = eventData.position;
        OnStartDrag (eventData);
    }

    // 当短暂脱手，视为不脱手
    protected bool isSimulatePressed = false;

    // 当短暂脱手，视为不脱手 -------------------------------- begin
    float remainPresssimulateTime = 0.1f;
    float timerSimulate;
    public void Set_RemainPresssimulateTime (float t) {
        remainPresssimulateTime = t;
    }
    void Update () {
        if (isSimulatePressed) {
            timerSimulate -= Time.deltaTime;
            if (timerSimulate <= 0) {
                timerSimulate = 0;
                Real_OnPointerUp ();
            }
        }
    }

    public void OnPointerUp (PointerEventData eventData) {
        // Log.Trace ("input", $"RightJoystick OnPointerUp() isStartDraging:{isStartDraging}");
        if (!isWork) {
            // Debug.Log ($"HiJoystick OnPointerUp() isWork false, touchCount:{touchCount}, isDraging:{isDraging}");
            return;
        }

        if (touchCount > 1) {
            touchCount--;
            // 已经有手指按下的情况下
            Debug.Assert (isStartDragingEnabled == true);
            return;
        }

        if (!isSimulatePressed) {
            isSimulatePressed = true;
            // touchCount--;
            timerSimulate = remainPresssimulateTime;
        }
    }
    void Real_OnPointerUp () {
        if (!UpCount ()) {
            // Debug.Log ($"HiJoystick OnPointerUp() isWork true, touchCount:{touchCount}, isStartDraging:{isStartDragingEnabled}");
            return;
        }

        isStartDragingEnabled = false;
        // isDraging = false;
        Set_IsDraging_False ();
        axisValue = Vector2.zero;
        DragDistance = 0;
        OnEndDrag ();
    }
    protected void OnPointerUp_Manually () {
        // Debug.LogWarning($"RightJoystick OnPointerUp_Manually() touchCount:{touchCount} isWork:{isWork}");
        if (!isWork) {
            // Debug.Log ($"HiJoystick OnPointerUp() isWork false, touchCount:{touchCount}, isDraging:{isDraging}");
            return;
        }

        isStartDragingEnabled = false;
        // isDraging = false;
        Set_IsDraging_False ();
        axisValue = Vector2.zero;
        DragDistance = 0;
        OnEndDrag ();
    }
    protected void ResetAutoDragState () {
        // Log.Trace ("input", $"RightJoystick OnPointerUp() isStartDraging:{isStartDraging}");
        if (!isWork) {
            // Debug.Log ($"HiJoystick OnPointerUp() isWork false, touchCount:{touchCount}, isDraging:{isDraging}");
            return;
        }

        // isStartDragingEnabled = false;
        // isDraging = false;
        Set_IsDraging_False ();
        axisValue = Vector2.zero;
        DragDistance = 0;
        // OnEndDrag ();
    }

    protected virtual void OnStartDrag (PointerEventData eventData) { }
    protected virtual void OnEndDrag () { }
    protected virtual void OnDraging () { }

    protected bool isWork;
    public virtual void SetWork (bool isWork) {
        this.isWork = isWork;
        ResetTouchCount ();
    }

    // Fix 多次按下，一旦取消某次按下(抬起)，不应该中断操作，应该在所有按下行为抬起之后，取消“本次”按下操作
    int touchCount;
    bool DownCount () {
        if (touchCount > 0) {
            touchCount++;
            return false;
        }
        touchCount++;
        return true;
    }
    bool UpCount () {
        touchCount--;
        if (touchCount > 0)
            return false;
        ResetTouchCount ();
        return true;
    }

    public void ResetTouchCount () {
        // Debug.LogError("ResetTouchCount");
        // Debug.LogError($"isWork:{isWork}");
        touchCount = 0;
        isStartDragingEnabled = false;

        // 当短暂脱手，视为不脱手
        isSimulatePressed = false;
    }
}