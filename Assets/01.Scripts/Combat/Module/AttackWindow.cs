// 공격마다 ID를 부여한다. 취소된 공격/중복 이벤트가 다음 공격을 소비하지 못하게 한다.
public sealed class AttackWindow
{
    public int Id { get; private set; }
    public bool IsPending { get; private set; }

    public int Begin()
    {
        Id = Id == int.MaxValue ? 1 : Id + 1;
        IsPending = true;
        return Id;
    }

    public bool TryConsume(int id)
    {
        if (!IsPending || id != Id)
            return false;
        IsPending = false;
        return true;
    }

    public void Cancel(int id)
    {
        if (id == Id)
            IsPending = false;
    }

    public void Cancel() => IsPending = false;
}
