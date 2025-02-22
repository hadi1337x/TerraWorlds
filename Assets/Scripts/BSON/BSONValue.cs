using System;
using System.Collections.Generic;
using System.Text;
using BasicTypes;

namespace Kernys.Bson
{
	public class BSONValue
	{
		public enum ValueType
		{
			Double = 0,
			String = 1,
			Array = 2,
			Binary = 3,
			Boolean = 4,
			UTCDateTime = 5,
			None = 6,
			Int32 = 7,
			Int64 = 8,
			Object = 9
		}

		private ValueType mValueType;

		private double _double;

		private string _string;

		private byte[] _binary;

		private bool _bool;

		private DateTime _dateTime;

		private int _int32;

		private long _int64;

		public ValueType valueType
		{
			get
			{
				return mValueType;
			}
		}

		public double doubleValue
		{
			get
			{
				switch (mValueType)
				{
				case ValueType.Int32:
					return _int32;
				case ValueType.Int64:
					return _int64;
				case ValueType.Double:
					return _double;
				case ValueType.None:
					return double.NaN;
				default:
					throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to double", mValueType));
				}
			}
		}

		public int int32Value
		{
			get
			{
				switch (mValueType)
				{
				case ValueType.Int32:
					return _int32;
				case ValueType.Int64:
					return (int)_int64;
				case ValueType.Double:
					return (int)_double;
				default:
					throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to Int32", mValueType));
				}
			}
		}

		public long int64Value
		{
			get
			{
				switch (mValueType)
				{
				case ValueType.Int32:
					return _int32;
				case ValueType.Int64:
					return _int64;
				case ValueType.Double:
					return (long)_double;
				default:
					throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to Int64", mValueType));
				}
			}
		}

		public byte[] binaryValue
		{
			get
			{
				ValueType valueType = mValueType;
				if (valueType == ValueType.Binary)
				{
					return _binary;
				}
				throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to binary", mValueType));
			}
		}

		public DateTime dateTimeValue
		{
			get
			{
				ValueType valueType = mValueType;
				if (valueType == ValueType.UTCDateTime)
				{
					return _dateTime;
				}
				throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to DateTime", mValueType));
			}
		}

		public string stringValue
		{
			get
			{
				switch (mValueType)
				{
				case ValueType.Int32:
					return Convert.ToString(_int32);
				case ValueType.Int64:
					return Convert.ToString(_int64);
				case ValueType.Double:
					return Convert.ToString(_double);
				case ValueType.String:
					return (_string == null) ? null : _string.TrimEnd(default(char));
				case ValueType.Boolean:
					return (!_bool) ? "false" : "true";
				case ValueType.Binary:
					return Encoding.UTF8.GetString(_binary).TrimEnd(default(char));
				default:
					throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to string", mValueType));
				}
			}
		}

		public bool boolValue
		{
			get
			{
				ValueType valueType = mValueType;
				if (valueType == ValueType.Boolean)
				{
					return _bool;
				}
				throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to bool", mValueType));
			}
		}

		public List<int> int32ListValue
		{
			get
			{
				ValueType valueType = mValueType;
				if (valueType == ValueType.Array)
				{
					List<int> list = new List<int>();
					BSONArray bSONArray = (BSONArray)this;
					for (int i = 0; i < bSONArray.Count; i++)
					{
						list.Add(bSONArray[i].int32Value);
					}
					return list;
				}
				throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to int32 array", mValueType));
			}
		}

		public List<long> int64ListValue
		{
			get
			{
				ValueType valueType = mValueType;
				if (valueType == ValueType.Array)
				{
					List<long> list = new List<long>();
					BSONArray bSONArray = (BSONArray)this;
					for (int i = 0; i < bSONArray.Count; i++)
					{
						list.Add(bSONArray[i].int64Value);
					}
					return list;
				}
				throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to int64 array", mValueType));
			}
		}

		public List<string> stringListValue
		{
			get
			{
				ValueType valueType = mValueType;
				if (valueType == ValueType.Array)
				{
					List<string> list = new List<string>();
					BSONArray bSONArray = (BSONArray)this;
					for (int i = 0; i < bSONArray.Count; i++)
					{
						list.Add(bSONArray[i].stringValue);
					}
					return list;
				}
				throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to string array", mValueType));
			}
		}

		public List<Vector2i> vector2iListValue
		{
			get
			{
				ValueType valueType = mValueType;
				if (valueType == ValueType.Binary)
				{
					List<Vector2i> list = new List<Vector2i>();
					byte[] array = new byte[8];
					for (int i = 0; i < _binary.Length; i += 8)
					{
						Buffer.BlockCopy(_binary, i, array, 0, 8);
						list.Add(new Vector2i(array));
					}
					return list;
				}
				throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to Vector2i list", mValueType));
			}
		}

