/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Avro;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avro
{
    /**
     * Following roughly https://github.com/apache/avro/blob/master/lang/java/avro/src/main/java/org/apache/avro/LogicalTypes.java
     */
    public class LogicalTypes
    {
        public interface ILogicalTypeFactory
        {
            LogicalType FromSchema(Schema schema);
        }

        private static readonly ConcurrentDictionary<string, ILogicalTypeFactory> RegisteredTypes =
            new ConcurrentDictionary<string, ILogicalTypeFactory>();

        public static void Register(string logicalTypeName, ILogicalTypeFactory factory)
        {
            if (logicalTypeName == null)
            {
                throw new NullReferenceException("Invalid logical type name: null");
            }

            if (factory == null)
            {
                throw new NullReferenceException("Invalid logical type factory: null");
            }

            RegisteredTypes.TryAdd(logicalTypeName, factory);
        }

        /**
         * Returns the {@link LogicalType} from the schema, if one is present.
         */
        public static LogicalType FromSchema(Schema schema) {
            return FromSchemaImpl(schema, true);
        }

        public static LogicalType FromSchemaIgnoreInvalid(Schema schema) {
            return FromSchemaImpl(schema, false);
        }

        private const string DECIMAL = "decimal";
        private const string UUID_TAG = "uuid";
        private const string DATE = "date";
        private const string TIME_MILLIS = "time-millis";
        private const string TIME_MICROS = "time-micros";
        private const string TIMESTAMP_MILLIS = "timestamp-millis";
        private const string TIMESTAMP_MICROS = "timestamp-micros";

        private static LogicalType FromSchemaImpl(Schema schema, bool throwErrors)
        {
            schema.Props.TryGetValue(LogicalType.LogicalTypeProp, out var typeName);
            LogicalType logicalType;

            switch (typeName)
            {
                case DECIMAL:
                    logicalType = new Decimal(schema);
                    break;
                case UUID_TAG:
                    logicalType = UUID.getInstance();
                    break;
                case DATE:
                    logicalType = Date.GetInstance();
                    break;
                case TIMESTAMP_MICROS:
                    logicalType = TimestampMicros.GetInstance();
                    break;
                case TIMESTAMP_MILLIS:
                    logicalType = TimestampMillis.GetInstance();
                    break;
                case TIME_MICROS:
                    logicalType = TimeMicros.GetInstance();
                    break;
                case TIME_MILLIS:
                    logicalType = TimeMillis.GetInstance();
                    break;
                default:
                    logicalType = null;
                    break;
            }

            return logicalType;
        }

        public class UUID : LogicalType
        {
            private static UUID instance;

            public UUID() : base(UUID_TAG)
            {
            }

            public static UUID getInstance()
            {
                if (instance == null)
                {
                    instance = new UUID();
                }

                return instance;
            }
        }

        /** Unlike Java, dotnet doesn't seem to have a standard BigDecimal class.
         * The decimal type should be enough in all practical cases though, and it's native
         * so it's much easier to deal with.
         */
        public class Decimal : LogicalType
        {
            private const string PRECISION_PROP = "precision";
            private const string SCALE_PROP = "scale";

            public int Precision { get; private set; }
            public int Scale { get; private set; }

            public Decimal(int precision, int scale) : base(DECIMAL)
            {
                Precision = precision;
                Scale = scale;
            }

            public Decimal(Schema schema) : base(DECIMAL)
            {
                if (!HasProperty(schema, PRECISION_PROP))
                {
                    throw new ArgumentException("Invalid decimal: missing precision");
                }

                Precision = GetInt(schema, PRECISION_PROP);

                Scale = HasProperty(schema, SCALE_PROP) ? GetInt(schema, SCALE_PROP) : 0;
            }

            public override Schema AddToSchema(Schema schema)
            {
                base.AddToSchema(schema);
                schema.Props.Add(PRECISION_PROP, Precision.ToString());
                schema.Props.Add(SCALE_PROP, Scale.ToString());
                return schema;
            }

//    public void validate(Schema schema) {
//      super.validate(schema);
//      // validate the type
//      if (schema.getType() != Schema.Type.FIXED &&
//          schema.getType() != Schema.Type.BYTES) {
//        throw new IllegalArgumentException(
//            "Logical type decimal must be backed by fixed or bytes");
//      }
//      if (precision <= 0) {
//        throw new IllegalArgumentException("Invalid decimal precision: " +
//            precision + " (must be positive)");
//      } else if (precision > maxPrecision(schema)) {
//        throw new IllegalArgumentException(
//            "fixed(" + schema.getFixedSize() + ") cannot store " +
//                precision + " digits (max " + maxPrecision(schema) + ")");
//      }
//      if (scale < 0) {
//        throw new IllegalArgumentException("Invalid decimal scale: " +
//            scale + " (must be positive)");
//      } else if (scale > precision) {
//        throw new IllegalArgumentException("Invalid decimal scale: " +
//            scale + " (greater than precision: " + precision + ")");
//      }
//    }

//    private long maxPrecision(Schema schema) {
//      if (schema.getType() == Schema.Type.BYTES) {
//        // not bounded
//        return Integer.MAX_VALUE;
//      } else if (schema.getType() == Schema.Type.FIXED) {
//        int size = schema.getFixedSize();
//        return Math.round(          // convert double to long
//            Math.floor(Math.log10(  // number of base-10 digits
//                Math.pow(2, 8 * size - 1) - 1)  // max value stored
//            ));
//      } else {
//        // not valid for any other type
//        return 0;
//      }
//    }

            private static bool HasProperty(Schema schema, string name)
            {
                return schema.Props.ContainsKey(name);
            }

            private int GetInt(Schema schema, string name)
            {
                var temp = schema.Props[name];
                return Convert.ToInt16(temp);
            }

            public override bool Equals(Object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType())
                    return false;

                var other = (Decimal) o;

                if (Precision != other.Precision) return false;
                if (Scale != other.Scale) return false;

                return true;
            }

            public override int GetHashCode()
            {
                int result = Precision;
                result = 31 * result + Scale;
                return result;
            }
        }

        /** Date represents a date without a time */
        public class Date : LogicalType
        {
            private static Date instance;

            private Date() : base(DATE)
            {
            }

            public static Date GetInstance()
            {
                if (instance == null)
                    instance = new Date();

                return instance;
            }

//    public void validate(Schema schema) {
//      super.validate(schema);
//      if (schema.getType() != Schema.Type.INT) {
//        throw new IllegalArgumentException(
//            "Date can only be used with an underlying int type");
//      }
//    }
        }

        /** TimeMillis represents a time in milliseconds without a date */
        public class TimeMillis : LogicalType
        {
            private static TimeMillis instance;

            public TimeMillis() : base(TIME_MILLIS)
            {
            }

            public static TimeMillis GetInstance()
            {
                if (instance == null)
                    instance = new TimeMillis();

                return instance;
            }

//    public void validate(Schema schema) {
//      super.validate(schema);
//      if (schema.getType() != Schema.Type.INT) {
//        throw new IllegalArgumentException(
//            "Time (millis) can only be used with an underlying int type");
//      }
//    }
        }

        /** TimeMicros represents a time in microseconds without a date */
        public class TimeMicros : LogicalType
        {
            private static TimeMicros instance;

            public TimeMicros() : base(TIME_MICROS)
            {
            }

            public static TimeMicros GetInstance()
            {
                if (instance == null)
                    instance = new TimeMicros();

                return instance;
            }

//    public void validate(Schema schema) {
//      super.validate(schema);
//      if (schema.getType() != Schema.Type.LONG) {
//        throw new IllegalArgumentException(
//            "Time (micros) can only be used with an underlying long type");
//      }
//    }
        }

        /** TimestampMillis represents a date and time in milliseconds */
        public class TimestampMillis : LogicalType
        {
            private static TimestampMillis instance;

            public TimestampMillis() : base(TIMESTAMP_MILLIS)
            {
            }

            public static TimestampMillis GetInstance()
            {
                if (instance == null)
                    instance = new TimestampMillis();

                return instance;
            }

//    public void validate(Schema schema) {
//      super.validate(schema);
//      if (schema.getType() != Schema.Type.LONG) {
//        throw new IllegalArgumentException(
//            "Timestamp (millis) can only be used with an underlying long type");
//      }
//    }
        }

        /** TimestampMicros represents a date and time in microseconds */
        public class TimestampMicros : LogicalType
        {
            private static TimestampMicros instance;

            private TimestampMicros() : base(TIMESTAMP_MICROS)
            {
            }

            public static TimestampMicros GetInstance()
            {
                if (instance == null)
                    instance = new TimestampMicros();

                return instance;
            }

//    public void validate(Schema schema) {
//      super.validate(schema);
//      if (schema.getType() != Schema.Type.LONG) {
//        throw new IllegalArgumentException(
//            "Timestamp (micros) can only be used with an underlying long type");
//      }
//    }
        }
    }
}
