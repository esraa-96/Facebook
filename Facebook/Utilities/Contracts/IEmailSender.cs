﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facebook.Contracts
{
    public interface IEmailSender
    {
        void Send(string toAddress, string subject, string body);
    }
}
