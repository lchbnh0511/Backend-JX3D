using BackendJX3D.Infrastructure.Repositories.IRepository;
using BackendJX3D.Infrastructure.Repositories.Repository;
namespace BackendJX3D.Infrastructure.Session.Data;
using Network.Header;

public class PlayerState
{
    public uint PlayerId { get; set; }
    public string? Name { get; set; }

    public CURPLAYER_SYNC? CurPlayer { get; set; }

    public NPC_SYNC? PlayerNpc { get; set; }

    public WORLD_SYNC? World { get; set; }

    public CURPLAYER_NORMAL_SYNC? PlayerStats { get; set; }
    public PLAYER_ATTRIBUTE_SYNC? Attribute { get; set; }

    // volatile để request thread thấy ngay cái recv thread vừa ghi.
    public volatile NpcDialog? Dialog;

    public PacketWaiters Waiters { get; } = new();

    public IItemRepository Items { get; } = new ItemRepository();
    public ISkillRepository Skills { get; } = new SkillRepository();
    public INpcRepository Npcs { get; } = new NpcRepository();
    public IPlayerInfoRepository PlayerInfos { get; } = new PlayerInfoRepository();
    public ITaskRepository Tasks { get; } = new TaskRepository();
    public IChatRepository Chats { get; } = new ChatRepository();
    public ITeamRepository Team { get; } = new TeamRepository();
}