﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UnitModel {
    public long instanceId = 0;
    public UnitMoveControl MoveControl {
        private set;
        get;
    }

    public UnitModel() {
        MoveControl = new UnitMoveControl(this);
    }

    public virtual float GetSpeed() {
        return 1f;
    }

    public virtual float GetHpRate() {
        return 1f;
    }

    public virtual string GetUnitName() {
        return "";
    }

    public virtual float GetAttackDamage() {
        return 0f;
    }

    public virtual bool IsAttackTargetable() {
        return true;
    }

    public virtual void OnTakeDamage(UnitModel attacker, float damage) {
        
    }

    public virtual void MoveToTile(TileUnit destination) {
        MoveControl.MoveToTile(destination);
    }

    public virtual void SetCurrentTile(TileUnit current) {
        MoveControl.SetCurrentTile(current);
    }

    public virtual TileUnit GetCurrentTile() {
        return null;
    }

    public virtual void UpdatePosition(Vector3 pos) {

    }

    public virtual Vector3 GetCurrentPos() {
        return Vector3.one;
    }

    public virtual void OnArriedPath(TileUnit target) {

    }
}

public class UnitMoveControl {
    public UnitModel model;
    public TileUnit currentTile;
    public TileUnit currentDestTile;
    public AstarCalculator.PathInfo _currentPath;
    float Speed { get { return model.GetSpeed(); } }

    public bool IsMoving {
        private set;
        get;
    }

    public UnitMoveControl(UnitModel model) {
        this.model = model;
    }

    public void SetCurrentTile(TileUnit current) {
        this.currentTile = current;
    }

    public void MoveToTile(TileUnit tile) {
        IsMoving = true;
        currentDestTile = tile;

        if (currentDestTile == currentTile) {
            StopMovement();
            return;
        }

        _currentPath = AstarCalculator.Instance.GetDestinationPath(currentTile, currentDestTile);
    }

    public void StopMovement() {
        IsMoving = false;
        _currentPath = null;
    }

    public void HaltMovement() {
        IsMoving = false;
    }

    public void RestartMovement() {
        IsMoving = true;
    }

    Vector3 PathStartPos() {
        return GetPathTile(_currentPath.currentPathIndex).transform.position;
    }

    Vector3 PathEndPos() {
        return GetCurrentDirectionTile().transform.position;
    }

    TileUnit GetPathTile(int index) {
        return _currentPath.path[index];
    }

    TileUnit GetCurrentDirectionTile() {
        return GetPathTile(_currentPath.currentPathIndex +1);
    }

    Vector3 GetDirectionVector() {
        return PathEndPos() - PathStartPos();
    }

    public void Update() {
        if (!IsMoving) return;
        if (_currentPath != null) {
            float movementValue = Speed * Time.deltaTime;
            if (_currentPath.currentPathIndex < _currentPath.path.Count - 1)
            {
                //var dir = GetDirectionVector();
                model.UpdatePosition(Vector3.MoveTowards(model.GetCurrentPos(), PathEndPos(), movementValue));
                //Vector3 pos = model.GetCurrentPos();
                //pos = pos + dir * movementValue;
                //model.UpdatePosition(pos);

                if (GetCurrentDirectionTile().IsArrived(model.GetCurrentPos()))
                {
                    _currentPath.currentPathIndex++;
                    //if (_currentPath.currentPathIndex == _currentPath.path.Count)
                    //{
                    //    //arrived
                    //    model.OnArriedPath(_currentPath.Destination);
                    //    StopMovement();
                    //}
                }
            }
            else if (_currentPath.currentPathIndex == _currentPath.path.Count - 1) {
                var end = _currentPath.Destination;
                //var dir = end.transform.position - GetPathTile(_currentPath.currentPathIndex - 2).transform.position;
                //Vector3 pos = model.GetCurrentPos();
                //pos = pos + dir * movementValue;
                //model.UpdatePosition(pos);
                model.UpdatePosition(Vector3.MoveTowards(model.GetCurrentPos(), PathEndPos(), movementValue));


                if (end.IsArrived(model.GetCurrentPos()))
                {
                    model.OnArriedPath(_currentPath.Destination);
                    StopMovement();
                }
            }
        }
    }
}
