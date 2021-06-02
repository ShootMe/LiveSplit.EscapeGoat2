using System;
using System.Diagnostics;
namespace LiveSplit.EscapeGoat2 {
    //.load C:\Windows\Microsoft.NET\Framework\v4.0.30319\SOS.dll
    public partial class MemoryManager {
        //MagicalTimeBean.Bastille.BastilleGame::InitializeScenes
        private static ProgramPointer SceneManagerEG1 = new ProgramPointer(new FindPointerSignature(PointerVersion.All, AutoDeref.Single, "8B40048B4804FF15????????FF15????????B9????????E8????????8BF88B1D", 0x20));
        //???
        private static ProgramPointer SceneManagerEG2 = new ProgramPointer(new FindPointerSignature(PointerVersion.All, AutoDeref.Single, "558BEC56833D????????00740B81E2FF00000075035E5DC333F683F908734BFF248D", 0x6));
        private static ProgramPointer IsSheepObtainedHere = new ProgramPointer(new FindPointerSignature(PointerVersion.All, AutoDeref.None, "568B51783A42588D4A588B318B41048BF88B4A1057563909E8????????25??0000005E5FC3", 0x1e));
        public static PointerVersion Version { get; set; } = PointerVersion.All;
        public Process Program { get; set; }
        public bool IsHooked { get; set; }
        public DateTime LastHooked { get; set; }
        public bool IsEG2 { get; set; } = true;
        private bool? sheepRoomPatch;

