﻿using System;

namespace DataChain.Infrastructures
{
    public class InvalidTransactionException : Exception
    {
        public string Reason { get; set; }

        public InvalidTransactionException(string _reason):
            base(string.Format($"Transaction was rejected {_reason}"))
        {
            this.Reason = _reason;
        }
    }
}
