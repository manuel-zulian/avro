using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;

namespace Avro
{
    public class LogicalTypes
    {
        private static readonly string DECIMAL = "decimal";

        public static LogicalType fromSchema(Schema schema)
        {
            if (!schema.Props.Keys.Contains(LogicalType.LogicalTypeProp))
                return null;

            var typeName = schema.Props[LogicalType.LogicalTypeProp];

            LogicalType logicalType;

            if (typeName == null)
            {
                logicalType = null;
            }
            else if (DECIMAL == typeName)
            {
                logicalType = new Decimal(schema);
            }
            else
            {
                logicalType = null;
            }

            return logicalType;
        }

        /** Decimal represents arbitrary-precision fixed-scale decimal numbers  */
        public class Decimal : LogicalType
        {
            private static readonly string PRECISION_PROP = "precision";
            private static readonly string SCALE_PROP = "scale";

            public int scale { get; set; }
            public int precision { get; set; }

            private Decimal(int precision, int scale) : base(DECIMAL)
            {
                this.precision = precision;
                this.scale = scale;
            }

            public Decimal(Schema schema) : base(DECIMAL)
            {
                if (!HasProperty(schema, PRECISION_PROP))
                {
                    throw new Exception(
                        "Invalid decimal: missing precision");
                }

                this.precision = getInt(schema, PRECISION_PROP);

                if (HasProperty(schema, SCALE_PROP))
                {
                    this.scale = getInt(schema, SCALE_PROP);
                }
                else
                {
                    this.scale = 0;
                }
            }

            public static decimal ConvertToDecimal(byte[] bytes, int scale)
            {
                var result = 0m;
                var bn = new BigInteger(bytes);
                for(var i = bytes.Length - 1; i >=0 ; i--)
                {
                    result += (bytes[i]) << ((bytes.Length - 1 - i) * 8);
                }
            
                result = result * (decimal) Math.Pow(10, -scale);
                return result;
            }

            private static bool HasProperty(Schema schema, String name)
            {
                return (schema.Props.ContainsKey(name));
            }

            private int getInt(Schema schema, string name)
            {
                var obj = schema.Props[name];
                return Convert.ToInt32(obj);
            }
        }
    }
}
