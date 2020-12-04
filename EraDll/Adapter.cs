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

       // string LastResponse ( byte GunNumb );

        //bool Start ( byte GunNumb );
        //bool Pause ( byte GunNumb );
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

        private byte indexByte = 0;

        public bool Connect ()
        {
            int[] GunsList = new int[] { 10, 15 };
            port.Connect(PortName, 9600, Array.ConvertAll(GunsList, el => Convert.ToByte(el)));
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
            indexByte = (byte)new Random(indexByte).Next(0, 255);
            byte localIndByte = indexByte;
            Request request = new Request();
            request.CreateRequest(GunNumb,localIndByte, ByteCounts.status, Commands.changeShift);
            port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest));
            indexByte++;
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse(localIndByte);
            }
            catch (Exception)
            {
            }
            return getResponse != "";
        }

        public string GetGunPressure ( byte GunNumb )
        {
            if (port.IsStart)
            {
                indexByte = (byte)new Random(indexByte).Next(0, 255);
                byte localIndByte = indexByte;
                Request request = new Request();
                request.CreateRequest(GunNumb,localIndByte, ByteCounts.status, Commands.pressure);
                port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest)); indexByte++;
                Thread.Sleep(SetTimeout);
                if (port.GetResponse(GunNumb) != "")
                {
                    return port.GetResponse(localIndByte);
                }
            }
            return "";
        }

        public string GetGunStatus ( byte GunNumb )
        {
            if (port.IsStart)
            {
                indexByte = (byte)new Random(indexByte).Next(0, 255);
                byte localIndByte = indexByte;
                Request request = new Request();
                request.CreateRequest(GunNumb,localIndByte, ByteCounts.status, Commands.status);
                port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest)); 
                indexByte++;
                Thread.Sleep(SetTimeout);
                if (port.GetResponse(localIndByte) != "")
                {
                    return port.GetResponse( localIndByte);
                }
            }
            return "";
        }
        public bool ReadyToWork ( byte GunNumb )
        {
            //GetGunStatus(GunNumb);
            //byte errorByte = port.GetByteResp(GunNumb, 4);
           return !port.responseGun[GunNumb].isBusy;
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
                indexByte = (byte)new Random(indexByte).Next(0, 255);
                byte localIndByte = indexByte;
                Console.WriteLine("---------Получаем Статус пистолета {0}---------", GunNumb);
                Request request = new Request();
                request.CreateRequest(GunNumb,localIndByte, ByteCounts.status, Commands.status);
                port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest));
                indexByte++;
                Thread.Sleep(2000);

                //byte byteResp = port.GetByteResp(GunNumb, 4);
                //Error currErr = port.GetCurrError( localIndByte);
                byte currErr = port.ResponseList[localIndByte].response[4];
                //Console.WriteLine(byteResp);
                Console.WriteLine("---------Возращаем Статус пистолета {0}---------", GunNumb);
                if (currErr == 132 || currErr == 133)
                {
                    return false;
                }    
                return true;
            }
            return false;
        }
        private double LastLiters ( byte GunNumb )
        {
            if (port.IsStart)
            {
                indexByte = (byte)new Random(indexByte).Next(0, 255);
                byte localIndByte = indexByte;
                Request request = new Request();
                request.CreateRequest(GunNumb,localIndByte, ByteCounts.curLit, Commands.curLit);
                port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest));
                indexByte++;
                Thread.Sleep(SetTimeout);
                if (port.GetResponse(localIndByte) != "")
                {
                    return port.GetLiters(localIndByte, 9, 3);
                }
            }
            return -1.0;
        }

        async private void SetCurrLitAndStat ()
        {
            bool isAnyoneBusy = false;

            foreach (byte gunNumb in port.responseGun.Keys)
            {
                Gun currGun = port.responseGun[gunNumb]; 
           
                if (currGun.isStop)
                {
                    //вызов стоп
                    continue;
                }
                else if (!currGun.isBusy)
                {
                    continue;
                }
                //byte localIndByte = indexByte;
                Task<bool> isFinish = Task.Factory.StartNew(() => GunStat(currGun.GunNumb));
                await isFinish;
                if (isFinish.Result)
                {
                    Console.WriteLine("---------Finish---------");
                    Console.WriteLine(port.GetCacheLit(currGun.GunNumb));
                    Console.WriteLine("---------Finish---------");
                    port.SetCacheStatus(gunNumb, 128);
                    currGun.isBusy = false;
                    currGun.isStop = false;
                    continue;
                }
                else
                {
                    Console.WriteLine("Запись Литров");
                    double liters = LastLiters(gunNumb);
                    //double liters = await Task.Run(() => LastLiters(currGun.GunNumb));
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
                indexByte = (byte)new Random(indexByte).Next(0, 255);
                byte localIndByte = indexByte;
                Request request = new Request();
                request.CreateRequest(GunNumb,localIndByte, ByteCounts.curLit, Commands.curLit);
                port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest)); 
                indexByte++;
                Thread.Sleep(SetTimeout);
                if (port.GetResponse(GunNumb) != "")
                {
                    return port.GetLiters( localIndByte, 9, 4);
                }
            }
            return -1;
        }
        public bool IsPour ( byte GunNumb )
        {
            indexByte = (byte)new Random(indexByte).Next(0, 255);
            byte localIndByte = indexByte;
            //GetGunStatus(GunNumb);
            byte CacheSt = port.GetCacheStatus(GunNumb);
            Console.WriteLine(CacheSt);

            if (CacheSt == 132 || CacheSt == 133)
            {
                return true;
            }
            return false;
        }

        public bool CheckConnection ()
        {
            return port.CheckConnection;
        }

        private string LastResponse ( byte GunNumb )
        {
            return port.GetResponse(GunNumb);
        }

        public double LitersShift ( byte GunNumb )
        {
            indexByte = (byte)new Random(indexByte).Next(0, 255);
            byte localIndByte = indexByte;
            Request request = new Request();
            request.CreateRequest(GunNumb,localIndByte, ByteCounts.status, Commands.litersShift);
            port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest)); 
            indexByte++;
            Thread.Sleep(SetTimeout);
            return (port.GetResponse(localIndByte) == "") ? -1.0 : port.GetLiters(GunNumb+localIndByte, 5, 3);
        }

        public bool NcashShift ( byte GunNumb )
        {
            indexByte = (byte)new Random(indexByte).Next(0, 255);
            byte localIndByte = indexByte;
            Request request = new Request();
            request.CreateRequest(GunNumb,localIndByte, ByteCounts.status, Commands.NcashShift);
            port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest));
            indexByte++;
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse( localIndByte);
            }
            catch (Exception)
            {
            }
            return getResponse != "";
        }

        private bool Pause ( byte GunNumb )
        {
            indexByte = (byte)new Random(indexByte).Next(0, 255);
            byte localIndByte = indexByte;
            Request request = new Request();
            request.CreateRequest(GunNumb,localIndByte, ByteCounts.status, Commands.pause);
            port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest));
            indexByte++;
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse( localIndByte);
            }
            catch (Exception)
            {
            }
            return getResponse != "";
        }

        public bool PourGasolineLit ( byte GunNumb, int liters, int PriceForLit )
        {
            indexByte = (byte)new Random(indexByte).Next(0, 255);
            byte localIndByte = indexByte;
            if (!port.IsStart)
            {
                return false;
            }
            Request request = new Request();
            request.CreateRequest(GunNumb,localIndByte, PriceForLit, liters, ByteCounts.pourLit, Commands.pourLit);
            port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest));
            indexByte++;
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            //try
            //{
                getResponse = port.GetResponse( localIndByte);
                port.SetCacheLit(GunNumb, 0);
                port.SetCacheStatus(GunNumb , 132);

                bool isAnyoneBusy = false;
                foreach (byte gunNumb in port.responseGun.Keys)
                {
                    Gun currGun = port.responseGun[gunNumb];

                    if (currGun.isBusy)
                    {
                        isAnyoneBusy = true;
                        continue;
                    }
                    else if (gunNumb == GunNumb)
                    {
                        currGun.isBusy = true;
                    }
                }
                if (!isAnyoneBusy)
                {
                    //запуск цикла  
                    SetCurrLitAndStat();
                }

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    throw ex;
            //}
            return getResponse != "";
        }

        public bool PourGasolinePrice ( byte GunNumb, int price, int PriceForLit )
        {
            indexByte = (byte)new Random(indexByte).Next(0, 255);
            byte localIndByte = indexByte;
            if (!port.IsStart)
            {
                return false;
            }
            Request request = new Request();
            request.CreateRequest(GunNumb,localIndByte, PriceForLit, price, ByteCounts.pourLit, Commands.pourPr);
            port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest));
            indexByte++;
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse( localIndByte);
            }
            catch (Exception) { }
            return getResponse != "";
        }

        public bool PourNcachLit ( byte GunNumb, int liters, int PriceForLit )
        {
            indexByte = (byte)new Random(indexByte).Next(0, 255);
            byte localIndByte = indexByte;
            Request request = new Request();
            request.CreateRequest(GunNumb, localIndByte, PriceForLit, liters, ByteCounts.pourLit, Commands.NcashLit);
            port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest)); 
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse( localIndByte);
            }
            catch (Exception)
            {
            }
            return getResponse != "";
        }

        public bool PriceShift ( byte GunNumb )
        {
            indexByte = (byte)new Random(indexByte).Next(0, 255);
            byte localIndByte = indexByte;
            Request request = new Request();
            request.CreateRequest(GunNumb,localIndByte, ByteCounts.status, Commands.priceShift);
            port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest)); 
            indexByte++;
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse( localIndByte);
            }
            catch (Exception)
            {
            }
            return getResponse != "";
        }

        public bool Start ( byte GunNumb )
        {
            indexByte = (byte)new Random(indexByte).Next(0, 255);
            byte localIndByte = indexByte;
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse(GunNumb);
            }
            catch (Exception)
            {
            }
            return getResponse != "";
        }

        public bool Stop ( byte GunNumb )
        {

            indexByte = (byte)new Random(indexByte).Next(0, 255);
            byte localIndByte = indexByte;
            Request request = new Request();
            request.CreateRequest(GunNumb,localIndByte, ByteCounts.status, Commands.stop);
            port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest));
            indexByte++;
            Thread.Sleep(SetTimeout);
            string getResponse = "";
            try
            {
                getResponse = port.GetResponse( localIndByte);
            }
            catch (Exception)
            {
            }
            if(getResponse != "")
            {
                port.responseGun[GunNumb].isStop = true;
                return true;
            }
            return false;

        }

        public double TotalLiters ( byte GunNumb )
        {
            indexByte = (byte)new Random(indexByte).Next(0, 255);
            byte localIndByte = indexByte;
            Request request = new Request();
            request.CreateRequest(GunNumb,localIndByte, ByteCounts.status, Commands.totLit);
            port.SendCommand(GunNumb,localIndByte, Converter.HexToBytes(request.GetRequest));
            indexByte++;
            Thread.Sleep(SetTimeout);
            return (port.GetResponse(localIndByte) == "") ? -1.0 : port.GetLiters(localIndByte, 5, 4);
        }
    }
}