        public MemoryManager() {
            LastHooked = DateTime.MinValue;
        }
        public string GamePointers() {
            return string.Concat(
                $"EG1: {SceneManagerEG1.GetPointer(Program):X} ",
                $"EG2: {SceneManagerEG2.GetPointer(Program):X} "
            );
        }
        public void PatchSheepRooms(bool enable) {
            if (!sheepRoomPatch.HasValue || enable != sheepRoomPatch.Value) {
                if (!IsEG2 || IsSheepObtainedHere.GetPointer(Program) == IntPtr.Zero) { return; }

                IsSheepObtainedHere.Write<byte>(Program, (byte)(enable ? 0x0 : 0xff));

                sheepRoomPatch = enable;
            }
        }
        public bool IsPaused() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance._suspendPlayerInput
                return SceneManagerEG2.Read<bool>(Program, 0x4, 0x94);
            } else {
                //SceneManager.ActionSceneInstance._suspendPlayerInput
                return SceneManagerEG1.Read<bool>(Program, -0xc, 0x70, 0x1a);
            }
        }
        public MapPosition CurrentPosition() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance.GameState._currentPosition
                MapPosition map = SceneManagerEG2.Read<MapPosition>(Program, 0x4, 0x84, 0x58);
                map.ValidatePosition();
                return map;
            } else {
                //SceneManager.ActionSceneInstance._currentLocation.RegionType
                //SceneManager.ActionSceneInstance._currentLocation.RegionPosition
                return SceneManagerEG1.Read<MapPosition>(Program, -0xc, 0x90, 0x4);
            }
        }
        public string RoomName() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance._currentSpeedrunSequenceId
                string roomName = SceneManagerEG2.Read(Program, 0x4, 0x7c, 0x0);
                if (string.IsNullOrEmpty(roomName)) {
                    //SceneManager.ActionSceneInstance.PositionTracker.SeqMap.Map[x,y]
                    MapPosition pos = CurrentPosition();
                    if (pos.X >= 0) {
                        int length = SceneManagerEG2.Read<int>(Program, 0x4, 0x5c, 0x7c, 0x8, 0xc);
                        roomName = SceneManagerEG2.Read(Program, 0x4, 0x5c, 0x7c, 0x8, 0x1c + (pos.X * length + pos.Y) * 0x20, 0x0);
                    }
                }
                return roomName;
            } else {
                return SceneManagerEG1.Read<int>(Program, -0xc, 0x74, 0x8c).ToString();
            }
        }
        public int OrbCount() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance.GameState._orbObtainedPositions._size
                return SceneManagerEG2.Read<int>(Program, 0x4, 0x84, 0x10, 0xc);
            } else {
                return 0;
            }
        }
        public int SecretRoomCount() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance.GameState._secretRoomsBeaten._size
                return SceneManagerEG2.Read<int>(Program, 0x4, 0x84, 0x14, 0xc);
            } else {
                return 0;
            }
        }
        public bool TitleShown() {
            if (IsEG2) {
                //SceneManager.TitleScreenInstance._titleShown
                return SceneManagerEG2.Read<bool>(Program, 0xc, 0x94);
            } else {
                //SceneManager.TitleScreenInstance.Visible
                return SceneManagerEG1.Read<bool>(Program, -0x4, 0x1a);
            }
        }
        public int TitleTextFadeTime() {
            if (IsEG2) {
                //SceneManager.TitleScreenInstance._titleTextFadeTimer
                return SceneManagerEG2.Read<int>(Program, 0xc, 0x8c);
            } else {
                //SceneManager.TitleScreenInstance._SaveGameToLaunch
                uint saveGame = SceneManagerEG1.Read<uint>(Program, -0x4, 0x84);
                //SceneManager.TitleScreenInstance._SaveGameToLaunch._totalTime
                long ticks = saveGame == 0 ? 0L : SceneManagerEG1.Read<long>(Program, -0x4, 0x84, 0x30);
                //SceneManager.TitleScreenInstance._fader._fadeOutFrames
                int timer = saveGame != 0 && ticks == 0 ? SceneManagerEG1.Read<int>(Program, -0x4, 0x6c, 0x68) : 0;
                return timer;
            }
        }
        public uint GameState() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance.GameState
                return SceneManagerEG2.Read<uint>(Program, 0x4, 0x84);
            } else {
                //SceneManager.ActionSceneInstance.GameState
                return SceneManagerEG1.Read<uint>(Program, -0xc, 0x80);
            }
        }
        public uint RoomState() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance.RoomInstance
                return SceneManagerEG2.Read<uint>(Program, 0x4, 0x60);
            } else {
                //SceneManager.ActionSceneInstance.RoomInstance.Enabled
                return SceneManagerEG1.Read<byte>(Program, -0xc, 0x74, 0x18);
            }
        }
        public bool EnteredDoor() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance.RoomInstance.StopCountingElapsedTime
                return SceneManagerEG2.Read<bool>(Program, 0x4, 0x60, 0xca);
            } else {
                //SceneManager.ActionSceneInstance._state
                return SceneManagerEG1.Read<ActionSceneStates>(Program, -0xc, 0xb0) == ActionSceneStates.ExitingCurrentRoom;
            }
        }
        public double RoomElapsedTime() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance.RoomInstance.RoomElapsedTime
                return (double)SceneManagerEG2.Read<long>(Program, 0x4, 0x60, 0xcc) / (double)10000000;
            } else {
                return (double)0;
            }
        }
        public double ElapsedTime() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance.GameState._totalTime
                return (double)SceneManagerEG2.Read<long>(Program, 0x4, 0x84, 0x3c) / (double)10000000;
            } else {
                //SceneManager.ActionSceneInstance.GameState._totalTime
                return (double)SceneManagerEG1.Read<long>(Program, -0xc, 0x80, 0x30) / (double)10000000;
            }
        }
        public int TotalDeaths() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance.GameState.TotalDeathCount
                return SceneManagerEG2.Read<int>(Program, 0x4, 0x84, 0x2c);
            } else {
                //SceneManager.ActionSceneInstance.GameState.TotalDeathCount
                return SceneManagerEG1.Read<int>(Program, -0xc, 0x80, 0x18);
            }
        }
        public void MoveToPreviousLevel() {
            if (!IsEG2) {
                //SceneManager.ActionSceneInstance._currentLocation.RegionPosition
                int pos = SceneManagerEG1.Read<int>(Program, -0xc, 0x90, 0x8);
                if (pos > 0) {
                    //SceneManager.ActionSceneInstance._nextLocation.RegionPosition
                    SceneManagerEG1.Write<int>(Program, pos - 1, -0xc, 0x94, 0x8);
                }
            }
        }
        public void MoveToNextLevel() {
            if (!IsEG2) {
                //SceneManager.ActionSceneInstance._currentLocation.RegionPosition
                int pos = SceneManagerEG1.Read<int>(Program, -0xc, 0x90, 0x8);
                //SceneManager.ActionSceneInstance._nextLocation.RegionPosition
                SceneManagerEG1.Write<int>(Program, pos + 1, -0xc, 0x94, 0x8);
            }
        }
        public bool HookProcess() {
            IsHooked = Program != null && !Program.HasExited;
            if (!IsHooked && DateTime.Now > LastHooked.AddSeconds(1)) {
                LastHooked = DateTime.Now;

                Process[] processes = Process.GetProcessesByName("EscapeGoat2");
                Program = processes != null && processes.Length > 0 ? processes[0] : null;

                if (Program == null) {
                    processes = Process.GetProcessesByName("EscapeGoat");
                    Program = processes != null && processes.Length > 0 ? processes[0] : null;
                }

                if (Program != null && !Program.HasExited) {
                    IsEG2 = Program.ProcessName.Equals("EscapeGoat2", StringComparison.OrdinalIgnoreCase);
                    MemoryReader.Update64Bit(Program);
                    MemoryManager.Version = PointerVersion.All;
                    IsHooked = true;
                    sheepRoomPatch = null;
                }
            }

            return IsHooked;
        }
        public void Dispose() {
            if (Program != null) {
                Program.Dispose();
            }
        }
    }
}