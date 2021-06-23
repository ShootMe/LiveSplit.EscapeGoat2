using System;
using System.Collections.Generic;
using System.IO;
namespace LiveSplit.EscapeGoat2 {
    public enum LogObject {
        CurrentSplit,
        Pointers,
        Version,
        MapPos,
        Room,
        Elapsed,
        RoomElapsed,
        TotalDeaths,
        TitleShown,
        TitleFadeTime,
        EnteredDoor,
        Invulnerable,
        OrbCount,
        SecretRooms,
        Paused,
        RoomInstance,
        GameState
    }
    public class LogManager {
        public const string LOG_FILE = "EscapeGoat.txt";
        private Dictionary<LogObject, string> currentValues = new Dictionary<LogObject, string>();
        private Dictionary<string, string> currentTags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private bool enableLogging;
        public bool EnableLogging {
            get { return enableLogging; }
            set {
                if (value != enableLogging) {
                    enableLogging = value;
                    if (value) {
                        AddEntryUnlocked(new EventLogEntry("Initialized"));
                    }
                }
            }
        }

        public LogManager() {
            EnableLogging = false;
            Clear();
        }
        public void Clear(bool deleteFile = false) {
            lock (currentValues) {
                if (deleteFile) {
                    try {
                        File.Delete(LOG_FILE);
                    } catch { }
                }
                foreach (LogObject key in Enum.GetValues(typeof(LogObject))) {
                    currentValues[key] = null;
                }
            }
        }
        public void AddEntry(ILogEntry entry) {
            lock (currentValues) {
                AddEntryUnlocked(entry);
            }
        }
        private void AddEntryUnlocked(ILogEntry entry) {
            string logEntry = entry.ToString();
            if (EnableLogging) {
                try {
                    using (StreamWriter sw = new StreamWriter(LOG_FILE, true)) {
                        sw.WriteLine(logEntry);
                    }
                } catch { }
                Console.WriteLine(logEntry);
            }
        }
        public void Update(LogicManager logic, SplitterSettings settings) {
            if (!EnableLogging) { return; }

            lock (currentValues) {
                DateTime date = DateTime.Now;
                uint roomInstance = logic.Memory.RoomState();
                uint gameState = logic.Memory.GameState();

                foreach (LogObject key in Enum.GetValues(typeof(LogObject))) {
                    string previous = currentValues[key];
                    string current = null;

                    switch (key) {
                        case LogObject.CurrentSplit: current = $"{logic.CurrentSplit}"; break;
                        case LogObject.Pointers: current = logic.Memory.GamePointers(); break;
                        case LogObject.Version: current = MemoryManager.Version.ToString(); break;
                        case LogObject.MapPos: current = gameState != 0 ? logic.Memory.CurrentPosition().ToString() : previous; break;
                        case LogObject.Room: current = roomInstance != 0 ? logic.Memory.RoomName() : previous; break;
                        //case LogObject.RoomElapsed: current = roomInstance ? logic.Memory.RoomElapsedTime().ToString("0") : previous; break;
                        //case LogObject.Elapsed: current = gameState ? logic.Memory.ElapsedTime().ToString("0") : previous; break;
                        case LogObject.TotalDeaths: current = gameState != 0 ? logic.Memory.TotalDeaths().ToString() : previous; break;
                        case LogObject.TitleShown: current = logic.Memory.TitleShown().ToString(); break;
                        case LogObject.EnteredDoor: current = !logic.Memory.IsEG2 || roomInstance != 0 ? logic.Memory.EnteredDoor().ToString() : previous; break;
                        case LogObject.OrbCount: current = gameState != 0 ? logic.Memory.OrbCount().ToString() : previous; break;
                        case LogObject.SecretRooms: current = gameState != 0 ? logic.Memory.SecretRoomCount().ToString() : previous; break;
                        case LogObject.Paused: current = logic.Memory.IsPaused().ToString(); break;
                        case LogObject.RoomInstance: current = $"{roomInstance:X}"; break;
                        case LogObject.GameState: current = $"{gameState:X}"; break;
                    }

                    if (previous != current) {
                        AddEntryUnlocked(new ValueLogEntry(date, key, previous, current));
                        currentValues[key] = current;
                    }
                }
            }
        }
    }
    public interface ILogEntry { }
    public class ValueLogEntry : ILogEntry {
        public DateTime Date;
        public LogObject Type;
        public object PreviousValue;
        public object CurrentValue;

        public ValueLogEntry(DateTime date, LogObject type, object previous, object current) {
            Date = date;
            Type = type;
            PreviousValue = previous;
            CurrentValue = current;
        }

        public override string ToString() {
            return string.Concat(
                Date.ToString(@"HH\:mm\:ss.fff"),
                ": (",
                Type.ToString(),
                ") ",
                PreviousValue,
                " -> ",
                CurrentValue
            );
        }
    }
    public class EventLogEntry : ILogEntry {
        public DateTime Date;
        public string Event;

        public EventLogEntry(string description) {
            Date = DateTime.Now;
            Event = description;
        }
        public EventLogEntry(DateTime date, string description) {
            Date = date;
            Event = description;
        }

        public override string ToString() {
            return string.Concat(
                Date.ToString(@"HH\:mm\:ss.fff"),
                ": ",
                Event
            );
        }
    }
}
