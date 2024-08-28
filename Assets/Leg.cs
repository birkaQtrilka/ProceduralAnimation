using System.Linq;
using UnityEngine;

public class Leg
{
    protected class GizmosInfo
    {
        public Vector3 current;
        public Vector3 last;
        public Vector3 live;
    }

    protected int jointCount;
    protected Transform baseTransf;
    protected float angleX;
    protected float angleY;
    protected readonly Member[] members;

    readonly float _acceptableDistance;
    readonly int _acceptableTries = 5;

    protected GizmosInfo _debug;
    protected float stepDistance = 1;
    Vector3 _currentTarget;
    Vector3 _lastTarget;

    float _currLerpTime;
    float _lerpTime = 0.4f;

    Leg[] _adjacentLegs;
    public bool IsGrounded => _currLerpTime > _lerpTime;

    public Leg(float angleX, float angleY, BoxCollider legPrefab, int jointCount, Transform baseTransf, float acceptableDistance, int acceptableTries)
    {
        this.jointCount = jointCount;
        this.baseTransf = baseTransf;
        this.angleX = angleX;
        this.angleY = angleY;
        _acceptableDistance = acceptableDistance;
        _acceptableTries = acceptableTries;

        members = new Member[jointCount];
        for (int i = 0; i < jointCount; i++)
            members[i] = new Member(GameObject.Instantiate(legPrefab, baseTransf));
        //PositionLeg();
    }

    public virtual void Update()
    {
        Vector3 direction = Vector3.RotateTowards(baseTransf.right, baseTransf.forward, angleY * (angleX < 0 ? -1 : 1), 1);
        if (angleX < 0) direction *= -1;
        direction += baseTransf.forward * .8f;
        if (!Physics.Raycast(baseTransf.position + direction, -baseTransf.up, out RaycastHit hit, GetLegReach(), 1<<LayerMask.NameToLayer("Ground")))
        {
            _debug = null;
            return;
        }

        _currLerpTime += Time.deltaTime;

        if (Vector3.Distance(_currentTarget, hit.point) > stepDistance && AdjacentLegsAreGrounded())
        {
            _lastTarget = _currentTarget;   
            _currLerpTime = 0;
            _currentTarget = hit.point;
        }

        Member foot = members[0];
        int tries = 0;
        PositionLeg();

        Vector3 target = Vector3.Lerp(_lastTarget, _currentTarget, _currLerpTime / _lerpTime);

        _debug = new() { last = _lastTarget, current = _currentTarget, live = hit.point};
        while (Vector3.Distance(foot.GetEndPosition(), target) > _acceptableDistance && tries < _acceptableTries)
        {
            InverseKinematics(target);
            tries++;
        }

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
        Vector3 direction = Vector3.RotateTowards(baseTransf.right, baseTransf.up, angleX, 1);
        Vector3 target = baseTransf.position;
        direction = Vector3.RotateTowards(direction, baseTransf.forward, angleY, 1);


        for (int i = members.Length - 1; i >= 0; i--)
        {
            Member current = members[i];

            float l = current.Box.transform.lossyScale.y * .5f;

            current.Box.transform.up = direction;
            current.Box.transform.position = target + direction * l;

            target = current.GetEndPosition();
        }
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
        legBase.SetStartPosition(baseTransf.position);
        for (int i = members.Length - 2; i >= 0; i--)
        {
            Member current = members[i];
            Member next = members[i + 1];
            current.SetStartPosition(next.GetEndPosition());
        }
    }
    
}
