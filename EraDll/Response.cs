using System.Collections.Generic;

namespace EraDll
{


    class Response
    {
#pragma warning disable IDE1006 // Стили именования
        public List<byte> response { get; set; } = new List<byte>();
#pragma warning restore IDE1006 // Стили именования

#pragma warning disable IDE1006 // Стили именования
        public string parseResponse { get; private set; } = "";
#pragma warning restore IDE1006 // Стили именования

        public Error CurrError { get; private set; } = null;

        public byte ResponseCode;
        public byte GunNumb;

        public void ClearResponse ()
        {
            parseResponse = "";
            response.Clear();
        }

        public double ParseLiters ( int startByte, int count )
        {
            if (CurrError == null)
            {
                return -1;
            }

            string litBytes = "";
            int i = 0;
            do
            {
                if (response[startByte + count - i - 1] == 0 & i + 1 < count)
                {
                    i++;
                    continue;
                }
                litBytes += Converter.ByteToHex(response[startByte + count - i - 1]);
                i++;
            } while (i < count);

            double liters = (double)Converter.HexToInt(litBytes) / 100;
            if (CurrError.RespBytes == 17)
            {
                double priceForLit = ParsePriceForLit() / 100;
                liters /= priceForLit;
            }
            return liters;
        }

        public int ParsePriceForLit ()
        {
            string hex = "";
            for (int i = 0; i < 2; i++)
            {
                hex += Converter.ByteToHex(response[14 - i]);
            }
            return Converter.HexToInt(hex);
        }
        public string ParseResponse ( bool isAsync = false )
        {
            CurrError = ErrorList.GetErrorByCode(Converter.ByteToHex(response[4]));
            if (parseResponse != "")
            {
                return parseResponse;
            }


            if (isAsync)
            {
                //if (this.GetAsyncResponse.Count > 0)
                //{
                //    this.parseResponse = "Номер пистолета: " + this.GetAsyncResponse[1].ToString();
                //    parseResponse += " Ответ: " + CurrError.ErrorDescription + "; Сообщение: " + CurrError.ErrorMessage +
                //        " Полный код ответа: " + Converter.BytesToHex(this.GetAsyncResponse.ToArray());
                //}
                //return this.parseAsyncResponse;
            }
            else
            {
                if (response.Count > 0)
                {
                    parseResponse = "Номер пистолета: " + response[1].ToString();
                    parseResponse += " Ответ: " + CurrError.ErrorDescription + "; Сообщение: " + CurrError.ErrorMessage +
                        " Полный код ответа: " + Converter.BytesToHex(response.ToArray());
                }

            }
            return parseResponse;

        }

        public string RespToString ()
        {
            return Converter.BytesToHex(response.ToArray());
        }
    }
}
