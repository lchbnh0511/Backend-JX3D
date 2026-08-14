using Network.Header;

namespace BackendJX3D.Core.Utils;



// So túi đồ trước/sau một lệnh để biết đã nhận thêm hàng gì.
public static class InventoryDiff
{
    // id -> số lượng
    public static Dictionary<int, int> Snapshot(IReadOnlyCollection<ITEM_SYNC> items)
    {
        var snapshot = new Dictionary<int, int>(items.Count);

        foreach (var item in items)
            snapshot[item.m_dwID] = item.m_Count;

        return snapshot;
    }

    public static List<InventoryGain> Gained(
        Dictionary<int, int> before,
        IReadOnlyCollection<ITEM_SYNC> after)
    {
        var gained = new List<InventoryGain>();

        foreach (var item in after)
        {
            var isNew = !before.TryGetValue(item.m_dwID, out var oldCount);

            // Món MỚI: tính là được 1 kể cả khi m_Count = 0, vì đồ không xếp chồng
            // (trang bị, sách...) có m_Count = 0 - lấy số lượng làm mốc là bỏ sót nó.
            //
            // Món CŨ: chỉ tính khi số lượng TĂNG. Món xếp chồng (thuốc, vật liệu) mua thêm
            // thì không sinh id mới, chỉ tăng m_Count. Giảm là do việc khác chứ không phải mua.
            var added = isNew
                ? Math.Max(item.m_Count, 1)
                : item.m_Count - oldCount;

            if (added <= 0) continue;

            gained.Add(new InventoryGain(item, added, isNew));
        }

        return gained;
    }
}

public readonly struct InventoryGain
{
    public readonly ITEM_SYNC Item;

    public readonly int AddedCount;

    // true = món hoàn toàn mới trong túi, false = món đã có, chỉ tăng số lượng
    public readonly bool IsNew;

    public InventoryGain(ITEM_SYNC item, int addedCount, bool isNew)
    {
        Item = item;
        AddedCount = addedCount;
        IsNew = isNew;
    }
}
