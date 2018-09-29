using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreWebApi.Exceptions
{
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string message) : base(message) { }
    }
}
