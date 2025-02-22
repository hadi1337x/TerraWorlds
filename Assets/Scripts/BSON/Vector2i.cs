using System;
using Kernys.Bson;

namespace BasicTypes
{
	[Serializable]
	public struct Vector2i
	{
		public int x;

		public int y;

		private static readonly string xKey = "x";

		private static readonly string yKey = "y";

		public const int sizeInBytes = 8;

		private static Vector2i tempAddition = new Vector2i(0, 0);

		public Vector2i(int xx, int yy)
		{
			x = xx;
			y = yy;
		}

		public Vector2i(BSONObject bson)
		{
			x = bson[xKey].int32Value;
			y = bson[yKey].int32Value;
		}

		public Vector2i(byte[] byteArray)
		{
			x = BitConverter.ToInt32(byteArray, 0);
			y = BitConverter.ToInt32(byteArray, 4);
		}

		public void StoreToBSON(BSONObject bson)
		{
			bson[xKey] = x;
			bson[yKey] = y;
		}

		public byte[] GetAsBinaryArray()
		{
			byte[] array = new byte[8];
			Buffer.BlockCopy(BitConverter.GetBytes(x), 0, array, 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(y), 0, array, 4, 4);
			return array;
		}

		public static bool DoesValidate(BSONObject bson)
		{
			return bson.ContainsKey(xKey) && bson.ContainsKey(yKey);
		}

		public override bool Equals(object obj)
		{
			return obj is Vector2i && this == (Vector2i)obj;
		}

		public override int GetHashCode()
		{
			return x << 16 + y;
		}

		public static bool operator ==(Vector2i v1, Vector2i v2)
		{
			return v1.x == v2.x && v1.y == v2.y;
		}

		public static bool operator !=(Vector2i v1, Vector2i v2)
		{
			return !(v1 == v2);
		}

		public static Vector2i operator +(Vector2i v1, Vector2i v2)
		{
			tempAddition.x = v1.x;
			tempAddition.x += v2.x;
			tempAddition.y = v1.y;
			tempAddition.y += v2.y;
			return tempAddition;
		}

		public static Vector2i GetZero()
		{
			return new Vector2i(0, 0);
		}

		public static Vector2i GetFromString(string vec2iString)
		{
			string[] array = vec2iString.Split(' ');
			return new Vector2i(int.Parse(array[0]), int.Parse(array[1]));
		}

		public override string ToString()
		{
			return xKey + " " + x + " " + yKey + " " + y;
		}
	}
}
