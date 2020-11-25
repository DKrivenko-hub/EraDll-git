using System;
using System.ComponentModel;
using System.Linq;

namespace EraDll
{
    static class Converter
    {
        public static byte[] HexToBytes ( string hex )
        {
            byte[] bytes = hex.Split().Select(s => Convert.ToByte(s, 16)).ToArray();
            return bytes;
        }
        public static string BytesToHex ( byte[] bytes )
        {
            return BitConverter.ToString(bytes).ToUpper();
        }
        public static string ByteToHex ( byte _byte )
        {
            return _byte.ToString("x2").ToUpper();
        }

        public static string IntToHex ( int number )
        {
            return BitConverter.ToString(BitConverter.GetBytes(number), 0, 3).Replace('-', ' ');
        }
        public static string PriceForLitrToHex ( int price )
        {
            return BitConverter.ToString(BitConverter.GetBytes(price), 0, 2).Replace('-', ' ');
        }
        public static int HexToInt (string hex )
        {
            return Convert.ToInt32(hex,16);
        }
        public static byte HexToByte( string hex )
        {
            return Convert.ToByte(hex,16);
        }
    }
}