		public bool isNone
		{
			get
			{
				return mValueType == ValueType.None;
			}
		}

		public virtual BSONValue this[string key]
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public virtual BSONValue this[int index]
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		protected BSONValue(ValueType valueType)
		{
			mValueType = valueType;
		}

		public BSONValue()
		{
			mValueType = ValueType.None;
		}

		public BSONValue(double v)
		{
			mValueType = ValueType.Double;
			_double = v;
		}

		public BSONValue(string v)
		{
			mValueType = ValueType.String;
			_string = v;
		}

		public BSONValue(byte[] v)
		{
			mValueType = ValueType.Binary;
			_binary = v;
		}

		public BSONValue(bool v)
		{
			mValueType = ValueType.Boolean;
			_bool = v;
		}

		public BSONValue(DateTime dt)
		{
			mValueType = ValueType.UTCDateTime;
			_dateTime = dt;
		}

		public BSONValue(int v)
		{
			mValueType = ValueType.Int32;
			_int32 = v;
		}

		public BSONValue(long v)
		{
			mValueType = ValueType.Int64;
			_int64 = v;
		}

		public virtual void Clear()
		{
		}

		public virtual void Add(string key, BSONValue value)
		{
		}

		public virtual void Add(BSONValue value)
		{
		}

		public virtual bool ContainsValue(BSONValue v)
		{
			return false;
		}

		public virtual bool ContainsKey(string key)
		{
			return false;
		}

		public static implicit operator BSONValue(double v)
		{
			return new BSONValue(v);
		}

		public static implicit operator BSONValue(int v)
		{
			return new BSONValue(v);
		}

		public static implicit operator BSONValue(long v)
		{
			return new BSONValue(v);
		}

		public static implicit operator BSONValue(byte[] v)
		{
			return new BSONValue(v);
		}

		public static implicit operator BSONValue(DateTime v)
		{
			return new BSONValue(v);
		}

		public static implicit operator BSONValue(string v)
		{
			return new BSONValue(v);
		}

		public static implicit operator BSONValue(bool v)
		{
			return new BSONValue(v);
		}

		public static implicit operator BSONValue(List<int> v)
		{
			BSONArray bSONArray = new BSONArray();
			for (int i = 0; i < v.Count; i++)
			{
				bSONArray.Add(new BSONValue(v[i]));
			}
			return bSONArray;
		}

		public static implicit operator BSONValue(List<long> v)
		{
			BSONArray bSONArray = new BSONArray();
			for (int i = 0; i < v.Count; i++)
			{
				bSONArray.Add(new BSONValue(v[i]));
			}
			return bSONArray;
		}

		public static implicit operator BSONValue(List<string> v)
		{
			BSONArray bSONArray = new BSONArray();
			for (int i = 0; i < v.Count; i++)
			{
				bSONArray.Add(new BSONValue(v[i]));
			}
			return bSONArray;
		}

		public static implicit operator BSONValue(List<Vector2i> v)
		{
			byte[] array = new byte[v.Count * 8];
			for (int i = 0; i < v.Count; i++)
			{
				Buffer.BlockCopy(v[i].GetAsBinaryArray(), 0, array, i * 8, 8);
			}
			return new BSONValue(array);
		}

		public static implicit operator double(BSONValue v)
		{
			return v.doubleValue;
		}

		public static implicit operator int(BSONValue v)
		{
			return v.int32Value;
		}

		public static implicit operator long(BSONValue v)
		{
			return v.int64Value;
		}

		public static implicit operator byte[](BSONValue v)
		{
			return v.binaryValue;
		}

		public static implicit operator DateTime(BSONValue v)
		{
			return v.dateTimeValue;
		}

		public static implicit operator string(BSONValue v)
		{
			return v.stringValue;
		}

		public static implicit operator bool(BSONValue v)
		{
			return v.boolValue;
		}

		public static implicit operator List<int>(BSONValue v)
		{
			return v.int32ListValue;
		}

		public static implicit operator List<long>(BSONValue v)
		{
			return v.int64ListValue;
		}

		public static implicit operator List<string>(BSONValue v)
		{
			return v.stringListValue;
		}

		public static implicit operator List<Vector2i>(BSONValue v)
		{
			return v.vector2iListValue;
		}

		public static bool operator ==(BSONValue a, object b)
		{
			return object.ReferenceEquals(a, b);
		}

		public static bool operator !=(BSONValue a, object b)
		{
			return !(a == b);
		}
	}
}
