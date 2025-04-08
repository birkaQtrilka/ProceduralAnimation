using System;
using System.Collections.Generic;
using UnityEngine;
[SelectionBase]
public class LegManager : MonoBehaviour
{
    public bool IsMoving { get; private set; }

    [SerializeField] BoxCollider _legPrefab;
    [SerializeField, Range(1, 20)] int _jointCount = 1;

    [field: SerializeField] public float AcceptableDistance { get; private set; } = .05f;
    [field: SerializeField] public float AngleX { get; private set; } = .31f;
    [field: SerializeField] public float AngleY { get; private set; } = .2f;
    [field: SerializeField] public int CalibrationAttempts { get; private set; } = 5;
    [field: SerializeField] public float MoveSpeed { get; private set; } = 5;
    [field: SerializeField] public bool Gizmos { get; private set; } = false;
    [field: SerializeField] public float WobbleSpeed { get; private set; } = 1.69f;
    [field: SerializeField] public float WobbleAmplitude { get; private set; } = .15f;
    [field: SerializeField] public float ForwardReach { get; private set; } = .8f;
    [field: SerializeField] public float StepSpeed { get; private set; } = .5f;
    [field: SerializeField] public float StepDistance { get; private set; } = 1f;
    [field: SerializeField] public float RestStepDistance { get; private set; } = .1f;
    [field: SerializeField] public float DistanceFromBody { get; private set; } = .5f;
    [SerializeField] bool _move;

    [SerializeField] float _groundOffset = 1f;
    [SerializeField] float _heightChangeLerp;

    [SerializeField] BoxCollider firstLeg;
    [SerializeField] BoxCollider firstSecondLeg;
    Leg[] _legs;
    public bool Step;
    float _lastBodyHeight;

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
        if(_move)
            MoveBody();
        IsMoving = _move;
        UpdateLegs();
        SetBodyHeight();
    }

    void UpdateLegs()
    {
        foreach (Leg leg in _legs)
            leg.Update();
    }

    void MoveBody()
    {
        transform.position += MoveSpeed * Time.deltaTime * transform.forward;
    }

    void SetBodyHeight()
    {
        float groundPositionAverage = 0;

        foreach (Leg leg in _legs)
            groundPositionAverage += leg.CurrentGroundPosition.y;
        
        groundPositionAverage = (groundPositionAverage / _legs.Length) + _groundOffset;
        //adding wobble
        groundPositionAverage += Time.deltaTime * WobbleAmplitude * Mathf.Sin(Time.time * WobbleSpeed);

        float lerpedHeight = Lerp(_lastBodyHeight, groundPositionAverage, _heightChangeLerp * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, lerpedHeight, transform.position.z);
        _lastBodyHeight = lerpedHeight;
    }


    float Lerp(float start, float end, float t)
    {
        return start + (end - start) * t;
    }

    void OnDrawGizmos()
    {
        if (_legs == null || !Gizmos) return;
        foreach (var leg in _legs)
            leg.OnDrawGizmos();

    }
}
