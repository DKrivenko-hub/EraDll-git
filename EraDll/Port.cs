using System;
using System.IO.Ports;

namespace EraDll
{
    class Port
    {
        private SerialPort _sp = new SerialPort();
        private readonly Response response = new Response();

        public bool CheckConnection => _sp.IsOpen;
        public byte IndexByte { get; private set; } = 0;
        public string GetResponse => response.ParseResponse();
        public int GetCacheLit ( byte GunNumb ) => response.CacheLit[GunNumb];
        public int SetCacheLit ( byte GunNumb ) = response.
        public bool GetParseStatus { get; private set; }

        public byte GetByteResp ( int index ) => response.GetResponse[index];
        public Errors GetStatusByte => response.CurrError;

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
                throw local1;
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
                SerialPort port = (SerialPort)sender;
                int bytesToRead = port.BytesToRead;
                GetParseStatus = true;
                int num2 = 0;
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
                    response.GetResponse.Add((byte)port.ReadByte());
                    num2++;
                }
            }
        }

        public void SendCommand ( byte[] command )
        {
            if (IsStart)
            {
                this.response.ClearResponse();
                this._sp.Write(command, 0, command.Length);
                byte indexByte = IndexByte;
                IndexByte = (byte)(indexByte + 1);
            }
        }

        public double GetLiters ( int StartByte, int count )
        {
            return this.response.ParseLiters(StartByte, count);
        }


    }

}

