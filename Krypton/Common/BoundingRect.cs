using Microsoft.Xna.Framework;

namespace Krypton.Common {
	/// <summary>
	/// Much like Rectangle, but stored as two Vector2s
	/// </summary>
	public struct BoundingRect {
		public static readonly BoundingRect Empty;
		public static readonly BoundingRect MinMax;
		public Vector2 Max;
		public Vector2 Min;

		static BoundingRect() {
			Empty = new BoundingRect();
			MinMax = new BoundingRect(Vector2.One*float.MinValue, Vector2.One*float.MaxValue);
		}

		public BoundingRect(Vector2 min, Vector2 max) {
			Min = min;
			Max = max;
		}

		public float Left {
			get { return Min.X; }
		}

		public float Right {
			get { return Max.X; }
		}

		public float Top {
			get { return Max.Y; }
		}

		public float Bottom {
			get { return Min.Y; }
		}

		public bool Intersects(BoundingRect rect) {
			return
				(Min.X < rect.Max.X) &&
				(Min.Y < rect.Max.Y) &&
				(Max.X > rect.Min.X) &&
				(Max.Y > rect.Min.Y);
		}

		public bool Equals(BoundingRect other) {
			return
				(Min.X == other.Min.X) &&
				(Min.Y == other.Min.Y) &&
				(Max.X == other.Max.X) &&
				(Max.Y == other.Max.Y);
		}

		public override int GetHashCode() {
			return Min.GetHashCode() + Max.GetHashCode();
		}

		public static bool operator ==(BoundingRect a, BoundingRect b) {
			return
				(a.Min.X == b.Min.X) &&
				(a.Min.Y == b.Min.Y) &&
				(a.Max.X == b.Max.X) &&
				(a.Max.Y == b.Max.Y);
		}

		public static bool operator !=(BoundingRect a, BoundingRect b) {
			return
				(a.Min.X != b.Min.X) ||
				(a.Min.Y != b.Min.Y) ||
				(a.Max.X != b.Max.X) ||
				(a.Max.Y != b.Max.Y);
		}

		public override bool Equals(object obj) {
			if (obj is BoundingRect) {
				return this == (BoundingRect) obj;
			}

			return false;
		}
	}
}
