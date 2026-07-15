using System;

namespace SinbadStudios.SharedSystems.Runtime
{
    [Serializable]
    public class WalletBalance
    {
        public string status;
        public WalletBalanceData data;
    }

    [Serializable]
    public class WalletBalanceData
    {
        public string userId;
        public int balance;
    }
}
