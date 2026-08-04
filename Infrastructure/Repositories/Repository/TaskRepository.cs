using BackendJX3D.Infrastructure.Repositories.IRepository;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class TaskRepository : ITaskRepository
{
    private readonly Dictionary<ushort, int> _tasks = new();

    public void AddOrUpdate(ushort taskId, int value)
    {
        _tasks[taskId] = value;
    }

    public bool Remove(ushort taskId)
    {
        return _tasks.Remove(taskId);
    }

    public bool TryGetValue(ushort taskId, out int value)
    {
        return _tasks.TryGetValue(taskId, out value);
    }

    public IReadOnlyDictionary<ushort, int> GetAll()
    {
        return _tasks;
    }

    public bool Contains(ushort taskId)
    {
        return _tasks.ContainsKey(taskId);
    }

    public int Count => _tasks.Count;
}