namespace EraDll
{
   public static class CRC16
    {
        public static string GetCrc(byte[] data)
        {
            string crc = CrcCalc(data).ToString("x4");
            string res = "";

            for (int i = 2; i >= 0; i -= 2)
            {
                res += " " + crc.Substring(i, 2);
            }

            res = res.ToUpper();

            return res;

        }
        private static ushort CrcCalc(byte[] data)
        {
            ushort wCRC = 0;
            for (int i = 0; i < data.Length; i++)
            {
                wCRC ^= (ushort)(data[i] << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((wCRC & 0x8000) != 0)
                        wCRC = (ushort)((wCRC << 1) ^ 0x1021);
                    else
                        wCRC <<= 1;
                }
            }
            return wCRC;
        } 
    }
}

