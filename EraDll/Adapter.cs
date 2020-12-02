using System;
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

        string LastResponse ();

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
            port.Connect(PortName, 9600);
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
            port.SendCommand(Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse;
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
                port.SendCommand(Converter.HexToBytes(request.GetRequest));
                Thread.Sleep(SetTimeout);
                if (port.GetResponse != "")
                {
                    return port.GetResponse;
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
                port.SendCommand(Converter.HexToBytes(request.GetRequest));
                Thread.Sleep(SetTimeout);
                if (port.GetResponse != "")
                {
                    return port.GetResponse;
                }
            }
            return "";
        }
        public bool ReadyToWork ( byte GunNumb )
        {
            GetGunStatus(GunNumb);
            Errors GunStatus = port.GetStatusByte;
            if (GunStatus.ErrorCode == "80" || GunStatus.ErrorCode == "81" || GunStatus.ErrorCode == "82")
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

        //TechFunc
        private bool GunStat ( byte GunNumb )
        {
            if (port.IsStart)
            {
                Request request = new Request();
                request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.status);
                port.SendCommand(Converter.HexToBytes(request.GetRequest));
                Thread.Sleep(2000);
                byte byteResp = port.GetByteResp(4);
                if (byteResp == 132 || byteResp == 133)
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
                port.SendCommand(Converter.HexToBytes(request.GetRequest));
                Thread.Sleep(SetTimeout);
                if (port.GetResponse != "")
                {
                    return port.GetLiters(9, 3);
                }
            }
            return -1.0;
        }

        async private void SetCurrLitAndStat ( byte GunNumb )
        {
            Task<bool> isFinish = Task.Factory.StartNew(() => GunStat(GunNumb));
            await isFinish;
            if (isFinish.Result)
            {
                Console.WriteLine("---------Finish---------");
                Console.WriteLine(port.GetCacheLit(GunNumb));
                Console.WriteLine("---------Finish---------");
                port.SetCacheStatus(GunNumb, 128);
                return;
            }
            else
            {
                Console.WriteLine("Запись Литров");
                double liters = await Task.Run(() => LastLiters(GunNumb));
                port.SetCacheLit(GunNumb, liters);
                SetCurrLitAndStat(GunNumb);
            }
        }

        //
        public double LastAmount ( byte GunNumb )
        {
            if (port.IsStart)
            {
                Request request = new Request();
                request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.curLit, Commands.curLit);
                port.SendCommand(Converter.HexToBytes(request.GetRequest));
                Thread.Sleep(SetTimeout);
                if (port.GetResponse != "")
                {
                    return port.GetLiters(9, 4);
                }
            }
            return -1;
        }
        public bool IsPour ( byte GunNumb )
        {
            //GetGunStatus(GunNumb);
            byte CacheSt = port.GetCacheStatus(GunNumb);
            if (Array.IndexOf(Errors.GetPourStatuses(), CacheSt) != -1)
            {
                return true;
            }
            return false;
        }

        public bool CheckConnection ()
        {
            return port.CheckConnection;
        }

        public string LastResponse ()
        {
            return port.GetResponse;
        }

        public double LitersShift ( byte GunNumb )
        {
            Request request = new Request();
            request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.litersShift);
            port.SendCommand(Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            return ((port.GetResponse == "") ? -1.0 : port.GetLiters(5, 3));
        }

        public bool NcashShift ( byte GunNumb )
        {
            Request request = new Request();
            request.CreateRequest(GunNumb, port.IndexByte, ByteCounts.status, Commands.NcashShift);
            port.SendCommand(Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse;
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
            port.SendCommand(Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse;
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
            port.SendCommand(Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse;
                port.SetCacheLit(GunNumb, 0);
                port.SetCacheStatus(GunNumb, 132);
                SetCurrLitAndStat(GunNumb);
            }
            catch (Exception)
            {

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
            port.SendCommand(Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse;
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
            port.SendCommand(Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse;
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
            port.SendCommand(Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse;
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
                getResponse = port.GetResponse;
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
            port.SendCommand(Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse;
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
            port.SendCommand(Converter.HexToBytes(request.GetRequest));
            Thread.Sleep(SetTimeout);
            return ((port.GetResponse == "") ? -1.0 : port.GetLiters(5, 4));
        }
    }
}
