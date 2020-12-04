using System;
using System.Collections.Generic;
using System.Linq;


namespace EraDll
{
    static class ErrorList
    {
        public static readonly List<Error> errorList = new List<Error>()
            {
                new Error("80", "Полностью остановленная ТРК и неактивная клавиатура", "SSR", "Stop Status Response", 9, false) ,
                new Error("81", "Полностью остановленная ТРК, неактивная клавиатура и расширенный режим ответа ", "SSE", "Stop Status Extend ", 11, false) ,
                new Error("82", "Полностью остановленная ТРК и активная клавиатура", "AKR", "Active Keyboard Response", 0x10, false),
                new Error("83", "Полностью остановленная ТРК, активная клавиатура и запрос на разрешение на налив с клавиатуры ТРК", "AKRR", "Active Keyboard Requist Response", 0x10, false),
                new Error("84", "ТРК в режиме налива и запрос на активный пистолет ", "AMAG", "Active Mode Active Gun ", false),
                new Error("85", "ТРК в режиме налива и запрос на не активный пистолет", "AMNAG", "Active Mode No Active Gun ", false),
                new Error("86", "Ответ на запрос о получении статуса в режиме отпуска топлива", "AMR", "Active Mode Response", 14, false),
                new Error("87", "Ответ на запрос о получении статуса в режиме отпуска топлива расширенный", "AME", "Keyboard Request Extend ", 0x10, false),
                new Error("87", "Ответ на запрос о получении статуса в режиме отпуска топлива расширенный", "AME", "Keyboard Request Extend ", 0x10, false),
                new Error("90", "Ответ на запрос о чтении цены по заданному виду топлива", "RPR", "Read Price Response", 9, false),
                new Error("91", "Ответ на запрос о чтении цены по заданному виду топлива и расширенный режим ответа", "RPE", "Read Price Extend", 11, false),
                new Error("92", "Ответ о текущем отпускаемом количестве литров", "CAR", "Current Amount Response ", 14, false                                                    ),
                new Error("93", "Ответ о последнем произведённом отпуске топлива", "MAR", "Made Amount Response ", 17, false),
                new Error("94", "Успешное чтение литров за текущую смену ", "RVC", "Read Volume of Change ", 10, false),
                new Error("95", "успешное чтение безналичного объёма за текущую смену", "RVCC", "Read Volume of Change non-Cash", 10, false),
                new Error("96", "Успешное чтение суммы наличных денег за текущую смену", "CMCR", "Cash Money of Change Response", 10, false),
                new Error("97", "Успешное чтение объёма при операции “прогон” за текущую смену", "PVCR", "Passage Volume of Change Response ", 10, false),
                new Error("A0", "данные о суммарных накопительных  счётчиках по запрашиваемому адресу", "TCR", "Total Counters Response ", 11, false),
                new Error("A1", "Успешное чтение всех отпущенных объёмов по всем адресам", "CCR", "Current Counters Response", 0x2f, false),
                new Error("A2", "Успешное чтение суммы наличных денег за текущую смену", "CMCR", "Cash Money of Change Response", 10, false),
                new Error("A3", "Успешное чтение суммы наличных денег за текущую смену", "CMCR", "Cash Money of Change Response", 10, false),
                new Error("FF", "Невозможность выполнения данной команды в настоящий момент", "CBR", "Command Bad Response", 7, true),
                new Error("FF", "Невозможность выполнения данной команды в настоящий момент", "CBE", "Command Bad Extend", 9, true),
                new Error("00", "Успешное  выполнение принятой команды", "CCR", "Command Carry out Response ", 7, false),
                new Error("00", "Успешное  выполнение принятой команды", "CCR", "Command Carry out Response Extend ", 9, false),
                new Error("84", "Ответ в состоянии налива", "AMR", "Active Mode Response", 14, false),
                new Error("", "Отказ в выполнении", "RPR", "Refusal of Performance Response", 9, true),
                new Error("", "Действие невыполнимо", "ACR", "Action Cancelled Response", 9, true),
                new Error("", "Неопознанный ответ", "U", "Unknown", 9, true)
            };
        public static Error GetErrorByCode (string hexCode)
        {
            return errorList.Find(error => error.ErrorCode == hexCode);
        }
    }
}
