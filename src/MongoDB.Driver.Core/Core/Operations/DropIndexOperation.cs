﻿/* Copyright 2013-2014 MongoDB Inc.
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    public class DropIndexOperation : IWriteOperation<BsonDocument>
    {
        // fields
        private readonly CollectionNamespace _collectionNamespace;
        private readonly string _indexName;
        private readonly MessageEncoderSettings _messageEncoderSettings;

        // constructors
        public DropIndexOperation(
            CollectionNamespace collectionNamespace,
            BsonDocument keys,
            MessageEncoderSettings messageEncoderSettings)
            : this(collectionNamespace, IndexNameHelper.GetIndexName(keys), messageEncoderSettings)
        {
        }

        public DropIndexOperation(
            CollectionNamespace collectionNamespace,
            string indexName,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, "collectionNamespace");
            _indexName = Ensure.IsNotNullOrEmpty(indexName, "indexName");
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, "messageEncoderSettings");
        }

        // properties
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        public string IndexName
        {
            get { return _indexName; }
        }

        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        // methods
        public BsonDocument CreateCommand()
        {
            return new BsonDocument
            {
                { "dropIndexes", _collectionNamespace.CollectionName },
                { "index", _indexName }
            };
        }

        public async Task<BsonDocument> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, "binding");
            var command = CreateCommand();
            var operation = new WriteCommandOperation<BsonDocument>(_collectionNamespace.DatabaseNamespace, command, BsonDocumentSerializer.Instance, _messageEncoderSettings);
            try
            {
                return await operation.ExecuteAsync(binding, cancellationToken).ConfigureAwait(false);
            }
            catch (MongoCommandException ex)
            {
                if (ex.ErrorMessage != null && ex.ErrorMessage.Contains("ns not found"))
                {
                    return ex.Result;
                }
                throw;
            }
        }
    }
}
