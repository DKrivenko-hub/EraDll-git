using System;
using System.Collections.Generic;
using System.IO.Ports;


namespace EraDll
{
    class Port
    {                       
    
        private SerialPort _sp = new SerialPort();
        public readonly Dictionary<byte, Response> responses = new Dictionary<byte, Response>();
        public Dictionary<byte, string> responseList { get; private set; } = new Dictionary<byte, string>();

        // 
        private byte lastGunNumb;
        //private byte lastAsyncGunNumb;
        //private bool asyncStatus = false;
        //private bool isBusy = false;

        public bool CheckConnection => _sp.IsOpen;
        public byte IndexByte { get; private set; } = 0;
        public string GetResponse ( byte GunNumb ) =>responses[GunNumb].ParseResponse();
        public double GetCacheLit ( byte GunNumb ) => responses[GunNumb].CacheGunLit;
        public byte GetCacheStatus ( byte GunNumb ) => responses[GunNumb].CacheGunStatus;
        public void SetCacheLit ( byte GunNumb, double liters ) => responses[GunNumb].CacheGunLit = liters;
        public void SetCacheStatus ( byte GunNumb, byte status ) => responses[GunNumb].CacheGunStatus = status;
        public void SetGunIsBusy( byte GunNumb, bool isBusy) => responses[GunNumb].isBusy = isBusy;
        public void SetGunIsStop( byte GunNumb, bool isStop) => responses[GunNumb].isBusy = isStop;
        public byte GetByteResp ( byte GunNumb, int index ) => responses[GunNumb].GetResponse[index];
        public Errors GetStatusByte ( byte GunNumb ) => responses[GunNumb].CurrError;

        public bool GetParseStatus { get; private set; }
        public bool IsStart { get; private set; }


        public bool Connect ( string PortName, int Speed, byte[] GunsList )
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
                for (int i = 0; i < GunsList.Length; i++)
                {
                    responses.Add(GunsList[i], new Response());
                }
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
                //if (responses.ContainsKey(lastGunNumb))
                //    responses[lastAsyncGunNumb].GetAsyncResponse.AddRange(response);
                //else
                //    responses.Add(lastAsyncGunNumb, new Response(response, true));
            }
            else
            {
                //if (responses.ContainsKey(lastGunNumb))
                //    responses[lastGunNumb].GetResponse.AddRange(response);
                //else
                //{
                //    responses.Add(lastGunNumb, new Response(response));
                //    responses[lastGunNumb].ResponseCode = response[4];
                //}


                //if(response[0] == 45 && response[1]==lastGunNumb && response[2] == (IndexByte-1))
                //{
                //    //начало сообщения

                //}        
                //else
                //{
                //    //конец сообщения
                //}

                responseList[--IndexByte] += string.Join(" ", response);

               //responses[lastGunNumb].ParseResponse();
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
                        }
                        break;
                    }
                    tmp[num2] = (byte)port.ReadByte();
                    num2++;
                }
                SetResponse(tmp);
            }
        }

        //private async void Work (byte GunNumb, byte[] command)
        //{
        //    try
        //    {
        //        byte[] data = await _sp.ReadAsync(command);
        //        SetResponse()
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.WriteLine(exception.Message);
        //    }
        //}

        public void SendCommand ( byte GunNumb, byte[] command )
        {
            if (IsStart)
            {
                lastGunNumb = GunNumb;
                Console.WriteLine(lastGunNumb);
                if (this.responses.ContainsKey(GunNumb))
                    this.responses[GunNumb].ClearResponse();
                this._sp.Write(command, 0, command.Length);
                byte indexByte = IndexByte;
                responseList.Add( IndexByte , "");
                IndexByte = (byte)(indexByte + 1);
            }
        }
        //public void SendAsyncCommand ( byte GunNumb, byte[] command )
        //{
        //    if (IsStart)
        //    {
                
        //        if (this.responses.ContainsKey(GunNumb))
        //            this.responses[GunNumb].ClearAsyncResponse();
        //        this._sp.Write(command, 0, command.Length);
        //       // Work(command);
        //        byte indexByte = IndexByte;
        //        IndexByte = (byte)(indexByte + 1);
        //    }
        //}

        public double GetLiters ( byte GunNumb, int StartByte, int count )
        {
            return this.responses[GunNumb].ParseLiters(StartByte, count);
        }


    }

}

