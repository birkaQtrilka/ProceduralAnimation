using UnityEngine;

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
        //PositionLeg();

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
