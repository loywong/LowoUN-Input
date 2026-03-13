using LowoUN.Util;
public class InputManager : SingletonSimple<InputManager> {
    public void Init() {
        ScreenJoystick.Self.SetWork(true);
    }
}