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

    class Request
    {
        public string GetRequest { get; private set; } = "2D ";

        private string IndexHex;


        public string CreateRequest ( byte GunNumb, byte indexByte, ByteCounts bytes, Commands command )
        {
            IndexHex = Converter.ByteToHex(indexByte);
            GetRequest += Converter.ByteToHex(GunNumb) +
                " " +
                IndexHex +
                " " +
                Converter.ByteToHex((byte)bytes) +
                " " +
                Converter.ByteToHex((byte)command);
            GetRequest += CRC16.GetCrc(Converter.HexToBytes(GetRequest));
            return GetRequest;
        }

        public string CreateRequest ( byte GunNumb, byte indexByte, int PriceForLit, int param, ByteCounts bytes, Commands command )
        {
            IndexHex = Converter.ByteToHex(indexByte);
            GetRequest += Converter.ByteToHex(GunNumb) +
                " " +
                this.IndexHex +
                " " +
                Converter.ByteToHex((byte)bytes) +
                " " +
                Converter.ByteToHex((byte)command) +
                " 00 00 " +
                Converter.IntToHex(param) +
                " " +
                Converter.PriceForLitrToHex(PriceForLit);
            GetRequest += CRC16.GetCrc(Converter.HexToBytes(GetRequest));
            return GetRequest;
        }

    }
}
