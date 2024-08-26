using System;
using UnityEngine;

public class LegManager : MonoBehaviour
{
    [SerializeField] BoxCollider _legPrefab;
    [SerializeField, Range(1,20)] int _jointCount = 1;

    [SerializeField] Transform _target;
    [SerializeField] float _acceptableDistance = .05f;
    [SerializeField] float _angleX = 1;
    [SerializeField] float _angleY = 1;
    [SerializeField] bool _step;
    [SerializeField] int _acceptableTries = 5;

    Leg[] _legs;

    void Start()
    {
        _legs = new Leg[]
        {
               new (_angleX,_angleY,_legPrefab,_target, _jointCount,transform, _acceptableDistance, _acceptableTries),
               new (_angleX,0,_legPrefab,_target, _jointCount,transform, _acceptableDistance, _acceptableTries),
               new (_angleX,-_angleY,_legPrefab,_target, _jointCount,transform, _acceptableDistance, _acceptableTries),

               new (-_angleX - 135,_angleY,_legPrefab,_target, _jointCount,transform, _acceptableDistance, _acceptableTries),
               new (-_angleX - 135,0,_legPrefab,_target, _jointCount,transform, _acceptableDistance, _acceptableTries),
               new (-_angleX - 135,-_angleY,_legPrefab,_target, _jointCount,transform, _acceptableDistance, _acceptableTries),
        };
    }

    void Update()
    {
        foreach (Leg leg in _legs)
        {
            leg.Update();
        }
    }

    
}
