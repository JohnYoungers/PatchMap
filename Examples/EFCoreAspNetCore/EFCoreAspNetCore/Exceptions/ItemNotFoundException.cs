using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreAspNetCore.Exceptions
{
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string message) : base(message) { }
    }
}
