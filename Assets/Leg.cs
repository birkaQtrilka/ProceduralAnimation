using System.Linq;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.GraphicsBuffer;

public class Leg
{
    protected class GizmosInfo
    {
        public Vector3 current;
        public Vector3 last;
        public Vector3 live;
    }


    protected readonly Member[] members;
    protected LegManager data;

    protected GizmosInfo _debug;
    Vector3 _nextTarget;
    Vector3 _lastTarget;

    float _currLerpTime;
    float _angleX, _angleY;

    Leg[] _adjacentLegs;
    public bool IsGrounded => _currLerpTime > data.StepSpeed;
    public Vector3 CurrentGroundPosition { get; private set; }

    public Leg(float angleX, float angleY, BoxCollider legPrefab, int jointCount, LegManager data)
    {
        this.data = data;
        _angleX = angleX;
        _angleY = angleY;

        members = new Member[jointCount];
        for (int i = 0; i < jointCount; i++)
            members[i] = new Member(GameObject.Instantiate(legPrefab, data.transform));
       
        //initial positioning
        Vector3 groundPoint;
        if(!GetGround(out groundPoint))
        {
            Vector3 direction = GetYAngle() + data.ForwardReach * data.transform.forward + GetLegReach() * -data.transform.up / 2f;
            groundPoint = data.transform.position + direction;
        }
        CurrentGroundPosition = groundPoint;
        
        _lastTarget = groundPoint;
        _nextTarget = groundPoint;
        PositionLeg();
        InverseKinematics(_lastTarget);
    }

    public virtual void Update()
    {
        if (!GetGround(out Vector3 currentTarget))
        {
            _debug = null;
            return;
        }
        CurrentGroundPosition = currentTarget;

        Vector3 target = InterpolateToTarget(currentTarget);

        _debug = new() { last = _lastTarget, current = _nextTarget, live = currentTarget };

        PositionLeg();

        Member foot = members[0];
        int tries = 0;
        while (Vector3.Distance(foot.GetEndPosition(), target) > data.AcceptableDistance && tries < data.CalibrationAttempts)
        {
            InverseKinematics(target);
            tries++;
        }

    }

    Vector3 InterpolateToTarget(Vector3 currentTarget)
    {
        _currLerpTime += Time.deltaTime;
        float triggerDistance = data.IsMoving ? data.StepDistance : data.RestStepDistance;
        if (Vector3.Distance(_nextTarget, currentTarget) > triggerDistance && AdjacentLegsAreGrounded())
        {
            _lastTarget = _nextTarget;
            _currLerpTime = 0;
            _nextTarget = currentTarget;
        }

        return Vector3.Slerp(_lastTarget, _nextTarget, _currLerpTime / data.StepSpeed);
    }

    bool GetGround(out Vector3 target)
    {
        Vector3 direction = GetYAngle() * data.DistanceFromBody + data.transform.forward * data.ForwardReach;
        //in case the ground is higher than the body position, so the ray doesn't ignore the mesh 
        Vector3 abovePoint = data.transform.up * 5;

        bool hasHit = Physics.Raycast(data.transform.position + abovePoint + direction, -data.transform.up, out RaycastHit hit, GetLegReach() + abovePoint.magnitude, 1 << LayerMask.NameToLayer("Ground"));
        target = hit.point;
        return hasHit;
    }
    
    bool AdjacentLegsAreGrounded()
    {
        return _adjacentLegs.All(l => l.IsGrounded);
    }

    public void SetAdjacentLegs(Leg[] adjacentLegs)
    {
        _adjacentLegs = adjacentLegs;
    }
    
    public void OnDrawGizmos()
    {
        if (_debug == null) return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawSphere(_debug.current, .1f);
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(_debug.last, .1f);
        Gizmos.DrawLine(_debug.live, _debug.last);
    }

    float GetLegReach()
    {
        Member member = members[0];
        return member.GetSize() * 2 * members.Length;
    }

    void PositionLeg()
    {
        Vector3 direction = GetYAngle();
        direction = GetXAngle(direction);

        Vector3 target = data.transform.position;

        for (int i = members.Length - 1; i >= 0; i--)
        {
            Member current = members[i];

            float l = current.Box.transform.lossyScale.y * .5f;

            current.Box.transform.up = direction;
            current.Box.transform.position = target + direction * l;

            target = current.GetEndPosition();
        }
    }
    
    Vector3 GetYAngle()
    {
        return Vector3.RotateTowards(data.transform.right, data.transform.forward, data.AngleY + _angleY, 1);
    }

    Vector3 GetXAngle(Vector3 Yrotated)
    {
        return Vector3.RotateTowards(Yrotated, data.transform.up, data.AngleX + _angleX, 1);
    }

    void InverseKinematics(Vector3 target)
    {
        //arrange legs above target if possible
        Member foot = members[0];
        foot.SetBoxTransform(target);

        for (int i = 1; i < members.Length; i++)
        {
            Member current = members[i];
            Member previous = members[i - 1];
            current.SetBoxTransform(previous.GetStartPosition());
        }

        Member legBase = members[^1];
        legBase.SetStartPosition(data.transform.position);
        for (int i = members.Length - 2; i >= 0; i--)
        {
            Member current = members[i];
            Member next = members[i + 1];
            current.SetStartPosition(next.GetEndPosition());
        }
    }
    
}
