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

		public BoundingRect(float x, float y, float width, float height) {
			Min.X = x;
			Min.Y = y;
			Max.X = x + width;
			Max.Y = y + height;
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

		public float Width {
			get { return Max.X - Min.X; }
		}

		public float Height {
			get { return Max.Y - Min.Y; }
		}

		public Vector2 Center {
			get { return (Min + Max)/2; }
		}

		public bool IsZero {
			get {
				return
					(Min.X == 0) &&
					(Min.Y == 0) &&
					(Max.X == 0) &&
					(Max.Y == 0);
			}
		}

		public bool Contains(float x, float y) {
			return
				(Min.X <= x) &&
				(Min.Y <= y) &&
				(Max.X >= x) &&
				(Max.Y >= y);
		}

		public bool Contains(Vector2 vector) {
			return
				(Min.X <= vector.X) &&
				(Min.Y <= vector.Y) &&
				(Max.X >= vector.X) &&
				(Max.Y >= vector.Y);
		}

		public void Contains(ref Vector2 rect, out bool result) {
			result =
				(Min.X <= rect.X) &&
				(Min.Y <= rect.Y) &&
				(Max.X >= rect.X) &&
				(Max.Y >= rect.Y);
		}

		public bool Contains(BoundingRect rect) {
			return
				(Min.X <= rect.Min.X) &&
				(Min.Y <= rect.Min.Y) &&
				(Max.X >= rect.Max.X) &&
				(Max.Y >= rect.Max.Y);
		}

		public void Contains(ref BoundingRect rect, out bool result) {
			result =
				(Min.X <= rect.Min.X) &&
				(Min.Y <= rect.Min.Y) &&
				(Max.X >= rect.Max.X) &&
				(Max.Y >= rect.Max.Y);
		}

		public bool Intersects(BoundingRect rect) {
			return
				(Min.X < rect.Max.X) &&
				(Min.Y < rect.Max.Y) &&
				(Max.X > rect.Min.X) &&
				(Max.Y > rect.Min.Y);
		}

		public void Intersects(ref BoundingRect rect, out bool result) {
			result =
				(Min.X < rect.Max.X) &&
				(Min.Y < rect.Max.Y) &&
				(Max.X > rect.Min.X) &&
				(Max.Y > rect.Min.Y);
		}

		public static BoundingRect Intersect(BoundingRect rect1, BoundingRect rect2) {
			BoundingRect result;

			float num8 = rect1.Max.X;
			float num7 = rect2.Max.X;
			float num6 = rect1.Max.Y;
			float num5 = rect2.Max.Y;
			float num2 = (rect1.Min.X > rect2.Min.X) ? rect1.Min.X : rect2.Min.X;
			float num = (rect1.Min.Y > rect2.Min.Y) ? rect1.Min.Y : rect2.Min.Y;
			float num4 = (num8 < num7) ? num8 : num7;
			float num3 = (num6 < num5) ? num6 : num5;

			if ((num4 > num2) && (num3 > num)) {
				result.Min.X = num2;
				result.Min.Y = num;
				result.Max.X = num4;
				result.Max.Y = num3;

				return result;
			}

			result.Min.X = 0;
			result.Min.Y = 0;
			result.Max.X = 0;
			result.Max.Y = 0;

			return result;
		}

		public static void Intersect(ref BoundingRect rect1, ref BoundingRect rect2, out BoundingRect result) {
			float num8 = rect1.Max.X;
			float num7 = rect2.Max.X;
			float num6 = rect1.Max.Y;
			float num5 = rect2.Max.Y;
			float num2 = (rect1.Min.X > rect2.Min.X) ? rect1.Min.X : rect2.Min.X;
			float num = (rect1.Min.Y > rect2.Min.Y) ? rect1.Min.Y : rect2.Min.Y;
			float num4 = (num8 < num7) ? num8 : num7;
			float num3 = (num6 < num5) ? num6 : num5;

			if ((num4 > num2) && (num3 > num)) {
				result.Min.X = num2;
				result.Min.Y = num;
				result.Max.X = num4;
				result.Max.Y = num3;
			}

			result.Min.X = 0;
			result.Min.Y = 0;
			result.Max.X = 0;
			result.Max.Y = 0;
		}

		public static BoundingRect Union(BoundingRect rect1, BoundingRect rect2) {
			BoundingRect result;

			float num6 = rect1.Max.X;
			float num5 = rect2.Max.X;
			float num4 = rect1.Max.Y;
			float num3 = rect2.Max.Y;
			float num2 = (rect1.Min.X < rect2.Min.X) ? rect1.Min.X : rect2.Min.X;
			float num = (rect1.Min.Y < rect2.Min.Y) ? rect1.Min.Y : rect2.Min.Y;
			float num8 = (num6 > num5) ? num6 : num5;
			float num7 = (num4 > num3) ? num4 : num3;

			result.Min.X = num2;
			result.Min.Y = num;
			result.Max.X = num8;
			result.Max.Y = num7;

			return result;
		}

		public static void Union(ref BoundingRect rect1, ref BoundingRect rect2, out BoundingRect result) {
			float num6 = rect1.Max.X;
			float num5 = rect2.Max.X;
			float num4 = rect1.Max.Y;
			float num3 = rect2.Max.Y;
			float num2 = (rect1.Min.X < rect2.Min.X) ? rect1.Min.X : rect2.Min.X;
			float num = (rect1.Min.Y < rect2.Min.Y) ? rect1.Min.Y : rect2.Min.Y;
			float num8 = (num6 > num5) ? num6 : num5;
			float num7 = (num4 > num3) ? num4 : num3;

			result.Min.X = num2;
			result.Min.Y = num;
			result.Max.X = num8;
			result.Max.Y = num7;
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
