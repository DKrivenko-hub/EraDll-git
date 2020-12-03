using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;


namespace EraDll
{
    using CuctomExt;
    class Port
    {                       
    
        private SerialPort _sp = new SerialPort();
        private readonly Dictionary<byte, Response> responses = new Dictionary<byte, Response>();
       
        // 
        private byte lastGunNumb;
        private byte lastAsyncGunNumb;
        private bool asyncStatus = false;
        private bool isBusy = false;

        public bool CheckConnection => _sp.IsOpen;
        public byte IndexByte { get; private set; } = 0;
        public string GetResponse ( byte GunNumb, bool isAsync = false ) => !isAsync ? responses[GunNumb].ParseResponse() : responses[GunNumb].ParseResponse();
        public double GetCacheLit ( byte GunNumb ) => responses[GunNumb].CacheGunLit;
        public byte GetCacheStatus ( byte GunNumb ) => responses[GunNumb].CacheGunStatus;
        public void SetCacheLit ( byte GunNumb, double liters ) => responses[GunNumb].CacheGunLit = liters;
        public void SetCacheStatus ( byte GunNumb, byte status ) => responses[GunNumb].CacheGunStatus = status;
        public byte GetByteResp ( byte GunNumb, int index ) => responses[GunNumb].GetResponse[index];
        public Errors GetStatusByte ( byte GunNumb ) => responses[GunNumb].CurrError;

        public bool GetParseStatus { get; private set; }
        public bool IsStart { get; private set; }


        public bool Connect ( string PortName, int Speed )
        {
            IsStart = false;
            this._sp = new SerialPort(PortName, Speed, Parity.None, 8, StopBits.One);
            this._sp.DataReceived += new SerialDataReceivedEventHandler(this.Receiver);

            this._sp.Handshake = Handshake.None;
            this._sp.WriteTimeout = 500;
            this._sp.ReadTimeout = 500;
            try
            {
                this._sp.Open();
            }
            catch (Exception exception1)
            {
                Exception local1 = exception1;
                Console.WriteLine(local1.Message);
                //     throw local1;
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
        private void SetResponse ( byte[] response, bool isAsync = false )
        {
            if (isAsync)
            {
                if (responses.ContainsKey(lastGunNumb))
                    responses[lastAsyncGunNumb].GetAsyncResponse.AddRange(response);
                else
                    responses.Add(lastAsyncGunNumb, new Response(response, true));
            }
            else
            {
                if (responses.ContainsKey(lastGunNumb))
                    responses[lastGunNumb].GetResponse.AddRange(response);
                else
                    responses.Add(lastGunNumb, new Response(response));
            }

        }
        private void Receiver ( object sender, SerialDataReceivedEventArgs e )
        {
            if (IsStart && _sp.IsOpen)
            {
                SerialPort port = (SerialPort)sender;
                int bytesToRead = port.BytesToRead;
                GetParseStatus = true;
                int num2 = 0;
                byte[] tmp = new byte[bytesToRead];
                while (true)
                {
                    if (num2 >= bytesToRead)
                    {
                        if (GetParseStatus)
                        {
                            GetParseStatus = false;
                            isBusy = false;
                            asyncStatus = false;
                        }
                        break;
                    }
                    tmp[num2] = (byte)port.ReadByte();
                    num2++;
                }
                SetResponse(tmp, asyncStatus && isBusy);
            }
        }

        public async void Work (byte[] command)
        {
            try
            {
                var data = await _sp.ReadAsync(command);

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void SendCommand ( byte GunNumb, byte[] command )
        {
            if (IsStart)
            { 
                isBusy = true;
                lastGunNumb = GunNumb;
                if (this.responses.ContainsKey(GunNumb))
                    this.responses[GunNumb].ClearResponse();
                this._sp.Write(command, 0, command.Length);
                byte indexByte = IndexByte;
                IndexByte = (byte)(indexByte + 1);
            }
        }
        public void SendAsyncCommand ( byte GunNumb, byte[] command )
        {
            if (IsStart)
            {
                asyncStatus = true;
                isBusy = true;
                lastAsyncGunNumb = GunNumb;
                if (this.responses.ContainsKey(GunNumb))
                    this.responses[GunNumb].ClearAsyncResponse();
                this._sp.Write(command, 0, command.Length);
                Work(command);
                byte indexByte = IndexByte;
                IndexByte = (byte)(indexByte + 1);
            }
        }

        public double GetLiters ( byte GunNumb, int StartByte, int count )
        {
            return this.responses[GunNumb].ParseLiters(StartByte, count);
        }


    }

}

