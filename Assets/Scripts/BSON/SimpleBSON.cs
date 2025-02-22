using System;
using System.IO;
using System.Text;

namespace Kernys.Bson
{
	public class SimpleBSON
	{
		private MemoryStream mMemoryStream;

		private BinaryReader mBinaryReader;

		private SimpleBSON(byte[] buf = null)
		{
			if (buf != null)
			{
				mMemoryStream = new MemoryStream(buf);
				mBinaryReader = new BinaryReader(mMemoryStream);
			}
			else
			{
				mMemoryStream = new MemoryStream();
			}
		}

		public static BSONObject Load(byte[] buf)
		{
			SimpleBSON simpleBSON = new SimpleBSON(buf);
			return simpleBSON.decodeDocument();
		}

		public static byte[] Dump(BSONObject obj)
		{
			SimpleBSON simpleBSON = new SimpleBSON();
			MemoryStream memoryStream = new MemoryStream();
			simpleBSON.encodeDocument(memoryStream, obj);
			byte[] array = new byte[memoryStream.Position];
			memoryStream.Seek(0L, SeekOrigin.Begin);
			memoryStream.Read(array, 0, array.Length);
			return array;
		}

		private BSONValue decodeElement(out string name)
		{
			byte b = mBinaryReader.ReadByte();
			switch (b)
			{
			case 1:
				name = decodeCString();
				return new BSONValue(mBinaryReader.ReadDouble());
			case 2:
				name = decodeCString();
				return new BSONValue(decodeString());
			case 3:
				name = decodeCString();
				return decodeDocument();
			case 4:
				name = decodeCString();
				return decodeArray();
			case 5:
			{
				name = decodeCString();
				int count = mBinaryReader.ReadInt32();
				mBinaryReader.ReadByte();
				return new BSONValue(mBinaryReader.ReadBytes(count));
			}
			case 8:
				name = decodeCString();
				return new BSONValue(mBinaryReader.ReadBoolean());
			case 9:
			{
				name = decodeCString();
				long num = mBinaryReader.ReadInt64();
				TimeSpan timeSpan = new TimeSpan(num * 10000);
				DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				if (timeSpan > DateTime.MaxValue - dateTime)
				{
					timeSpan = DateTime.MaxValue - dateTime;
				}
				DateTime dateTime2 = dateTime;
				return new BSONValue(dateTime2 + timeSpan);
			}
			case 10:
				name = decodeCString();
				return new BSONValue();
			case 16:
				name = decodeCString();
				return new BSONValue(mBinaryReader.ReadInt32());
			case 18:
				name = decodeCString();
				return new BSONValue(mBinaryReader.ReadInt64());
			default:
				throw new Exception(string.Format("Don't know elementType={0}", b));
			}
		}

		private BSONObject decodeDocument()
		{
			int num = mBinaryReader.ReadInt32() - 4;
			BSONObject bSONObject = new BSONObject();
			int num2 = (int)mBinaryReader.BaseStream.Position;
			while (mBinaryReader.BaseStream.Position < num2 + num - 1)
			{
				string name;
				BSONValue value = decodeElement(out name);
				bSONObject.Add(name, value);
			}
			mBinaryReader.ReadByte();
			return bSONObject;
		}

		private BSONArray decodeArray()
		{
			BSONObject bSONObject = decodeDocument();
			int i = 0;
			BSONArray bSONArray = new BSONArray();
			for (; bSONObject.ContainsKey(Convert.ToString(i)); i++)
			{
				bSONArray.Add(bSONObject[Convert.ToString(i)]);
			}
			return bSONArray;
		}

		private string decodeString()
		{
			int count = mBinaryReader.ReadInt32();
			byte[] bytes = mBinaryReader.ReadBytes(count);
			return Encoding.UTF8.GetString(bytes);
		}

		private string decodeCString()
		{
			MemoryStream memoryStream = new MemoryStream();
			while (true)
			{
				byte b = mBinaryReader.ReadByte();
				if (b == 0)
				{
					break;
				}
				memoryStream.WriteByte(b);
			}
			return Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Position);
		}

