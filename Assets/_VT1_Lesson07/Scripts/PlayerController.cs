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

    public void Movement()
    {
        var inputAxis = new Vector3(_inputMoveVector.x, 0, _inputMoveVector.y);

        //カメラの向ている方向を移動ベクトルとする
        //1.カメラの正面ベクトルを取得
        var cameraForward = _cameraController.transform.forward;

        //2.入力値と合わせる
        var moveVector =  (inputAxis.x * _cameraController.transform.right) + (inputAxis.z * cameraForward);
        moveVector.y = 0; //Y軸方向の移動は無効化する

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
}
