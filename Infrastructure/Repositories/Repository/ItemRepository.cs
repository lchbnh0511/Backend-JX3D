using BackendJX3D.Domain.Entities;
using BackendJX3D.Infrastructure.Repositories.IRepository;
using Network.Header;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class ItemRepository : IItemRepository
{
    private readonly Dictionary<int, ITEM_SYNC> _items = new();

    // index theo Place
    private readonly Dictionary<int, ITEM_SYNC>[] _itemsByPlace;

    public ItemRepository()
    {
        _itemsByPlace = new Dictionary<int, ITEM_SYNC>[32];

        for (int i = 0; i < _itemsByPlace.Length; i++)
            _itemsByPlace[i] = new Dictionary<int, ITEM_SYNC>();
    }

    public void AddOrUpdate(ITEM_SYNC item)
    {
        if (_items.TryGetValue(item.m_dwID, out var oldItem))
        {
            _itemsByPlace[oldItem.m_btPlace].Remove(oldItem.m_dwID);
        }

        _items[item.m_dwID] = item;
        _itemsByPlace[item.m_btPlace][item.m_dwID] = item;
    }

    public bool Remove(int itemId)
    {
        if (!_items.TryGetValue(itemId, out var item))
            return false;

        _items.Remove(itemId);
        _itemsByPlace[item.m_btPlace].Remove(item.m_dwID);

        return true;
    }

    public ITEM_SYNC? Get(int itemId)
    {
        _items.TryGetValue(itemId, out var item);
        return item;
    }

    public IReadOnlyCollection<ITEM_SYNC> GetAll()
    {
        return _items.Values;
    }

    public IReadOnlyCollection<ITEM_SYNC> GetByPlace(byte place)
    {
        return _itemsByPlace[place].Values;
    }

    public bool Contains(int itemId)
    {
        return _items.ContainsKey(itemId);
    }

    public int Count => _items.Count;
}