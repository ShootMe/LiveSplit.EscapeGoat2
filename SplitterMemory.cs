using LiveSplit.Memory;
using System;
using System.Diagnostics;
namespace LiveSplit.EscapeGoat2 {
    //.load C:\Windows\Microsoft.NET\Framework\v4.0.30319\SOS.dll
    public partial class SplitterMemory {
        //MagicalTimeBean.Bastille.BastilleGame::InitializeScenes
        private static ProgramPointer SceneManagerEG1 = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "8B40048B4804FF15????????FF15????????B9????????E8????????8BF88B1D", 32));
        //???
        private static ProgramPointer SceneManagerEG2 = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC56833D????????00740B81E2FF00000075035E5DC333F683F908734BFF248D", 6));
        public Process Program { get; set; }
        public bool IsHooked { get; set; } = false;
        public bool IsEG2 { get; set; } = true;
        private DateTime lastHooked;

        public SplitterMemory() {
            lastHooked = DateTime.MinValue;
        }
        public string SceneManagerPointer() {
            if (IsEG2) {
                return SceneManagerEG2.GetPointer(Program).ToString("X");
            } else {
                return SceneManagerEG1.GetPointer(Program).ToString("X");
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
                //SceneManager.ActionSceneInstance.GameState._currentPosition._x
                int x = SceneManagerEG2.Read<int>(Program, 0x4, 0x84, 0x58);
                //SceneManager.ActionSceneInstance.GameState._currentPosition._y
                int y = SceneManagerEG2.Read<int>(Program, 0x4, 0x84, 0x5c);
                if (x < 0 || x > 20 || y < 0 || y > 30) {
                    x = -1;
                    y = -1;
                }
                return new MapPosition() { X = x, Y = y };
            } else {
                //SceneManager.ActionSceneInstance._currentLocation.RegionType
                int x = SceneManagerEG1.Read<int>(Program, -0xc, 0x90, 0x4);
                //SceneManager.ActionSceneInstance._currentLocation.RegionPosition
                int y = SceneManagerEG1.Read<int>(Program, -0xc, 0x90, 0x8);
                return new MapPosition() { X = x, Y = y };
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
        public bool HasRoomInstance() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance.RoomInstance
                return SceneManagerEG2.Read<uint>(Program, 0x4, 0x60) != 0;
            } else {
                //SceneManager.ActionSceneInstance.RoomInstance
                return SceneManagerEG1.Read<uint>(Program, -0xc, 0x74) != 0;
            }
        }
        public bool EnteredDoor() {
            if (IsEG2) {
                //SceneManager.ActionSceneInstance.RoomInstance.StopCountingElapsedTime
                return HasRoomInstance() && SceneManagerEG2.Read<bool>(Program, 0x4, 0x60, 0xca);
            } else {
                //SceneManager.ActionSceneInstance._state
                return HasRoomInstance() && SceneManagerEG1.Read<int>(Program, -0xc, 0xb0) == (int)ActionSceneStates.ExitingCurrentRoom;
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
        public bool HookProcess() {
            IsHooked = Program != null && !Program.HasExited;
            if (!IsHooked && DateTime.Now > lastHooked.AddSeconds(1)) {
                lastHooked = DateTime.Now;
                Process[] processes = Process.GetProcessesByName("EscapeGoat2");
                Program = processes != null && processes.Length > 0 ? processes[0] : null;

                if (Program == null) {
                    processes = Process.GetProcessesByName("EscapeGoat");
                    Program = processes != null && processes.Length > 0 ? processes[0] : null;
                    if (Program != null) {
                        IsEG2 = false;
                    }
                } else {
                    IsEG2 = true;
                }

                if (Program != null && !Program.HasExited) {
                    MemoryReader.Update64Bit(Program);
                    IsHooked = true;
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
    public enum PointerVersion {
        V1
    }
    public enum AutoDeref {
        None,
        Single,
        Double
    }
    public class ProgramSignature {
        public PointerVersion Version { get; set; }
        public string Signature { get; set; }
        public int Offset { get; set; }
        public ProgramSignature(PointerVersion version, string signature, int offset) {
            Version = version;
            Signature = signature;
            Offset = offset;
        }
        public override string ToString() {
            return Version.ToString() + " - " + Signature;
        }
    }
    public class ProgramPointer {
        private int lastID;
        private DateTime lastTry;
        private ProgramSignature[] signatures;
        private int[] offsets;
        public IntPtr Pointer { get; private set; }
        public PointerVersion Version { get; private set; }
        public AutoDeref AutoDeref { get; private set; }

        public ProgramPointer(AutoDeref autoDeref, params ProgramSignature[] signatures) {
            AutoDeref = autoDeref;
            this.signatures = signatures;
            lastID = -1;
            lastTry = DateTime.MinValue;
        }
        public ProgramPointer(AutoDeref autoDeref, params int[] offsets) {
            AutoDeref = autoDeref;
            this.offsets = offsets;
            lastID = -1;
            lastTry = DateTime.MinValue;
        }

        public T Read<T>(Process program, params int[] offsets) where T : struct {
            GetPointer(program);
            return program.Read<T>(Pointer, offsets);
        }
        public string Read(Process program, params int[] offsets) {
            GetPointer(program);
            return program.Read(Pointer, offsets);
        }
        public byte[] ReadBytes(Process program, int length, params int[] offsets) {
            GetPointer(program);
            return program.Read(Pointer, length, offsets);
        }
        public void Write<T>(Process program, T value, params int[] offsets) where T : struct {
            GetPointer(program);
            program.Write<T>(Pointer, value, offsets);
        }
        public void Write(Process program, byte[] value, params int[] offsets) {
            GetPointer(program);
            program.Write(Pointer, value, offsets);
        }
        public void ClearPointer() {
            Pointer = IntPtr.Zero;
        }
        public IntPtr GetPointer(Process program) {
            if (program == null) {
                Pointer = IntPtr.Zero;
                lastID = -1;
                return Pointer;
            } else if (program.Id != lastID) {
                Pointer = IntPtr.Zero;
                lastID = program.Id;
            }

            if (Pointer == IntPtr.Zero && DateTime.Now > lastTry.AddSeconds(1)) {
                lastTry = DateTime.Now;

                Pointer = GetVersionedFunctionPointer(program);
                if (Pointer != IntPtr.Zero) {
                    if (AutoDeref != AutoDeref.None) {
                        if (MemoryReader.is64Bit) {
                            Pointer = (IntPtr)program.Read<ulong>(Pointer);
                        } else {
                            Pointer = (IntPtr)program.Read<uint>(Pointer);
                        }
                        if (AutoDeref == AutoDeref.Double) {
                            if (MemoryReader.is64Bit) {
                                Pointer = (IntPtr)program.Read<ulong>(Pointer);
                            } else {
                                Pointer = (IntPtr)program.Read<uint>(Pointer);
                            }
                        }
                    }
                }
            }
            return Pointer;
        }
        private IntPtr GetVersionedFunctionPointer(Process program) {
            if (signatures != null) {
                MemorySearcher searcher = new MemorySearcher();
                searcher.MemoryFilter = delegate (MemInfo info) {
                    return (info.State & 0x1000) != 0 && (info.Protect & 0x40) != 0 && (info.Protect & 0x100) == 0;
                };
                for (int i = 0; i < signatures.Length; i++) {
                    ProgramSignature signature = signatures[i];

                    IntPtr ptr = searcher.FindSignature(program, signature.Signature);
                    if (ptr != IntPtr.Zero) {
                        Version = signature.Version;
                        return ptr + signature.Offset;
                    }
                }
                return IntPtr.Zero;
            }

            if (MemoryReader.is64Bit) {
                return (IntPtr)program.Read<ulong>(program.MainModule.BaseAddress, offsets);
            } else {
                return (IntPtr)program.Read<uint>(program.MainModule.BaseAddress, offsets);
            }
        }
    }
}