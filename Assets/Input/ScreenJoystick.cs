using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScreenJoystick : HiJoystick {
    public static ScreenJoystick Self { private set; get; }

    private RectTransform self_rect;
    [SerializeField] private RectTransform imgBackgroud;
    [SerializeField] private RectTransform root; // MARK 方向摇杆
    [SerializeField] private RectTransform thumb;
    [SerializeField] private float appearTimer;
    [SerializeField] private Ease appearEase;
    public GameObject noActivePart; // MARK 方向摇杆 按下抬起动画操作对象
    [SerializeField] private RectTransform arrowRotateTransform;
    [SerializeField] private Transform rotatePart;

    private Vector3 defalutPoint;
    private Vector3 thumbLocalPoint;

    public Action OnDragStart;
    public Action<float, float> OnDragScreenJoystick;
    public Action OnDragEnd;

    void Awake () {
        Self = this;
        self_rect = GetComponent<RectTransform> ();
        defalutPoint = root.position;
        thumbLocalPoint = default (Vector3);
        root.gameObject.SetActive (false);

        // UIManager.Self.OnScreenCanvas_Enable+=ResetTouchCount;
        // UIManager.Self.OnScreenCanvas_Disable+=ResetTouchCount;
    }

    void OnEnable () { ResetTouchCount (); }
    void OnDisable () {
        DOTween.Kill (this);
        ResetTouchCount ();
    }

    public override void SetWork (bool isWork) {
        if (root == null || root.gameObject == null)
            return;
        base.SetWork (isWork);
        root.gameObject.SetActive (isWork);
    }

    public void Show () {
        // LowoUN.Util.LLog.Print("ScreenJoystick >>> Show");
        if (this != null && gameObject != null && gameObject.activeInHierarchy == false)
            gameObject.SetActive (true);
    }
    public void Hide () {
        // LowoUN.Util.LLog.Print("ScreenJoystick >>> Hide");
        if (this != null && gameObject != null && gameObject.activeInHierarchy == true)
            gameObject.SetActive (false);
    }

    RectTransform rtLeft;

    float input_dir_area_width = 0.4f;
    float input_dir_area_height = 0.7f;

    // 当前的Canvas
    Canvas curCanvas;

    void Start () {
        curCanvas = FindObjectOfType<Canvas>();

        if (noActivePart != null) {
            noActivePart.transform.localScale = Vector3.zero;
            noActivePart.SetActive (true);
            noActivePart.transform.DOScale (Vector3.one, appearTimer).SetEase (appearEase).SetUpdate (true).SetTarget (this);
        }
        arrowRotateTransform.gameObject.SetActive (false);
        thumbLocalPoint = thumb.localPosition;

        // 响应区域 --------------------------------------------------- begin
        rtLeft = imgBackgroud;
        rtLeft.anchorMin = new Vector2 (0, 0);
        rtLeft.anchorMax = new Vector2 (0, 0);
        rtLeft.pivot = new Vector2 (0, 0);
        rtLeft.anchoredPosition = Vector2.zero;

        // 直接设置偏移
        rtLeft.offsetMin = new Vector2 (offset_left, offset_bottom); // left, bottom
        // rtLeft.offsetMax = new Vector2(0, 0);      // right, top

        // 只需要和cannas的标准比较：高度 占屏幕 1080*2/3 = 720  宽度 占屏幕2160*1/2 = 1080
        var canvasSize = curCanvas.GetComponent<RectTransform> ().sizeDelta;
        // Debug.LogError($"Screen.width:{Screen.width}, Screen.height:{Screen.height}, canvasSize:{canvasSize}");
        rtLeft.sizeDelta = new Vector2 (canvasSize.x * input_dir_area_width, canvasSize.y * input_dir_area_height);
        // rtLeft.sizeDelta = new Vector2 (2160 * input_dir_area_width, 1080 * input_dir_area_height);

#if UNITY_EDITOR
        Debug.Log ($"[Editor临时] ScreenJoystick rtLeft.sizeDelta:{rtLeft.sizeDelta}");
#endif
        // 响应区域 --------------------------------------------------- end
    }

    uint offset_left = 4;
    uint offset_bottom = 4;
    public void TEST_SetSafeFrameSize (uint ol, uint ob) {
        if (ol < 4) ol = 4;
        if (ob < 4) ob = 4;
        offset_left = ol;
        offset_bottom = ob;
        Debug.Log ($"TEST_SetSafeFrameSize -- offset_left:{offset_left}, offset_bottom:{offset_bottom}");

        rtLeft.offsetMin = new Vector2 (offset_left, offset_bottom);
    }

    protected override void OnDraging () {
        thumb.localPosition = AxisValue * DragDistance;
        float angle = Vector2.SignedAngle (Vector2.right, AxisValue);
        angle %= 360;
        if (rotatePart == null) { rotatePart = arrowRotateTransform; }
        rotatePart.rotation = Quaternion.Euler (0, 0, angle);
        OnDragScreenJoystick?.Invoke (angle, DragDistance);
    }

    public void SetAutoDrag_Restart_PushStone () {
        if (isPointerDown) {
            ResetAutoDragState ();
            SetAutoDrag (Vector2.zero, 0);
        } else SetAutoDrag_Restart ();
    }
    public void SetAutoDrag_Restart () {
        // Debug.LogWarning("[临时测试] SetAutoDrag_Restart");
        CancelOnPointerUp ();

        if (this == null || gameObject == null) {
            Debug.LogError ("ScreenJoystick !this.IsValid()");
            return;
        }
        if (root == null || thumb == null || arrowRotateTransform == null) {
            Debug.LogError ("root==null||thumb==null||arrowRotateTransform==null");
            return;
        }

        root.transform.position = defalutPoint;
        thumb.localPosition = thumbLocalPoint;
        arrowRotateTransform.gameObject.SetActive (false);
        if (noActivePart != null && noActivePart.transform != null) {
            noActivePart.transform.localScale = Vector3.zero;
            noActivePart.SetActive (true);
            // Debug.LogError("ScreenJoystick 2");
            noActivePart.transform.DOScale (Vector3.one, appearTimer).SetEase (appearEase).SetUpdate (true).SetTarget (this);
        }
        SetAutoDrag (Vector2.zero, 0);
    }
    void SetAutoDrag (Vector2 AxisValue, float DragDistance) {
        SetAxisValue (AxisValue);
        this._DragDistance = DragDistance;
        thumb.localPosition = AxisValue * DragDistance;
        float angle = Vector2.SignedAngle (Vector2.right, AxisValue);
        angle %= 360;
        if (rotatePart == null) { rotatePart = arrowRotateTransform; }
        rotatePart.rotation = Quaternion.Euler (0, 0, angle);
        OnDragScreenJoystick?.Invoke (angle, DragDistance);
    }

    TweenerCore<Vector3, Vector3, VectorOptions> OnStartDragTween;
    protected override void OnStartDrag (PointerEventData eventData) {
        // Debug.LogError("OnStartDrag --> show arrowRotateTransform");
        if (OnStartDragTween != null) OnStartDragTween.Kill ();

        root.anchoredPosition = GetScreenPos (eventData);
        if (noActivePart != null) {
            noActivePart.gameObject.SetActive (false);
        }
        arrowRotateTransform.transform.localScale = Vector3.zero;
        arrowRotateTransform.gameObject.SetActive (true);
        // arrowRotateTransform.localScale = Vector3.one;
        // Debug.LogError("ScreenJoystick 3");
        OnStartDragTween = arrowRotateTransform.transform.DOScale (Vector3.one, appearTimer).SetEase (appearEase).SetUpdate (true).SetTarget (this);
        OnDragStart?.Invoke ();
    }

    protected override void OnEndDrag () {
        // Debug.LogWarning("OnEndDrag");

        if (root == null || root.transform == null)
            return;
        // Debug.LogError("OnEndDrag");
        root.transform.position = defalutPoint;
        thumb.localPosition = thumbLocalPoint;
        arrowRotateTransform.gameObject.SetActive (false);
        if (noActivePart != null) {
            noActivePart.gameObject.transform.localScale = Vector3.zero;
            noActivePart.gameObject.SetActive (true);
            // Debug.LogError("ScreenJoystick 4");
            noActivePart.transform.DOScale (Vector3.one, appearTimer).SetEase (appearEase).SetUpdate (true).SetTarget (this);
        }
        OnDragEnd?.Invoke ();
    }
    private Vector2 GetScreenPos (PointerEventData eventData) {
        Vector2 res_pos = eventData.position / curCanvas.transform.localScale.x;
        return res_pos;
    }

    public void CancelOnPointerUp () {
        // Debug.LogWarning("[临时测试] joystick 断开 CancelOnPointerUp");
        ResetTouchCount ();
        OnPointerUp_Manually ();
    }

    void OnDestroy () {
        DOTween.Kill (this);
    }
}