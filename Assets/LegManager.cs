using System;
using UnityEngine;

public class LegManager : MonoBehaviour
{
    [SerializeField] BoxCollider _legPrefab;
    [SerializeField, Range(1,20)] int _jointCount = 1;

    [SerializeField] float _acceptableDistance = .05f;
    [SerializeField] float _angleX = 1;
    [SerializeField] float _angleY = 1;
    public bool Step;
    [SerializeField] int _acceptableTries = 5;
    [SerializeField] int _moveSpeed = 5;
    [SerializeField] bool _gizmos;

    Leg[] _legs;

    void Start()
    {
        _legs = new Leg[]
        {
               new (_angleX, _angleY, _legPrefab, _jointCount,transform, _acceptableDistance, _acceptableTries),
               //new (_angleX, 0, _legPrefab, _jointCount,transform, _acceptableDistance, _acceptableTries),
               new (_angleX, -_angleY, _legPrefab, _jointCount,transform, _acceptableDistance, _acceptableTries),

               new (-_angleX - 135, _angleY, _legPrefab, _jointCount,transform, _acceptableDistance, _acceptableTries),
               //new (-_angleX - 135, 0, _legPrefab, _jointCount,transform, _acceptableDistance, _acceptableTries),
               new (-_angleX - 135, -_angleY, _legPrefab, _jointCount,transform, _acceptableDistance, _acceptableTries),

        };

        _legs[0].SetAdjacentLegs(new Leg[] { _legs[1], _legs[2] });
        _legs[1].SetAdjacentLegs(new Leg[] { _legs[0], _legs[3] });
        _legs[2].SetAdjacentLegs(new Leg[] { _legs[3], _legs[0] });
        _legs[3].SetAdjacentLegs(new Leg[] { _legs[2], _legs[1] });

        //_legs = new DebugLeg[]
        //{
        //    new (_angleX, _angleY, _legPrefab, _jointCount,transform, _acceptableDistance, _acceptableTries, this),
        //    new (_angleX, 0, _legPrefab, _jointCount,transform, _acceptableDistance, _acceptableTries, this),
        //    new (_angleX, -_angleY, _legPrefab, _jointCount,transform, _acceptableDistance, _acceptableTries, this),

        //    new (-_angleX - 135, _angleY, _legPrefab, _jointCount,transform, _acceptableDistance, _acceptableTries, this),
        //    new (-_angleX - 135, 0, _legPrefab, _jointCount,transform, _acceptableDistance, _acceptableTries, this),
        //    new (-_angleX - 135, -_angleY, _legPrefab, _jointCount,transform, _acceptableDistance, _acceptableTries, this),
        //};
    }

    void Update()
    {
        transform.position += _moveSpeed * Time.deltaTime * transform.forward;

        foreach (Leg leg in _legs)
        {
            leg.Update();
        }
    }

    private void OnDrawGizmos()
    {
        if (_legs == null || !_gizmos) return;
        foreach (var leg in _legs)
        {
            leg.OnDrawGizmos();
        }

    }
}
