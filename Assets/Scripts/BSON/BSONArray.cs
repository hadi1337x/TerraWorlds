using System.Collections;
using System.Collections.Generic;

namespace Kernys.Bson
{
	public class BSONArray : BSONValue, IEnumerable
	{
		private List<BSONValue> mList = new List<BSONValue>();

		public override BSONValue this[int index]
		{
			get
			{
				return mList[index];
			}
			set
			{
				mList[index] = value;
			}
		}

		public int Count
		{
			get
			{
				return mList.Count;
			}
		}

		public BSONArray()
			: base(ValueType.Array)
		{
		}

		public override void Add(BSONValue v)
		{
			mList.Add(v);
		}

		public int IndexOf(BSONValue item)
		{
			return mList.IndexOf(item);
		}

		public void Insert(int index, BSONValue item)
		{
			mList.Insert(index, item);
		}

		public bool Remove(BSONValue v)
		{
			return mList.Remove(v);
		}

		public void RemoveAt(int index)
		{
			mList.RemoveAt(index);
		}

		public override void Clear()
		{
			mList.Clear();
		}

		public new virtual bool ContainsValue(BSONValue v)
		{
			return mList.Contains(v);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return mList.GetEnumerator();
		}
	}
}
