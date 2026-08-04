
using Network.Resource.StatusEx;
using Network.Header;
using Network.Resource.Event;
using Network.Resource.Header;
using System.Text;
// Giả lập các namespace chứa Logic Game nếu cần cấu hình compilation
// Bạn có thể đổi các kiểu dữ liệu trả về của Sprite/Audio thành kiểu của Engine mới (ví dụ: SkiaSharp, ImageSharp, OpenTK, v.v.)
namespace BackendJX3D.Infrastructure.Resources;
public class FakeSprite { public byte[] RawData; public string Path; }
public class FakeAudioClip { public byte[] RawData; public string Name; }

public static class LoadResource
{
    public static Action<float, string> OnProgressChanged;
    private static readonly bool s_IsExternalResourceMode = true;
    public static readonly string uploadBaseUrl = "http://192.168.107.37/upload/dev";
    public static ChatChannelResourceManager ChatChannelManager { get; set; }
    private static string GetUploadBaseUrl()
    {
        return uploadBaseUrl;
    }

    private static readonly Dictionary<int, string> s_LoadingTips = new Dictionary<int, string>();
    private static bool s_LoadingTipsInitialized;
    public static Action OnBenefitDataReady;
    private const int StreamingAssetPreloadConcurrency = 6;
    private const float StreamingAssetPreloadProgressWeight = 0.45f;
    private const float AddressableWarmupProgressWeight = 0.05f;
    private const int SyncFetchTimeoutSeconds = 8;
    
    private static readonly Dictionary<string, byte[]> s_StreamingAssetBytesCache = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
    private static readonly object s_StreamingAssetCacheLock = new object();
    private static bool s_IsLoading;
    
    private static readonly HashSet<string> s_FailedSprites = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public static bool IsLoading => s_IsLoading;
    public static bool IsReady { get; private set; }

    private static readonly HttpClient s_HttpClient = new HttpClient();

    public static async void InitResources()
    {
        await InitResourcesAsync();
        if (!IsReady) return;

        OnProgressChanged?.Invoke(1f, "Tài nguyên đã được tải xong");
        
        // Loại bỏ logic UI Unity (UiLoading, MainManager)
        Console.WriteLine("[LoadResource] Tài nguyên đã sẵn sàng.");
    }

