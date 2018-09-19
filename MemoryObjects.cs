namespace LiveSplit.EscapeGoat2 {
	public enum LogObject {
		CurrentSplit,
		Pointer,
		MapPos,
		Room,
		Elapsed,
		RoomElapsed,
		TotalDeaths,
		TotalBonks,
		GameBeaten,
		TitleShown,
		TitleFadeTime,
		EnteredDoor,
		Invulnerable,
		OrbCount,
		SecretRooms,
		Paused,
		RoomInstance
	}
	public class MapPosition {
		public int X { get; set; }
		public int Y { get; set; }
		public override string ToString() {
			return $"[{X},{Y}]";
		}
		public override bool Equals(object obj) {
			return obj != null && obj is MapPosition && ((MapPosition)obj).GetHashCode() == GetHashCode();
		}
		public override int GetHashCode() {
			return X * 16 + Y;
		}
		public static bool operator ==(MapPosition one, MapPosition two) {
			return (object)one != null && (object)two != null && one.GetHashCode() == two.GetHashCode();
		}
		public static bool operator !=(MapPosition one, MapPosition two) {
			return (object)one == null || (object)two == null || one.GetHashCode() != two.GetHashCode();
		}
	}
}