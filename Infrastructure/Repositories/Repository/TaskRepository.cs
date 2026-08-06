using BackendJX3D.Infrastructure.Repositories.IRepository;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class TaskRepository : ITaskRepository
{
    // Recv thread của GS ghi, request thread của API đọc -> mọi truy cập phải trong lock.
    private readonly object _gate = new();

    private readonly Dictionary<ushort, int> _tasks = new();

    public void AddOrUpdate(ushort taskId, int value)
    {
        lock (_gate)
        {
            _tasks[taskId] = value;
        }
    }

    public bool Remove(ushort taskId)
    {
        lock (_gate)
        {
            return _tasks.Remove(taskId);
        }
    }

    public bool TryGetValue(ushort taskId, out int value)
    {
        lock (_gate)
        {
            return _tasks.TryGetValue(taskId, out value);
        }
    }

    // Trả bản sao, không trả _tasks (dictionary sống - recv thread ghi giữa lúc caller duyệt là nổ)
    public IReadOnlyDictionary<ushort, int> GetAll()
    {
        lock (_gate)
        {
            return new Dictionary<ushort, int>(_tasks);
        }
    }

    public bool Contains(ushort taskId)
    {
        lock (_gate)
        {
            return _tasks.ContainsKey(taskId);
        }
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _tasks.Count;
            }
        }
    }
}
