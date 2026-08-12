using BackendJX3D.Domain.Entities;
using BackendJX3D.Infrastructure.Repositories.IRepository;
using Network.Header;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class ItemRepository : IItemRepository
{
    // Recv thread của GS ghi, request thread của API đọc -> mọi truy cập phải trong lock.
    private readonly object _gate = new();

    private readonly Dictionary<int, ITEM_SYNC> _items = new();

    // Index theo Place. Trước đây là mảng 32 phần tử, mà m_btPlace là byte (0..255)
    // -> place >= 32 là IndexOutOfRangeException NGAY TRONG recv thread, giết luôn RecvLoop
    // và rớt kết nối GS. Dùng dictionary tạo theo nhu cầu: không có biên để tràn,
    // và cũng không cấp phát cho những place chẳng bao giờ dùng.
    private readonly Dictionary<byte, Dictionary<int, ITEM_SYNC>> _itemsByPlace = new();

    public void AddOrUpdate(ITEM_SYNC item)
    {
        lock (_gate)
        {
            if (_items.TryGetValue(item.m_dwID, out var oldItem) &&
                _itemsByPlace.TryGetValue(oldItem.m_btPlace, out var oldPlace))
            {
                oldPlace.Remove(oldItem.m_dwID);
            }

            _items[item.m_dwID] = item;

            if (!_itemsByPlace.TryGetValue(item.m_btPlace, out var place))
            {
                place = new Dictionary<int, ITEM_SYNC>();
                _itemsByPlace.Add(item.m_btPlace, place);
            }

            place[item.m_dwID] = item;
        }
    }

    public bool Remove(int itemId)
    {
        lock (_gate)
        {
            if (!_items.TryGetValue(itemId, out var item))
                return false;

            _items.Remove(itemId);

            if (_itemsByPlace.TryGetValue(item.m_btPlace, out var place))
                place.Remove(item.m_dwID);

            return true;
        }
    }

    public ITEM_SYNC? Get(int itemId)
    {
        lock (_gate)
        {
            return _items.TryGetValue(itemId, out var item) ? item : null;
        }
    }

    // Trả bản sao, không trả .Values (view sống - recv thread ghi giữa lúc caller duyệt là nổ)
    public IReadOnlyCollection<ITEM_SYNC> GetAll()
    {
        lock (_gate)
        {
            return _items.Values.ToArray();
        }
    }

    public IReadOnlyCollection<ITEM_SYNC> GetByPlace(byte place)
    {
        lock (_gate)
        {
            return _itemsByPlace.TryGetValue(place, out var items)
                ? items.Values.ToArray()
                : Array.Empty<ITEM_SYNC>();
        }
    }

    public bool Contains(int itemId)
    {
        lock (_gate)
        {
            return _items.ContainsKey(itemId);
        }
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _items.Count;
            }
        }
    }
}
