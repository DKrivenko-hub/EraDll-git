using System.Collections.Generic;

namespace EraDll
{


    class Response
    {
        private string parseResponse = "";
        private string parseAsyncResponse = "";

        public List<byte> GetResponse { get; set; } = new List<byte>();
        public List<byte> GetAsyncResponse { get; set; } = new List<byte>();

      //  public Dictionary<string, Response> response = new Dictionary<string, Response>();

        public double CacheGunLit { get; set; }
        public byte CacheGunStatus { get; set; }

        public bool isBusy = false;
        public bool isStoped = false;
        public byte ResponseCode;


        public Error CurrError { get; private set; } = null;

        //public Response ( byte[] response, bool isAsync = false )
        //{
        //    if (isAsync)
        //    {
        //        GetAsyncResponse.AddRange(response);
        //    }
        //    else
        //    {
        //        GetResponse.AddRange(response);
        //    }

        //}
        //public Response () { }

        public void ClearResponse ()
        {
            parseResponse = "";
            GetResponse.Clear();
        }
        public void ClearAsyncResponse ()
        {
            parseAsyncResponse = "";
            GetAsyncResponse.Clear();
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
                if (GetResponse[startByte + count - i - 1] == 0 & i + 1 < count)
                {
                    i++;
                    continue;
                }
                litBytes += Converter.ByteToHex(GetResponse[startByte + count - i - 1]);
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
                hex += Converter.ByteToHex(GetResponse[14 - i]);
            }
            return Converter.HexToInt(hex);
        }
        public string ParseResponse ( bool isAsync = false )
        {
            if(GetResponse.Count>4)
            {
                 CurrError =  ErrorList.GetErrorByCode(Converter.ByteToHex(GetResponse[4]));
            }

            
            if (isAsync)
            {
                if (this.GetAsyncResponse.Count > 0)
                {
                    this.parseResponse = "Номер пистолета: " + this.GetAsyncResponse[1].ToString();
                    parseResponse += " Ответ: " + CurrError.ErrorDescription + "; Сообщение: " + CurrError.ErrorMessage +
                        " Полный код ответа: " + Converter.BytesToHex(this.GetAsyncResponse.ToArray());
                }
                return this.parseAsyncResponse;
            }
            else
            {
                if (GetResponse.Count > 0)
                {
                    this.parseResponse = "Номер пистолета: " + this.GetResponse[1].ToString();
                    parseResponse += " Ответ: " + CurrError.ErrorDescription + "; Сообщение: " + CurrError.ErrorMessage +
                        " Полный код ответа: " + Converter.BytesToHex(this.GetResponse.ToArray());
                }
                return this.parseResponse;
            }

        }

        public string RespToString ()
        {
            return Converter.BytesToHex(this.GetResponse.ToArray());
        }


    }
}
