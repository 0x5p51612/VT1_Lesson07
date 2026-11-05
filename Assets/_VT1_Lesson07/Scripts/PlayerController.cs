using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("=== 移動設定 ===")]
    [SerializeField] private float _moveSpeed = 5f;  //基本移動速度

    [Header("=== カメラ設定 ===")]
    [SerializeField] private CameraController _cameraController;

    [Header("=== Player Input ===")]
    [SerializeField] private PlayerInput _playerInput;

    // === Private 変数
    private Vector3 _velocity;      //移動ベクトル
    private Vector3 _inputMoveVector; //入力ベクトル

    void Start()
    {
        MoveInputEnable();  //移動入力の有効化

    }

    void Update()
    {
        Movement();

    }

    #region 入力関連

    public void Movement()
    {
        var inputAxis = new Vector3(_inputMoveVector.x, 0, _inputMoveVector.y);

        //カメラの向ている方向を移動ベクトルとする
        //1.カメラの正面ベクトルを取得
        var cameraForward = _cameraController.transform.forward;

        //2.入力値と合わせる
        var moveVector =  (inputAxis.x * _cameraController.transform.right) + (inputAxis.z * cameraForward);
        moveVector.y = 0; //Y軸方向の移動は無効化する

        //=== 地形に沿ったベクトル
        moveVector = GetSlopeVector(moveVector);        //移動ベクトルを基準に坂道のベクトルを取得


        //入力をそのまま移動ベクトルへ
        _velocity = moveVector.normalized;
        transform.Translate(_moveSpeed * _velocity * Time.deltaTime, Space.World);

    }

    public void MoveInputEnable()
    {
        if(_playerInput == null)
        {
            Debug.LogError("PlayerInputが設定されていません。");
        }

        //移動入力の設定登録
        _playerInput.actions["Move"].performed += context =>
        {
           //入力時の処理 
           var value =context.ReadValue<Vector2>();     //入力値の取得
            _inputMoveVector = value;                   //変数に保存
        };

        _playerInput.actions["Move"].canceled += context =>
        {
            _inputMoveVector = Vector2.zero;    //変数の値をリセット
        };

        //アクションを有効化する
        _playerInput.actions["Move"].Enable();

    }
    #endregion

    #region Ray関連

    Ray MakeRay(Transform rayPoint, Vector3 direction)
    {
        //Rayを作る
        //Ray(1: 始点, 2: 方向ベクトル, 3: 長さ
        Ray ray = new Ray(rayPoint.position + Vector3.up * 0.2f, direction);

        //DrawRayで可視化する
        //Debug.DrawRay(1: 始点, 2:方向, 3: 色, 4:持続時間, 5:深度描画)
        Debug.DrawRay(rayPoint.position + Vector3.up * 0.2f, direction, Color.red);

        return ray;
    }

    bool TryGetRaycast(Ray ray, float distance, out RaycastHit hit)
    {
        //Raycastで判定を行う
        //Raycast(1: Ray, 2: 衝突対象の情報, 3: 距離, 4: レイヤーマスク)
        //※ outは参照私のため、変数を渡すことができる
        return Physics.Raycast(ray, out hit, distance, LayerMask.GetMask("Default"));

    }

    //足元の地形に合わせた移動ベクトルを計算する
   Vector3 GetSlopeVector(Vector3 vel)
   {
        Ray terrainRay = MakeRay(transform, Vector3.up * -1f);
        bool raycast = TryGetRaycast(terrainRay, 10f, out RaycastHit hit);

        if(raycast)
        {
            //入力されたベクトルが0に近い状態ならそのまま返す
            if (vel.sqrMagnitude < 0.1f) return vel;

            Vector3 normal = hit.normal;    //法線の取得
            Vector3 point = hit.point;      //衝突点の取得

            //おまけ 設置点に無理やり引っ付ける
            var pos = transform.position;
            pos.y = point.y;
            transform.position = pos;

            //移動先のベクトルを可視化
            Debug.DrawRay(transform.position, vel, Color.yellow, 0, false);

            //法線のベクトルを可視化
            Debug.DrawRay(transform.position, normal, Color.blue, 0, false);

            //外積で「進行方向」と「法線の向き」に垂直のベクトル
            Vector3 crossRight = Vector3.Cross(vel, normal);
            Debug.DrawRay(transform.position, crossRight, Color.black, 0, false);

            //さらに外積で「垂直のベクトル（右向き）」に「法線の向き」に垂直のベクトルを
            Vector3 crossForward = Vector3.Cross(crossRight, normal) * -1;
            Debug.DrawRay(transform.position, crossForward, Color.yellowGreen, 0, false);

            //地形の角度を求める
            float dot = Vector3.Dot(vel, normal);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            Debug.Log($"角度：{angle}");

            //正面のベクトルが確定
            vel = crossForward;
        }

        return vel;
   }

    #endregion
}
