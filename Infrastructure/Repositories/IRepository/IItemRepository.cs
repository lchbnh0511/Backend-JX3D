using BackendJX3D.Domain.Entities;
using Network.Header;

namespace BackendJX3D.Infrastructure.Repositories.IRepository;

public interface IItemRepository
{
    void AddOrUpdate(ITEM_SYNC item);

    bool Remove(int itemId);

    ITEM_SYNC? Get(int itemId);

    IReadOnlyCollection<ITEM_SYNC> GetAll();

    IReadOnlyCollection<ITEM_SYNC> GetByPlace(byte place);

    bool Contains(int itemId);

    int Count { get; }
}