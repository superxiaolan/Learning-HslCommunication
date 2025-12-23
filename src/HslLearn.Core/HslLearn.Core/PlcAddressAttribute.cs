using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HslLearn.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PlcAddressAttribute : Attribute
    {
        public string Address { get; }
        public PlcAddressAttribute(string address) => Address = address;
    }
}
