using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace EraDll
{

    [Guid("76B62B2A-8AED-4BDB-BDA6-DFA9596BE5C5")]


    internal interface IAdapter
    {
        [DispId(1)]
        int SetTimeout { get; set; }
        string PortName { get; set; }


        bool Connect ();
        bool Disconnect ();

        bool CheckConnection ();

        string GetGunStatus ( byte GunNumb );
        string GetGunPressure ( byte GunNumb );

        bool IsPour ( byte GunNumb );

        //public void SetCacheGunStatus ( byte GunNumb );

        bool PourGasolineLit ( byte GunNumb, int liters, int PriceForLit );
        bool PourGasolinePrice ( byte GunNumb, int price, int PriceForLit );
        bool PourNcachLit ( byte GunNumb, int liters, int PriceForLit );

        //double LastLiters ( byte GunNumb );                      
        double LastAmount ( byte GunNumb );

        double GetCacheLit ( byte GunNumb );
        byte GetCacheStatus ( byte GunNumb );

        bool ChangeShift ( byte GunNumb );
        double LitersShift ( byte GunNumb );                                                                                                               
        bool PriceShift ( byte GunNumb );
        bool NcashShift ( byte GunNumb );

        double TotalLiters ( byte GunNumb );

        string LastResponse ( byte GunNumb );

        bool Start ( byte GunNumb );
        bool Pause ( byte GunNumb );
        bool Stop ( byte GunNumb );

        bool ReadyToWork ( byte GunNumb );
    }

    [Guid("853F959A-81DB-4021-B8E5-DB322F8E829E"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IEvents
    {

    }
    [Guid("58597A70-322C-42E6-98CE-BD98E891E531"), ClassInterface(ClassInterfaceType.None), ComSourceInterfaces(typeof(IEvents))]
    public class Adapter : IAdapter
    {
        private readonly Port port = new Port();
        public string PortName { get; set; } = "COM3";
        public int SetTimeout { get; set; } = 2000;


        public bool Connect ()
        {
            int[] GunsList = new int[] { 10, 15 };
            port.Connect(PortName, 9600, Array.ConvertAll(GunsList, el=>Convert.ToByte(el)));
            return port.IsStart;
        }

        public bool Disconnect ()
        {
            port.Disconnect();
            return !port.IsStart;
        }

        ~Adapter ()
        {
            if (port.IsStart)
            {
                port.Disconnect();
            }
        }


        public bool ChangeShift ( byte GunNumb )
        {
            Request request = new Request();
            request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.changeShift);
            port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse(GunNumb);
            }
            catch (Exception)
            {
            }
            return (getResponse != "");
        }

        public string GetGunPressure ( byte GunNumb )
        {
            if (port.IsStart)
            {
                Request request = new Request();
                request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.pressure);
                port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
                Thread.Sleep(SetTimeout);
                if (port.GetResponse(GunNumb) != "")
                {
                    return port.GetResponse(GunNumb);
                }
            }
            return "";
        }

        public string GetGunStatus ( byte GunNumb )
        {
            if (port.IsStart)
            {
                Request request = new Request();
                request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.status);
                port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
                Thread.Sleep(SetTimeout);
                if (port.GetResponse(GunNumb) != "")
                {
                    return port.GetResponse(GunNumb);
                }
            }
            return "";
        }
        public bool ReadyToWork ( byte GunNumb )
        {
            GetGunStatus(GunNumb);
            byte errorByte = port.GetByteResp(GunNumb, 4);
            if (errorByte == 128 || errorByte == 129 || errorByte == 130)
            {
                return true;
            }
            return false;
        }


        public double GetCacheLit ( byte GunNumb )
        {
            try
            {
                return port.GetCacheLit(GunNumb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return double.NaN;
            }
        }
        public byte GetCacheStatus ( byte GunNumb )
        {
            try
            {
                return port.GetCacheStatus(GunNumb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        //TechFunc   ---------------------------------------------------------------------------------------------------------------------------
        private bool GunStat ( byte GunNumb )
        {
            if (port.IsStart)
            {
                Console.WriteLine("---------Получаем Статус пистолета {0}---------", GunNumb);
                Request request = new Request();
                request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.status);
                port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
                Thread.Sleep(1000);

                //byte byteResp = port.GetByteResp(GunNumb, 4);
               Error currErr = port.GetStatusByte(GunNumb);
                //Console.WriteLine(byteResp);
                Console.WriteLine("---------Возращаем Статус пистолета {0}---------", GunNumb);
                if (currErr.ErrorCode == "84"  || currErr.ErrorCode == "85" )
                {
                    return false;
                }

            }
            return true;
        }
        private double LastLiters ( byte GunNumb )
        {
            if (port.IsStart)
            {
                Request request = new Request();
                request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.curLit, Commands.curLit);
                port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
                Thread.Sleep(SetTimeout);
                if (port.GetResponse(GunNumb) != "")
                {
                    return port.GetLiters(GunNumb, 9, 3);
                }
            }
            return -1.0;
        }

        async private void SetCurrLitAndStat ()
        {
            bool isAnyoneBusy = false;
           // Dictionary<byte, Response> resp = port.responses;
            foreach (byte gunNumb in port.responses.Keys)
            {
                Response currGun = port.responses[gunNumb];
                if (currGun.isStoped)
                {
                    //вызов стоп
                    continue;
                }
                else if (!currGun.isBusy)
                {
                    continue;
                }

                Task<bool> isFinish = Task.Factory.StartNew(() => GunStat(gunNumb));
                await isFinish;
                if (isFinish.Result)
                {
                    Console.WriteLine("---------Finish---------");
                    Console.WriteLine(port.GetCacheLit(gunNumb));
                    Console.WriteLine("---------Finish---------");
                    port.SetCacheStatus(gunNumb, 128);
                    currGun.isBusy = false;
                    continue;
                }
                else
                {
                    Console.WriteLine("Запись Литров");
                    double liters = await Task.Run(() => LastLiters(gunNumb));
                    port.SetCacheLit(gunNumb, liters);
                    isAnyoneBusy = true;
                }
            }
            if (isAnyoneBusy)
            {
                SetCurrLitAndStat();
            }
        }

        //
        public double LastAmount ( byte GunNumb )
        {
            if (port.IsStart)
            {
                Request request = new Request();
                request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.curLit, Commands.curLit);
                port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
                Thread.Sleep(SetTimeout);
                if (port.GetResponse(GunNumb) != "")
                {
                    return port.GetLiters(GunNumb, 9, 4);
                }
            }
            return -1;
        }
        public bool IsPour ( byte GunNumb )
        {
            //GetGunStatus(GunNumb);
            byte CacheSt = port.GetCacheStatus(GunNumb);
            Console.WriteLine(CacheSt);

            if (CacheSt == 132  || CacheSt == 133)
            {
                return true;
            }
            return false;
        }

        public bool CheckConnection ()
        {
            return port.CheckConnection;
        }

        public string LastResponse ( byte GunNumb )
        {
            return port.GetResponse(GunNumb);
        }

        public double LitersShift ( byte GunNumb )
        {
            Request request = new Request();
            request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.litersShift);
            port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            return ((port.GetResponse(GunNumb) == "") ? -1.0 : port.GetLiters(GunNumb, 5, 3));
        }

        public bool NcashShift ( byte GunNumb )
        {
            Request request = new Request();
            request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.NcashShift);
            port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse(GunNumb);
            }
            catch (Exception)
            {
            }
            return (getResponse != "");
        }

        public bool Pause ( byte GunNumb )
        {
            Request request = new Request();
            request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.pause);
            port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse(GunNumb);
            }
            catch (Exception)
            {
            }
            return (getResponse != "");
        }

        public bool PourGasolineLit ( byte GunNumb, int liters, int PriceForLit )
        {
            if (!port.IsStart)
            {
                return false;
            }
            Request request = new Request();
            request.CreateRequest(GunNumb, port.IndexByte, PriceForLit, liters, ByteCounts.pourLit, Commands.pourLit);
            port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse(GunNumb);
                port.SetCacheLit(GunNumb, 0);
                port.SetCacheStatus(GunNumb, 132);
               
                bool isAnyoneBusy = false;
                foreach (byte gunNumb in port.responses.Keys)
                {
                    Response currGun = port.responses[gunNumb];  
                   
                    if (currGun.isBusy)
                    {
                        isAnyoneBusy = true;
                        continue;
                    }
                    else if(gunNumb == GunNumb)
                    {
                        currGun.isBusy = true;
                    }
                   
                }
                if (!isAnyoneBusy)
                {
                    //запуск цикла  
                    SetCurrLitAndStat();
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return getResponse != "";
        }

        public bool PourGasolinePrice ( byte GunNumb, int price, int PriceForLit )
        {
            if (!port.IsStart)
            {
                return false;
            }
            Request request = new Request();
            request.CreateRequest(GunNumb, port.IndexByte, PriceForLit, price, ByteCounts.pourLit, Commands.pourPr);
            port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse(GunNumb);
            }
            catch (Exception)
            {
            }
            return (getResponse != "");
        }

        public bool PourNcachLit ( byte GunNumb, int liters, int PriceForLit )
        {
            Request request = new Request();
            request.CreateRequest(GunNumb, port.IndexByte, PriceForLit, liters, ByteCounts.pourLit, Commands.NcashLit);
            port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse(GunNumb);
            }
            catch (Exception)
            {
            }
            return (getResponse != "");
        }

        public bool PriceShift ( byte GunNumb )
        {
            Request request = new Request();
            request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.priceShift);
            port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse(GunNumb);
            }
            catch (Exception)
            {
            }
            return (getResponse != "");
        }

        public bool Start ( byte GunNumb )
        {
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse(GunNumb);
            }
            catch (Exception)
            {
            }
            return (getResponse != "");
        }

        public bool Stop ( byte GunNumb )
        {
            Request request = new Request();
            request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.stop);
            port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse(GunNumb);
            }
            catch (Exception)
            {
            }
            return (getResponse != "");
        }

        public double TotalLiters ( byte GunNumb )
        {
            Request request = new Request();
            request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.totLit);
            port.SendCommand(GunNumb, Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            return (port.GetResponse(GunNumb) == "") ? -1.0 : port.GetLiters(GunNumb, 5, 4);
        }
    }
}
