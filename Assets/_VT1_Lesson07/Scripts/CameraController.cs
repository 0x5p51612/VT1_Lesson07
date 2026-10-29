using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // === オブジェクトの参照 === //
    public Transform Target;        // カメラの注視点となるオブジェクトの参照
    public Transform CameraJoint;   // 距離をとるためのオブジェクトの参照
    public Camera Camera;           // カメラの参照

    // === プレイヤー入力の参照 === //
    public PlayerInput PlayerInput; // PlayerInput の参照

    // === パラメータとなる変数 === //
    public Parameters parameter;    // パラメータクラスの参照

    [System.Serializable]
    public class Parameters
    {   // [内部クラス] ※ クラスの中でクラスを定義する。
        public Vector3 Position;        // カメラの位置
        public Vector3 Angles;          // カメラの角度
        public Vector3 Distance;        // 原点（親オブジェクト）からの距離
        public float DistanceMin;       // 距離の最小制限
        public float DistanceMax;       // 距離の最大制限
        public float TiltMin = -90;         // チルトの最小値
        public float TiltMax = 90;          // チルトの最大値
        public float CameraIntencity = 1;   // カメラの操作感度
    }


    void Start()
    {
        // 入力アクションの登録
        InputAction();
    }

    void Update()
    {
        // カメラの移動や入力などの演算を行う
        parameter.Position = Vector3.Lerp(parameter.Position, Target.position, Time.deltaTime * 5);   // 注視点の位置をカメラの位置にする

        // スクロールで距離変更
        var scroll = Mouse.current.scroll.value;
        parameter.Distance.z += scroll.y;
        parameter.Distance.z = Mathf.Clamp(parameter.Distance.z, -5, 0);

        // カメラのチルト（X軸回転）に制限を設ける
        parameter.Angles.x = Mathf.Clamp(parameter.Angles.x, parameter.TiltMin, parameter.TiltMax);
    }

    /// <summary>
    /// Update よりも後に実行されるメソッド
    /// </summary>
    void LateUpdate()
    {
        // Updateで計算した結果を反映する

        // カメラの位置と回転を更新する
        transform.position = parameter.Position;
        transform.eulerAngles = parameter.Angles;
        // rotation(Quaternion型)に直接代入したい場合👇
        // transform.rotation = Quaternion.Euler(Angles);

        // 注視点からの距離を更新する
        CameraJoint.localPosition = parameter.Distance;
    }

    private void InputAction()
    {
        if(PlayerInput == null)
        {
            Debug.LogError("PlayerInput が見つかりません。");
            return;     // メソッドを強制終了させる
        }

        // Look アクションが実行されたときの処理を登録する
        PlayerInput.actions["Look"].performed += context =>
        {
            var value = context.ReadValue<Vector2>();   // 入力値を取得する
            parameter.Angles.x += value.y * -1 * parameter.CameraIntencity; // 上下の回転を加算する
            parameter.Angles.y += value.x * parameter.CameraIntencity;      // 左右の回転を加算する
        };
        PlayerInput.actions["Look"].Enable();   // Look アクションを有効化する
    }
}
