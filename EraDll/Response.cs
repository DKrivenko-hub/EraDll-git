using System.Collections.Generic;

namespace EraDll
{


    class Response
    {
        public readonly List<Errors> ErrorList = new List<Errors>()
            {
                new Errors("80", "Полностью остановленная ТРК и неактивная клавиатура", "SSR", "Stop Status Response", 9, false) ,
                new Errors("81", "Полностью остановленная ТРК, неактивная клавиатура и расширенный режим ответа ", "SSE", "Stop Status Extend ", 11, false) ,
                new Errors("82", "Полностью остановленная ТРК и активная клавиатура", "AKR", "Active Keyboard Response", 0x10, false),
                new Errors("83", "Полностью остановленная ТРК, активная клавиатура и запрос на разрешение на налив с клавиатуры ТРК", "AKRR", "Active Keyboard Requist Response", 0x10, false),
                new Errors("84", "ТРК в режиме налива и запрос на активный пистолет ", "AMAG", "Active Mode Active Gun ", false),
                new Errors("85", "ТРК в режиме налива и запрос на не активный пистолет", "AMNAG", "Active Mode No Active Gun ", false),
                new Errors("86", "Ответ на запрос о получении статуса в режиме отпуска топлива", "AMR", "Active Mode Response", 14, false),
                new Errors("87", "Ответ на запрос о получении статуса в режиме отпуска топлива расширенный", "AME", "Keyboard Request Extend ", 0x10, false),
                new Errors("87", "Ответ на запрос о получении статуса в режиме отпуска топлива расширенный", "AME", "Keyboard Request Extend ", 0x10, false),
                new Errors("90", "Ответ на запрос о чтении цены по заданному виду топлива", "RPR", "Read Price Response", 9, false),
                new Errors("91", "Ответ на запрос о чтении цены по заданному виду топлива и расширенный режим ответа", "RPE", "Read Price Extend", 11, false),
                new Errors("92", "Ответ о текущем отпускаемом количестве литров", "CAR", "Current Amount Response ", 14, false                                                    ),
                new Errors("93", "Ответ о последнем произведённом отпуске топлива", "MAR", "Made Amount Response ", 17, false),
                new Errors("94", "Успешное чтение литров за текущую смену ", "RVC", "Read Volume of Change ", 10, false),
                new Errors("95", "успешное чтение безналичного объёма за текущую смену", "RVCC", "Read Volume of Change non-Cash", 10, false),
                new Errors("96", "Успешное чтение суммы наличных денег за текущую смену", "CMCR", "Cash Money of Change Response", 10, false),
                new Errors("97", "Успешное чтение объёма при операции “прогон” за текущую смену", "PVCR", "Passage Volume of Change Response ", 10, false),
                new Errors("A0", "данные о суммарных накопительных  счётчиках по запрашиваемому адресу", "TCR", "Total Counters Response ", 11, false),
                new Errors("A1", "Успешное чтение всех отпущенных объёмов по всем адресам", "CCR", "Current Counters Response", 0x2f, false),
                new Errors("A2", "Успешное чтение суммы наличных денег за текущую смену", "CMCR", "Cash Money of Change Response", 10, false),
                new Errors("A3", "Успешное чтение суммы наличных денег за текущую смену", "CMCR", "Cash Money of Change Response", 10, false),
                new Errors("FF", "Невозможность выполнения данной команды в настоящий момент", "CBR", "Command Bad Response", 7, true),
                new Errors("FF", "Невозможность выполнения данной команды в настоящий момент", "CBE", "Command Bad Extend", 9, true),
                new Errors("00", "Успешное  выполнение принятой команды", "CCR", "Command Carry out Response ", 7, false),
                new Errors("00", "Успешное  выполнение принятой команды", "CCR", "Command Carry out Response Extend ", 9, false),
                new Errors("84", "Ответ в состоянии налива", "AMR", "Active Mode Response", 14, false),
                new Errors("", "Отказ в выполнении", "RPR", "Refusal of Performance Response", 9, true),
                new Errors("", "Действие невыполнимо", "ACR", "Action Cancelled Response", 9, true),
                new Errors("", "Неопознанный ответ", "U", "Unknown", 9, true)
            };
        private string parseResponse = "";
        private string parseAsyncResponse = "";

        public List<byte> GetResponse { get; set; } = new List<byte>();
        public List<byte> GetAsyncResponse { get; set; } = new List<byte>();

        public double CacheGunLit { get; set; }
        public byte CacheGunStatus { get; set; }


        public Errors CurrError { get; private set; } = null;

        public Response ( byte[] response, bool isAsync = false )
        {
            if (isAsync)
            {
                GetAsyncResponse.AddRange(response);
            }
            else
            {
                GetResponse.AddRange(response);
            }

        }

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
            CurrError = this.ErrorList.Find(er => er.ErrorCode.Contains(Converter.ByteToHex(this.GetResponse[4]))) ?? this.ErrorList[20];
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
                if (this.GetResponse.Count > 0)
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
