using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DataChain.Abstractions
{

    [Serializable]
    public class HexString : IEquatable<HexString>
    {
        private readonly byte[] data;

        public IReadOnlyList<byte> Value { get; }
        public static HexString Empty { get; }

       static HexString()
        {
            Empty = new HexString(new byte[0]);
        }
     
        public HexString(byte[] arr)
        {
            this.data = new byte[arr.Length];
            Buffer.BlockCopy(arr, 0, this.data, 0, data.Length);
            this.Value = new ReadOnlyCollection<byte>(this.data);
        }

        public static HexString Parse(string hexValue)
        {
            if (hexValue == null)
                throw new FormatException("Хекс значение не должно быть равно null");

            if (hexValue.Length % 2 == 1)
                throw new FormatException("Хекс значение не должно быть нечетной длины");

            byte[] result = new byte[hexValue.Length >> 1];

            for (int i = 0; i < (hexValue.Length >> 1); ++i)
                result[i] = (byte)((GetHexValue(hexValue[i << 1]) << 4) + (GetHexValue(hexValue[(i << 1) + 1])));

            return new HexString(result);
        }

        private static int GetHexValue(char hex)
        {
            int value = "0123456789ABCDEF".IndexOf(char.ToUpper(hex));

            if (value < 0)
                throw new FormatException(string.Format("The character '{0}' is not a hexadecimal digit.", hex));
            else
                return value;
        }

        public static byte[] operator +(HexString left, HexString right)
        {
            var leftArr = left.ToByteArray();
            var rightArr = right.ToByteArray();
            var count = (leftArr.LongLength > rightArr.LongLength) ? leftArr.LongLength : rightArr.LongLength;
            byte[] result = new byte[count];

            for (int i = 0; i < count; i++)
            {
                if (i > leftArr.LongLength)
                {
                    result[i] = rightArr[i];
                }
                else if (i > rightArr.LongLength)
                {
                    result[i] = leftArr[i];
                }
                else
                {
                    result[i] = (byte)(leftArr[i] + rightArr[i]);
                }

            }
            return result;
        }

        public byte[] ToByteArray()
        {
            byte[] result = new byte[data.Length];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            return result;
        }

        public  bool Equals(HexString obj)
        {
            if(obj == null)
            {
                return false;
            }

            else
            {
                if (this.data.Length != obj.data.Length)
                    return false;

                for (int i = 0; i < obj.data.Length; i++)
                    if (this.data[i] != obj.data[i])
                        return false;

                return true;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var byteValue in Value)
                 builder.Append(byteValue.ToString("X2"));

            return builder.ToString();
           
        }
    }
}
