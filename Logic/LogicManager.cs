using System;
using System.Threading;
namespace LiveSplit.EscapeGoat2 {
    public class LogicManager {
        public bool ShouldSplit { get; private set; }
        public bool ShouldReset { get; private set; }
        public int CurrentSplit { get; private set; }
        public bool Running { get; private set; }
        public bool Paused { get; private set; }
        public double GameTime { get; private set; }
        public MemoryManager Memory { get; private set; }
        public SplitterSettings Settings { get; private set; }
        public string RoomTimer = "0.000", DeathTimeLost = "0.000";
        private bool lastBoolValue, lastRoomActive, exitingLevel, currentRoomActive;
        private int lastIntValue;
        private DateTime splitLate, deathTimer;
        private int TotalSplits, lastDeathCount;
        private double roomTimerStart, currentElapsed, totalDeathTime, lastElapsed;
        public LogicManager(SplitterSettings settings) {
            Memory = new MemoryManager();
            Settings = settings;
            splitLate = DateTime.MaxValue;
        }

        public void Reset() {
            splitLate = DateTime.MaxValue;
            Paused = false;
            Running = false;
            CurrentSplit = 0;
            InitializeSplit();
            ShouldSplit = false;
            ShouldReset = false;
            ResetCustomStats();
        }
        public void Decrement() {
            CurrentSplit--;
            splitLate = DateTime.MaxValue;
            InitializeSplit();
        }
        public void Increment() {
            Running = true;
            splitLate = DateTime.MaxValue;
            if (CurrentSplit == 0) {
                ResetCustomStats();
            }
            CurrentSplit++;
            InitializeSplit();
        }
        private void ResetCustomStats() {
            deathTimer = DateTime.MinValue;
            DeathTimeLost = "0.000";
            lastDeathCount = 0;
            totalDeathTime = 0;
            RoomTimer = "0.000";
            exitingLevel = false;
            lastElapsed = 0;
            currentElapsed = 0;
            roomTimerStart = 0;
        }
        private void InitializeSplit() {
            if (CurrentSplit <= TotalSplits) {
                bool temp = ShouldSplit;
                CheckSplit(CurrentSplit == TotalSplits, true);
                ShouldSplit = temp;
            }
        }
        public bool IsHooked() {
            bool hooked = Memory.HookProcess();
            Paused = !hooked;
            ShouldSplit = false;
            ShouldReset = false;
            GameTime = -1;
            return hooked;
        }
        public void Update(int currentSplit, int totalSplits) {
            currentElapsed = Memory.HasGameState() ? Memory.ElapsedTime() : lastElapsed;
            TotalSplits = totalSplits;

            if (currentSplit != CurrentSplit) {
                CurrentSplit = currentSplit;
                Running = CurrentSplit > 0;
                InitializeSplit();
            }

            UpdateDeaths();
            UpdateRoomTimer();

            if (CurrentSplit <= TotalSplits) {
                CheckSplit(CurrentSplit == TotalSplits, !Running);
                if (!Running) {
                    Paused = true;
                    if (ShouldSplit) {
                        Running = true;
                    }
                }

                if (ShouldSplit) {
                    Increment();
                }
            }

            lastRoomActive = currentRoomActive;

            lastElapsed = currentElapsed;
            if (Running) {
                GameTime = currentElapsed;
            }
        }
        private void UpdateRoomTimer() {
            currentRoomActive = Memory.RoomActive();
            if (currentRoomActive) {
                if (!lastRoomActive && Math.Abs(currentElapsed - roomTimerStart) > 0.5) {
                    roomTimerStart = currentElapsed;
                }
            } else if (lastRoomActive && (currentElapsed - roomTimerStart) > 0.5) {
                if (deathTimer > DateTime.Now) {
                    totalDeathTime += currentElapsed - roomTimerStart;
                    DeathTimeLost = totalDeathTime.ToString("0.000");
                }
                RoomTimer = (currentElapsed - roomTimerStart).ToString("0.000");
            }
        }
        private void UpdateDeaths() {
            int deathCount = Memory.TotalDeaths();
            if (deathCount > lastDeathCount) {
                deathTimer = DateTime.Now.AddSeconds(2.1);
            }
            lastDeathCount = deathCount;
        }
        private void CheckSplit(bool finalSplit, bool updateValues) {
            ShouldSplit = false;
            DateTime date = DateTime.Now;

            if (CurrentSplit == 0) {
                ShouldSplit = Memory.TitleShown() && Memory.TitleTextFadeTime() > 0;
            } else if (!Memory.IsEG2) {
                if (TotalSplits == 10 && !finalSplit) {
                    MapPosition mapPosition = Memory.CurrentPosition();
                    ShouldSplit = currentElapsed > 0 && mapPosition.X == 10 && lastIntValue != 10;
                    lastIntValue = mapPosition.X;
                } else {
                    bool enteredDoor = Memory.EnteredDoor();
                    ShouldSplit = currentElapsed > 0 && enteredDoor && !lastBoolValue && currentRoomActive;
                    lastBoolValue = enteredDoor;
                }
            } else {
                if (!exitingLevel && date > deathTimer) {
                    bool enteredDoor = Memory.EnteredDoor();
                    int extraCount = Memory.OrbCount() + Memory.SecretRoomCount();
                    exitingLevel = currentElapsed > 0 && ((enteredDoor && !lastBoolValue) || (extraCount == lastIntValue + 1)) && currentRoomActive;
                    lastBoolValue = enteredDoor;
                    lastIntValue = extraCount;
                }

                ShouldSplit = exitingLevel && (finalSplit || Settings.SplitOnEnterPickup ? true : !currentRoomActive && lastRoomActive);

                if (ShouldSplit) {
                    exitingLevel = false;
                    deathTimer = date.AddSeconds(3);
                    return;
                }
            }

            Paused = !currentRoomActive || Memory.IsPaused();

            if (Running && date < deathTimer) {
                exitingLevel = false;
                ShouldSplit = false;
            } else if (DateTime.Now > splitLate) {
                ShouldSplit = true;
                splitLate = DateTime.MaxValue;
            }
        }
    }
}