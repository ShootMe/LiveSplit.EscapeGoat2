using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.EscapeGoat2 {
    public class SplitterComponent : IComponent {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetAsyncKeyState(Keys vkey);
        public string ComponentName { get { return "Escape Goat 1/2 Autosplitter"; } }
        public TimerModel Model { get; set; }
        public IDictionary<string, Action> ContextMenuControls { get { return null; } }
        private static string LOGFILE = "_EscapeGoat.txt";
        private Dictionary<LogObject, string> currentValues = new Dictionary<LogObject, string>();
        private SplitterMemory mem;
        private TextComponent deathComponent, roomComponent;
        private SplitterSettings settings;
        private int currentSplit = -1, lastLogCheck, lastExtraCount, lastDeathCount, deathTimer;
        private bool hasLog, lastEnteredDoor, exitingLevel, stillHolding, lastRoomActive;
        private double lastElapsed, roomTimerStart;
        private string roomTimer = "0.000", lastRoomTimer = "0.000";
        private MapPosition lastMapPosition = new MapPosition() { X = 10, Y = 0 };
        private Thread goatLoop;

        public SplitterComponent(LiveSplitState state) {
            mem = new SplitterMemory();
            settings = new SplitterSettings();
            foreach (LogObject key in Enum.GetValues(typeof(LogObject))) {
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

                goatLoop = new Thread(UpdateLoop);
                goatLoop.IsBackground = true;
                goatLoop.Start();
            }
        }

        private static bool IsKeyDown(Keys key) {
            return (GetAsyncKeyState(key) >> 15 & 1) == 1;
        }
        private void UpdateLoop() {
            while (goatLoop != null) {
                try {
                    GetValues();
                } catch (Exception ex) {
                    WriteLog(ex.ToString());
                }
                Thread.Sleep(8);
            }
        }
        public void GetValues() {
            if (!mem.HookProcess()) { return; }

            if (Model != null) {
                HandleSplits();
            }

            if ((Model == null || Model.CurrentState.CurrentPhase == TimerPhase.NotRunning) && mem.Program.MainWindowHandle == GetForegroundWindow()) {
                if (IsKeyDown(Keys.ControlKey) && IsKeyDown(Keys.A)) {
                    if (!stillHolding) {
                        mem.MoveToPreviousLevel();
                        stillHolding = true;
                    }
                } else if (IsKeyDown(Keys.ControlKey) && IsKeyDown(Keys.D)) {
                    if (!stillHolding) {
                        mem.MoveToNextLevel();
                        stillHolding = true;
                    }
                } else {
                    stillHolding = false;
                }
            }

            LogValues();
        }
        private void HandleSplits() {
            bool shouldSplit = false;

            double elapsed = mem.ElapsedTime();
            bool roomActive = mem.RoomActive();
            if (roomActive) {
                if (!lastRoomActive && (elapsed - roomTimerStart) > 1) {
                    lastRoomTimer = roomTimer;
                    roomTimer = "0.000";
                    roomTimerStart = elapsed;
                }
                roomTimer = (elapsed - roomTimerStart).ToString("0.000");
            }
            lastRoomActive = roomActive;

            if (currentSplit == -1) {
                shouldSplit = mem.TitleShown() && mem.TitleTextFadeTime() > 0;
            } else {
                int deathCount = mem.TotalDeaths();

                if (deathCount > lastDeathCount) {
                    deathTimer = 280;
                    exitingLevel = false;
                } else if (deathTimer > 0) {
                    deathTimer--;
                }

                bool enteredDoor = mem.EnteredDoor();
                int extraCount = mem.OrbCount() + mem.SecretRoomCount();

                if (Model.CurrentState.CurrentPhase == TimerPhase.Running && deathTimer <= 0) {
                    if (currentSplit + 1 < Model.CurrentState.Run.Count) {
                        if (Model.CurrentState.Run.Count == 10) {
                            MapPosition mapPosition = mem.CurrentPosition();
                            exitingLevel = elapsed > 0 && mapPosition.X == 10 && lastMapPosition.X != 10;
                            lastMapPosition = mapPosition;
                        } else if (!exitingLevel) {
                            exitingLevel = elapsed > 0 && ((enteredDoor && !lastEnteredDoor) || extraCount == lastExtraCount + 1);
                            if (exitingLevel) {
                                Thread.Sleep(3);
                                enteredDoor = mem.EnteredDoor();
                                extraCount = mem.OrbCount() + mem.SecretRoomCount();
                                exitingLevel = elapsed > 0 && ((enteredDoor && !lastEnteredDoor) || extraCount == lastExtraCount + 1);
                            }
                        }
                        shouldSplit = exitingLevel && (settings.SplitOnEnterPickup || !mem.IsEG2 ? true : !mem.HasRoomInstance());
                        if (shouldSplit) {
                            deathTimer = 360;
                            exitingLevel = false;
                        }
                    } else {
                        shouldSplit = enteredDoor && !lastEnteredDoor;
                    }
                }

                lastEnteredDoor = enteredDoor;
                lastExtraCount = extraCount;

                if (elapsed > 0 || lastElapsed == elapsed) {
                    if (Model.CurrentState.CurrentPhase == TimerPhase.Running) {
                        if (Math.Abs(elapsed - Model.CurrentState.CurrentTime.GameTime.GetValueOrDefault(TimeSpan.FromSeconds(0)).TotalSeconds) <= 1 || (deathTimer <= 0 && mem.HasRoomInstance())) {
                            Model.CurrentState.SetGameTime(TimeSpan.FromSeconds(elapsed));
                        }
                    } else if (lastElapsed != elapsed && currentSplit == Model.CurrentState.Run.Count) {
                        ISegment segment = Model.CurrentState.Run[currentSplit - 1];
                        segment.SplitTime = new Time(segment.SplitTime.RealTime, TimeSpan.FromSeconds(elapsed));
                    }
                }

                Model.CurrentState.IsGameTimePaused = Model.CurrentState.CurrentPhase != TimerPhase.Running || lastElapsed == elapsed;

                lastElapsed = elapsed;
                lastDeathCount = deathCount;
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
                foreach (LogObject key in Enum.GetValues(typeof(LogObject))) {
                    prev = currentValues[key];

                    switch (key) {
                        case LogObject.CurrentSplit: curr = currentSplit.ToString(); break;
                        case LogObject.Pointer: curr = mem.SceneManagerPointer(); break;
                        case LogObject.MapPos: curr = mem.CurrentPosition().ToString(); break;
                        case LogObject.Room: curr = mem.RoomName(); break;
                        case LogObject.RoomElapsed: curr = mem.RoomElapsedTime().ToString("0"); break;
                        case LogObject.Elapsed: curr = mem.ElapsedTime().ToString("0"); break;
                        case LogObject.TotalDeaths: curr = mem.TotalDeaths().ToString(); break;
                        case LogObject.TitleShown: curr = mem.TitleShown().ToString(); break;
                        case LogObject.TitleFadeTime: curr = mem.TitleTextFadeTime().ToString(); break;
                        case LogObject.EnteredDoor: curr = mem.EnteredDoor().ToString(); break;
                        case LogObject.OrbCount: curr = mem.OrbCount().ToString(); break;
                        case LogObject.SecretRooms: curr = mem.SecretRoomCount().ToString(); break;
                        case LogObject.Paused: curr = mem.IsPaused().ToString(); break;
                        case LogObject.RoomInstance: curr = mem.HasRoomInstance().ToString(); break;
                        default: curr = string.Empty; break;
                    }

                    if (string.IsNullOrEmpty(prev)) { prev = string.Empty; }
                    if (string.IsNullOrEmpty(curr)) { curr = string.Empty; }
                    if (!prev.Equals(curr)) {
                        WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + (Model != null ? " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) : "") + ": " + key + ": ".PadRight(16 - key.ToString().Length, ' ') + prev.PadLeft(25, ' ') + " -> " + curr);

                        currentValues[key] = curr;
                    }
                }
            }
        }

        public void Update(IInvalidator invalidator, LiveSplitState lvstate, float width, float height, LayoutMode mode) {
            if ((lastLogCheck & 63) == 0) {
                IList<ILayoutComponent> components = lvstate.Layout.LayoutComponents;
                deathComponent = null;
                for (int i = components.Count - 1; i >= 0; i--) {
                    ILayoutComponent component = components[i];
                    if (component.Component is TextComponent) {
                        TextComponent text = (TextComponent)component.Component;
                        if (text.Settings.Text1.IndexOf("Death", StringComparison.OrdinalIgnoreCase) >= 0) {
                            deathComponent = text;
                        } else if (text.Settings.Text1.IndexOf("Room", StringComparison.OrdinalIgnoreCase) >= 0) {
                            roomComponent = text;
                            break;
                        }
                    }
                }
            }

            if (deathComponent != null) {
                deathComponent.Settings.Text2 = mem.TotalDeaths().ToString();
            }
            if (roomComponent != null) {
                roomComponent.Settings.Text2 = $"{lastRoomTimer}/{roomTimer}";
            }
        }
        public void OnReset(object sender, TimerPhase e) {
            currentSplit = -1;
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
            Model.CurrentState.SetGameTime(TimeSpan.Zero);
            Model.CurrentState.IsGameTimePaused = true;
            WriteLog("---------New Game " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + "-------------------------");
        }
        public void OnUndoSplit(object sender, EventArgs e) {
            currentSplit--;
            exitingLevel = false;
            WriteLog("---------Undo-----------------------------------");
        }
        public void OnSkipSplit(object sender, EventArgs e) {
            currentSplit++;
            exitingLevel = false;
            WriteLog("---------Skip-----------------------------------");
        }
        public void OnSplit(object sender, EventArgs e) {
            currentSplit++;
            exitingLevel = false;
            WriteLog("---------Split-----------------------------------");
        }
        private void WriteLog(string data) {
            lock (LOGFILE) {
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
        }

        public Control GetSettingsControl(LayoutMode mode) { return settings; }
        public void SetSettings(XmlNode document) { settings.SetSettings(document); }
        public XmlNode GetSettings(XmlDocument document) { return settings.UpdateSettings(document); }
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
        public void Dispose() {
            if (goatLoop != null) {
                goatLoop = null;
            }
            if (Model != null) {
                Model.CurrentState.OnReset -= OnReset;
                Model.CurrentState.OnPause -= OnPause;
                Model.CurrentState.OnResume -= OnResume;
                Model.CurrentState.OnStart -= OnStart;
                Model.CurrentState.OnSplit -= OnSplit;
                Model.CurrentState.OnUndoSplit -= OnUndoSplit;
                Model.CurrentState.OnSkipSplit -= OnSkipSplit;
                Model = null;
            }
        }
    }
}