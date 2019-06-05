using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace OnlineDataCrawler.Util
{
    public class MongoDBHelper
    {
        private readonly string connectionString = null;
        private readonly string databaseName = null;
        private MongoDB.Driver.IMongoDatabase database = null;
        private readonly bool autoCreateDb = true;
        private readonly bool autoCreateCollection = true;
        private const string MongoDBConnect = "mongodb://localhost:27017";
        private const string DefaultDBName = "test";


        public MongoDBHelper()
        {
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
            this.connectionString = MongoDBConnect;
            this.databaseName = DefaultDBName;
        }

        public MongoDBHelper(string mongoConnStr, string dbName, bool autoCreateDb = true, bool autoCreateCollection = true)
        {
            this.connectionString = mongoConnStr;
            this.databaseName = dbName;
            this.autoCreateDb = autoCreateDb;
            this.autoCreateCollection = autoCreateCollection;
        }

        #region 私有方法

        private MongoClient CreateMongoClient()
        {
            return new MongoClient(connectionString);
        }


        private MongoDB.Driver.IMongoDatabase GetMongoDatabase()
        {
            if (database == null)
            {
                MongoClient client = CreateMongoClient();
                if (!DatabaseExists(client, databaseName) && !autoCreateDb)
                {
                    throw new KeyNotFoundException("此MongoDB名称不存在：" + databaseName);
                }

                database = CreateMongoClient().GetDatabase(databaseName);
            }

            return database;
        }

        private bool DatabaseExists(MongoClient client, string dbName)
        {
            try
            {
                IEnumerable<string> dbNames = client.ListDatabases().ToList().Select(db => db.GetValue("name").AsString);
                return dbNames.Contains(dbName);
            }
            catch //如果连接的账号不能枚举出所有DB会报错，则默认为true
            {
                return true;
            }

        }

        private bool CollectionExists(IMongoDatabase database, string collectionName)
        {
            ListCollectionsOptions options = new ListCollectionsOptions
            {
                Filter = Builders<BsonDocument>.Filter.Eq("name", collectionName)
            };

            return database.ListCollections(options).ToEnumerable().Any();
        }


        private MongoDB.Driver.IMongoCollection<TDoc> GetMongoCollection<TDoc>(string name, MongoCollectionSettings settings = null)
        {
            IMongoDatabase mongoDatabase = GetMongoDatabase();

            if (!CollectionExists(mongoDatabase, name) && !autoCreateCollection)
            {
                throw new KeyNotFoundException("此Collection名称不存在：" + name);
            }

            return mongoDatabase.GetCollection<TDoc>(name, settings);
        }

        private List<UpdateDefinition<TDoc>> BuildUpdateDefinition<TDoc>(object doc, string parent)
        {
            List<UpdateDefinition<TDoc>> updateList = new List<UpdateDefinition<TDoc>>();
            foreach (PropertyInfo property in typeof(TDoc).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                string key = parent == null ? property.Name : string.Format("{0}.{1}", parent, property.Name);
                //非空的复杂类型
                if ((property.PropertyType.IsClass || property.PropertyType.IsInterface) && property.PropertyType != typeof(string) && property.GetValue(doc) != null)
                {
                    if (typeof(IList).IsAssignableFrom(property.PropertyType))
                    {
                        #region 集合类型
                        int i = 0;
                        object subObj = property.GetValue(doc);
                        foreach (object item in subObj as IList)
                        {
                            if (item.GetType().IsClass || item.GetType().IsInterface)
                            {
                                updateList.AddRange(BuildUpdateDefinition<TDoc>(doc, string.Format("{0}.{1}", key, i)));
                            }
                            else
                            {
                                updateList.Add(Builders<TDoc>.Update.Set(string.Format("{0}.{1}", key, i), item));
                            }
                            i++;
                        }
                        #endregion
                    }
                    else
                    {
                        #region 实体类型
                        //复杂类型，导航属性，类对象和集合对象 
                        object subObj = property.GetValue(doc);
                        foreach (PropertyInfo sub in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                        {
                            updateList.Add(Builders<TDoc>.Update.Set(string.Format("{0}.{1}", key, sub.Name), sub.GetValue(subObj)));
                        }
                        #endregion
                    }
                }
                else //简单类型
                {
                    updateList.Add(Builders<TDoc>.Update.Set(key, property.GetValue(doc)));
                }
            }

            return updateList;
        }


        private void CreateIndex<TDoc>(IMongoCollection<TDoc> col, string[] indexFields, CreateIndexOptions options = null)
        {
            if (indexFields == null)
            {
                return;
            }
            IndexKeysDefinitionBuilder<TDoc> indexKeys = Builders<TDoc>.IndexKeys;
            IndexKeysDefinition<TDoc> keys = null;
            if (indexFields.Length > 0)
            {
                keys = indexKeys.Descending(indexFields[0]);
            }
            for (int i = 1; i < indexFields.Length; i++)
            {
                string strIndex = indexFields[i];
                keys = keys.Descending(strIndex);
            }

            if (keys != null)
            {
                col.Indexes.CreateOne(keys, options);
            }

        }

        #endregion

        public bool CollectionExists(string name)
        {
            return CollectionExists(GetMongoDatabase(), name);
        }

        public void CreateCollectionIndex<TDoc>(string collectionName, string[] indexFields, CreateIndexOptions options = null)
        {
            CreateIndex(GetMongoCollection<TDoc>(collectionName), indexFields, options);
        }

        public void CreateCollection<TDoc>(string[] indexFields = null, CreateIndexOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            CreateCollection<TDoc>(collectionName, indexFields, options);
        }

        public void CreateCollection<TDoc>(string collectionName, string[] indexFields = null, CreateIndexOptions options = null)
        {
            IMongoDatabase mongoDatabase = GetMongoDatabase();
            mongoDatabase.CreateCollection(collectionName);
            CreateIndex(GetMongoCollection<TDoc>(collectionName), indexFields, options);
        }


        public List<TDoc> Find<TDoc>(Expression<Func<TDoc, bool>> filter, FindOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            return Find<TDoc>(collectionName, filter, options);
        }

        public List<TDoc> FindAll<TDoc>()
        {
            string collectionName = typeof(TDoc).Name;
            IMongoCollection<TDoc> collection = GetMongoCollection<TDoc>(collectionName);
            return collection.Find(null).ToList();
        }

        public List<TDoc> Find<TDoc>(string collectionName, Expression<Func<TDoc, bool>> filter, FindOptions options = null)
        {
            IMongoCollection<TDoc> colleciton = GetMongoCollection<TDoc>(collectionName);
            return colleciton.Find(filter, options).ToList();
        }


        public List<TDoc> FindByPage<TDoc>(Expression<Func<TDoc, bool>> filter, int pageIndex, int pageSize, out int rsCount)
        {
            string collectionName = typeof(TDoc).Name;
            return FindByPage<TDoc>(collectionName, filter, pageIndex, pageSize, out rsCount);
        }

        public List<TDoc> FindAndSort<TDoc>(Expression<Func<TDoc, bool>> filter, int size, int direction, Expression<Func<TDoc, object>> sortField)
        {
            string collectionName = typeof(TDoc).Name;
            IMongoCollection<TDoc> collection = GetMongoCollection<TDoc>(collectionName);
            var filtered = collection.Find(filter);
            if (direction > 0)
            {
                //ascending
                filtered = filtered.SortBy(sortField).Limit(size);
            }
            else
            {
                //descending
                filtered = filtered.SortByDescending(sortField).Limit(size);
            }
            return filtered.ToList();
        }

        public List<TDoc> FindByPage<TDoc>(string collectionName, Expression<Func<TDoc, bool>> filter, int pageIndex, int pageSize, out int rsCount)
        {
            IMongoCollection<TDoc> colleciton = GetMongoCollection<TDoc>(collectionName);
            if (filter != null)
                rsCount = colleciton.AsQueryable().Where(filter).Count();
            else
                rsCount = colleciton.AsQueryable().Count();

            int pageCount = rsCount / pageSize + ((rsCount % pageSize) > 0 ? 1 : 0);
            if (pageIndex > pageCount)
            {
                pageIndex = pageCount;
            }

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            return colleciton.AsQueryable(new AggregateOptions { AllowDiskUse = true }).Where(filter).ToList();
        }

        public void Insert<TDoc>(TDoc doc, InsertOneOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            Insert<TDoc>(collectionName, doc, options);
        }

        public void Insert<TDoc>(string collectionName, TDoc doc, InsertOneOptions options = null)
        {
            IMongoCollection<TDoc> colleciton = GetMongoCollection<TDoc>(collectionName);
            colleciton.InsertOne(doc, options);
        }


        public void InsertMany<TDoc>(IEnumerable<TDoc> docs, InsertManyOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            InsertMany<TDoc>(collectionName, docs, options);
        }

        public void InsertMany<TDoc>(string collectionName, IEnumerable<TDoc> docs, InsertManyOptions options = null)
        {
            IMongoCollection<TDoc> colleciton = GetMongoCollection<TDoc>(collectionName);
            colleciton.InsertMany(docs, options);
        }

        public void Update<TDoc>(TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            IMongoCollection<TDoc> colleciton = GetMongoCollection<TDoc>(collectionName);
            List<UpdateDefinition<TDoc>> updateList = BuildUpdateDefinition<TDoc>(doc, null);
            colleciton.UpdateOne(filter, Builders<TDoc>.Update.Combine(updateList), options);
        }

        public void Update<TDoc>(string collectionName, TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateOptions options = null)
        {
            IMongoCollection<TDoc> colleciton = GetMongoCollection<TDoc>(collectionName);
            List<UpdateDefinition<TDoc>> updateList = BuildUpdateDefinition<TDoc>(doc, null);
            colleciton.UpdateOne(filter, Builders<TDoc>.Update.Combine(updateList), options);
        }


        public void Update<TDoc>(TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateDefinition<TDoc> updateFields, UpdateOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            Update<TDoc>(collectionName, doc, filter, updateFields, options);
        }

        public void Update<TDoc>(string collectionName, TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateDefinition<TDoc> updateFields, UpdateOptions options = null)
        {
            IMongoCollection<TDoc> colleciton = GetMongoCollection<TDoc>(collectionName);
            colleciton.UpdateOne(filter, updateFields, options);
        }


        public void UpdateMany<TDoc>(TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            UpdateMany<TDoc>(collectionName, doc, filter, options);
        }


        public void UpdateMany<TDoc>(string collectionName, TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateOptions options = null)
        {
            IMongoCollection<TDoc> colleciton = GetMongoCollection<TDoc>(collectionName);
            List<UpdateDefinition<TDoc>> updateList = BuildUpdateDefinition<TDoc>(doc, null);
            colleciton.UpdateMany(filter, Builders<TDoc>.Update.Combine(updateList), options);
        }


        public void Delete<TDoc>(Expression<Func<TDoc, bool>> filter, DeleteOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            Delete<TDoc>(collectionName, filter, options);
        }

        public void Delete<TDoc>(string collectionName, Expression<Func<TDoc, bool>> filter, DeleteOptions options = null)
        {
            IMongoCollection<TDoc> colleciton = GetMongoCollection<TDoc>(collectionName);
            colleciton.DeleteOne(filter, options);
        }


        public void DeleteMany<TDoc>(Expression<Func<TDoc, bool>> filter, DeleteOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            DeleteMany<TDoc>(collectionName, filter, options);
        }


        public void DeleteMany<TDoc>(string collectionName, Expression<Func<TDoc, bool>> filter, DeleteOptions options = null)
        {
            IMongoCollection<TDoc> colleciton = GetMongoCollection<TDoc>(collectionName);
            colleciton.DeleteMany(filter, options);
        }

        public async System.Threading.Tasks.Task CreateIndexesAsync<TDoc>(Expression<Func<TDoc, object>>[] index, int[] asc)
        {
            if (index.Length != asc.Length)
            {
                throw new TargetParameterCountException("index长度与asc 长度不一致，请检查");
            }
            IndexKeysDefinition<TDoc> indexes = null;
            for (int i = 0; i < index.Length; i++)
            {
                if (indexes == null)
                {
                    if (asc[i] > 0)
                    {
                        indexes = Builders<TDoc>.IndexKeys.Ascending(index[i]);
                    }
                    else
                    {
                        indexes = Builders<TDoc>.IndexKeys.Descending(index[i]);
                    }
                }
                else
                {
                    if (asc[i] > 0)
                    {
                        indexes.Ascending(index[i]);
                    }
                    else
                    {
                        indexes.Descending(index[i]);
                    }
                }
            }
            CreateIndexModel<TDoc> model = new CreateIndexModel<TDoc>(indexes);
            var collection = GetMongoCollection<TDoc>(typeof(TDoc).Name);
            string result = await collection.Indexes.CreateOneAsync(model);
        }

        public void ClearCollection<TDoc>(string collectionName)
        {
            IMongoCollection<TDoc> colleciton = GetMongoCollection<TDoc>(collectionName);
            IAsyncCursor<BsonDocument> inddexs = colleciton.Indexes.List();
            List<IEnumerable<BsonDocument>> docIndexs = new List<IEnumerable<BsonDocument>>();
            while (inddexs.MoveNext())
            {
                docIndexs.Add(inddexs.Current);
            }
            IMongoDatabase mongoDatabase = GetMongoDatabase();
            mongoDatabase.DropCollection(collectionName);

            if (!CollectionExists(mongoDatabase, collectionName))
            {
                CreateCollection<TDoc>(collectionName);
            }

            if (docIndexs.Count > 0)
            {
                colleciton = mongoDatabase.GetCollection<TDoc>(collectionName);
                foreach (IEnumerable<BsonDocument> index in docIndexs)
                {
                    foreach (IndexKeysDefinition<TDoc> indexItem in index)
                    {
                        try
                        {
                            colleciton.Indexes.CreateOne(indexItem);
                        }
                        catch
                        { }
                    }
                }
            }

        }
    }
}
