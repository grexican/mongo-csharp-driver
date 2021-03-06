﻿/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
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
using MongoDB.Bson;

namespace MongoDB.Driver
{
    /// <summary>
    /// The result of an update operation.
    /// </summary>
    public abstract class ReplaceOneResult
    {
        // static
        internal static ReplaceOneResult FromCore(BulkWriteResult result)
        {
            if (result.IsAcknowledged)
            {
                var upsert = result.Upserts.Count == 1
                    ? result.Upserts[0].Id
                    : null;
                var modifiedCount = result.IsModifiedCountAvailable
                    ? (long?)result.ModifiedCount
                    : null;

                return new Acknowledged(result.MatchedCount, modifiedCount, upsert);
            }

            return Unacknowledged.Instance;
        }

        // properties
        /// <summary>
        /// Gets a value indicating whether the result is acknowleded.
        /// </summary>
        public abstract bool IsAcknowledged { get; }

        /// <summary>
        /// Gets a value indicating whether the modified count is available.
        /// </summary>
        /// <remarks>
        /// The modified count is only available when all servers have been upgraded to 2.6 or above.
        /// </remarks>
        public abstract bool IsModifiedCountAvailable { get; }

        /// <summary>
        /// Gets the matched count. If IsAcknowledged is false, this will throw an exception.
        /// </summary>
        public abstract long MatchedCount { get; }

        /// <summary>
        /// Gets the modified count. If IsAcknowledged is false, this will throw an exception.
        /// </summary>
        public abstract long ModifiedCount { get; }

        /// <summary>
        /// Gets the upserted id, if one exists. If IsAcknowledged is false, this will throw an exception.
        /// </summary>
        public abstract BsonValue UpsertedId { get; }

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceOneResult"/> class.
        /// </summary>
        protected ReplaceOneResult()
        {
        }

        // nested classes
        /// <summary>
        /// Result from an acknowledged write concern.
        /// </summary>
        public class Acknowledged : ReplaceOneResult
        {
            private readonly long _matchedCount;
            private readonly long? _modifiedCount;
            private readonly BsonValue _upsertedId;

            /// <summary>
            /// Initializes a new instance of the <see cref="Acknowledged"/> class.
            /// </summary>
            /// <param name="matchedCount">The matched count.</param>
            /// <param name="modifiedCount">The modified count.</param>
            /// <param name="upsertedId">The upserted identifier.</param>
            public Acknowledged(long matchedCount, long? modifiedCount, BsonValue upsertedId)
            {
                _matchedCount = matchedCount;
                _modifiedCount = modifiedCount;
                _upsertedId = upsertedId;
            }

            /// <summary>
            /// Gets a value indicating whether the result is acknowleded.
            /// </summary>
            public override bool IsAcknowledged
            {
                get { return true; }
            }

            /// <summary>
            /// Gets a value indicating whether the modified count is available.
            /// </summary>
            /// <remarks>
            /// The modified count is only available when all servers have been upgraded to 2.6 or above.
            /// </remarks>
            public override bool IsModifiedCountAvailable
            {
                get { return _modifiedCount.HasValue; }
            }

            /// <summary>
            /// Gets the matched count. If IsAcknowledged is false, this will throw an exception.
            /// </summary>
            public override long MatchedCount
            {
                get { return _matchedCount; }
            }

            /// <summary>
            /// Gets the modified count. If IsAcknowledged is false, this will throw an exception.
            /// </summary>
            public override long ModifiedCount
            {
                get
                {
                    if (!_modifiedCount.HasValue)
                    {
                        throw new NotSupportedException("ModifiedCount is not available.");
                    }
                    return _modifiedCount.Value;
                }
            }

            /// <summary>
            /// Gets the upserted id, if one exists. If IsAcknowledged is false, this will throw an exception.
            /// </summary>
            public override BsonValue UpsertedId
            {
                get { return _upsertedId; }
            }
        }

        /// <summary>
        /// Result from an unacknowledged write concern.
        /// </summary>
        public class Unacknowledged : ReplaceOneResult
        {
            private static Unacknowledged __instance = new Unacknowledged();

            /// <summary>
            /// Gets the instance.
            /// </summary>
            public static Unacknowledged Instance
            {
                get { return __instance; }
            }

            private Unacknowledged()
            {
            }

            /// <summary>
            /// Gets a value indicating whether the result is acknowleded.
            /// </summary>
            public override bool IsAcknowledged
            {
                get { return false; }
            }

            /// <summary>
            /// Gets a value indicating whether the modified count is available.
            /// </summary>
            /// <remarks>
            /// The modified count is only available when all servers have been upgraded to 2.6 or above.
            /// </remarks>
            public override bool IsModifiedCountAvailable
            {
                get { return false; }
            }

            /// <summary>
            /// Gets the matched count. If IsAcknowledged is false, this will throw an exception.
            /// </summary>
            /// <exception cref="System.NotSupportedException">Only acknowledged writes support the MatchedCount property.</exception>
            public override long MatchedCount
            {
                get { throw new NotSupportedException("Only acknowledged writes support the MatchedCount property."); }
            }

            /// <summary>
            /// Gets the modified count. If IsAcknowledged is false, this will throw an exception.
            /// </summary>
            /// <exception cref="System.NotSupportedException">Only acknowledged writes support the ModifiedCount property.</exception>
            public override long ModifiedCount
            {
                get { throw new NotSupportedException("Only acknowledged writes support the ModifiedCount property."); }
            }

            /// <summary>
            /// Gets the upserted id, if one exists. If IsAcknowledged is false, this will throw an exception.
            /// </summary>
            /// <exception cref="System.NotSupportedException">Only acknowledged writes support the UpsertedId property.</exception>
            public override BsonValue UpsertedId
            {
                get { throw new NotSupportedException("Only acknowledged writes support the UpsertedId property."); }
            }
        }
    }
}