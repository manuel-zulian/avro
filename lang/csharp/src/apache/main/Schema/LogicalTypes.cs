using System;
using System.Collections.Generic;
using System.Linq;
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

            var typeName = JsonConvert.DeserializeObject<string>(schema.Props[LogicalType.LogicalTypeProp]);

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

            private readonly int precision;
            private readonly int scale;

            private Decimal(int precision, int scale) : base(DECIMAL)
            {
                this.precision = precision;
                this.scale = scale;
            }

            public Decimal(Schema schema) : base(DECIMAL)
            {
                if (!hasProperty(schema, PRECISION_PROP))
                {
                    //throw new Exception(
                    //    "Invalid decimal: missing precision");
                }

                //this.precision = getInt(schema, PRECISION_PROP);

                if (hasProperty(schema, SCALE_PROP))
                {
                    this.scale = getInt(schema, SCALE_PROP);
                }
                else
                {
                    this.scale = 0;
                }
            }

            public int getPrecision()
            {
                return precision;
            }

            public int getScale()
            {
                return scale;
            }

            private bool hasProperty(Schema schema, String name)
            {
                return (schema.Props.ContainsKey(name));
            }

            private int getInt(Schema schema, String name)
            {
                var obj = schema.Props[name];
                return Convert.ToInt32(obj);
            }
        }
    }
}
