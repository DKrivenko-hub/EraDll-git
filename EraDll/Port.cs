using System;
using System.Collections.Generic;
using System.IO.Ports;


namespace EraDll
{
    class Port
    {

        private SerialPort _sp = new SerialPort();
        // public readonly Dictionary<byte, Response> responses = new Dictionary<byte, Response>();
        public Dictionary<int, Response> ResponseList { get; private set; } = new Dictionary<int, Response>();

        public Dictionary<byte, Gun> responseGun = new Dictionary<byte, Gun>();

        private byte lastGunNumb;
        private int lastInsertInd = 0;

        public bool CheckConnection => _sp.IsOpen;
        public byte IndexByte { get; private set; } = 0;

        public string GetResponse ( int index ) => ResponseList[index].ParseResponse();

        public double GetCacheLit ( byte GunNumb ) => responseGun[GunNumb].CacheLiters;
        public byte GetCacheStatus ( byte GunNumb ) => responseGun[GunNumb].CacheStatus;

        public void SetCacheLit ( byte GunNumb, double liters ) => responseGun[GunNumb].CacheLiters = liters;
        public void SetCacheStatus ( byte GunNumb, byte status ) => responseGun[GunNumb].CacheStatus = status;

        public byte GetByteResp ( int index, int byteIndex ) => ResponseList[index].response[byteIndex];
        public Error GetCurrError ( int index ) => ResponseList[index].CurrError;

        public bool GetParseStatus { get; private set; }
        public bool IsStart { get; private set; }

        public bool Connect ( string PortName, int Speed, byte[] GunsList )
        {
            IsStart = false;
            _sp = new SerialPort(PortName, Speed, Parity.None, 8, StopBits.One);
            _sp.DataReceived += new SerialDataReceivedEventHandler(this.Receiver);

            _sp.Handshake = Handshake.None;
            _sp.WriteTimeout = 500;
            _sp.ReadTimeout = 500;
            try
            {
                _sp.Open();
               
                for (int i = 0; i < GunsList.Length; i++)
                {
                    responseGun.Add(GunsList[i], new Gun(GunsList[i]));
                }
            }
            catch (Exception exception1)
            {
                Exception local1 = exception1;
                Console.WriteLine(local1.Message);
            }
            if (this._sp.IsOpen)
            {
                IsStart = true;
            }
            else
            {
                Console.WriteLine("Не удалось открыть порт");
            }
            return IsStart;
        }
        public void Disconnect ()
        {
            if (this._sp.IsOpen)
            {
                try
                {
                    this.IsStart = false;
                    this._sp.Close();
                }
                catch (Exception exception1)
                {
                    Console.WriteLine(exception1.Message);
                    throw;
                }
            }
        }

        private void Receiver ( object sender, SerialDataReceivedEventArgs e )
        {
            if (IsStart && _sp.IsOpen)
            {
                int localIndex = lastInsertInd;
                
                SerialPort port = (SerialPort)sender;
                int bytesToRead = port.BytesToRead;
                GetParseStatus = true;
                int num2 = 0;
                //byte[] tmp = new byte[bytesToRead];
                while (true)
                {
                    if (num2 >= bytesToRead)
                    {
                        break;
                    }
                    ResponseList[localIndex].response.Add((byte)port.ReadByte());
                    num2++;
                }
            }
        }

        public void SendCommand ( byte GunNumb, byte index, byte[] command )
        {
            if (IsStart)
            {
                lastGunNumb = GunNumb;
                Console.WriteLine(lastGunNumb);
                if (ResponseList.ContainsKey(index))
                {
                    ResponseList[index].ClearResponse();
                    ResponseList.Remove(index);
                }
                this._sp.Write(command, 0, command.Length);
                ResponseList.Add(index, new Response());
                ResponseList[index].GunNumb = GunNumb;
                lastInsertInd = index;
                IndexByte = (byte)(IndexByte + 1);
            }
        }

        public double GetLiters ( int index, int StartByte, int count )
        {
            return ResponseList[index].ParseLiters(StartByte, count);
        }




    }

}

