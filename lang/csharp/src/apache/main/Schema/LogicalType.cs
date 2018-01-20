using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avro
{
    public class LogicalType
    {
        public static readonly string LogicalTypeProp = "logicalType";

        public string Name { get; }

        public LogicalType(string name)
        {
            this.Name = name;
        }
    }
}
