using System.ComponentModel;
namespace LiveSplit.EscapeGoat2 {
    public class SplitterSettings {
        public bool SplitOnEnterPickup;
        public bool SheepRoomPatch;
        public SplitterSettings() {
            SplitOnEnterPickup = false;
            SheepRoomPatch = false;
        }
    }
}