    public static async Task InitResourcesAsync()
    {
        if (IsReady) return;

        while (s_IsLoading)
            await Task.Yield();

        if (IsReady) return;

        s_IsLoading = true;
        try
        {
            await InitResourcesInternalAsync();
            IsReady = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoadResource] InitResourcesAsync failed: {ex}");
        }
        finally
        {
            s_IsLoading = false;
        }
    }
    private class InitMemoryResult
    {
        public string Name { get; set; }
        public double MemoryMB { get; set; }
        public long TimeMs { get; set; }
    }

    private static readonly List<InitMemoryResult> _initMemoryResults = new();
    private static async Task InitResourcesInternalAsync()
    {
        var watcher = new System.Diagnostics.Stopwatch();
        watcher.Start();
        var streamingAssetPaths = GetBootStreamingAssetPaths();
        await WarmupBootAssetsAsync(streamingAssetPaths);

        // Group initialization steps
        // var steps = new Action[] {
        //     InitQuickChat, InitTipsLoading, InitServerList, InitNativePlaceList,
        //     InitEquipment, InitSkills, InitNPC, InitMapList, InitSoundList,
        //     InitStringResource, InitRankSetting, InitMusicSet, InitLevelExp,
        //     InitMagicLevelExp, InitMagicDesc, InitUI, InitGameSetting,
        //     InitGoodsBuySell, InitObj, InitMissle, InitLevelAdd, InitNewPlayer,
        //     InitBaseValue, InitPlayerStamina, InitNPCRes, InitPlayerTitle,
        //     InitItemAbrade, InitEnhanceTab, InitChatChannel, InitChatSentFilter,
        //     InitMission, InitStatusEx, InitMinimap, InitLadderInfo, InitShopCustom,
        //     InitGiftCode, InitStatusExAttrib, InitEventTypeReward, InitEventTypeRewardDetail,
        //     InitEmojiText, InitEmojiSprite, InitEventStoreReward, InitEventStoreRewardDetail,
        //     InitActivityTask, InitTypeSuperShop, InitSkillConfig,
        // };
        
        var steps = new (string Name, Action Action)[]
        {
            // ("InitQuickChat", InitQuickChat),
            // ("InitTipsLoading", InitTipsLoading),
             ("InitServerList", InitServerList),
            // ("InitNativePlaceList", InitNativePlaceList),
            //
            // ("InitEquipment", InitEquipment),
            // ("InitSkills", InitSkills),
            // ("InitNPC", InitNPC),
            // ("InitMapList", InitMapList),
            // ("InitSoundList", InitSoundList),
            //
            // ("InitStringResource", InitStringResource),
            // ("InitRankSetting", InitRankSetting),
            // ("InitMusicSet", InitMusicSet),
            //
            // ("InitLevelExp", InitLevelExp),
            // ("InitMagicLevelExp", InitMagicLevelExp),
            // ("InitMagicDesc", InitMagicDesc),
            //
            // ("InitUI", InitUI),
            // ("InitGameSetting", InitGameSetting),
            //
            // ("InitGoodsBuySell", InitGoodsBuySell),
            // ("InitObj", InitObj),
            // ("InitMissle", InitMissle),
            //
            // ("InitLevelAdd", InitLevelAdd),
            // ("InitNewPlayer", InitNewPlayer),
            //
            // ("InitBaseValue", InitBaseValue),
            // ("InitPlayerStamina", InitPlayerStamina),
            //
            // ("InitNPCRes", InitNPCRes),
            // ("InitPlayerTitle", InitPlayerTitle),
            //
            // ("InitItemAbrade", InitItemAbrade),
            // ("InitEnhanceTab", InitEnhanceTab),
            //
            // ("InitChatChannel", InitChatChannel),
            // ("InitChatSentFilter", InitChatSentFilter),
            //
            // ("InitMission", InitMission),
            // ("InitStatusEx", InitStatusEx),
            //
            // ("InitMinimap", InitMinimap),
            // ("InitLadderInfo", InitLadderInfo),
            //
            // ("InitShopCustom", InitShopCustom),
            //
            // ("InitGiftCode", InitGiftCode),
            //
            // ("InitStatusExAttrib", InitStatusExAttrib),
            //
            // ("InitEventTypeReward", InitEventTypeReward),
            // ("InitEventTypeRewardDetail", InitEventTypeRewardDetail),
            //
            // ("InitEmojiText", InitEmojiText),
            // ("InitEmojiSprite", InitEmojiSprite),
            //
            // ("InitEventStoreReward", InitEventStoreReward),
            // ("InitEventStoreRewardDetail", InitEventStoreRewardDetail),
            //
            // ("InitActivityTask", InitActivityTask),
            // ("InitTypeSuperShop", InitTypeSuperShop),
            //
            // ("InitSkillConfig", InitSkillConfig),
        };

        int total = steps.Length;
        for (int i = 0; i < total; i++)
        {
            try
            {
                //steps[i]?.Invoke();
                var step = steps[i];

                DebugInitMemory(
                    step.Name,
                    step.Action
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadResource] Exception in step {i}: {ex.Message}");
            }

            float stepProgress = (i + 1f) / total;
            float progress = StreamingAssetPreloadProgressWeight + AddressableWarmupProgressWeight
                             + stepProgress * (1f - StreamingAssetPreloadProgressWeight - AddressableWarmupProgressWeight);
            // string stepName = steps[i]?.Method?.Name ?? $"Step {i + 1}";
            string stepName = steps[i].Name;
            OnProgressChanged?.Invoke(progress, $"Tải tập tin: ({i + 1}/{total}) - {stepName}");
            await Task.Yield();
        }

        // LuaManager.Init();

        watcher.Stop();
        Console.WriteLine($"[LoadResource] ✅ Tải tài nguyên hoàn tất trong {watcher.ElapsedMilliseconds} ms");
        OnProgressChanged?.Invoke(1f, "Tài nguyên đã được tải xong");
    }

    private static void DebugInitMemory(
        string name,
        Action action)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();


        long before =
            GC.GetTotalMemory(true);


        var sw =
            System.Diagnostics.Stopwatch.StartNew();


        try
        {
            action();
        }
        catch(Exception ex)
        {
            Console.WriteLine(
                $"[LoadResource] {name} ERROR: {ex}"
            );
        }

        PrintInitMemoryReport();
        sw.Stop();


        long after =
            GC.GetTotalMemory(true);


        double diff =
            (after - before) / 1024.0 / 1024.0;


        _initMemoryResults.Add(new InitMemoryResult
        {
            Name = name,
            MemoryMB = diff,
            TimeMs = sw.ElapsedMilliseconds
        });


        Console.WriteLine(
            $"[RESOURCE] {name,-35} " +
            $"RAM +{diff:F2} MB " +
            $"TIME {sw.ElapsedMilliseconds} ms"
        );
    }
    
    private static void PrintInitMemoryReport()
    {
        Console.WriteLine("");
        Console.WriteLine("============== RESOURCE MEMORY REPORT ==============");
        Console.WriteLine(
            $"{"NAME",-40} {"RAM MB",12} {"TIME MS",12}"
        );
        Console.WriteLine(
            "----------------------------------------------------"
        );


        foreach(var item in _initMemoryResults
                    .OrderByDescending(x => x.MemoryMB))
        {
            Console.WriteLine(
                $"{item.Name,-40} " +
                $"{item.MemoryMB,12:F2} " +
                $"{item.TimeMs,12}"
            );
        }


        Console.WriteLine(
            "===================================================="
        );


        double total =
            _initMemoryResults.Sum(x => x.MemoryMB);


        Console.WriteLine(
            $"TOTAL ESTIMATE MEMORY: {total:F2} MB"
        );
    }
    private static async Task WarmupBootAssetsAsync(IReadOnlyList<string> streamingAssetPaths)
    {
        OnProgressChanged?.Invoke(0.01f, "Đang khởi tạo tài nguyên...");
        
        // Bỏ PreloadLoginAddressablesAsync vì Addressables thuộc Unity
        await PreloadStreamingAssetsAsync(streamingAssetPaths, StreamingAssetPreloadConcurrency);
    }

    private static async Task PreloadStreamingAssetsAsync(IReadOnlyList<string> paths, int maxConcurrency)
    {
        if (paths == null || paths.Count == 0) return;

        int total = paths.Count;
        int completed = 0;
        int nextIndex = 0;
        int workerCount = Math.Max(1, Math.Min(maxConcurrency, total));
        var workers = new Task[workerCount];

        for (var i = 0; i < workerCount; i++)
        {
            workers[i] = Worker();
        }

        await Task.WhenAll(workers);

        async Task Worker()
        {
            while (true)
            {
                int index;
                lock (s_StreamingAssetCacheLock)
                {
                    if (nextIndex >= total) return;
                    index = nextIndex++;
                }

                try
                {
                    await LoadStreamingAssetBytesAsync(paths[index]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LoadResource] Failed to preload {paths[index]}: {ex.Message}");
                }

                int done;
                lock (s_StreamingAssetCacheLock)
                {
                    completed++;
                    done = completed;
                }

                float progress = Math.Max(0.01f, ((float)done / total) * StreamingAssetPreloadProgressWeight);
                OnProgressChanged?.Invoke(progress, $"Đang tải tài nguyên: {done}/{total}");
            }
        }
    }

    private static List<string> GetBootStreamingAssetPaths()
    {
        var paths = new List<string>();
        void Add(string path)
        {
            path = NormalizeStreamingAssetPath(path);
            if (!string.IsNullOrEmpty(path) && !paths.Contains(path))
                paths.Add(path);
        }

        Add(ResourcePaths.UI_LOADTIPS);
        Add(ResourcePaths.UI_LADDER_INFO);
        Add(ResourcePaths.SETTING_SERVER_LIST);
        Add(ResourcePaths.SETTING_NATIVE_PLACE_LIST);
        Add(ResourcePaths.SETTING_SKILLS);
        Add(ResourcePaths.SETTING_NPCS);
        Add(ResourcePaths.SETTING_NPC_MAP);
        Add(ResourcePaths.SETTING_MINIMAP);
        Add(ResourcePaths.SETTING_STRING_RESOURCE);
        Add(ResourcePaths.SETTING_RANK);
        Add(ResourcePaths.UI_STATUSEX);
        Add(ResourcePaths.SETTING_MAGIC_LEVEL_EXP);
        Add(ResourcePaths.SETTING_MAGIC_DESC);
        Add(ResourcePaths.UI_NEW_PLAYER);
        Add(ResourcePaths.SETTING_GAMESETTING);
        Add(ResourcePaths.SETTING_GOODS);
        Add(ResourcePaths.SETTING_BUYSELL);
        Add(ResourcePaths.SETTING_OBJ_DATA);
        Add(ResourcePaths.SETTING_OBJ_MONEY);
        Add(ResourcePaths.SETTING_OBJ_COLOR);
        Add(ResourcePaths.SETTING_MISSLES);
        Add(ResourcePaths.SETTING_LEVEL_ADD);
        Add(ResourcePaths.SETTING_BASE_VALUE_FILE);
        Add(ResourcePaths.SETTING_PLAYER_STAMINA_FILE);
        Add(ResourcePaths.STATE_MAGIC_TABLE_NAME);
        Add(ResourcePaths.SETTING_PLAYER_TITLE_FILE);
        Add(ResourcePaths.ITEM_ABRADE_FILE);
        Add(ResourcePaths.ITEM_ENHANCE_FILE);
        Add(ResourcePaths.CHAT_CHANNEL_TABLE_NAME);
        Add(ResourcePaths.CHAT_FILTER_TABLE_NAME);
        Add(ResourcePaths.SETTING_MISSION);
        Add(ResourcePaths.SETTING_MISSION_FORMAT);
        Add(ResourcePaths.SETTING_MUSIC_SET);
        Add(ResourcePaths.SETTING_LEVEL_EXP);
        Add(ResourcePaths.SETTING_CUSTOM_SHOP);
        Add(ResourcePaths.SETTING_SOUND_LIST);
        Add(ResourcePaths.SETTING_GIFTCODE);

        foreach (var itemPath in ResourcePaths.SETTING_ITEM_EQUIPMENT)
            Add(itemPath);

        Add(ResourcePaths.SETTING_ITEM_GOLDEQUIP);
        Add(ResourcePaths.SETTING_ITEM_MEDICINE);
        Add(ResourcePaths.SETTING_ITEM_OTHER);
        Add(ResourcePaths.SETTING_ITEM_MAGICATTRIB);
        Add(ResourcePaths.SETTING_ITEM_MAGICATTRIB_GE);

        for (int i = 0; i < 10; i++)
            Add(string.Format(ResourcePaths.SETTING_NEW_PLAYER_FILE, i));

        return paths;
    }

    private static string NormalizeStreamingAssetPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return string.Empty;

        string path = relativePath.Replace('\\', '/').Trim();
        while (path.StartsWith("/"))
            path = path.Substring(1);
        return path;
    }

    private static async Task<byte[]> LoadStreamingAssetBytesAsync(string relativePath)
    {
        relativePath = NormalizeStreamingAssetPath(relativePath);
        if (string.IsNullOrEmpty(relativePath)) return null;

        lock (s_StreamingAssetCacheLock)
        {
            if (s_StreamingAssetBytesCache.TryGetValue(relativePath, out var cached))
                return cached;
        }

        string fullPath = Path.Combine(GetUploadBaseUrl(), relativePath);
        byte[] bytes = null;

        try
        {
            bytes = await s_HttpClient.GetByteArrayAsync(fullPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoadResource] HTTP load fail: {fullPath} | {ex.Message}");
            return null;
        }

        lock (s_StreamingAssetCacheLock)
        {
            if (!s_StreamingAssetBytesCache.ContainsKey(relativePath))
                s_StreamingAssetBytesCache.Add(relativePath, bytes);
        }

        return bytes;
    }

    private static byte[] GetStreamingAssetBytesSync(string relativePath)
    {
        relativePath = NormalizeStreamingAssetPath(relativePath);
        if (string.IsNullOrEmpty(relativePath)) return null;

        lock (s_StreamingAssetCacheLock)
        {
            if (s_StreamingAssetBytesCache.TryGetValue(relativePath, out var cached))
                return cached;
        }

        string fullPath = Path.Combine(GetUploadBaseUrl(), relativePath);
        byte[] bytes = null;

        try
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(SyncFetchTimeoutSeconds)))
            {
                // Đồng bộ hóa Task trong C# thuần bằng GetAwaiter().GetResult()
                var response = s_HttpClient.GetAsync(fullPath, cts.Token).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    bytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoadResource] HTTP sync fail/timeout: {fullPath} | {ex.Message}");
            return null;
        }

        if (bytes == null) return null;

        lock (s_StreamingAssetCacheLock)
        {
            if (!s_StreamingAssetBytesCache.ContainsKey(relativePath))
                s_StreamingAssetBytesCache.Add(relativePath, bytes);
        }

        return bytes;
    }

    public static void InitShopCustom()
    {
        string[] shopLine = StreamAsssetHelper.ReadLinesRaw(ResourcePaths.SETTING_CUSTOM_SHOP);
        // ShopUiService.LoadDataToService(shopLine);
    }

    private static byte[] StripUtf8Bom(byte[] bytes)
    {
        if (bytes == null) return null;
        if (bytes.Length <= 3 || bytes[0] != 0xEF || bytes[1] != 0xBB || bytes[2] != 0xBF)
            return bytes;

        byte[] noBom = new byte[bytes.Length - 3];
        Array.Copy(bytes, 3, noBom, 0, noBom.Length);
        return noBom;
    }

    private static string DecodeRawText(byte[] bytes)
    {
        bytes = StripUtf8Bom(bytes);
        if (bytes == null) return null;
        return Encoding.UTF8.GetString(bytes);
    }

    private static string DecodeTCVN3Text(byte[] bytes)
    {
        bytes = StripUtf8Bom(bytes);
        if (bytes == null) return null;

        string raw = new string(bytes.Select(b => (char)b).ToArray());
        return Converter.TCVN3ToUnicode(raw);
    }

    public static bool LoadResources(this KTabFile kTab, string path)
    {
        try
        {
            byte[] bytes = GetStreamingAssetBytesSync(path);
            if (bytes == null || bytes.Length == 0) return false;

            if (bytes.Length > 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                string utf16Text = Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
                kTab.m_Memory = Encoding.UTF8.GetBytes(utf16Text);
            }
            else
            {
                if (bytes.Length > 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                {
                    byte[] noBom = new byte[bytes.Length - 3];
                    Array.Copy(bytes, 3, noBom, 0, noBom.Length);
                    kTab.m_Memory = noBom;
                }
                else
                {
                    kTab.m_Memory = bytes;
                }
            }

            kTab.CreateTabOffset();

            string utf8Text = Encoding.UTF8.GetString(kTab.m_Memory);
            string[] lines = utf8Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            kTab.m_Lines = new List<string[]>();
            foreach (string raw in lines)
            {
                string[] cols = raw.Split('\t');
                kTab.m_Lines.Add(cols);
            }

            kTab.m_Height = kTab.m_Lines.Count;
            kTab.m_Width = kTab.m_Height > 0 ? kTab.m_Lines[0].Length : 0;

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KTabFile] ❌ Lỗi LoadResources: {ex.Message}");
            return false;
        }
    }

    public static void InitEquipment()
    {
        var instance = KLibOfBPT.Instance;

        for (int i = 0; i < instance.GetEquipmentDetailNum(); i++)
        {
            var equip = instance.GetEquipment(i);
            var loader = new KTabFile();
            string resourcePath = $"{ResourcePaths.SETTING_ITEM_EQUIPMENT[i]}";

            if (!loader.LoadResources(resourcePath))
                continue;

            equip.SetCountKBPT(loader.GetHeight() - 1);

            if (!equip.GetMemoryKBPT())
                continue;

            for (int row = 0; row < equip.NumOfEntries(); row++)
                equip.LoadRecordKBPT(row, loader);
        }

        LoadSingleTable(instance.GetEquipmentGold(), ResourcePaths.SETTING_ITEM_GOLDEQUIP);
        LoadSingleTable(instance.GetMedicine(), ResourcePaths.SETTING_ITEM_MEDICINE);
        LoadSingleTable(instance.GetOther(), ResourcePaths.SETTING_ITEM_OTHER);
        LoadSingleTable(instance.GetMagicAttribTF(), ResourcePaths.SETTING_ITEM_MAGICATTRIB);
        LoadSingleTable(instance.GetGoldEqMagicAttribTF(), ResourcePaths.SETTING_ITEM_MAGICATTRIB_GE);
    }

    public static void InitTipsLoading()
    {
        s_LoadingTips.Clear();
        s_LoadingTipsInitialized = true;

        string[] lines = StreamAsssetHelper.ReadLinesRaw(ResourcePaths.UI_LOADTIPS);

        if (lines == null || lines.Length == 0)
            return;

        int autoKey = 1;
        foreach (string rawLine in lines)
        {
            if (string.IsNullOrWhiteSpace(rawLine))
                continue;

            string line = rawLine.Trim();
            string tip = line;
            if (tip.EndsWith(","))
                tip = tip.Substring(0, tip.Length - 1).Trim();

            if (tip.StartsWith("\"") && tip.EndsWith("\"") && tip.Length >= 2)
                tip = tip.Substring(1, tip.Length - 2);

            if (string.IsNullOrEmpty(tip))
                continue;

            s_LoadingTips[autoKey++] = tip;
        }
    }

    public static string GetRandomLoadingTip()
    {
        if (!s_LoadingTipsInitialized)
            InitTipsLoading();

        if (s_LoadingTips.Count == 0)
            return string.Empty;

        List<string> tips = new List<string>(s_LoadingTips.Values);
        var random = new Random();
        int randomIndex = random.Next(0, tips.Count);
        return tips[randomIndex];
    }

    public static void InitSkills()
    {
        var loader = new KTabFile();
        if (!loader.LoadResources(ResourcePaths.SETTING_SKILLS))
            return;

        KSkill_cpp.SetExternalTab(loader);
        KSkillManager.InitSKill();
    }

    public static void InitNPC()
    {
        var loader = new KTabFile();
        if (!loader.LoadResources(ResourcePaths.SETTING_NPCS))
            return;
        NpcManager.SetExternalTab(loader);

        var npcMap = new KTabFile();
        if (npcMap.LoadResources(ResourcePaths.SETTING_NPC_MAP))
        {
            NpcManager.SetExternalNPCMapTab(npcMap);
        }
    }

    public static void InitMapList()
    {
        string[] mapListLines = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.SETTING_MAP_LIST, true);
        var parsedMapList = KMapLoader.ParseMapList(mapListLines);
        KMapManager.SetExternalMapList(parsedMapList);
    }

    public static void InitNativePlaceList()
    {
        string[] mapInfoLines = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.SETTING_NATIVE_PLACE_LIST);
        var parsedMapInfo = KMapLoader.ParseNativePlaceList(mapInfoLines);
        KMapManager.SetExternalMapInfo(parsedMapInfo);
    }

    public static void InitSoundList()
    {
        string fullPath = Path.Combine(GetUploadBaseUrl(), ResourcePaths.SETTING_SOUND_LIST);
        string[] lines = fullPath.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        KSoundManager.Initialize(lines);
    }

    public static void InitStringResource()
    {
        string[] linesString = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.SETTING_STRING_RESOURCE);
        KStringManager.Initialize(linesString);
    }

    public static void InitServerList()
    {
        string[] serverlist = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.SETTING_SERVER_LIST);
        var parsedServerList = KServerLoader.LoadFromLines(serverlist);
        KServerManager.SetExternalServerList(parsedServerList);
    }

    public static void InitRankSetting()
    {
        string[] rankline = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.SETTING_RANK);
        KRankManager.Initialize(rankline);
    }

    public static void InitStatusEx()
    {
        string[] statusEx = StreamAsssetHelper.ReadLinesRaw(ResourcePaths.UI_STATUSEX);
        string raw = statusEx == null ? string.Empty : string.Join("\n", statusEx);
        // UiCharacter.StatusExText = JXRichTextUtil.NormalizeColorTags(raw);
    }

    public static void InitMusicSet()
    {
        string fullPath = Path.Combine(GetUploadBaseUrl(), ResourcePaths.SETTING_MUSIC_SET);
        string[] lines = fullPath.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        KMusicManager.Initialize(lines);
    }

    public static void InitLevelExp()
    {
        string fullPath = Path.Combine(GetUploadBaseUrl(), ResourcePaths.SETTING_LEVEL_EXP);
        string[] lines = fullPath.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        KLevelManager.InitializeLevel(lines);
    }

    public static void InitMagicLevelExp()
    {
        string fullPath = Path.Combine(GetUploadBaseUrl(), ResourcePaths.SETTING_MAGIC_LEVEL_EXP);
        string[] levelMagicExpLine = fullPath.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        KLevelMagicManager.Initialize(levelMagicExpLine);
    }

    public static void InitMagicDesc()
    {
        string[] textMagicDesclines = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.SETTING_MAGIC_DESC);
        MagicDescManager.Init(textMagicDesclines);
    }

    public static void InitUI()
    {
        string[] textUiChooseSeriesLine = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.UI_NEW_PLAYER);
        var newplayer = KUILoader.ParseUINewPlayer(textUiChooseSeriesLine);
        KUIManager.SetExternalUINewPlayer(newplayer);
    }

    public static void InitEmojiText()
    {
        var emoji = StreamAsssetHelper.LoadStreamingAssetLines(ResourcePaths.UI_EMOJI_TEXT);
        KEmojiManager.Init(emoji);
    }
    
    public static void InitEmojiSprite()
    {
        // EmoteSpriteProvider.PreloadAll();
    }

    public static void InitEventStoreReward()
    {
        var eventTypeReward = new KTabFile();
        if (eventTypeReward.LoadResources(ResourcePaths.SETTING_EVENT_STORE_REWARD))
        {
            KEventStoreManager.SetExternalTab(eventTypeReward);
        }
    }

    public static void InitEventStoreRewardDetail()
    {
        var detail = new KTabFile();
        if (detail.LoadResources(ResourcePaths.SETTING_EVENT_STORE_REWARD_DETAIL))
        {
            KEventStoreManager.SetExternalEventDetailTab(detail);
        }
    }

    public static void InitActivityTask()
    {
        var activityTask = new KTabFile();
        if (activityTask.LoadResources(ResourcePaths.SETTING_ACTIVITY_TASK))
        {
            KActivityTaskManager.SetExternalTab(activityTask);
        }
        //DailyActivityService.Instance.PreloadRewardIcons();
    }
    
    public static void InitTypeSuperShop()
    {
        var type = new KTabFile();
        if (type.LoadResources(ResourcePaths.SETTING_SHOP_TYPE))
        {
            KSuperShopManager.SetExternalTab(type);
        }

        foreach (var step in KSuperShopManager.superShopType)
        {
            Console.WriteLine("[LoadResource] step.Name: KSuperSopManager " + step.Key + " - " + Converter.DecodeBytes(step.Value.sTypeName));
        }
    }
    
    public static void InitGameSetting()
    {
        var luaCode = StreamAsssetHelper.LoadStreamingAssetLines(ResourcePaths.SETTING_GAMESETTING);
        GameSettingManager.Init(luaCode);
    }

    public static void InitGoodsBuySell()
    {
        var goods = new KTabFile();
        if (!goods.LoadResources(ResourcePaths.SETTING_GOODS)) return;

        var buysell = new KTabFile();
        if (!buysell.LoadResources(ResourcePaths.SETTING_BUYSELL)) return;

        KBuySell.BuySell.Init(goods, buysell);
    }

    private static void LoadSingleTable(KBasicPropertyTable table, string resourcePath)
    {
        var loader = new KTabFile();
        if (!loader.LoadResources(resourcePath)) return;

        table.SetCountKBPT(loader.GetHeight() - 1);
        if (!table.GetMemoryKBPT()) return;

        for (int i = 0; i < table.NumOfEntries(); i++)
            table.LoadRecordKBPT(i, loader);
    }

    public static void InitObj()
    {
        var objData = new KTabFile();
        if (objData.LoadResources(ResourcePaths.SETTING_OBJ_DATA)) ObjManager.SetExternalTab(objData);
        
        var moneyObj = new KTabFile();
        if (moneyObj.LoadResources(ResourcePaths.SETTING_OBJ_MONEY)) ObjManager.SetExternalMoneyTab(moneyObj);
        
        var colorObj = new KTabFile();
        if (colorObj.LoadResources(ResourcePaths.SETTING_OBJ_COLOR)) ObjManager.SetExternalColorTab(colorObj);
    }

    public static void InitMissle()
    {
        var loader = new KTabFile();
        if (!loader.LoadResources(ResourcePaths.SETTING_MISSLES)) return;
        KMissleManager.SetExternalTab(loader);
    }
    
    public static void InitLevelAdd()
    {
        var LevelAdd = new KTabFile();
        if (LevelAdd.LoadResources(ResourcePaths.SETTING_LEVEL_ADD)) KLevelAddManager.SetExternalTab(LevelAdd);
    }

    public static void InitNewPlayer()
    {
        KNewPlayerManager.Clear();
        for (int i = 0; i < 10; i++)
        {
            string relativePath = string.Format(ResourcePaths.SETTING_NEW_PLAYER_FILE, i);
            string[] lines = StreamAsssetHelper.ReadLinesTCVN3(relativePath);
            if (lines == null || lines.Length == 0) continue;

            var data = NewPlayerIniLoader.Load(lines);
            KNewPlayerManager.Add(i, data);
        }
        KNewPlayerManager.FinalizeInit();
    }

    public static void InitBaseValue()
    {
        string[] baseValueLines = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.SETTING_BASE_VALUE_FILE);
        var parseBaseValue = BaseValueIniLoader.Load(baseValueLines);
        KBaseValueManager.SetExternalTemplates(parseBaseValue);
    }

    public static void InitPlayerStamina()
    {
        string[] staminaLines = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.SETTING_PLAYER_STAMINA_FILE);
        var stamina = new KPlayerStamina();
        stamina.Init(staminaLines);
        //PlayerInfoService.Instance.m_cPlayerStamina = stamina;
    }

    public static void InitNPCRes()
    {
        var stateMagic = new KTabFile();
        if (stateMagic.LoadResources(ResourcePaths.STATE_MAGIC_TABLE_NAME)) KNpcResManager.SetExternalTab(stateMagic);
    }

    public static void InitPlayerTitle()
    {
        var playerTitle = new KTabFile();
        if (playerTitle.LoadResources(ResourcePaths.SETTING_PLAYER_TITLE_FILE)) TitleManager.SetExternalTab(playerTitle);
    }

    public static void InitItemAbrade()
    {
        string[] abradeLines = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.ITEM_ABRADE_FILE);
        KItemSetManager.ItemSet.SetExternalAbradeIni(abradeLines);
    }

    public static void InitEnhanceTab()
    {
        var enhanceTab = new KTabFile();
        if (enhanceTab.LoadResources(ResourcePaths.ITEM_ENHANCE_FILE)) KItemSetManager.ItemSet.SetExternalEnhanceTab(enhanceTab);
    }
    
    public static void InitChatChannel()
    {
        string[] chatChannelLines = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.CHAT_CHANNEL_TABLE_NAME);
        ChatChannelManager.SetExternalChatIni(chatChannelLines);
    }

    public static void InitChatSentFilter()
    {
        string[] chatFilterLines = StreamAsssetHelper.ReadLinesRaw(ResourcePaths.CHAT_FILTER_TABLE_NAME);
        ChatSentFilterManager.Load(chatFilterLines);
    }

    public static void InitMission()
    {
        var missionTab = new KTabFile();
        if (missionTab.LoadResources(ResourcePaths.SETTING_MISSION)) KMissionManager.Mission.SetExternalMissionFile(missionTab);
        
        var missionFormatTab = new KTabFile();
        if (missionFormatTab.LoadResources(ResourcePaths.SETTING_MISSION_FORMAT)) KMissionManager.Mission.SetExternalFormatFile(missionFormatTab);
    }

    public static void InitMinimap()
    {
        string[] minimapLines = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.SETTING_MINIMAP);
        KScenePlaceMapManager.Map.SetExternalMinimapIni(minimapLines);
    }

    public static void InitLadderInfo()
    {
        string[] ladderInfoLines = StreamAsssetHelper.ReadLinesTCVN3(ResourcePaths.UI_LADDER_INFO);
        KLadderManager.List.SetExternalLadderIni(ladderInfoLines);
    }

    public static void InitGiftCode()
    {
        byte[] bytes = GetStreamingAssetBytesSync(ResourcePaths.SETTING_GIFTCODE);
        string[] giftCodeLines = DecodeGiftcodeLines(bytes);
        //GiftcodeManager.SetExternalGiftcode(giftCodeLines);
    }

    public static void InitEventTypeReward()
    {
        var eventTypeReward = new KTabFile();
        if (eventTypeReward.LoadResources(ResourcePaths.SETTING_EVENT_TYPE_REWARD))
        {
            KEventTypeManager.SetExternalTab(eventTypeReward);
            OnBenefitDataReady?.Invoke();
        }
    }

    public static void InitEventTypeRewardDetail()
    {
        var detail = new KTabFile();
        if (detail.LoadResources(ResourcePaths.SETTING_EVENT_TYPE_REWARD_DETAIL))
        {
            KEventTypeManager.SetExternalEventDetailTab(detail);
            OnBenefitDataReady?.Invoke();
        }
    }
    
    public static void InitQuickChat()
    {
        string content = StreamAsssetHelper.LoadStreamingAsset(ResourcePaths.SETTING_QUICK_CHAT);
        KPressChatManager.SetExternalTab(content);
    }
    
    private static string[] DecodeGiftcodeLines(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;

        byte[] single;
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) 
        {
            single = new byte[(bytes.Length - 2) / 2];
            for (int i = 0; i < single.Length; i++)
                single[i] = bytes[2 + i * 2];
        }
        else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) 
        {
            single = new byte[(bytes.Length - 2) / 2];
            for (int i = 0; i < single.Length; i++)
                single[i] = bytes[2 + i * 2 + 1];
        }
        else 
        {
            single = StripUtf8Bom(bytes);
        }

        string raw = new string(single.Select(b => (char)b).ToArray());
        string decoded = Converter.TCVN3ToUnicode(raw);
        return decoded.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    }

    public static void InitStatusExAttrib()
    {
        var luaCode = StreamAsssetHelper.LoadStreamingAssetLines(ResourcePaths.UI_STATUS_EX_ATTRIB);
        KStatusExManager.Init(luaCode);
    }
    
    public static void InitSkillConfig()
    {
        string[] skillConfigLines = StreamAsssetHelper.ReadLinesRaw(ResourcePaths.SETTING_SKILLS_CONFIG);
        //SkillConfig.Load(skillConfigLines);
    }

    public static class StreamAsssetHelper
    {
        // Thay thế bằng đường dẫn thư mục local vật lý (Ví dụ: AppDomain.CurrentDomain.BaseDirectory)
        public static string StreamingAssetsPathLocal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StreamingAssets");

        public static string ToStreamingPath(string scriptPath)
        {
            string s = scriptPath.Replace('\\', '/').Trim();
            if (s.StartsWith("/")) s = s.Substring(1);
            return s;
        }

        public static string[] ReadLinesTCVN3(string relativePath, bool isLoadForMap = false)
        {
            if (isLoadForMap)
            {
                string fullPath = Path.Combine(StreamingAssetsPathLocal, relativePath);
                byte[] bytes;

                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"❌ Không thấy file StreamingAssets Local: {fullPath}");
                    return null;
                }
                bytes = File.ReadAllBytes(fullPath);

                if (bytes.Length > 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                    bytes = bytes.Skip(3).ToArray();

                string raw = new string(bytes.Select(b => (char)b).ToArray());
                string decoded = Converter.TCVN3ToUnicode(raw);
                return decoded.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                byte[] bytes = GetStreamingAssetBytesSync(relativePath);
                if (bytes == null || bytes.Length == 0) return null;

                string decoded = DecodeTCVN3Text(bytes);
                return decoded?.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string[] ReadLinesRaw(string relativePath)
        {
            byte[] bytes = GetStreamingAssetBytesSync(relativePath);
            if (bytes == null || bytes.Length == 0) return null;

            string text = DecodeRawText(bytes);
            return text?.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        public static string LoadStreamingAsset(string relativePath)
        {
            byte[] bytes = GetStreamingAssetBytesSync(relativePath);
            if (bytes == null || bytes.Length == 0) return null;

            return DecodeRawText(bytes);
        }

        public static string[] LoadStreamingAssetLines(string relativePath)
        {
            byte[] bytes = GetStreamingAssetBytesSync(relativePath);
            if (bytes == null || bytes.Length == 0) return null;

            string decoded = DecodeTCVN3Text(bytes);
            return decoded?.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        private static readonly Dictionary<string, FakeSprite> s_SpriteCache = new Dictionary<string, FakeSprite>(StringComparer.OrdinalIgnoreCase);

        public static void ClearSpriteCache() => s_SpriteCache.Clear();

        private static FakeSprite MakeIconSprite(byte[] textureBytes, string relativePath)
        {
            if (textureBytes == null) return null;
            // Loại bỏ các hàm tính toán đồ họa Texture2D/Sprite đặc thù của Unity, chuyển sang lưu trữ cấu trúc thô.
            return new FakeSprite { RawData = textureBytes, Path = relativePath };
        }

        public static FakeSprite LoadSpriteFromStreamingAssets(string relativePath, bool logOnFail = true, bool allowSprFallback = true)
        {
            relativePath = TryFixSpritePath(relativePath);
            if (string.IsNullOrEmpty(relativePath)) return null;
            if (s_SpriteCache.TryGetValue(relativePath, out var cachedSprite) && cachedSprite != null)
                return cachedSprite;
            if (allowSprFallback && s_FailedSprites.Contains(relativePath)) return null;

            string fullPath = Path.Combine(GetUploadBaseUrl(), relativePath);
            byte[] imageData = null;
            bool is404 = false;

            try
            {
                var response = s_HttpClient.GetAsync(fullPath).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                    imageData = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    is404 = true;
            }
            catch
            {
                is404 = false;
            }

            if (imageData == null)
            {
                if (allowSprFallback)
                {
                    FakeSprite sprFallback = TryLoadSpriteFromSpr(relativePath);
                    if (sprFallback != null)
                    {
                        s_SpriteCache[relativePath] = sprFallback;
                        return sprFallback;
                    }
                }

                if (allowSprFallback && is404) s_FailedSprites.Add(relativePath);
                if (logOnFail)
                    Console.WriteLine($"[LoadResource] ❌ load fail (.png & .spr fallback): {fullPath}");
                return null;
            }

            FakeSprite sprite = MakeIconSprite(imageData, relativePath);
            s_SpriteCache[relativePath] = sprite;
            return sprite;
        }

        private static FakeSprite TryLoadSpriteFromSpr(string relativePath)
        {
            string sprPath = Path.ChangeExtension(relativePath, ".spr");
            byte[] data = LoadBytesFromStreamingAssets(sprPath);
            if (data == null) return null;
            
            // Giả lập Decode SPR không dùng Unity Texture
            return new FakeSprite { RawData = data, Path = relativePath };
        }

        private static async Task<FakeSprite> TryLoadSpriteFromSprAsync(string relativePath, CancellationToken ct)
        {
            string sprPath = Path.ChangeExtension(relativePath, ".spr");
            byte[] data = await LoadBytesFromStreamingAssetsAsync(sprPath, ct);
            if (data == null) return null;
            
            return new FakeSprite { RawData = data, Path = relativePath };
        }

        public static FakeSprite GetCachedSprite(string relativePath)
        {
            relativePath = TryFixSpritePath(relativePath);
            if (string.IsNullOrEmpty(relativePath)) return null;
            return s_SpriteCache.TryGetValue(relativePath, out var s) && s != null ? s : null;
        }

        public static async Task<FakeSprite> LoadSpriteFromStreamingAssetsAsync(string relativePath, CancellationToken ct = default)
        {
            relativePath = TryFixSpritePath(relativePath);
            if (string.IsNullOrEmpty(relativePath)) return null;
            if (s_SpriteCache.TryGetValue(relativePath, out var cached) && cached != null)
                return cached;
            if (s_FailedSprites.Contains(relativePath)) return null;

            string fullPath = Path.Combine(GetUploadBaseUrl(), relativePath);
            try
            {
                var response = await s_HttpClient.GetAsync(fullPath, ct);
                if (response.IsSuccessStatusCode)
                {
                    byte[] imageData = await response.Content.ReadAsByteArrayAsync();
                    FakeSprite sprite = MakeIconSprite(imageData, relativePath);
                    s_SpriteCache[relativePath] = sprite;
                    return sprite;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    s_FailedSprites.Add(relativePath);
                }
            }
            catch
            {
                if (ct.IsCancellationRequested) return null;
                FakeSprite sprFallback = await TryLoadSpriteFromSprAsync(relativePath, ct);
                if (sprFallback != null)
                {
                    s_SpriteCache[relativePath] = sprFallback;
                    return sprFallback;
                }
            }
            return null;
        }

        public static string TryFixSpritePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            string fixedPath = path.Replace('\\', '/');
            if (fixedPath.Contains("Spr/")) fixedPath = fixedPath.Replace("Spr/", "spr/");
            if (fixedPath.Contains("/ui/")) fixedPath = fixedPath.Replace("/ui/", "/Ui/");
            if (fixedPath.Contains("/spr/")) fixedPath = fixedPath.Replace("/spr/", "spr/");
            return fixedPath;
        }

        private static readonly Dictionary<string, byte[]> s_SprBytesCache = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        public static byte[] LoadBytesFromStreamingAssets(string relativePath)
        {
            relativePath = TryFixSpritePath(relativePath);
            if (string.IsNullOrEmpty(relativePath)) return null;

            if (s_SprBytesCache.TryGetValue(relativePath, out var cachedBytes) && cachedBytes != null)
                return cachedBytes;

            byte[] data = null;
            string localPath = Path.Combine(StreamingAssetsPathLocal, relativePath);
            if (File.Exists(localPath))
            {
                data = File.ReadAllBytes(localPath);
            }
            else
            {
                string fullPath = Path.Combine(GetUploadBaseUrl(), relativePath);
                try
                {
                    data = s_HttpClient.GetByteArrayAsync(fullPath).GetAwaiter().GetResult();
                }
                catch
                {
                    return null;
                }
            }

            if (data != null) s_SprBytesCache[relativePath] = data;
            return data;
        }

        public static async Task<byte[]> LoadBytesFromStreamingAssetsAsync(string relativePath, CancellationToken ct = default)
        {
            relativePath = TryFixSpritePath(relativePath);
            if (string.IsNullOrEmpty(relativePath)) return null;

            if (s_SprBytesCache.TryGetValue(relativePath, out var cachedBytes) && cachedBytes != null)
                return cachedBytes;

            byte[] data = null;
            string localPath = Path.Combine(StreamingAssetsPathLocal, relativePath);
            if (File.Exists(localPath))
            {
                data = File.ReadAllBytes(localPath);
            }
            else
            {
                string fullPath = Path.Combine(GetUploadBaseUrl(), relativePath);
                try
                {
                    var response = await s_HttpClient.GetAsync(fullPath, ct);
                    if (response.IsSuccessStatusCode)
                        data = await response.Content.ReadAsByteArrayAsync();
                }
                catch
                {
                    return null;
                }
            }

            if (data != null) s_SprBytesCache[relativePath] = data;
            return data;
        }

        private static string TryCleanAudioPath(string path)
        {
            string cleanedPath = path.Replace('\\', '/');
            if (cleanedPath.StartsWith("/")) cleanedPath = cleanedPath.Substring(1);
            return cleanedPath;
        }

        public static FakeAudioClip LoadAudioClipFromStreamingAssets(string relativePath)
        {
            string cleanedPath = TryCleanAudioPath(relativePath);
            string fullPath = Path.Combine(StreamingAssetsPathLocal, cleanedPath);
            byte[] fileData = null;

            if (File.Exists(fullPath))
            {
                fileData = File.ReadAllBytes(fullPath);
            }
            else
            {
                // Thử tải từ CDN thay cho WebRequest Unity cũ
                string remotePath = Path.Combine(GetUploadBaseUrl(), cleanedPath);
                try
                {
                    fileData = s_HttpClient.GetByteArrayAsync(remotePath).GetAwaiter().GetResult();
                }
                catch
                {
                    Console.WriteLine($"[LoadResource] Không tìm thấy audio file: {fullPath} / {remotePath}");
                    return null;
                }
            }

            return new FakeAudioClip { RawData = fileData, Name = Path.GetFileNameWithoutExtension(fullPath) };
        }

        public static string NormalizeScriptKey(string scriptPath)
        {
            return scriptPath.Replace('\\', '/').Trim().ToLowerInvariant();
        }

        public static string ToResourcesPath(string scriptPath)
        {
            string s = scriptPath.Replace('\\', '/').Trim();
            if (s.StartsWith("/")) s = s.Substring(1);
            if (s.EndsWith(".lua")) s = s.Substring(0, s.Length - 4);
            return s;
        }

        public static void PreloadSprites(IEnumerable<string> paths)
        {
            if (paths == null) return;

            foreach (string path in paths)
            {
                if (string.IsNullOrEmpty(path)) continue;
                string fixedPath = TryFixSpritePath(path);
                if (string.IsNullOrEmpty(fixedPath) || s_SpriteCache.ContainsKey(fixedPath)) continue;

                FakeSprite sprite = TryLoadSpriteFromSpr(fixedPath);
                if (sprite != null) s_SpriteCache[fixedPath] = sprite;
            }
            Console.WriteLine($"[StreamAsset] Preloaded {s_SpriteCache.Count} sprites.");
        }
    }
}