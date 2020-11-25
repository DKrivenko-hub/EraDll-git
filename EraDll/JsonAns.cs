namespace EraDll
{
    class JsonAns
    {
        public string ANSWER { get; set; }
        public string NAME { get; set; }
        public int SENSORS { get; set; }
        public byte RS485_ADRESS { get; set; }
        public int[] IP_ADDRESS { get; set; }
        public int[] NETMASK { get; set; }
        public int[] GATEWAY { get; set; }
    }
}
