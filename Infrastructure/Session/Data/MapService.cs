// using GameWorld;
// using Network.Header;
// using Network.Resource.Header;
// using UnityEngine;
//
// public static class MapService
// {
//     private static float m00 = -2.4064f;
//     private static float m01 = 3.584f;
//     private static float m10 = 3.8912f;
//     private static float m11 = 3.328f;
//     private static float offsetX = -225.52f;
//     private static float offsetZ = -1433.84f;
//     public const int defLOGIC_CELL_WIDTH = 32;
//     public const int defLOGIC_CELL_HEIGHT = 32;
//     public const int WEATHERID_NOTHING = 0;
//
//     // ── Local-player position trail (drift-vs-teleport discriminator) ──────────
//     // HandleSyncNpcMinPlayer's regionReload branch must tell prediction-lead DRIFT
//     // from a genuine TELEPORT. A fixed magnitude bound does NOT work: the server
//     // min-sync is effectively a STALE position broadcast that lags the client's
//     // predicted run by ~the broadcast interval × speed (observed ~2 regions, and it
//     // grows with speed) — so any bound just sets where the accumulated lag trips a
//     // snap, and a back-snap then forces a forward-snap when the player stops and the
//     // server catches up (the back↔forward rubber-band seen in the logs).
//     //
//     // The robust signal is geometric, not magnitude: the lagged server position lies
//     // ON the path the client actually ran (→ drift, trust the client, do NOT snap),
//     // whereas a teleport destination is OFF that recent path (→ snap). We keep a short
//     // trail of the local player's recent positions (sampled ~10 Hz from JXMovement)
//     // and, in the reload branch, skip the snap only when the server pos lies on a
//     // recent trail SEGMENT.
//     private const int TRAIL_CAP = 256;   // ring slots (≈25 s at 10 Hz)
//     private const float TRAIL_TTL_SEC = 16f;   // only segments newer than this count (covers observed ≤5.7s lag + margin)
//     private const float TRAIL_HIT_MPS = 320f;  // tolerance to the path (10 cells; observed client-vs-server route divergence around obstacles ~270-280)
//     private const float TRAIL_SAMPLE_INTERVAL = 0.1f;     // 10 Hz
//     private const int TRAIL_MIN_STEP_MPS = 16;       // skip near-duplicate samples
//     private static readonly int[] _trailX = new int[TRAIL_CAP];
//     private static readonly int[] _trailY = new int[TRAIL_CAP];
//     private static readonly float[] _trailT = new float[TRAIL_CAP];
//     private static int _trailHead = 0;   // next write slot
//     private static int _trailCount = 0;
//     private static float _trailNextSampleT = 0f;
//     private static int _trailLastX = int.MinValue, _trailLastY = int.MinValue;
//     private static float _invDet = 0f;
//     private static float _mpsWorldScale = 0f;
//     public static SubWorld CurrentSubworld = new SubWorld();
//
//     /// <summary>
//     /// Lazily initialise (or re-initialise after Calibrate) the cached inverse-determinant.
//     /// </summary>
//     private static void EnsureInvDet()
//     {
//         float det = m00 * m11 - m01 * m10;
//         _invDet = (det != 0f) ? 1f / det : 0f;
//     }
//
//     /// <summary>
//     /// Average Unity world-units travelled per 1 map-unit (MPS) of movement.
//     ///
//     /// The MPS grid is now symmetric (1024×1024 region, 32×32 cells), so the
//     /// MPS→world transform is near-isotropic: the two principal per-MPS world
//     /// lengths |(m00,m10)|/512 and |(m01,m11)|/512 are ≈ equal (≈0.0092 wu each).
//     /// Their geometric mean is the single scalar used by server-speed→client-speed
//     /// calibration; it is now an accurate (not just diagonal-best) estimate.
//     ///
//     /// Derived live from the calibration matrix so it auto-tracks Calibrate().
//     /// </summary>
//     public static float MpsWorldScale
//     {
//         get
//         {
//             if (_mpsWorldScale == 0f)
//             {
//                 float lenX = (float)(Math.Sqrt(m00 * m00 + m10 * m10) / 512f); // world u per map-X unit
//                 float lenY = (float)(Math.Sqrt(m01 * m01 + m11 * m11) / 512f); // world u per map-Y unit
//                 _mpsWorldScale = (float)(Math.Sqrt(lenX * lenY));
//             }
//             return _mpsWorldScale;
//         }
//     }
//
//     // ── Server-speed → client-speed scale ────────────────────────────────────────
//     // ServerSpeedToWorld uses MpsWorldScaleForSpeed = MpsWorldScale × SPEED_SCALE_MULT,
//     // kept separate from the range/visual scalar (UNITY_SCALE_FACTOR = MpsWorldScale).
//     //
//     // The 1024/32 symmetrization dropped MpsWorldScale by 1/√2 (0.0131→0.0092, X axis), so
//     // at face value entities move ~29% slower in world. The SERVER restores the old pace by
//     // silently multiplying its movement integration by √2 at RUNTIME, while still TRANSMITTING
//     // the unchanged m_CurrentRunSpeed (it does NOT bake √2 into the stat). So the client must
//     // MIRROR that hidden √2 here → 1.414f, not 1.0. Both sides apply √2 independently and the
//     // transmitted stat carries none, so this is a MIRROR, not a double-application — setting 1.0
//     // would leave the client √2 slower than the server (it lags / gets dragged back).
//     // (A [SPDCAL] probe, since removed, measured the pre-change scale = MpsWorldScale; the √2
//     // is the runtime speed-up both sides now add on top.)
//     public static float SPEED_SCALE_MULT = 1.414f;   // mirrors the server's silent runtime ×√2
//
//     /// <summary>
//     /// World-units per MPS for SERVER-SPEED → client-speed conversion
//     /// (<see cref="SkillService.ServerSpeedToWorld"/>) = MpsWorldScale × SPEED_SCALE_MULT.
//     /// A separate knob from <see cref="MpsWorldScale"/> (the range/visual scalar): client
//     /// movement must match the SERVER's authoritative pace. Currently ×1.0 (see comment above).
//     /// </summary>
//     public static float MpsWorldScaleForSpeed => MpsWorldScale * SPEED_SCALE_MULT;
//
//
//     // ── MPS-metric helpers ───────────────────────────────────────────────
//     // Server check tầm bằng Euclid trên toạ độ MPS (GetDistance). Sau khi đổi lưới
//     // sang đối xứng (1024×1024, 32×32 cell), ma trận MPS→world gần như đẳng hướng
//     // (map-X ≈ map-Y ≈ 0.0092 wu/unit) → vòng tròn MPS ra gần tròn trong world.
//     // Vẫn nên đo range gửi server bằng metric MPS qua các helper dưới đây để chính
//     // xác tuyệt đối (sai số trục còn ~4% do norm hai cột ma trận chưa bằng hệt nhau).
//
//     /// <summary>Đổi world-delta (mặt phẳng XZ) sang MPS-delta dạng float (không floor).</summary>
//     public static Vector2 WorldDeltaToMpsDelta(Vector3 worldDelta)
//     {
//         if (_invDet == 0f) EnsureInvDet();
//         float gx = (m11 * worldDelta.x - m01 * worldDelta.z) * _invDet;
//         float gy = (-m10 * worldDelta.x + m00 * worldDelta.z) * _invDet;
//         return new Vector2(gx * 512f, gy * 512f);
//     }
//
//     /// <summary>Độ dài của một world-delta đo theo metric MPS (metric server check tầm).</summary>
//     public static float WorldDeltaToMpsDistance(Vector3 worldDelta)
//         => WorldDeltaToMpsDelta(worldDelta).magnitude;
//
//     /// <summary>
//     /// Bán kính world THEO HƯỚNG worldDir sao cho khoảng cách MPS đúng bằng mpsRange.
//     /// Tầm thật trong world space là ellipse — không tồn tại một bán kính world chung
//     /// cho mọi hướng.
//     /// </summary>
//     public static float WorldRadiusForMpsRange(Vector3 worldDir, float mpsRange)
//     {
//         worldDir.y = 0f;
//         float worldLen = worldDir.magnitude;
//         if (worldLen < 1e-6f) return 0f;
//         float mpsPerWu = WorldDeltaToMpsDistance(worldDir / worldLen);
//         return mpsPerWu > 1e-6f ? mpsRange / mpsPerWu : 0f;
//     }
//
//     /// <summary>
//     /// Kẹp target world sao cho khoảng cách MPS từ origin ≤ maxMpsRange.
//     /// Giữ nguyên hướng; chỉ rút ngắn khi vượt tầm theo metric server.
//     /// </summary>
//     public static Vector3 ClampWorldTargetToMpsRange(Vector3 origin, Vector3 target, float maxMpsRange)
//     {
//         Vector3 delta = target - origin;
//         float mpsDist = WorldDeltaToMpsDistance(delta);
//         if (mpsDist <= maxMpsRange || mpsDist <= 1e-6f) return target;
//         return origin + delta * (maxMpsRange / mpsDist);
//     }
//
//     // Ellipse "vòng tròn MPS" trong world space — cache theo ma trận calibrate.
//     private static float _ellipseUnitMajor = 0f;   // world-u per 1 MPS unit, trục dài
//     private static float _ellipseUnitMinor = 0f;   // world-u per 1 MPS unit, trục ngắn
//     private static float _ellipseYawDeg = 0f;      // Euler-Y đưa local +X về trục dài
//
//     private static void EnsureEllipseParams()
//     {
//         if (_ellipseUnitMajor != 0f) return;
//         // A = đạo hàm MPS→world; ellipse = ảnh của vòng tròn MPS đơn vị qua A.
//         float a11 = m00 / 512f, a12 = m01 / 512f;
//         float a21 = m10 / 512f, a22 = m11 / 512f;
//         // B = A·Aᵀ: eigenvector = trục ellipse trong world, sqrt(eigenvalue) = semi-axis.
//         float b00 = a11 * a11 + a12 * a12;
//         float b01 = a11 * a21 + a12 * a22;
//         float b11 = a21 * a21 + a22 * a22;
//         float half = (b00 + b11) * 0.5f;
//         float disc = Mathf.Sqrt(Mathf.Max(0f, half * half - (b00 * b11 - b01 * b01)));
//         _ellipseUnitMajor = Mathf.Sqrt(Mathf.Max(0f, half + disc));
//         _ellipseUnitMinor = Mathf.Sqrt(Mathf.Max(0f, half - disc));
//         float phi = 0.5f * Mathf.Atan2(2f * b01, b00 - b11);   // hướng trục dài trong (x,z)
//         _ellipseYawDeg = -phi * Mathf.Rad2Deg;                 // Euler-Y quay +X tới (cosφ, sinφ)
//     }
//
//     /// <summary>
//     /// Ellipse world tương ứng tầm mpsRange: bán trục dài/ngắn (world-units) và góc
//     /// quay quanh Y (độ, world space) để trục dài nằm theo localScale-X của visual.
//     /// </summary>
//     public static void GetWorldEllipseForMpsRange(float mpsRange, out float radiusMajor, out float radiusMinor, out float yawDeg)
//     {
//         EnsureEllipseParams();
//         radiusMajor = _ellipseUnitMajor * mpsRange;
//         radiusMinor = _ellipseUnitMinor * mpsRange;
//         yawDeg = _ellipseYawDeg;
//     }
//
//     public static Vector3 Mps2WorldPos(uint mapX, uint mapY, float worldY = 0f)
//     {
//         float gx = mapX / 512f;
//         float gy = mapY / 512f;
//         float worldX = m00 * gx + m01 * gy + offsetX;
//         float worldZ = m10 * gx + m11 * gy + offsetZ;
//
//         return new Vector3(worldX, worldY, worldZ);
//     }
//     public static Vector3 Map2WorldPos(int regionIndex, uint mapX, uint mapY, int offX, int offY, float worldY = 0f)
//     {
//         CurrentSubworld.Map2Mps(regionIndex, (int)mapX, (int)mapY, offX, offY, out int nMapX, out int nMapY);
//         //Debug.Log($"[MapService] Map2WorldPos: RegionIndex: {regionIndex}, MapX: {mapX}, MapY: {mapY}, OffX: {offX}, OffY: {offY} => MpsX: {nMapX}, MpsY: {nMapY}");
//         return Mps2WorldPos((uint)nMapX, (uint)nMapY, worldY);
//     }
//     public static Vector2Int WorldPos2Mps(Vector3 worldPos)
//     {
//         float gx = (m11 * (worldPos.x - offsetX) - m01 * (worldPos.z - offsetZ)) /
//                     (m00 * m11 - m01 * m10);
//         float gy = (-m10 * (worldPos.x - offsetX) + m00 * (worldPos.z - offsetZ)) /
//                     (m00 * m11 - m01 * m10);
//
//         int mapX = Mathf.FloorToInt(gx * 512f);
//         int mapY = Mathf.FloorToInt(gy * 512f);
//
//         return new Vector2Int(mapX, mapY);
//     }
//
//     public static void WorldPos2GridCell(Vector3 worldPos, out int gridX, out int gridY)
//     {
//         // Lazy-init the cached inverse denominator
//         if (_invDet == 0f) EnsureInvDet();
//
//         float dx = worldPos.x - offsetX;
//         float dz = worldPos.z - offsetZ;
//
//         // Inverse-matrix rows:
//         //   gx =  (m11 * dx - m01 * dz) * invDet
//         //   gy =  (-m10 * dx + m00 * dz) * invDet
//         float gx = (m11 * dx - m01 * dz) * _invDet;
//         float gy = (-m10 * dx + m00 * dz) * _invDet;
//
//         // mpsX = FloorToInt(gx * 512),  gridX = mpsX / 32 = FloorToInt(gx * 16)
//         // mpsY = FloorToInt(gy * 512),  gridY = mpsY / 32 = FloorToInt(gy * 16)
//         gridX = Mathf.FloorToInt(gx * 16f);
//         gridY = Mathf.FloorToInt(gy * 16f);
//     }
//
//     public static Vector3 DirToRotation(int dir)
//     {
//         //server dir is 0-64 (anticlockwise), unity rotation is 0-360 (clockwise)
//         if (dir < 0 || dir >= 64) dir = 0;
//         float rotationY = (360f - (dir * (360f / 64f))) % 360f;
//         return new Vector3(0, rotationY, 0);
//     }
//
//     public static int RotationToDir(float rotationY)
//     {
//         //unity rotation is 0-360 (clockwise), server dir is 0-64 (anticlockwise)
//         rotationY = rotationY % 360f;
//         if (rotationY < 0) rotationY += 360f;
//         int dir = Mathf.FloorToInt((360f - rotationY) / (360f / 64f)) % 64;
//         return dir;
//     }
//
//     public static int GetDirIndex(int nDx, int nDy)
//     {
//         int nRet = -1;
//
//         if (nDx == 0 && nDy == 0)
//             return -1;
//
//         int nDistance = GetLength(nDx, nDy);
//
//         if (nDistance == 0) return -1;
//
//         int nSin = (nDy << 10) / nDistance;
//
//
//         for (int i = 0; i < 32; i++)
//         {
//             if (nSin > g_nSin[i])
//                 break;
//             nRet = i;
//         }
//
//         int nD1, nD2;
//         if (g_nSin[nRet] != nSin)
//         {
//             nD1 = g_nSin[nRet] - nSin;
//             nD2 = nSin - g_nSin[nRet + 1];
//             if (nD1 > nD2)
//                 nRet++;
//         }
//
//         if (nDx >= 0 && nRet != 0)
//         {
//             nRet = 64 - nRet;
//         }
//         return nRet;
//     }
//
//     public static int GetDirIndex(int nX1, int nY1, int nX2, int nY2)
//     {
//         int nRet = -1;
//
//         if (nX1 == nX2 && nY1 == nY2)
//             return -1;
//
//         int nDistance = GetDistance(nX1, nY1, nX2, nY2);
//
//         if (nDistance == 0) return -1;
//
//         int nYLength = nY2 - nY1;
//         int nSin = (nYLength << 10) / nDistance;	// multiply by 1024
//
//         //find more than me as my dir
//         for (int i = 0; i < 32; i++)
//         {
//             if (nSin > g_nSin[i])
//                 break;
//             nRet = i;
//         }
//
//         // Handle edge case: if nSin > g_nSin[0] (1024), nRet stays -1
//         // This means the angle is beyond the measurable range, clamp to 0
//         if (nRet == -1)
//         {
//             nRet = 0;
//         }
//
//         int nD1, nD2;
//         if (g_nSin[nRet] != nSin)
//         {
//             // Make sure we don't go out of bounds when accessing nRet + 1
//             if (nRet < 31)
//             {
//                 nD1 = g_nSin[nRet] - nSin;
//                 nD2 = nSin - g_nSin[nRet + 1];
//                 if (nD1 > nD2)
//                     nRet++;
//             }
//             else if (nRet == 31)
//             {
//                 // Special case: nRet is at the boundary (index 31)
//                 // Check if we should round to index 32
//                 nD1 = g_nSin[31] - nSin;
//                 nD2 = nSin - g_nSin[32];
//                 if (nD1 > nD2)
//                     nRet = 32;
//             }
//         }
//
//         if ((nX2 - nX1) >= 0 && nRet != 0)
//         {
//             nRet = 64 - nRet;
//         }
//
//         //Debug.Log("[MapService] GetDirIndex: From (" + nX1 + ", " + nY1 + ") to (" + nX2 + ", " + nY2 + ") => DirIndex: " + nRet);
//         return nRet;
//     }
//
//     public static int DirIndex2Dir(int nDir, int nMaxDir)
//     {
//         int nRet = -1;
//
//         if (nMaxDir <= 0)
//             return nRet;
//
//         nRet = (nMaxDir * nDir) >> 6;	// (nMaxDir / 64) * nDir
//         //Debug.Log("[MapService] DirIndex2Dir: DirIndex: " + nDir + ", MaxDir: " + nMaxDir + " => Dir: " + nRet);
//         return nRet;
//     }
//
//     public static int Dir2DirIndex(int nDir, int nMaxDir)
//     {
//         int nRet = -1;
//
//         if (nMaxDir <= 0)
//             return nRet;
//
//         nRet = (nDir << 6) / nMaxDir;
//         return nRet;
//     }
//
//     public static int GetLength(int nDx, int Dy)
//     {
//         return (int)Mathf.Sqrt((float)(nDx * nDx + Dy * Dy));
//     }
//
//     /// <summary>
//     /// Transform direction from MPS space to Unity world space (X component)
//     /// </summary>
//     public static float TransformDirectionX(float dirMpsX, float dirMpsY)
//     {
//         return (m00 / 512f) * dirMpsX + (m01 / 512f) * dirMpsY;
//     }
//
//     /// <summary>
//     /// Transform direction from MPS space to Unity world space (Z component)
//     /// </summary>
//     public static float TransformDirectionZ(float dirMpsX, float dirMpsY)
//     {
//         return (m10 / 512f) * dirMpsX + (m11 / 512f) * dirMpsY;
//     }
//
//     public static Vector3 TransformDirection(float dirMpsX, float dirMpsY)
//     {
//         float worldX = TransformDirectionX(dirMpsX, dirMpsY);
//         float worldZ = TransformDirectionZ(dirMpsX, dirMpsY);
//         return new Vector3(worldX, 0f, worldZ);
//     }
//
//     public static int InternalDirSinCos(int[] pSinCosTable, int nDir, int nMaxDir)
//     {
//         if (nDir < 0 || nDir >= nMaxDir)
//             return -1;
//
//         int nIndex = (nDir << 6) / nMaxDir;
//
//         return pSinCosTable[nIndex];
//     }
//
//     public static int DirCos(int nDir, int nMaxDir)
//     {
//         return InternalDirSinCos(g_nCos, nDir, nMaxDir);
//     }
//
//     public static int DirSin(int nDir, int nMaxDir)
//     {
//         return InternalDirSinCos(g_nSin, nDir, nMaxDir);
//     }
//
//     public static int GetDistance(int nX1, int nY1, int nX2, int nY2)
//     {
//         //Debug.Log("[MapService] GetDistance: From (" + nX1 + ", " + nY1 + ") to (" + nX2 + ", " + nY2 + ")");
//         return (int)Mathf.Sqrt((nX1 - nX2) * (nX1 - nX2) + (nY1 - nY2) * (nY1 - nY2));
//     }
//
//     public static void UpdateWorldInfo(WORLD_SYNC data)
//     {
//         CurrentSubworld.SubWorldID = data.SubWorld;
//         CurrentSubworld.SubWorldName = data.GetName();
//         ResetPlayerTrail();
//         GameManager.Instance.ForceCancelPathFinding();
//         CurrentSubworld.LoadMap(data.MapCopyID, data.SubWorld, data.Region, 0, 0);
//         //CurrentSubworld.CurrentTime = data.Frame;
//         CurrentSubworld.Weather = data.Weather;
//         CurrentSubworld.Activate();
//
//         var localId = PlayerInfoService.Instance.CurrentPlayerID;
//         var localInfo = NPCInfoService.Instance.GetNPCInfo((uint)localId);
//         if (localInfo.IsValid())
//         {
//             localInfo.m_RegionIndex = -1;
//             localInfo.m_dwRegionID = 0;
//             NPCInfoService.Instance.UpdatePlayerSync(localInfo);
//         }
//     }
//
//     /// <summary>
//     /// Handle player-specific minimal sync (NPC_PLAYER_TYPE_NORMAL_SYNC)
//     /// Call this from your OnSyncNpcMinPlayer method
//     /// </summary>
//     /// <summary>
//     /// Records the local player's current world position into the recent-position trail.
//     /// Called ~every frame from JXMovement.Update (local player only); self-throttled to
//     /// TRAIL_SAMPLE_INTERVAL and de-duplicated while standing. Used by the regionReload
//     /// drift guard in <see cref="HandleSyncNpcMinPlayer"/>.
//     /// </summary>
//     /// <summary>Clears the recent-position trail (e.g. on a subworld change / teleport into a new MPS space).</summary>
//     public static void ResetPlayerTrail()
//     {
//         _trailHead = 0;
//         _trailCount = 0;
//         _trailNextSampleT = 0f;
//         _trailLastX = int.MinValue;
//         _trailLastY = int.MinValue;
//     }
//
//     public static void SampleLocalPlayerTrail(Vector3 worldPos)
//     {
//         float now = Time.realtimeSinceStartup;
//         if (now < _trailNextSampleT) return;
//         _trailNextSampleT = now + TRAIL_SAMPLE_INTERVAL;
//
//         Vector2Int mps = WorldPos2Mps(worldPos);
//         if (_trailLastX != int.MinValue)
//         {
//             long dx = mps.x - _trailLastX, dy = mps.y - _trailLastY;
//             if (dx * dx + dy * dy < TRAIL_MIN_STEP_MPS * TRAIL_MIN_STEP_MPS) return; // standing — skip dup
//         }
//
//         _trailX[_trailHead] = mps.x;
//         _trailY[_trailHead] = mps.y;
//         _trailT[_trailHead] = now;
//         _trailLastX = mps.x;
//         _trailLastY = mps.y;
//         _trailHead = (_trailHead + 1) % TRAIL_CAP;
//         if (_trailCount < TRAIL_CAP) _trailCount++;
//     }
//
//     /// <summary>
//     /// Nearest distance (MPS) from (mpsX,mpsY) to the local player's recent run-path SEGMENTS
//     /// within TRAIL_TTL_SEC. Distinguishes a lagged server min-sync (lands ON the path the
//     /// client ran → small distance → prediction drift) from a teleport (OFF the recent path →
//     /// large distance). Segment-based so it tolerates the gaps between samples and turns.
//     /// Also reports <paramref name="coverageSec"/> (seconds of history actually walked — small
//     /// right after a reset/spawn) and <paramref name="samples"/>, for diagnostics/decisions.
//     /// </summary>
//     private static float NearestTrailDistMps(int mpsX, int mpsY, out float coverageSec, out int samples)
//     {
//         coverageSec = 0f;
//         samples = _trailCount;
//         if (_trailCount == 0) return float.MaxValue;
//
//         float now = Time.realtimeSinceStartup;
//         int newest = (_trailHead - 1 + TRAIL_CAP) % TRAIL_CAP;
//
//         float bestSqr = float.MaxValue;
//         if (now - _trailT[newest] <= TRAIL_TTL_SEC)
//         {
//             float ddx = mpsX - _trailX[newest], ddy = mpsY - _trailY[newest];
//             bestSqr = ddx * ddx + ddy * ddy;
//         }
//
//         // Walk segments newest→oldest until they fall outside the TTL window.
//         int prev = newest;
//         for (int k = 1; k < _trailCount; k++)
//         {
//             int cur = (newest - k + TRAIL_CAP) % TRAIL_CAP;
//             if (now - _trailT[cur] > TRAIL_TTL_SEC) break;
//             float d = SegDistSqr(mpsX, mpsY, _trailX[prev], _trailY[prev], _trailX[cur], _trailY[cur]);
//             if (d < bestSqr) bestSqr = d;
//             coverageSec = now - _trailT[cur]; // age of the oldest in-window sample reached
//             prev = cur;
//         }
//
//         return bestSqr == float.MaxValue ? float.MaxValue : Mathf.Sqrt(bestSqr);
//     }
//
//     /// <summary>Squared distance from point P to segment AB (2D, MPS units).</summary>
//     private static float SegDistSqr(float px, float py, float ax, float ay, float bx, float by)
//     {
//         float abx = bx - ax, aby = by - ay;
//         float ab2 = abx * abx + aby * aby;
//         float t = ab2 > 1e-3f ? ((px - ax) * abx + (py - ay) * aby) / ab2 : 0f;
//         t = Mathf.Clamp01(t);
//         float cx = ax + t * abx, cy = ay + t * aby;
//         float dx = px - cx, dy = py - cy;
//         return dx * dx + dy * dy;
//     }
//
//     public static void HandleSyncNpcMinPlayer(NPC_PLAYER_TYPE_NORMAL_SYNC data)
//     {
//         if (!PlayerInfoService.Instance.IsCurrentPlayer(data.m_dwNpcID)) return;
//
//         int nRegion, nMapX, nMapY, nOffX, nOffY;
//         //SubWorld[0].Mps2Map(data.m_dwMapX, data.m_dwMapY, &nRegion, &nMapX, &nMapY, &nOffX, &nOffY);
//         CurrentSubworld.Mps2Map((int)data.m_dwMapX, (int)data.m_dwMapY, out nRegion, out nMapX, out nMapY, out nOffX, out nOffY);
//         var npcInfo = NPCInfoService.Instance.GetNPCInfo(data.m_dwNpcID);
//         if (!npcInfo.IsValid()) return;
//         if (npcInfo.m_RegionIndex == -1)
//         {
//             if (nRegion < 0) return;
//             npcInfo.m_RegionIndex = nRegion;
//             npcInfo.m_dwRegionID = (uint)CurrentSubworld.Regions[nRegion].RegionID;
//             CurrentSubworld.NpcChangeRegion(-1, CurrentSubworld.Regions[nRegion].RegionID, (int)data.m_dwNpcID);
//             CurrentSubworld.Regions[nRegion].AddRef(nMapX, nMapY, MoveObjKind.Npc);
//
//             npcInfo.m_MapX = nMapX;
//             npcInfo.m_MapY = nMapY;
//             npcInfo.m_OffX = data.m_wOffX;
//             npcInfo.m_OffY = data.m_wOffY;
//             NPCInfoService.Instance.UpdatePlayerSync(npcInfo);
//             NPCInfoService.DebugLogPosSnap("HandleSyncNpcMinPlayer:regionChange", data.m_dwNpcID,
//                 GameManager.MainPlayerCharacter.transform.position, Map2WorldPos(nRegion, (uint)nMapX, (uint)nMapY, data.m_wOffX, data.m_wOffY),
//                 (int)data.m_dwMapX, (int)data.m_dwMapY);
//             return;
//         }
//
//         if (nRegion == -1)
//         {
//             // ── Prediction-lead drift guard (local player; handler is current-player-only) ──
//             // The server min-sync is effectively a STALE position broadcast that lags the
//             // client's predicted run. The client already recenters its 3x3 window on its OWN
//             // position as it moves (NPC.MovePos → NpcChangeRegion → LoadMap), so the lagged
//             // server pos lands outside the window → Mps2Map == -1 and we reach this branch.
//             // Re-centering the window backward onto that stale pos and snapping the transform
//             // there is the visible "drag-back while running" bug (and it then forces a forward
//             // re-snap when the player stops and the server catches up).
//             //
//             // We must NOT blanket-skip: a genuine far teleport (gate/recall/trap, SAME subworld)
//             // also lands here — SetPos no-ops when its destination region isn't loaded
//             // (NPCInfoService.SetPos, the `nRegion < 0` early-return), so this reload branch is
//             // the ONLY code that loads a far destination region and moves the player there.
//             //
//             // Magnitude can't discriminate them (the lag grows with speed/broadcast interval),
//             // so use geometry: the lagged server pos lies ON the path the client actually ran
//             // (→ drift, skip), whereas a teleport destination is OFF that recent path (→ snap).
//             // Guarded by clientRegion>=0 so a true client-side desync (own cell unloaded) still
//             // falls through to the recovery snap.
//             Vector2Int clientMps = WorldPos2Mps(GameManager.MainPlayerCharacter.transform.position);
//             CurrentSubworld.Mps2Map(clientMps.x, clientMps.y,
//                 out int clientRegion, out _, out _, out _, out _);
//             float nearestMps = NearestTrailDistMps((int)data.m_dwMapX, (int)data.m_dwMapY,
//                 out float trailCovSec, out int trailN);
//             if (clientRegion >= 0 && nearestMps <= TRAIL_HIT_MPS)
//             {
//                 NPCInfoService.DebugLogPosSnap(
//                     $"HandleSyncNpcMinPlayer:regionReload-DRIFT-SKIP(near={nearestMps:F0} cov={trailCovSec:F1}s n={trailN})",
//                     data.m_dwNpcID,
//                     GameManager.MainPlayerCharacter.transform.position,
//                     GameManager.MainPlayerCharacter.transform.position, // no snap — trust client
//                     (int)data.m_dwMapX, (int)data.m_dwMapY);
//                 return; // prediction-lead drift — server pos is on the client's own trail
//             }
//
//             CurrentSubworld.Regions[npcInfo.m_RegionIndex].DecRef(npcInfo.m_MapX, npcInfo.m_MapY, MoveObjKind.Npc);
//
//             int nRegionX = (int)data.m_dwMapX / (KRegion.SCENE_LOGIC_CELL_WIDTH * KRegion.SCENE_REGION_CELL_NUM_H);
//             int nRegionY = (int)data.m_dwMapY / (KRegion.SCENE_LOGIC_CELL_HEIGHT * KRegion.SCENE_REGION_CELL_NUM_V);
//
//             int dwRegionID = NpcManager.MAKELONG(nRegionX, (short)nRegionY);
//             CurrentSubworld.LoadMap(CurrentSubworld.MapTemplateID, CurrentSubworld.SubWorldID, dwRegionID, (int)data.m_dwMapX, (int)data.m_dwMapY);
//
//             nRegion = CurrentSubworld.FindRegion(dwRegionID);
//             // GUARD: if the destination region still isn't loaded (LoadMap couldn't place it),
//             // every line below would index Regions[-1] (RegionID / AddRef / Map2WorldPos) and
//             // throw — the crash seen at far teleports / city-gate traps. Bail gracefully; the
//             // next min-sync re-attempts once the region is loadable. m_RegionIndex is left at its
//             // pre-reload value (the DecRef above is a harmless off-by-one on a region we are
//             // leaving), so the player is not stranded in a -1 region.
//             if (nRegion < 0)
//             {
//                 Debug.LogWarning($"[MapService] HandleSyncNpcMinPlayer: destination region {dwRegionID} " +
//                     $"not loaded after LoadMap (mps {data.m_dwMapX},{data.m_dwMapY}); skipping snap this tick.");
//                 return;
//             }
//             npcInfo.m_RegionIndex = nRegion;
//             npcInfo.m_dwRegionID = (uint)dwRegionID;
//             CurrentSubworld.NpcChangeRegion(-1, CurrentSubworld.Regions[nRegion].RegionID, (int)data.m_dwNpcID);
//
//             CurrentSubworld.Mps2Map((int)data.m_dwMapX, (int)data.m_dwMapY, out nRegion, out nMapX, out nMapY, out nOffX, out nOffY);
//             npcInfo.m_MapX = nMapX;
//             npcInfo.m_MapY = nMapY;
//             npcInfo.m_OffX = data.m_wOffX;
//             npcInfo.m_OffY = data.m_wOffY;
//
//             CurrentSubworld.Regions[npcInfo.m_RegionIndex].AddRef(npcInfo.m_MapX, npcInfo.m_MapY, MoveObjKind.Npc);
//
//             //npcInfo.m_SyncSignal = (int)CurrentSubworld.CurrentTime;
//             //Debug.Log($"[REVIVE-TRACE] Updating sync for NPC {npcInfo.m_dwID} in region {nRegion} with MPS({nMapX},{nMapY}) and off({npcInfo.m_OffX},{npcInfo.m_OffY})");
//             var worldPos = Map2WorldPos(nRegion, (uint)nMapX, (uint)nMapY, npcInfo.m_OffX, npcInfo.m_OffY);
//             // 🔎 [PosSyncDbg] region-reload snap of the local player (see NPCInfoService).
//             // near = nearest distance (MPS) the server pos came to the client's recent path;
//             // cov = seconds of trail history walked (small ⇒ startup/post-reset, trail too young).
//             NPCInfoService.DebugLogPosSnap(
//                 $"HandleSyncNpcMinPlayer:regionReload-SNAP(near={nearestMps:F0} cov={trailCovSec:F1}s n={trailN})",
//                 data.m_dwNpcID,
//                 GameManager.MainPlayerCharacter.transform.position, worldPos,
//                 (int)data.m_dwMapX, (int)data.m_dwMapY);
//             GameManager.MainPlayerCharacter.transform.position = worldPos;
//             NPCInfoService.Instance.UpdatePlayerSync(npcInfo);
//
//             var mpMove = GameManager.MainPlayerCharacter != null ? GameManager.MainPlayerCharacter.JXMovement : null;
//             if (mpMove != null && (mpMove.IsInPathfinding || mpMove.IsComputingPath))
//                 mpMove.ResumePfFromCurrentPos(CurrentSubworld.SubWorldID);
//
//             // NOTE: do NOT ResetPlayerTrail() here. Clearing the trail after a snap leaves it
//             // too young to cover the lag on the very next sync (cov≈0), which false-snaps again
//             // — an observed cascade. The pre-snap trail is still valid path history (the snap
//             // moved the player onto a cell that path already covers), so keep it.
//
//             return;
//         }
//
//
//         // BYTE	byBarrier = SubWorld[0].m_Region[npcInfo.m_RegionIndex].GetBarrier(npcInfo.m_MapX, npcInfo.m_MapY, npcInfo.m_OffX, npcInfo.m_OffY);
//         // if (0 != byBarrier && Obstacle_JumpFly != byBarrier)
//         // {
//         //     g_DebugLog("[Barrier]Player in Barrier");
//         // }
//     }
//
//     /// <summary>
//     /// Remove NPC from region system
//     /// Call this when NPC is deleted/removed
//     /// </summary>
//     public static void RemoveNpcFromRegion(uint npcId)
//     {
//         var npc = NPCInfoService.Instance.GetNPCInfo(npcId);
//         if (!npc.IsValid())
//             return;
//
//         if (npc.m_RegionIndex >= 0 && npc.m_RegionIndex < CurrentSubworld.TotalRegion)
//         {
//             CurrentSubworld.Regions[npc.m_RegionIndex].RemoveNpc((int)npcId);
//             CurrentSubworld.Regions[npc.m_RegionIndex].DecRef(npc.m_MapX, npc.m_MapY, MoveObjKind.Npc);
//         }
//
//         npc.m_RegionIndex = -1;
//     }
//
//     // Helper methods
//     private static bool IsMovementCommand(byte cmdKind)
//     {
//         return cmdKind == (byte)NPCCMD.do_run ||
//                cmdKind == (byte)NPCCMD.do_walk ||
//                cmdKind == (byte)NPCCMD.do_blurmove;
//     }
//
//     public static void Calibrate(
//         Vector2 serverA, Vector2 unityA,
//         Vector2 serverB, Vector2 unityB,
//         Vector2 serverC, Vector2 unityC)
//     {
//         // Convert server pixel coordinates into logical map-grid coordinates
//         Vector2 A = new Vector2(serverA.x / 512f, serverA.y / 512f);
//         Vector2 B = new Vector2(serverB.x / 512f, serverB.y / 512f);
//         Vector2 C = new Vector2(serverC.x / 512f, serverC.y / 512f);
//
//         // Solve for matrix values for worldX
//         Solve3x3(
//             A.x, A.y, 1f, unityA.x,
//             B.x, B.y, 1f, unityB.x,
//             C.x, C.y, 1f, unityC.x,
//             out m00, out m01, out offsetX
//         );
//
//         // Solve for matrix values for worldZ
//         Solve3x3(
//             A.x, A.y, 1f, unityA.y,
//             B.x, B.y, 1f, unityB.y,
//             C.x, C.y, 1f, unityC.y,
//             out m10, out m11, out offsetZ
//         );
//
//         // Recalibration changed the matrix — drop cached derived values so they
//         // re-compute on next access.
//         _invDet = 0f;
//         _mpsWorldScale = 0f;
//         _ellipseUnitMajor = 0f;
//
//         Debug.Log($"Calibrated:\n" +
//                   $" Matrix:\n" +
//                   $"   [{m00:F6}  {m01:F6}]\n" +
//                   $"   [{m10:F6}  {m11:F6}]\n" +
//                   $" Offset: ({offsetX:F6}, {offsetZ:F6})");
//     }
//
//     /// <summary>
//     /// Solve a 3×3 linear system using Cramer's rule.
//     /// </summary>
//     private static void Solve3x3(
//         float A1, float B1, float C1, float D1,
//         float A2, float B2, float C2, float D2,
//         float A3, float B3, float C3, float D3,
//         out float x, out float y, out float z)
//     {
//         float det =
//             A1 * (B2 * C3 - B3 * C2) -
//             B1 * (A2 * C3 - A3 * C2) +
//             C1 * (A2 * B3 - A3 * B2);
//
//         if (Mathf.Abs(det) < 1e-6f)
//         {
//             Debug.LogError("Cannot solve 3x3 system (degenerate).");
//             x = y = z = 0f;
//             return;
//         }
//
//         float detX =
//             D1 * (B2 * C3 - B3 * C2) -
//             B1 * (D2 * C3 - D3 * C2) +
//             C1 * (D2 * B3 - D3 * B2);
//
//         float detY =
//             A1 * (D2 * C3 - D3 * C2) -
//             D1 * (A2 * C3 - A3 * C2) +
//             C1 * (A2 * D3 - A3 * D2);
//
//         float detZ =
//             A1 * (B2 * D3 - B3 * D2) -
//             B1 * (A2 * D3 - A3 * D2) +
//             D1 * (A2 * B3 - A3 * B2);
//
//         x = detX / det;
//         y = detY / det;
//         z = detZ / det;
//     }
//
//     private static int[] g_nSinBuffer =
//     {
//         1024,   1019,   1004,   979,    946,    903,    851,    791,
//         724,    649,    568,    482,    391,    297,    199,    100,
//         0,     -100,    -199,   -297,   -391,   -482,   -568,   -649,
//         -724,   -791,   -851,   -903,   -946,   -979,   -1004,  -1019,
//         -1024,  -1019,  -1004,  -979,   -946,   -903,   -851,   -791,
//         -724,   -649,   -568,   -482,   -391,   -297,   -199,   -100,
//         0,       100,   199,    297,    391,    482,    568,    649,
//         724,    791,    851,    903,    946,    979,    1004,   1019
//     };
//
//     private static int[] g_nCosBuffer =
//     {
//         0,      -100,   -199,   -297,   -391,   -482,   -568,   -649,
//         -724,   -791,   -851,   -903,   -946,   -979,   -1004,  -1019,
//         -1024,  -1019,  -1004,  -979,   -946,   -903,   -851,   -791,
//         -724,   -649,   -568,   -482,   -391,   -297,   -199,   -100,
//         0,       100,   199,    297,    391,    482,    568,    649,
//         724,    791,    851,    903,    946,    979,    1004,   1019,
//         1024,   1019,   1004,   979,    946,    903,    851,    791,
//         724,    649,    568,    482,    391,    297,    199,    100,
//     };
//
//     static int[] g_nSin = g_nSinBuffer;
//     static int[] g_nCos = g_nCosBuffer;
// }