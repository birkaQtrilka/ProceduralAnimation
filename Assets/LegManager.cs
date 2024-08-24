using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LegManager : MonoBehaviour
{
    [SerializeField] BoxCollider _legPrefab;
    [SerializeField, Range(1,20)] int _jointCount = 1;
    [SerializeField] Transform _target;

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
        Member legBase = _leg[0];

        legBase.SetBoxTransform(_target.position);
        for (int i = 1; i < _leg.Length; i++)
        {
            Member current = _leg[i];
            Member previous = _leg[i - 1];
            current.SetBoxTransform(previous.GetEndPosition());
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
        Box.transform.up = (target - Box.transform.position).normalized;
        float l = Box.transform.lossyScale.y;
        Box.transform.position = target - l * .5f * Box.transform.up;
    }

    public Vector3 GetEndPosition()
    {
        float l = Box.transform.lossyScale.y;
        return Box.transform.position - l * .5f * Box.transform.up;
    }
}