		private void encodeElement(MemoryStream ms, string name, BSONValue v)
		{
			switch (v.valueType)
			{
			case BSONValue.ValueType.Double:
				ms.WriteByte(1);
				encodeCString(ms, name);
				encodeDouble(ms, v.doubleValue);
				break;
			case BSONValue.ValueType.String:
				ms.WriteByte(2);
				encodeCString(ms, name);
				encodeString(ms, v.stringValue);
				break;
			case BSONValue.ValueType.Object:
				ms.WriteByte(3);
				encodeCString(ms, name);
				encodeDocument(ms, v as BSONObject);
				break;
			case BSONValue.ValueType.Array:
				ms.WriteByte(4);
				encodeCString(ms, name);
				encodeArray(ms, v as BSONArray);
				break;
			case BSONValue.ValueType.Binary:
				ms.WriteByte(5);
				encodeCString(ms, name);
				encodeBinary(ms, v.binaryValue);
				break;
			case BSONValue.ValueType.Boolean:
				ms.WriteByte(8);
				encodeCString(ms, name);
				encodeBool(ms, v.boolValue);
				break;
			case BSONValue.ValueType.UTCDateTime:
				ms.WriteByte(9);
				encodeCString(ms, name);
				encodeUTCDateTime(ms, v.dateTimeValue);
				break;
			case BSONValue.ValueType.None:
				ms.WriteByte(10);
				encodeCString(ms, name);
				break;
			case BSONValue.ValueType.Int32:
				ms.WriteByte(16);
				encodeCString(ms, name);
				encodeInt32(ms, v.int32Value);
				break;
			case BSONValue.ValueType.Int64:
				ms.WriteByte(18);
				encodeCString(ms, name);
				encodeInt64(ms, v.int64Value);
				break;
			}
		}

		private void encodeDocument(MemoryStream ms, BSONObject obj)
		{
			MemoryStream memoryStream = new MemoryStream();
			foreach (string key in obj.Keys)
			{
				encodeElement(memoryStream, key, obj[key]);
			}
			BinaryWriter binaryWriter = new BinaryWriter(ms);
			binaryWriter.Write((int)(memoryStream.Position + 4 + 1));
			binaryWriter.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Position);
			binaryWriter.Write((byte)0);
		}

		private void encodeArray(MemoryStream ms, BSONArray lst)
		{
			BSONObject bSONObject = new BSONObject();
			for (int i = 0; i < lst.Count; i++)
			{
				bSONObject.Add(Convert.ToString(i), lst[i]);
			}
			encodeDocument(ms, bSONObject);
		}

		private void encodeBinary(MemoryStream ms, byte[] buf)
		{
			byte[] bytes = BitConverter.GetBytes(buf.Length);
			ms.Write(bytes, 0, bytes.Length);
			ms.WriteByte(0);
			ms.Write(buf, 0, buf.Length);
		}

		private void encodeCString(MemoryStream ms, string v)
		{
			byte[] bytes = new UTF8Encoding().GetBytes(v);
			ms.Write(bytes, 0, bytes.Length);
			ms.WriteByte(0);
		}

		private void encodeString(MemoryStream ms, string v)
		{
			byte[] bytes = new UTF8Encoding().GetBytes(v);
			byte[] bytes2 = BitConverter.GetBytes(bytes.Length + 1);
			ms.Write(bytes2, 0, bytes2.Length);
			ms.Write(bytes, 0, bytes.Length);
			ms.WriteByte(0);
		}

		private void encodeDouble(MemoryStream ms, double v)
		{
			byte[] bytes = BitConverter.GetBytes(v);
			ms.Write(bytes, 0, bytes.Length);
		}

		private void encodeBool(MemoryStream ms, bool v)
		{
			byte[] bytes = BitConverter.GetBytes(v);
			ms.Write(bytes, 0, bytes.Length);
		}

		private void encodeInt32(MemoryStream ms, int v)
		{
			byte[] bytes = BitConverter.GetBytes(v);
			ms.Write(bytes, 0, bytes.Length);
		}

		private void encodeInt64(MemoryStream ms, long v)
		{
			byte[] bytes = BitConverter.GetBytes(v);
			ms.Write(bytes, 0, bytes.Length);
		}

		private void encodeUTCDateTime(MemoryStream ms, DateTime dt)
		{
			byte[] bytes = BitConverter.GetBytes((long)(((dt.Kind != DateTimeKind.Local) ? (dt - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)) : (dt - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToLocalTime())).TotalSeconds * 1000.0));
			ms.Write(bytes, 0, bytes.Length);
		}
	}
}
