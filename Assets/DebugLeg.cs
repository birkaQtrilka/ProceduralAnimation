using System;
using UnityEngine;

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
