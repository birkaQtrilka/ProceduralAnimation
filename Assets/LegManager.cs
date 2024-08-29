using System;
using System.Collections.Generic;
using UnityEngine;

public class LegManager : MonoBehaviour
{
    [SerializeField] BoxCollider _legPrefab;
    [SerializeField, Range(1,20)] int _jointCount = 1;

    [field: SerializeField] public float AcceptableDistance {get; private set;} = .05f;
    [field: SerializeField] public float AngleX {get; private set;} = .31f;
    [field: SerializeField] public float AngleY {get; private set;} = .2f;
    [field: SerializeField] public int CalibrationAttempts {get; private set;} = 5;
    [field: SerializeField] public float MoveSpeed {get; private set;} = 5;
    [field: SerializeField] public bool Gizmos {get; private set;} = false;
    [field: SerializeField] public float WobbleSpeed {get; private set;} = 1.69f;
    [field: SerializeField] public float WobbleAmplitude { get; private set; } = .15f;
    [field: SerializeField] public float ForwardReach { get; private set; } = .8f;

    [SerializeField] BoxCollider firstLeg;
    [SerializeField] BoxCollider firstSecondLeg;

    Leg[] _legs;
    public bool Step;

    void Start()
    {
        _legs = new Leg[]
        {
           new (AngleX, 0, firstLeg, _jointCount, this),
           new (AngleX, -AngleY, _legPrefab, _jointCount, this),
           new (AngleX, -AngleY*2, _legPrefab, _jointCount, this),

           new (AngleX, -AngleY*2 - 135, firstSecondLeg, _jointCount, this),
           new (AngleX, -AngleY-135, _legPrefab, _jointCount, this),
           new (AngleX, - 135, _legPrefab, _jointCount, this),

        };

        SetAdjacentLegs(_legs);

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

    void SetAdjacentLegs( Leg[] arr)
    {
        Debug.Assert(arr.Length % 2 == 0);

        int legsOnSide = (int)(arr.Length * .5f);
        for (int i = 0; i< arr.Length; i++)//attaches left right up down neighbours but assumes that the grid has only two rows
        {
            int left = i - 1;
            int right = i+1;
            int up = i - legsOnSide;
            int down = i + legsOnSide;
            bool firstRow = i < legsOnSide;

            List<Leg> adjLegs = new(3);
            if (firstRow)//there are no up neighbours
            {
                if (left > -1) adjLegs.Add(arr[left]);
                if (right < legsOnSide) adjLegs.Add(arr[right]);
                adjLegs.Add(arr[down]);
            }
            else//there are no down neighbours
            {
                if (left > legsOnSide-1) adjLegs.Add(arr[left]);
                if (right < arr.Length) adjLegs.Add(arr[right]);
                adjLegs.Add(arr[up]);
            }
            
            arr[i].SetAdjacentLegs(adjLegs.ToArray());
        }
    }

    void Update()
    {
        transform.position += MoveSpeed * Time.deltaTime * transform.forward;
        transform.position += Time.deltaTime * WobbleAmplitude * Mathf.Sin(Time.time * WobbleSpeed) * transform.up;
        foreach (Leg leg in _legs)
        {
            leg.Update();
        }
    }

    void OnDrawGizmos()
    {
        if (_legs == null || !Gizmos) return;
        foreach (var leg in _legs)
            leg.OnDrawGizmos();

    }
}
