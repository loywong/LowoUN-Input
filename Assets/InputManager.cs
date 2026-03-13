using LowoUN.Util;
using UnityEngine;

public class InputManager : SingletonSimple<InputManager> {
    float input_dir_area_width = 0.4f;
    float input_dir_area_height = 0.7f;

    public void Init() {
        Canvas curCanvas = Object.FindObjectOfType<Canvas>();
        var canvasSize = curCanvas.GetComponent<RectTransform>().sizeDelta;
        var sizeDelta = default(Vector2);
        // 只需要和cannas的标准比较：高度 占屏幕 1080*2/3 = 720  宽度 占屏幕2160*1/2 = 1080
            sizeDelta = new Vector2 (canvasSize.x * input_dir_area_width, canvasSize.y * input_dir_area_height);

        ScreenJoystick.Self.SetInit(curCanvas,sizeDelta);
        ScreenJoystick.Self.SetInit2();
        ScreenJoystick.Self.SetWork(true);
    }
}