using UnityEngine;

[CreateAssetMenu(fileName = "Observer_", menuName = "Event Observer")]
public class EventObserver : ScriptableObject
{
    public delegate void MyDelegate(ISubject subject);

    public MyDelegate eventHandle;

    public void Raise(ISubject subject)
    {
        eventHandle?.Invoke(subject);
    }
}
