using System;
using System.Collections.Generic;

namespace EraDll
{

    enum ByteCounts : byte
    {
        status = 07,
        curLit = 07,
        changeShift = 07,
        litersShift = 07,
        priceShift = 07,
        NcashShift = 07,
        totLit = 07,
        pressure = 07,
        pourLit = 14,
        pourPr = 14,
        NcashLit = 14,
        start = 07,
        stop = 07,
        pause = 07
    }

    enum Commands : byte
    {
        status = 01,
        curLit = 04,
        pourLit = 05,
        NcashLit = 06,
        pourPr = 09,
        changeShift = 10,
        pause = 11,
        stop = 12,
        litersShift = 13,
        NcashShift = 14,
        priceShift = 15,
        start = 17,
        totLit = 21,
        pressure = 25,
    }

    internal class Data
    {
        private string parseResponse = "";
        public string Request { get; private set; } = "2D ";

        public List<byte> Response { get; set; } = new List<byte>();

        private string IndexHex;

        private readonly List<Errors> ErrorList = new List<Errors>()
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

        public Errors CurrError { get; private set; } = null;


        public void ClearResponse ()
        {
            parseResponse = "";
            Response.Clear();
        }

        public string CreateRequest ( byte PistolNumb, byte indexByte, ByteCounts bytes, Commands command )
        {
            IndexHex = Converter.ByteToHex(indexByte);
            string[] textArray1 = new string[] { 
                Request, 
                Converter.ByteToHex(PistolNumb),
                " ",
                IndexHex,
                " ",
                Converter.ByteToHex((byte)bytes),
                " ",
                Converter.ByteToHex((byte)command) };
            Request = string.Concat(textArray1);
            Request = Request + CRC16.GetCrc(Converter.HexToBytes(Request));
            return Request;
        }

        public string CreateRequest ( byte PistolNumb, byte indexByte, int PriceForLit, int param, ByteCounts bytes, Commands command )
        {
            IndexHex = Converter.ByteToHex(indexByte);
            string[] textArray1 = new string[12];
            textArray1[0] = Request;
            textArray1[1] = Converter.ByteToHex(PistolNumb);
            textArray1[2] = " ";
            textArray1[3] = this.IndexHex;
            textArray1[4] = " ";
            textArray1[5] = Converter.ByteToHex((byte)bytes);
            textArray1[6] = " ";
            textArray1[7] = Converter.ByteToHex((byte)command);
            textArray1[8] = " 00 00 ";
            textArray1[9] = Converter.IntToHex(param);
            textArray1[10] = " ";
            textArray1[11] = Converter.PriceForLitrToHex(PriceForLit);
            Request = string.Concat(textArray1);
            Request = Request + CRC16.GetCrc(Converter.HexToBytes(Request));
            return Request;
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
                if (Response[startByte + count - i - 1] == 0)
                {
                    i++;
                    continue;
                }
                litBytes += Converter.ByteToHex(Response[startByte + count - i - 1]);
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
                hex += Converter.ByteToHex(Response[14 - i]);
            }
            return Converter.HexToInt(hex);
        }
        public string ParseResponse ()
        {
            if (this.Response.Count > 0)
            {
                CurrError = this.ErrorList.Find(er => er.ErrorCode.Contains(Converter.ByteToHex(this.Response[4]))) ?? this.ErrorList[20];

                this.parseResponse = "Номер пистолета: " + this.Response[1].ToString();
                string[] textArray1 = new string[] { this.parseResponse, " Ответ: ", CurrError.ErrorDescription, "; Сообщение: ", CurrError.ErrorMessage };
                this.parseResponse = string.Concat(textArray1);
                this.parseResponse = this.parseResponse + " Полный код ответа: " + Converter.BytesToHex(this.Response.ToArray());
            }
            return this.parseResponse;
        }

        public string RespToString ()
        {
            return Converter.BytesToHex(this.Response.ToArray());
        }
    }
}

