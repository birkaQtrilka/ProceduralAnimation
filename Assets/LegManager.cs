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
               new (_angleX,0,_legPrefab,_target, _jointCount,transform, _acceptableDistance, _acceptableTries),
               new (_angleX,_angleY,_legPrefab,_target, _jointCount,transform, _acceptableDistance, _acceptableTries),
               new (_angleX,_angleY*2,_legPrefab,_target, _jointCount,transform, _acceptableDistance, _acceptableTries),
        };
    }

    void Update()
    {
        //DebugAlgorithm();

        foreach (Leg leg in _legs)
        {
            leg.Update();
        }
    }

    
}

public class Leg
{
    protected Transform target;
    protected int jointCount;
    protected Transform baseTransf;
    protected float angleX;
    protected float angleY;
    protected readonly Member[] members;

    readonly float _acceptableDistance;
    readonly int _acceptableTries = 5;

    public Leg(float angleX, float angleY, BoxCollider legPrefab, Transform target, int jointCount, Transform baseTransf, float acceptableDistance, int acceptableTries)
    {
        this.target = target;
        this.jointCount = jointCount;
        this.baseTransf = baseTransf;
        this.angleX = angleX;
        this.angleY = angleY;
        _acceptableDistance = acceptableDistance;
        _acceptableTries = acceptableTries;

        members = new Member[jointCount];
        for (int i = 0; i < jointCount; i++)
            members[i] = new Member(GameObject.Instantiate(legPrefab, baseTransf));
    }

    public void Update()
    {
        Member foot = members[0];

        int tries = 0;
        PositionLeg();
        while (Vector3.Distance(foot.GetEndPosition(), target.position) > _acceptableDistance && tries < _acceptableTries)
        {
            InverseKinematics();
            tries++;
        }
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

    void InverseKinematics()
    {
        //arrange legs above target if possible
        Member foot = members[0];
        foot.SetBoxTransform(target.position);

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

public class DebugLeg : Leg
{
    int _updateIndex;
    Func<bool>[] steps;
    int currentJoint = 0;
    public bool Step;

    public DebugLeg(float angleX, float angleY, BoxCollider legPrefab, Transform target,int jointCount, Transform baseTransf, float acceptableDistance, int acceptableTries) : base(angleX, angleY, legPrefab, target, jointCount, baseTransf, acceptableDistance, acceptableTries)
    {
    }

    void InitSteps()
    {
        steps = new Func<bool>[]
        {
            DebugPositionLeg,
            DebugIK,
            DebugIK,
            DebugIK,
        };

        _updateIndex = 0;
    }

    bool DebugPositionLeg()
    {
        Vector3 direction = Vector3.RotateTowards(baseTransf.right, baseTransf.up, angleX, 1);
        Vector3 target = baseTransf.position;

        for (int i = members.Length - 1; i >= 0; i--)
        {
            Member current = members[i];

            float l = current.Box.transform.lossyScale.y * .5f;

            current.Box.transform.up = direction * l;
            current.Box.transform.position = target + direction * l;

            target = current.GetEndPosition();
        }

        return true;
    }

    bool DebugIK()
    {
        //stop
        if (currentJoint >= jointCount * 2)
        {
            currentJoint = 0;
            return true;
        }

        if (currentJoint < jointCount)
        {
            //go to target
            if (currentJoint == 0)
            {
                Member foot = members[0];
                foot.SetBoxTransform(target.position);
                currentJoint++;

                return false;
            }

            Member cur = members[currentJoint];
            Member previous = members[currentJoint - 1];
            cur.SetBoxTransform(previous.GetStartPosition());

            currentJoint++;
            return false;
        }
        //go to base
        int i = (currentJoint % jointCount) + 1;

        if (i == 1)
        {
            Member legBase = members[^1];
            legBase.SetStartPosition(baseTransf.position);
            currentJoint++;
            return false;
        }

        Member current = members[^i];
        Member next = members[^(i - 1)];
        current.SetStartPosition(next.GetEndPosition());

        currentJoint++;
        return false;
    }

    void DebugAlgorithm()
    {
        if (Step)
        {
            if (steps == null || _updateIndex >= steps.Length)
                InitSteps();

            bool result = steps[_updateIndex].Invoke();

            if (result) _updateIndex++;

            Step = false;
        }
    }
}

public class Member
{
    public readonly BoxCollider Box;

    public Member(BoxCollider box)
    {
        Box = box;
    }

    public void SetBoxTransform(Vector3 target)
    {
        Vector3 up = (target - GetStartPosition()).normalized;
        //up.z = 0;
        Box.transform.up = up;
        //Box.transform.localEulerAngles = new Vector3(Box.transform.localEulerAngles.x, 0, Box.transform.localEulerAngles.z);
        Box.transform.position = target - GetSizeAndDirection();
    }

    public Vector3 GetStartPosition()
    {
        return Box.transform.position - GetSizeAndDirection();
    }

    public void SetStartPosition(Vector3 pos)
    {
        Box.transform.position = pos + GetSizeAndDirection();
    }

    public Vector3 GetEndPosition()
    {
        return Box.transform.position + GetSizeAndDirection() ;
    }

    Vector3 GetSizeAndDirection()
    {
        return Box.transform.lossyScale.y * .5f * Box.transform.up;

    }
}
