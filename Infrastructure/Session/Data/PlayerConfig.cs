namespace BackendJX3D.Infrastructure.Session.Data;

// Tên property viết PascalCase nhưng JSON là camelCase - khớp được nhờ
// PropertyNameCaseInsensitive trong PlayerConfigClient.
public sealed class PlayerConfig
{
    public List<PlayerConfigSkill>? Skills { get; set; }
    public List<PlayerConfigItem>? Items { get; set; }

    public List<AutoPlayConfig> AutoPlay { get; set; } = [];
    
    public static PlayerConfig Default() => new()
    {
        Skills = null,
        Items = null,

        AutoPlay =
        [
            new AutoPlayConfig
            {
                Recovery = new AutoPlayRecovery(),
                Fight = new AutoPlayFight(),
                Pick = new AutoPlayPick(),
            },
        ],
    };
}


public sealed class PlayerConfigSaveRequest
{
    public uint Uuid { get; set; }

    public List<PlayerConfigSkill> Skills { get; set; } = [];
    public List<PlayerConfigItem> Items { get; set; } = [];
    public List<AutoPlayConfig> AutoPlay { get; set; } = [];

    public static PlayerConfigSaveRequest From(uint uuid, PlayerConfig config) => new()
    {
        Uuid = uuid,
        Skills = config.Skills ?? [],
        Items = config.Items ?? [],
        AutoPlay = config.AutoPlay ?? [],
    };
}

//Bọc ngoài của API: { data, message, statusCode, code }
public sealed class PlayerConfigEnvelope
{
    public PlayerConfig? Data { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; }
    public string? Code { get; set; }
}

public sealed class PlayerConfigSkill
{
    public int SkillIdx { get; set; }
    public int Slot { get; set; }
    public int Usage { get; set; }
}

public sealed class PlayerConfigItem
{
    public int Genre { get; set; }
    public int Particur { get; set; }
    public int Detail { get; set; }
    public int Level { get; set; }
    public int Slot { get; set; }
}

public sealed class AutoPlayConfig
{
    public AutoPlayRecovery? Recovery { get; set; }
    public AutoPlayFight? Fight { get; set; }
    public AutoPlayPick? Pick { get; set; }
}

public sealed class AutoPlayRecovery
{
    public bool EatLife { get; set; }
    public int LifeAutoP { get; set; }
    public int LifeTimeUse { get; set; }
    public int LifeTimeUseP { get; set; }

    public bool EatMana { get; set; }
    public int ManaAutoP { get; set; }
    public int ManaTimeUse { get; set; }

    public bool TpLife { get; set; }
    public int TpAutoP { get; set; }
    public int TpLifeP { get; set; }
    public bool TpMana { get; set; }
    public int TpManaP { get; set; }
    public bool TpEndurance { get; set; }
    public int TpEnduranceP { get; set; }
    public bool TpNotMedicineBlood { get; set; }
    public bool TpNotMedicineMana { get; set; }

    public bool ExecuteExpSkill { get; set; }
    public bool ExecuteItem020 { get; set; }

    public bool BuyPotion { get; set; }
    public bool BuyPotionP { get; set; }
    public bool SellItem { get; set; }
    public bool RepairEq { get; set; }
    public bool BuyTownPortal { get; set; }

    public int SelMap { get; set; }
}

public sealed class AutoPlayFight
{
    public int RadiusAuto { get; set; }
    public int DistanceAuto { get; set; }

    public bool FightUseSB { get; set; }
    public bool AttackPeople { get; set; }
    public bool AttackNpc { get; set; }
    public bool FollowTarget { get; set; }
    public bool FightNear { get; set; }
    public bool EvadeBOSS { get; set; }

    public bool LifeReplenish { get; set; }
    public bool LifeReplenishTeam { get; set; }
    public int LifeReplenishP { get; set; }

    public int PrioTarget { get; set; }

    public List<int> FightSkills { get; set; } = [];
    public List<int> AuraSkills { get; set; } = [];
    public List<int> SupportSkills { get; set; } = [];

    public bool AutoDismount { get; set; }
}

public sealed class AutoPlayPick
{
    public bool PickFightNone { get; set; }
    public bool PickMoney { get; set; }
    public bool PickNotEquip { get; set; }
    public bool PickEquip { get; set; }
    public bool FollowPick { get; set; }
    public int PickEquipKind { get; set; }
    public bool PickHorse { get; set; }

    public bool FilterEquipment { get; set; }
    public bool HoldEquipMagic { get; set; }
    public bool HoldEquipExp { get; set; }
    public int HoldEquipExpV { get; set; }
    public bool HoldEquipFair { get; set; }

    public List<AutoPlayMagicFilter> MagicFilters { get; set; } = [];

    public string? EquipFilter { get; set; }
}

public sealed class AutoPlayMagicFilter
{
    public int AttribType { get; set; }
    public int MinValue { get; set; }
}
