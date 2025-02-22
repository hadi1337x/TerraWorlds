using System.Collections;
using System.Collections.Generic;

namespace Kernys.Bson
{
	public class BSONObject : BSONValue, IEnumerable
	{
		private Dictionary<string, BSONValue> mMap = new Dictionary<string, BSONValue>();

		public ICollection<string> Keys
		{
			get
			{
				return mMap.Keys;
			}
		}

		public ICollection<BSONValue> Values
		{
			get
			{
				return mMap.Values;
			}
		}

		public int Count
		{
			get
			{
				return mMap.Count;
			}
		}

		public override BSONValue this[string key]
		{
			get
			{
				return mMap[key];
			}
			set
			{
				mMap[key] = value;
			}
		}

		public BSONObject()
			: base(ValueType.Object)
		{
		}

		public override void Clear()
		{
			mMap.Clear();
		}

		public override void Add(string key, BSONValue value)
		{
			mMap.Add(key, value);
		}

		public override bool ContainsValue(BSONValue v)
		{
			return mMap.ContainsValue(v);
		}

		public override bool ContainsKey(string key)
		{
			return mMap.ContainsKey(key);
		}

		public bool Remove(string key)
		{
			return mMap.Remove(key);
		}

		public bool TryGetValue(string key, out BSONValue value)
		{
			return mMap.TryGetValue(key, out value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return mMap.GetEnumerator();
		}
	}
}
