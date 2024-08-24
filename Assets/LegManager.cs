using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LegManager : MonoBehaviour
{
    [SerializeField] BoxCollider _legPrefab;
    [SerializeField, Range(1,20)] int _jointCount = 1;
    [SerializeField] Transform _target;
    [SerializeField] float _acceptableDistance = .05f;
    Member[] _leg;

    void Start()
    {
        _leg = new Member[_jointCount];
        for (int i = 0; i < _jointCount; i++)
        {
            _leg[i] = new Member(Instantiate(_legPrefab, transform));
        }
    }

    void Update()
    {
        Member foot = _leg[0];
        int acceptableTries = 10;
        int tries = 0;
        while(Vector3.Distance(foot.GetEndPosition(), _target.position) > _acceptableDistance && tries < acceptableTries)
        {
            foot.SetBoxTransform(_target.position);

            for (int i = 1; i < _leg.Length; i++)
            {
                Member current = _leg[i];
                Member previous = _leg[i - 1];
                current.SetBoxTransform(previous.GetStartPosition());
            }
            Member legBase = _leg[^1];
            legBase.SetStartPosition(transform.position);

            for (int i = _leg.Length - 2; i >= 0; i--)
            {
                Member current = _leg[i];
                Member next = _leg[i + 1];
                current.SetStartPosition(next.GetEndPosition());
            }
            tries++;
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
        Vector3 up = (target - Box.transform.position).normalized;
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
