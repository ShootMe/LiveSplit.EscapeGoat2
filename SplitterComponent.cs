using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.EscapeGoat2 {
	public class SplitterComponent : IComponent {
		public string ComponentName { get { return "Escape Goat 2 Autosplitter"; } }
		public TimerModel Model { get; set; }
		public IDictionary<string, Action> ContextMenuControls { get { return null; } }
		private static string LOGFILE = "_EscapeGoat.log";
		private static string[] keys = { "CurrentSplit", "Pointer", "MapPos", "Room", "Elapsed", "RoomElapsed", "TotalDeaths", "TotalBonks", "GameBeaten", "TitleShown", "TitleFadeTime", "EnteredDoor", "Invulnerable", "OrbCount", "SecretRooms" };
		private Dictionary<string, string> currentValues = new Dictionary<string, string>();
		private SplitterMemory mem;
		private int currentSplit = -1, lastLogCheck, elapsedCounter, lastExtraCount;
		private bool hasLog, lastEnteredDoor, exitingLevel;
		private double lastElapsed;

		public SplitterComponent(LiveSplitState state) {
			mem = new SplitterMemory();
			foreach (string key in keys) {
				currentValues[key] = "";
			}

			if (state != null) {
				Model = new TimerModel() { CurrentState = state };
				Model.InitializeGameTime();
				Model.CurrentState.IsGameTimePaused = true;
				state.OnReset += OnReset;
				state.OnPause += OnPause;
				state.OnResume += OnResume;
				state.OnStart += OnStart;
				state.OnSplit += OnSplit;
				state.OnUndoSplit += OnUndoSplit;
				state.OnSkipSplit += OnSkipSplit;
			}
		}

		public void GetValues() {
			if (!mem.HookProcess()) { return; }

			if (Model != null) {
				HandleSplits();
			}

			LogValues();
		}
		private void HandleSplits() {
			bool shouldSplit = false;

			if (currentSplit == -1) {
				shouldSplit = mem.TitleShown() && mem.TitleTextFadeTime() > 0;
			} else {
				double elapsed = mem.ElapsedTime();

				if (Model.CurrentState.CurrentPhase == TimerPhase.Running) {
					bool enteredDoor = mem.EnteredDoor();
					int extraCount = mem.OrbCount() + mem.SecretRoomCount();

					if (currentSplit + 1 < Model.CurrentState.Run.Count) {
						if (!exitingLevel) {
							exitingLevel = elapsed > 0 && ((enteredDoor && !lastEnteredDoor) || extraCount == lastExtraCount + 1);
						} else if (elapsedCounter < 3) {
							if (elapsed == lastElapsed) {
								elapsedCounter++;
							} else {
								elapsedCounter = 0;
							}
						}
						shouldSplit = elapsedCounter >= 3;
					} else {
						shouldSplit = enteredDoor && !lastEnteredDoor;
					}

					lastEnteredDoor = enteredDoor;
					lastExtraCount = extraCount;
				}

				if (elapsed > 0 || lastElapsed == elapsed) {
					if (Model.CurrentState.CurrentPhase == TimerPhase.Running) {
						if (Math.Abs(elapsed - Model.CurrentState.CurrentTime.GameTime.GetValueOrDefault(TimeSpan.FromSeconds(0)).TotalSeconds) <= 1) {
							Model.CurrentState.SetGameTime(TimeSpan.FromSeconds(elapsed));
						}
					} else if (lastElapsed != elapsed && currentSplit == Model.CurrentState.Run.Count) {
						ISegment segment = Model.CurrentState.Run[currentSplit - 1];
						segment.SplitTime = new Time(segment.SplitTime.RealTime, TimeSpan.FromSeconds(elapsed));
					}
				}

				Model.CurrentState.IsGameTimePaused = Model.CurrentState.CurrentPhase != TimerPhase.Running || lastElapsed == elapsed;

				lastElapsed = elapsed;
			}

			HandleSplit(shouldSplit, false);
		}
		private void HandleSplit(bool shouldSplit, bool shouldReset = false) {
			if (shouldReset) {
				if (currentSplit >= 0) {
					Model.Reset();
				}
			} else if (shouldSplit) {
				if (currentSplit == -1) {
					Model.Start();
				} else {
					Model.Split();
				}
			}
		}
		private void LogValues() {
			if (lastLogCheck == 0) {
				hasLog = File.Exists(LOGFILE);
				lastLogCheck = 300;
			}
			lastLogCheck--;

			if (hasLog || !Console.IsOutputRedirected) {
				string prev = string.Empty, curr = string.Empty;
				foreach (string key in keys) {
					prev = currentValues[key];

					switch (key) {
						case "CurrentSplit": curr = currentSplit.ToString(); break;
						case "Pointer": curr = mem.SceneManagerPointer(); break;
						case "MapPos": curr = mem.CurrentPosition().ToString(); break;
						case "Room": curr = mem.RoomName(); break;
						case "RoomElapsed": curr = mem.RoomElapsedTime().ToString("0"); break;
						case "Elapsed": curr = mem.ElapsedTime().ToString("0"); break;
						case "TotalDeaths": curr = mem.TotalDeaths().ToString(); break;
						case "TotalBonks": curr = mem.TotalBonks().ToString(); break;
						case "GameBeaten": curr = mem.IsGameBeaten().ToString(); break;
						case "TitleShown": curr = mem.TitleShown().ToString(); break;
						case "TitleFadeTime": curr = mem.TitleTextFadeTime().ToString(); break;
						case "EnteredDoor": curr = mem.EnteredDoor().ToString(); break;
						case "Invulnerable": curr = mem.GoatInvulnerable().ToString(); break;
						case "OrbCount": curr = mem.OrbCount().ToString(); break;
						case "SecretRooms": curr = mem.SecretRoomCount().ToString(); break;
						default: curr = string.Empty; break;
					}

					if (string.IsNullOrEmpty(prev)) { prev = string.Empty; }
					if (string.IsNullOrEmpty(curr)) { curr = string.Empty; }
					if (!prev.Equals(curr)) {
						WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + (Model != null ? " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) : "") + ": " + key + ": ".PadRight(16 - key.Length, ' ') + prev.PadLeft(25, ' ') + " -> " + curr);

						currentValues[key] = curr;
					}
				}
			}
		}

		public void Update(IInvalidator invalidator, LiveSplitState lvstate, float width, float height, LayoutMode mode) {
			IList<ILayoutComponent> components = lvstate.Layout.LayoutComponents;
			for (int i = components.Count - 1; i >= 0; i--) {
				ILayoutComponent component = components[i];
				if (component.Component is SplitterComponent && invalidator == null && width == 0 && height == 0) {
					components.Remove(component);
				}
			}

			GetValues();
		}
		public void OnReset(object sender, TimerPhase e) {
			currentSplit = -1;
			Model.CurrentState.IsGameTimePaused = true;
			WriteLog("---------Reset----------------------------------");
		}
		public void OnResume(object sender, EventArgs e) {
			WriteLog("---------Resumed--------------------------------");
		}
		public void OnPause(object sender, EventArgs e) {
			WriteLog("---------Paused---------------------------------");
		}
		public void OnStart(object sender, EventArgs e) {
			currentSplit = 0;
			lastElapsed = 0;
			lastEnteredDoor = mem.EnteredDoor();
			lastExtraCount = mem.OrbCount() + mem.SecretRoomCount();
			exitingLevel = false;
			elapsedCounter = 0;
			Model.CurrentState.IsGameTimePaused = true;
			WriteLog("---------New Game " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + "-------------------------");
		}
		public void OnUndoSplit(object sender, EventArgs e) {
			currentSplit--;
			exitingLevel = false;
			elapsedCounter = 0;
			WriteLog("---------Undo-----------------------------------");
		}
		public void OnSkipSplit(object sender, EventArgs e) {
			currentSplit++;
			exitingLevel = false;
			elapsedCounter = 0;
			WriteLog("---------Skip-----------------------------------");
		}
		public void OnSplit(object sender, EventArgs e) {
			currentSplit++;
			exitingLevel = false;
			elapsedCounter = 0;
			WriteLog("---------Split-----------------------------------");
		}
		private void WriteLog(string data) {
			if (hasLog || !Console.IsOutputRedirected) {
				if (Console.IsOutputRedirected) {
					using (StreamWriter wr = new StreamWriter(LOGFILE, true)) {
						wr.WriteLine(data);
					}
				} else {
					Console.WriteLine(data);
				}
			}
		}

		public Control GetSettingsControl(LayoutMode mode) { return null; }
		public void SetSettings(XmlNode document) { }
		public XmlNode GetSettings(XmlDocument document) { return document.CreateElement("Settings"); }
		public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }
		public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }
		public float HorizontalWidth { get { return 0; } }
		public float MinimumHeight { get { return 0; } }
		public float MinimumWidth { get { return 0; } }
		public float PaddingBottom { get { return 0; } }
		public float PaddingLeft { get { return 0; } }
		public float PaddingRight { get { return 0; } }
		public float PaddingTop { get { return 0; } }
		public float VerticalHeight { get { return 0; } }
		public void Dispose() { }
	}
}