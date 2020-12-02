namespace EraDll
{
    internal class Errors
    {
        private static readonly byte[] pourStat = 
        {
            132,
           133,
        };
        public Errors ( string ErrorCode, string ErrorMessage, string ErrorShDescription, string ErrorDescription, bool IsError = false )
        {
            this.ErrorCode = ErrorCode;
            this.ErrorMessage = ErrorMessage;
            this.ErrorShDescription = ErrorShDescription;
            this.ErrorDescription = ErrorDescription;
            RespBytes = 9;
            this.IsError = IsError;
        }

        public Errors ( string ErrorCode, string ErrorMessage, string ErrorShDescription, string ErrorDescription, int RespBytes, bool IsError = false )
        {
            this.ErrorCode = ErrorCode;
            this.ErrorMessage = ErrorMessage;
            this.ErrorShDescription = ErrorShDescription;
            this.ErrorDescription = ErrorDescription;
            this.RespBytes = RespBytes;
            this.IsError = IsError;
        }

        public string ErrorCode { get; private set; }

        public string ErrorMessage { get; private set; }

        public string ErrorShDescription { get; private set; }

        public string ErrorDescription { get; private set; }

        public int RespBytes { get; private set; }

        public bool IsError { get; private set; }
        
        public static byte[] GetPourStatuses ()
        {
            return pourStat;
        }
    }
}
