// 1) Interface
public interface IRewindable
{
    void RecordState();
    void ApplyRewind();
    bool CanInstantRewind { get; }
}
