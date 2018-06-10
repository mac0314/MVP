﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour {
    const float _SPEED_FACTOR = 0.2f;
    const float _ZERO_TILE_SPEED_PANELTY = 0.5f;
    const float _ZERO_TILE_STAMINA_PANELTY = 2f;
    public float MoveSpeed {
        get {
            return _character.CurrentStats.MoveSpeed * _SPEED_FACTOR * _zeroTilePanelty;
        }
    }
    private float _zeroTilePanelty = 1f;

    public CharacterModel _character = null;
    public float jumpDelay = 1f;
    public Animator PlayerMovementCTRL;
    public Transform FlipPivot;
	public static PlayerMoveController Player{ get; private set;}

    public float MaxHealth = 100f;

    public float health = 100f;
    public float stamina = 100f;

    public AutoTileMovementSetter autoTileMovementSetter;
    TileUnit currentTile = null;

    float CurrentPivotXScale { get { return FlipPivot.transform.localScale.x; } }

    Timer _jumpDelayTimer = new Timer();
    Timer _heightChangeTimer = new Timer();
    public float _heightAscendTime = 0.1f;
    public float _heightDescendTime = 0.25f;

    public float HorizontalMovementFactor = 1f;
    public float VerticalMovementFactor = 1f;

    private int _jumpingLevel = 0;

    private bool _isInZeroTile = false;

    public GameObject joystickObject;
    public Joystick joystick;
    
    void Awake(){
		Player = this;        
	}

    private void Start()
    {
        currentTile = RandomMapGenerator.Instance.GetTile(transform.position);
        autoTileMovementSetter.SetChangeAction(OnChangeCurrentTile);
        
        joystickObject = GameObject.FindGameObjectWithTag("Joystick");
        if (joystickObject != null)
        {
            joystick = joystickObject.GetComponent<Joystick>();
        }
        
    }

    void OnChangeCurrentTile(TileUnit tile)
    {
        //revert old tile transparent

        SetNearTileAlpha(currentTile, 1f);

        currentTile = tile;

        SetNearTileAlpha(currentTile, 0.6f);
        CheckEnterTile(tile);

        //_targetHeight = currentTile.HeightLevel * 0.25f;//no magic numer, change const
        //_initialHeight = FlipPivot.transform.localPosition.y;
        //if (_targetHeight != _initialHeight) {
        //    float time = _targetHeight > _initialHeight ? _heightAscendTime : _heightDescendTime;
        //    _heightChangeTimer.StartTimer(time);
        //}
    }

    void CheckEnterTile(TileUnit current) {
        if (current.HeightLevel == 0)
        {
            _zeroTilePanelty = _ZERO_TILE_SPEED_PANELTY;
            if (_isInZeroTile) {
                CharacterModel.Instance.SubtractStamina(_ZERO_TILE_STAMINA_PANELTY);
            }
            _isInZeroTile = true;
        }
        else {
            _zeroTilePanelty = 1f;
            _isInZeroTile = false;
        }
    }
    
    void EnterZeroTile() {
        
    }

    void SetNearTileAlpha(TileUnit tile, float alpha)
    {
        if (tile == null) return;
        var list = GetTransparentTargets(tile);

        foreach (var t in list) {
            if (t.HeightLevel > tile.HeightLevel) {
                t.SetRendererAlpha(alpha);
            }
        }
    }

    /// <summary>
    /// 알파값을 1로 되돌리는 것은 배열을 받지 말고 저장하고 있는 편이 유리하지 않을까
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    List<TileUnit> GetTransparentTargets(TileUnit tile)
    {
        List<TileUnit> output = new List<TileUnit>
        {
            //RandomMapGenerator.Instance.GetTile(tile.x - 1  , tile.y - 1),
            RandomMapGenerator.Instance.GetTile(tile.x - 1  , tile.y    ),
            RandomMapGenerator.Instance.GetTile(tile.x - 1  , tile.y + 1),
            RandomMapGenerator.Instance.GetTile(tile.x      , tile.y + 1),
            //RandomMapGenerator.Instance.GetTile(tile.x + 1  , tile.y + 1)
        };

        for (int i = output.Count - 1; i >= 0; i--) {
            if (output[i] == null) {
                output.RemoveAt(i);
            }
        }

        return output;
    }

    void Update() {




        //var tile = RandomMapGenerator.Instance.GetTile(transform.position);
        //if (tile != currentTile) {
        //    OnChangeCurrentTile(tile);
        //}

        //if (_heightChangeTimer.started) {
        //    float rate = Mathf.Lerp(_initialHeight, _targetHeight, _heightChangeTimer.Rate);
        //    var lp = FlipPivot.transform.localPosition;
        //    if (_heightChangeTimer.RunTimer()) {
        //        rate = _targetHeight;
        //    }
        //    lp.y = rate;
        //    FlipPivot.transform.localPosition = lp;
        //}


        
        Vector3 currentPos = transform.position;
        Vector3 movePos = Vector3.zero;

#if true
        if (Input.GetKey(KeyCode.W)) {
            MoveUp(ref movePos);
        }
        if (Input.GetKey(KeyCode.S)) {
            MoveDown(ref movePos);
        }
        if (Input.GetKey(KeyCode.A))
        {
            MoveLeft(ref movePos);
        }
        if (Input.GetKey(KeyCode.D))
        {
            MoveRight(ref movePos);
        }
#endif
        if (joystick != null)
        {
            if (joystick.JoyVec.x > 0.4)
            {
                MoveRight(ref movePos);
            }

            if (joystick.JoyVec.x < -0.4)
            {
                MoveLeft(ref movePos);
            }

            if (joystick.JoyVec.y > 0.4)
            {
                MoveUp(ref movePos);
            }

            if (joystick.JoyVec.y < -0.4)
            {
                MoveDown(ref movePos);
            }
        }
        Move(movePos);

        

        if (movePos != Vector3.zero)
        {
            AnimatorUtil.SetBool(PlayerMovementCTRL, "Move", true);
        }
        else {
            AnimatorUtil.SetBool(PlayerMovementCTRL, "Move", false);
        }

        if (movePos.x > 0f)
        {
            if (CurrentPivotXScale < 0f)
            {
                Flip();
            }
        }
        else if (movePos.x < 0f)
        {
            if (CurrentPivotXScale > 0f)
            {
                Flip();
            }
        }

        if (_jumpDelayTimer.started) {
            if (_jumpDelayTimer.RunTimer()) {
                _jumpingLevel = 0;
            }
        }
    
        
    }

    void Flip() {
        var scale = FlipPivot.transform.localScale;
        scale.x *= -1f;
        FlipPivot.transform.localScale = scale;
    }

    const float _JUMP_COST = 5f;
    /// <summary>
    /// Player Jump
    /// </summary>
    /// <param name="input">사용자 입력에 의한 점프인가</param>
    public void Jump(bool input = true) {
        //position update needed?
        if (_character.IsDead()) return;
        if (input && _jumpDelayTimer.started) return;
        if (_heightChangeTimer.started) return;

        if (_character != null) {
            if (_character.CurrentStats.Stamina < _JUMP_COST) {
                return;
            }
        }
        _character.SubtractStamina(_JUMP_COST);

        AnimatorUtil.SetTrigger(PlayerMovementCTRL, "Jump");
        SoundLayer.CurrentLayer.PlaySound("se_jump");
        
        if (GlobalGameManager.Instance.GameNetworkType == GameNetworkType.Multi)
        {
            NetworkComponents.GameServer.Instance.SendPlayerAnimTrigger("Jump");
        }

        if (input) {
            _jumpingLevel = 1;
            _jumpDelayTimer.StartTimer(jumpDelay);
        }
    }

    public Vector3 GetCurrentPos()
    {
        return transform.position;
    }

    //deltaTime : 프레임에 렉이 걸린만큼 값이 커져 프레임렉을 보정

    //공용 이동처리
    void Move(Vector3 pos) {
        if (_character.IsDead()) return;
        if (autoTileMovementSetter.Owner != null) {
            if (autoTileMovementSetter.Owner.IsInShelter())
            {
                //calc movement range
                if (autoTileMovementSetter.Owner.CurrentShelter.Unit.GetTile(pos + transform.position) != null)
                    transform.Translate(pos);
                return;
            }
        }

        int nextDepth = RandomMapGenerator.Instance.GetDepth(transform.position + pos);
        if (nextDepth == -1) return;
        int currentDepth = RandomMapGenerator.Instance.GetDepth(currentTile.x, currentTile.y);
        if (nextDepth - currentDepth - _jumpingLevel < 2)
        {
            transform.Translate(pos);
        }
    }

    void MoveUp(ref Vector3 pos)
    {
        pos.y += MoveSpeed * Time.deltaTime * GameStaticInfo.HorizontalRatio * HorizontalMovementFactor;
        //Move(pos);
    }

    void MoveDown(ref Vector3 pos)
    {
        pos.y -= MoveSpeed * Time.deltaTime * GameStaticInfo.HorizontalRatio * HorizontalMovementFactor;
        //Move(pos);
    }

    void MoveLeft(ref Vector3 pos)
    {
        pos.x -= MoveSpeed * Time.deltaTime * VerticalMovementFactor;
        //Move(pos);
    }

    void MoveRight(ref Vector3 pos)
    {
        pos.x += MoveSpeed * Time.deltaTime * VerticalMovementFactor;
        //Move(pos);
    }

    public void OnPlayerDead() {
        AnimatorUtil.SetTrigger(PlayerMovementCTRL, "Dead");
        if (GlobalGameManager.Instance.GameNetworkType == GameNetworkType.Multi) {
            NetworkComponents.GameServer.Instance.SendPlayerAnimTrigger("Dead");
        }
    }
}