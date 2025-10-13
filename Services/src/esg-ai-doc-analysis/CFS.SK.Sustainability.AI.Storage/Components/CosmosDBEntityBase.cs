﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CFS.SK.Sustainability.AI.Storage.Components
{
    public class CosmosDBEntityBase : IEntityModel<Guid>
    {
        public CosmosDBEntityBase()
        {
            this.id = Guid.NewGuid();
            this.__partitionkey = CosmosDBEntityBase.GetKey(id, 9999);
        }

        /// <summary>
        /// id will be generated automatically. you don't need to manage it by yourself
        /// </summary>
        public Guid id { get; set; }

        /// <summary>
        /// the partitionkey will be used for storage partitioning. you don't need to manage it by yourself
        /// </summary>
        public string __partitionkey { get; set; }

        static SHA1 _sha1;

        static CosmosDBEntityBase()
        {
            _sha1 = SHA1.Create(); // CodeQL [SM02196] This repository is no longer actively maintained. Fixing this issue is not feasible as no further development is planned.
        }

        /// <summary>
        /// Generate partitionkey for CosmosDB
        /// using SHA1 hash with id, convert it to uint and divide with number of partitions
        /// assigned default value as 9999 (9999 partition at this moment)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="numberofPartitions"></param>
        /// <returns></returns>
        public static string GetKey(Guid id, int numberofPartitions)
        {
            var hasedVal = _sha1.ComputeHash(id.ToByteArray());
            var intHashedVal = BitConverter.ToUInt32(hasedVal, 0);

            var range = numberofPartitions - 1;
            var length = range.ToString().Length;

            var key = (intHashedVal % numberofPartitions).ToString();
            return key.PadLeft(length, '0');
        }

    }
}
