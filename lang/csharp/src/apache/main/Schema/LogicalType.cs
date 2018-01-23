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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avro
{
    /**
     * Following the java version at
     * https://raw.githubusercontent.com/apache/avro/master/lang/java/avro/src/main/java/org/apache/avro/LogicalType.java
     */
    public class LogicalType
    {
        public const string LogicalTypeProp = "logicalType";

        public LogicalType(string logicalTypeName) {
            Name = logicalTypeName;
        }

        /// <summary>
        /// Get the name of this logical type.
        /// </summary>
        public string Name { get; private set; }

        // TODO
//        public Schema addToSchema(Schema schema) {
//            validate(schema);
//            schema.addProp(LOGICAL_TYPE_PROP, name);
//            schema.setLogicalType(this);
//            return schema;
//        }
//
//        public void validate(Schema schema) {
//            for (String incompatible : INCOMPATIBLE_PROPS) {
//                if (schema.getProp(incompatible) != null) {
//                    throw new IllegalArgumentException(
//                        LOGICAL_TYPE_PROP + " cannot be used with " + incompatible);
//                }
//            }
//        }
    }
}
