using LiveSplit.Memory;
using System;
using System.Diagnostics;
namespace LiveSplit.EscapeGoat2 {
	//.load C:\Windows\Microsoft.NET\Framework\v4.0.30319\SOS.dll
	public partial class SplitterMemory {
		private static ProgramPointer SceneManager = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC56833D????????00740B81E2FF00000075035E5DC333F683F908734BFF248D", 6));
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		private DateTime lastHooked;

		public SplitterMemory() {
			lastHooked = DateTime.MinValue;
		}
		public string SceneManagerPointer() {
			return SceneManager.GetPointer(Program).ToString("X");
		}
		public MapPosition CurrentPosition() {
			//SceneManager.ActionSceneInstance.GameState._currentPosition._x
			int x = SceneManager.Read<int>(Program, 0x4, 0x84, 0x58);
			//SceneManager.ActionSceneInstance.GameState._currentPosition._y
			int y = SceneManager.Read<int>(Program, 0x4, 0x84, 0x5c);
			return new MapPosition() { X = x, Y = y };
		}
		public string RoomName() {
			//SceneManager.ActionSceneInstance._currentSpeedrunSequenceId
			string roomName = SceneManager.Read(Program, 0x4, 0x7c, 0x0);
			if (string.IsNullOrEmpty(roomName)) {
				//SceneManager.ActionSceneInstance.PositionTracker.SeqMap.Map[x,y]
				MapPosition pos = CurrentPosition();
				int length = SceneManager.Read<int>(Program, 0x4, 0x5c, 0x7c, 0x8, 0xc);
				roomName = SceneManager.Read(Program, 0x4, 0x5c, 0x7c, 0x8, 0x1c + (pos.X * length + pos.Y) * 0x20, 0x0);
			}
			return roomName;
		}
		public bool GoatInvulnerable() {
			//SceneManager.ActionSceneInstance._player.Invulnerable
			return SceneManager.Read<bool>(Program, 0x4, 0x64, 0x45);
		}
		public bool TitleShown() {
			//SceneManager.TitleScreenInstance._titleShown
			return SceneManager.Read<bool>(Program, 0xc, 0x94);
		}
		public int TitleTextFadeTime() {
			//SceneManager.TitleScreenInstance._titleTextFadeTimer
			return SceneManager.Read<int>(Program, 0xc, 0x8c);
		}
		public bool EnteredDoor() {
			//SceneManager.ActionSceneInstance.RoomInstance.StopCountingElapsedTime
			return SceneManager.Read<bool>(Program, 0x4, 0x60, 0xca);
		}
		public double RoomElapsedTime() {
			//SceneManager.ActionSceneInstance.RoomInstance.RoomElapsedTime
			return (double)SceneManager.Read<long>(Program, 0x4, 0x60, 0xcc) / (double)10000000;
		}
		public double ElapsedTime() {
			//SceneManager.ActionSceneInstance.GameState._totalTime
			return (double)SceneManager.Read<long>(Program, 0x4, 0x84, 0x3c) / (double)10000000;
		}
		public int TotalDeaths() {
			//SceneManager.ActionSceneInstance.GameState.TotalDeathCount
			return SceneManager.Read<int>(Program, 0x4, 0x84, 0x2c);
		}
		public int TotalBonks() {
			//SceneManager.ActionSceneInstance.GameState.TotalBonkCount
			return SceneManager.Read<int>(Program, 0x4, 0x84, 0x30);
		}
		public bool IsGameBeaten() {
			//SceneManager.ActionSceneInstance.GameState.IsGameBeaten
			return SceneManager.Read<bool>(Program, 0x4, 0x84, 0x3b);
		}
		public bool HookProcess() {
			IsHooked = Program != null && !Program.HasExited;
			if (!IsHooked && DateTime.Now > lastHooked.AddSeconds(1)) {
				lastHooked = DateTime.Now;
				Process[] processes = Process.GetProcessesByName("EscapeGoat2");
				Program = processes != null && processes.Length > 0 ? processes[0] : null;

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