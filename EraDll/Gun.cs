namespace EraDll
{
    class Gun
    {
        public byte GunNumb; 

        public bool isBusy = false;
        public bool isStop = false;

        public byte CacheStatus = 128;
        public double CacheLiters = 0;

        public Gun(byte GunNumb )
        {
            this.GunNumb = GunNumb;
        }
    }
}
