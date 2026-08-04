namespace BackendJX3D.Infrastructure.Repositories.IRepository;

public interface ITaskRepository
{
    void AddOrUpdate(ushort taskId, int value);

    bool Remove(ushort taskId);

    bool TryGetValue(ushort taskId, out int value);

    IReadOnlyDictionary<ushort, int> GetAll();

    bool Contains(ushort taskId);

    int Count { get; }
}