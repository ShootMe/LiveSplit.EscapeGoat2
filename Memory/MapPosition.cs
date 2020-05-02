using System.Runtime.InteropServices;
namespace LiveSplit.EscapeGoat2 {
    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
    public struct MapPosition {
        [FieldOffset(0)]
        public int X;
        [FieldOffset(4)]
        public int Y;
        public void ValidatePosition() {
            if (X < 0 || X > 20 || Y < 0 || Y > 30) {
                X = -1;
                Y = -1;
            }
        }
        public override string ToString() {
            return $"[{X},{Y}]";
        }
        public override bool Equals(object obj) {
            return obj is MapPosition map && map.GetHashCode() == GetHashCode();
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