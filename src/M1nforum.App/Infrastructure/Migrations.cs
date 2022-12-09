using System.Data.Common;
using System.Data;
using System.IO;
using System;

//namespace M1nforum.Web.Infrastructure
//{
//	public class Migrations
//	{
//	}
//}


//using System;
//using System.Data;
//using System.Data.Common;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

////// a bit of a hack
////// todo:  genericify this
////var databasePath = Path.Combine(SqliteDAL.SqlitePath, "SunForum.db");

////if (!File.Exists(databasePath))
////{
////	SQLiteConnection.CreateFile(databasePath);
////}


//namespace Min.Web.Data
//{
//	public abstract class Migrator
//	{
//		public abstract DbConnection GetOpenDbConnection(string connectionString);

//		public void Migrate(string migrationFilePath, string connectionString)
//		{
//			using (var dbConnection = GetOpenDbConnection(connectionString))
//			{
//				string migrationVersion = "";

//				try
//				{
//					// migrationVersion = (await dal.ConfigGetByName(dbConnection, "version")).Val;
//				}
//				catch (Exception /* exception */)
//				{
//					migrationVersion = "";
//				}

//				var files = Directory.GetFiles(migrationFilePath)
//					.OrderBy(f => Path.GetFileNameWithoutExtension(f))
//					.ToList();

//				foreach (var file in files)
//				{
//					var filename = Path.GetFileNameWithoutExtension(file);

//					if (filename.CompareTo(migrationVersion) > 0)
//					{
//						using (var dbTransaction = dbConnection.BeginTransaction())
//						{
//							try
//							{
//								MigrateFile(file, dbConnection, dbTransaction);
//								// await dal.ConfigUpsert(dbConnection, dbTransaction, "version", filename);

//								dbTransaction.Commit();

//							}
//							catch (Exception /* exception */)
//							{
//								dbTransaction.Rollback();
//								throw;
//							}
//						}
//					}
//				}
//			}
//		}

//		private void MigrateFile(string file, DbConnection dbConnection, DbTransaction dbTransaction)
//		{
//			var buffer = File.ReadAllText(file);

//			var statements = buffer.Split("GO;".ToCharArray(), StringSplitOptions.None);

//			foreach (var statement in statements)
//			{
//				using (var dbCommand = dbConnection.CreateCommand())
//				{
//					if (dbTransaction != null)
//					{
//						dbCommand.Transaction = dbTransaction;
//					}

//					dbCommand.CommandText = statement;
//					dbCommand.CommandType = CommandType.Text;

//					dbCommand.ExecuteNonQuery();
//				}
//			}
//		}
//	}
//}

























//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Common;
//using System.IO;
//using System.Linq;

//namespace Portal.EF.Migrations
//{
//	// todo - public TextWriter Log = TextWriter.Null;
//	public class TransactionalDb : IDisposable
//	{
//		private IDbTransaction _transaction;
//		private IDbConnection _dbConnection;

//		//// https://weblog.west-wind.com/posts/2017/nov/27/working-around-the-lack-of-dynamic-dbproviderfactory-loading-in-net-core
//		//public void Open(string connectionStringName)
//		//{
//		//	var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];

//		//	_dbConnection = DbProviderFactories.GetFactory(connectionString.ProviderName).CreateConnection();
//		//	_dbConnection.ConnectionString = connectionString.ConnectionString;

//		//	_dbConnection.Open();
//		//	_transaction = _dbConnection.BeginTransaction();
//		//}

//		public void Open<T>(string connectionString) where T : IDbConnection
//		{
//			_dbConnection = Activator.CreateInstance(typeof(T)) as IDbConnection;
//			_dbConnection.ConnectionString = connectionString;

//			_dbConnection.Open();
//			_transaction = _dbConnection.BeginTransaction();
//		}

//		public void Commit()
//		{
//			_transaction.Commit();
//		}

//		public object ExecuteScalar(string command, params object[] parameters)
//		{
//			return Execute(true, command, parameters);
//		}

//		public int ExecuteNonQuery(string command, params object[] parameters)
//		{
//			return (int)Execute(false, command, parameters);
//		}

//		private object Execute(bool scalar, string command, params object[] parameters)
//		{
//			using (var dbCommand = _dbConnection.CreateCommand())
//			{
//				dbCommand.Transaction = _transaction;

//				dbCommand.CommandText = command;
//				foreach (var parameter in parameters)
//				{
//					dbCommand.Parameters.Add(parameter);
//				}

//				return scalar ? dbCommand.ExecuteScalar() : dbCommand.ExecuteNonQuery();
//			}
//		}

//		public void Dispose()
//		{
//			_dbConnection.Close();
//			_dbConnection.Dispose();
//		}
//	}

//	public class Migrator
//	{
//		private MigrationVersioning _versioning;

//		public void Execute<T>(string connectionString, string context, string path) where T : IDbConnection
//		{
//			using (var transactionalDb = new TransactionalDb())
//			{
//				transactionalDb.Open<T>(connectionString);

//				_versioning = new MigrationVersioning(transactionalDb, context);
//				ProcessSqlFiles(transactionalDb, path);

//				transactionalDb.Commit();
//			}
//		}

//		private void ProcessSqlFiles(TransactionalDb db, string path)
//		{
//			var lastFile = string.Empty;

//			foreach (var file in GetSqlFiles(path))
//			{
//				ProcessFile(db, file);
//				lastFile = file;
//			}

//			_versioning.UpdateLatestVersion(Path.GetFileName(lastFile));
//		}

//		private void ProcessFile(TransactionalDb db, string file)
//		{
//			var commands = File.ReadAllText(file).Split(new string[] { Environment.NewLine + "GO" }, StringSplitOptions.RemoveEmptyEntries);

//			foreach (var command in commands)
//			{
//				try
//				{
//					//Logger.WriteLine("Processing file " + Path.GetFileName(file));

//					db.ExecuteNonQuery(command);

//					//Logger.WriteLine(ConsoleColor.Green, command);
//					//Logger.WriteLine();
//				}
//				catch (Exception ex)
//				{
//					//Logger.WriteLine(ConsoleColor.Red, command);
//					//Logger.WriteLine();
//					//Logger.WriteLine(ConsoleColor.Red, ex.Message);
//					//Logger.WriteLine();
//					throw;
//				}
//			}
//		}

//		private IEnumerable<string> GetSqlFiles(string path)
//		{
//			var files = Directory.GetFiles(path).OrderBy(s => s);
//			var latestVersion = _versioning.GetLatestVersion();

//			if (!string.IsNullOrEmpty(latestVersion))
//			{
//				return files.Where(f => string.Compare(Path.GetFileName(f), latestVersion, System.StringComparison.OrdinalIgnoreCase) > 0);
//			}

//			return files;
//		}
//	}

//	public class MigrationVersioning
//	{
//		const string DEFAULT_TABLE_NAME = "__migrations";
//		const string DEFAULT_CONTEXT = "_DEFAULT_";

//		private readonly string _tableName;
//		private readonly string _context;
//		private readonly TransactionalDb _transactionalDb;

//		public string GetLatestVersion()
//		{
//			var value = _transactionalDb.ExecuteScalar("SELECT MAX(file_name) file_name FROM " + _tableName + "  where context = '" + _context + "'");
//			return value == DBNull.Value ? null : (string)value;
//		}

//		public void UpdateLatestVersion(string latestVersion)
//		{
//			if (!string.IsNullOrEmpty(latestVersion))
//			{
//				_transactionalDb.ExecuteNonQuery("INSERT INTO " + _tableName + " (file_name, context) VALUES ('" + latestVersion + "', '" + _context + "');");
//			}
//		}

//		public MigrationVersioning(TransactionalDb db, string context, string tableName = DEFAULT_TABLE_NAME)
//		{
//			_tableName = tableName;
//			_context = context;
//			_transactionalDb = db;

//			EnsureTableExists();
//		}

//		private void EnsureTableExists()
//		{
//			try
//			{
//				_transactionalDb.ExecuteScalar("SELECT count(*) FROM " + _tableName + ";");
//			}
//			catch (DbException)
//			{
//				// sqlite
//				_transactionalDb.ExecuteNonQuery(@"CREATE TABLE " + _tableName + @" (file_name varchar(255) NOT NULL, context varchar(255) NOT NULL, PRIMARY KEY (file_name, context));");

//				// sql server
//				//				_transactionalDb.ExecuteNonQuery(@"CREATE TABLE " + _tableName + @" (file_name varchar(255) NOT NULL, context varchar(255) NOT NULL 

//				// CONSTRAINT [PK_" + _tableName + @"] PRIMARY KEY CLUSTERED 
//				//(
//				//	Context ASC, 
//				//	[file_name] ASC
//				//)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 

//				//)");


//			}
//		}
//	}
//}









































//using SunForum.Web.Data.Models;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Common;
//using System.Data.SQLite;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;
//using System.Xml.Serialization;
//using System.Text.RegularExpressions;
//using System.Xml.Linq;

//namespace SunForum.Web.Data
//{
//	public interface IDAL : IDisposable
//	{
//		Task<int> CommentCountNonDeletedByTopic(Guid topicId);
//		Task<List<Comment>> CommentListByTopicPaged(Guid topicId, int pageSize, int pageIndex);
//		Task<Config> ConfigGetByName(string name);
//		Task<List<Config>> ConfigList();
//		Task<int> ConfigUpsert(DbTransaction dbTransaction, string name, string value);
//		Task<GroupAdmin> GroupAdminGetByUserGroup(Guid userId, Guid groupId);
//		Task<GroupMod> GroupModGetByUserGroup(Guid userId, Guid groupId);
//		Task<Group> GroupGetById(Guid id);
//		Task<Group> GroupGetBySlug(string slug);
//		Task GroupInsert(DbTransaction dbTransaction, Group group);
//		Task<List<Group>> GroupList();
//		Task<List<Group>> GroupListOpen();
//		Task<GroupSubscription> GroupSubscriptionGetByGroupAndUser(Guid groupId, Guid userId);
//		Task<Session> SessionGetById(Guid sessionId);
//		Task<List<Session>> SessionList();
//		Task<Session> SessionOpen(Guid sessionId, int maxSessionLifeSectionds);
//		Task SessionPurgeOldSessions(int maxSessionLifeSeconds);
//		Task SessionUpdate(Session session);
//		Task<Topic> TopicGetBySlug(string slug);
//		Task TopicInsert(DbTransaction dbTransaction, Topic topic);
//		Task<TopicSubscription> TopicSubscriptionGetByTopicAndUser(Guid topicId, Guid userId);
//		Task<List<TopicView>> TopicViewListByGroup(Guid groupId, int topicsPerPage = 0, DateTime? lastTopicDate = null);
//		Task<List<TopicView>> TopicViewListLast20();
//		Task UserInsert(DbTransaction dbTransaction, User user);
//		Task<User> UserGetById(Guid id);
//		Task<List<User>> UserList();

//		Task<IDAL> Initialize();
//		DbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Serializable);
//	}

//	// note:  went with DbConnection base classes instead of IDbConnection interfaces for async.

//	public class SQLiteDAL : IDAL
//	{
//		private DbConnection _dbConnection;

//		public static string SqlitePath { get; set; }

//		public DbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Serializable)
//		{
//			return _dbConnection.BeginTransaction(isolationLevel);
//		}

//		public async Task<List<Comment>> CommentListByTopicPaged(Guid topicId, int pageSize, int pageIndex)
//		{
//			return await Query<Comment>("WHERE TopicId = @0 LIMIT @1 OFFSET @2", topicId, pageSize, pageSize * pageIndex);
//		}

//		public async Task<int> CommentCountNonDeletedByTopic(Guid topicId)
//		{
//			return (await Query<int>("SELECT COUNT(*) FROM Comment WHERE TopicId = @0 AND IsDeleted = 0", (dbReader) =>
//			{
//				return dbReader.GetInt32(0);
//			}, topicId)).FirstOrDefault();
//		}

//		public async Task<Comment> CommentGetLastByTopic(Guid topicId)
//		{
//			return (await Query<Comment>("WHERE TopicId = @0 ORDER BY Pos DESC LIMIT 1", topicId)).FirstOrDefault();
//		}

//		public async Task<Config> ConfigGetByName(string name)
//		{
//			// note:  this is how to do this leveraging the full list from an existing method
//			//return (await ConfigList(dbConnection))
//			//	.FirstOrDefault(c => c.Name == name);

//			// note:  this is how to query sqlite for the specific config value we want.
//			return (await Query<Config>("WHERE Name = @0", name)).First();
//		}

//		public async Task<List<Config>> ConfigList()
//		{
//			return await Query<Config>();
//		}

//		public async Task<int> ConfigUpsert(DbTransaction dbTransaction, string name, string value)
//		{
//			var query = @"
//				INSERT INTO Config(name, val)
//				  VALUES (@0, @1)
//				  ON CONFLICT(name) DO UPDATE SET
//					val = @1
//				  WHERE name = @0;
//				";

//			return await ExecuteNonQuery(dbTransaction, query, name, value);
//		}

//		public void Dispose()
//		{
//			if (_dbConnection != null)
//			{
//				_dbConnection.Dispose();
//			}
//		}

//		public async Task<GroupAdmin> GroupAdminGetByUserGroup(Guid userId, Guid groupId)
//		{
//			return (await Query<GroupAdmin>("WHERE UserId = @0 and GroupId = @1", userId, groupId)).FirstOrDefault();
//		}

//		public async Task<GroupMod> GroupModGetByUserGroup(Guid userId, Guid groupId)
//		{
//			return (await Query<GroupMod>("WHERE UserId = @0 and GroupId = @1", userId, groupId)).FirstOrDefault();
//		}

//		public async Task<Group> GroupGetById(Guid id)
//		{
//			return (await Query<Group>("WHERE Id = @0", id)).FirstOrDefault();
//		}


//		public async Task<Group> GroupGetBySlug(string slug)
//		{
//			return (await Query<Group>("WHERE Slug = @0", slug)).FirstOrDefault();
//		}

//		public async Task GroupInsert(DbTransaction dbTransaction, Group group)
//		{
//			await Insert(dbTransaction, group);
//		}

//		public async Task<List<Group>> GroupList()
//		{
//			// todo:  censor description
//			return await Query<Group>();
//		}

//		public async Task<List<Group>> GroupListOpen()
//		{
//			// limit 25
//			// todo:  censor description
//			return await Query<Group>("WHERE IsClosed = 0 ORDER BY IsSticky DESC, Name");
//		}

//		public async Task<GroupSubscription> GroupSubscriptionGetByGroupAndUser(Guid groupId, Guid userId)
//		{
//			return (await Query<GroupSubscription>("WHERE GroupId = @0 AND UserId = @1", groupId, userId)).FirstOrDefault();
//		}

//		public async Task<IDAL> Initialize() // note - could pass in an existing connection here if we wanted to enable reusable connections.
//		{
//			_dbConnection = new SQLiteConnection(string.Format("Data Source={0}; Version=3;BinaryGUID=False;", SqlitePath), false);

//			await _dbConnection.OpenAsync();

//			await ExecuteNonQuery(null, "PRAGMA journal_mode = WAL;");
//			await ExecuteNonQuery(null, "PRAGMA synchronous = FULL;");
//			await ExecuteNonQuery(null, "PRAGMA foreign_keys = ON;");

//			return this;
//		}

//		public async Task<Session> SessionGetById(Guid id)
//		{
//			// todo:  should we take into account session expiration time here?
//			var session = (await Query<Session>("WHERE Id = @0", id)).FirstOrDefault();

//			return session;
//		}

//		public async Task<List<Session>> SessionList()
//		{
//			return await Query<Session>();
//		}

//		public async Task<Session> SessionOpen(Guid sessionId, int maxSessionLifeSectionds)
//		{
//			// todo:  understand the bits about expiring sessions in the original code.B

//			await SessionPurgeOldSessions(maxSessionLifeSectionds);

//			var session = await SessionGetById(sessionId);

//			if (session == null)
//			{
//				// new session
//				session = new Session()
//				{
//					CreatedOn = DateTime.UtcNow,
//					CSRF = Guid.NewGuid().ToString(), // not using comb to make less predictable.
//					Id = Comb.NewGuid(),
//					UpdatedOn = DateTime.UtcNow
//					// UserId = null // UserId gets set elsewhere explicitly.
//				};

//				await Insert(null, session);
//			}
//			else
//			{
//				session.UpdatedOn = DateTime.UtcNow;
//				await SessionUpdate(session);
//			}

//			return session;
//		}

//		public async Task SessionPurgeOldSessions(int maxSessionLifeSeconds)
//		{
//			var query = @"delete from Session WHERE UpdatedOn < @0";

//			await ExecuteNonQuery(null, query, DateTime.UtcNow.AddSeconds(maxSessionLifeSeconds * -1));
//		}

//		public async Task SessionUpdate(Session session)
//		{
//			await Update(null, session);
//		}

//		public async Task<Topic> TopicGetBySlug(string slug)
//		{
//			return (await Query<Topic>("WHERE Slug = @0", slug)).FirstOrDefault();
//		}

//		public async Task TopicInsert(DbTransaction dbTransaction, Topic topic)
//		{
//			await Insert(dbTransaction, topic);
//		}

//		public async Task<TopicSubscription> TopicSubscriptionGetByTopicAndUser(Guid topicId, Guid userId)
//		{
//			return (await Query<TopicSubscription>("WHERE TopicId = @0 AND UserId = @1", topicId, userId)).FirstOrDefault();
//		}

//		private string GetTopicViewQuery(string where, DateTime? lastTopicDate, int limit)
//		{
//			var query = @"SELECT [Group].Name as GroupName, [Group].Slug as GroupSlug, Topic.Id as Id, Topic.CreatedOn as TopicCreatedOn, Topic.IsDeleted as TopicIsDeleted, Topic.IsClosed as TopicIsClosed, Topic.NumComments as TopicNumComments, Topic.Slug as TopicSlug, Topic.Title as TopicTitle, User.Username as UserUsername
//							FROM Topic
//							INNER JOIN [Group] ON Topic.GroupId = [Group].Id
//							INNER JOIN [User] ON Topic.UserId = [User].Id
//							{0}
//							ORDER BY Topic.IsSticky DESC, Topic.ActivityDate DESC {1};";

//			return string.Format(query, where ?? "", limit > 0 ? "LIMIT " + limit.ToString() : "");
//		}

//		public async Task<List<TopicView>> TopicViewListLast20()
//		{
//			var query = GetTopicViewQuery("WHERE Topic.IsClosed = 0 AND Topic.IsDeleted = 0 AND [Group].IsClosed = 0", null, 20);

//			var topicViews = await Query(query, (dbDataReader) =>
//			{
//				var topicView = dbDataReader.MapToEntity<TopicView>();
//				// todo:  censor
//				return topicView;
//			});

//			return topicViews;
//		}

//		public async Task<List<TopicView>> TopicViewListByGroup(Guid groupId, int topicsPerPage = 0, DateTime? lastTopicDate = null)
//		{
//			var query = GetTopicViewQuery("WHERE [Group].Id = @0", lastTopicDate, 20);

//			var topicViews = await Query(query, (dbDataReader) =>
//			{
//				var topicView = dbDataReader.MapToEntity<TopicView>();
//				// todo:  censor
//				return topicView;
//			}, groupId);

//			return topicViews;
//		}

//		public async Task UserInsert(DbTransaction dbTransaction, User user)
//		{
//			await Insert(dbTransaction, user);
//		}

//		public async Task<User> UserGetById(Guid id)
//		{
//			return (await Query<User>("WHERE Id = @0", id)).FirstOrDefault();
//		}

//		public async Task<List<User>> UserList()
//		{
//			return await Query<User>();
//		}



















//		// helpers

//		private async Task Insert<T>(DbTransaction dbTransaction, T entity)
//		{
//			var query = typeof(T).GetInsertStatement();

//			using (var dbCommand = CreateCommand(query, dbTransaction))
//			{
//				dbCommand.Parameters.AddRange(dbCommand.MapToInsertParameters(entity));

//				await dbCommand.ExecuteNonQueryAsync();
//			}
//		}

//		private async Task<List<T>> Query<T>() where T : new()
//		{
//			return await Query<T>(typeof(T).GetSelectStatement());
//		}

//		private async Task<List<T>> Query<T>(string commandText, params object[] parameters) where T : new()
//		{
//			return await Query(commandText, (dbDataReader) =>
//			{
//				return dbDataReader.MapToEntity<T>();
//			}, parameters);
//		}

//		private async Task<List<T>> Query<T>(string commandText, Func<DbDataReader, T> map, params object[] parameters) where T : new()
//		{
//			if (commandText.StartsWith("WHERE", StringComparison.OrdinalIgnoreCase))
//			{
//				commandText = typeof(T).GetSelectStatement() + " " + commandText;
//			}

//			using (var dbCommand = CreateCommand(commandText, null, parameters))
//			{
//				using (var dbDataReader = await dbCommand.ExecuteReaderAsync())
//				{
//					List<T> results = new List<T>();

//					while (dbDataReader.Read())
//					{
//						results.Add(map(dbDataReader));
//					}

//					return results;
//				}
//			}
//		}

//		private async Task Update<T>(DbTransaction dbTransaction, T entity)
//		{
//			var query = typeof(T).GetUpdateStatement();

//			using (var dbCommand = CreateCommand(query, dbTransaction))
//			{
//				dbCommand.Parameters.AddRange(dbCommand.MapToInsertParameters(entity));

//				await dbCommand.ExecuteNonQueryAsync();
//			}
//		}

//		private async Task<int> ExecuteNonQuery(DbTransaction dbTransaction, string commandText, params object[] parameters)
//		{
//			using (var dbCommand = CreateCommand(commandText, dbTransaction, parameters))
//			{
//				return await dbCommand.ExecuteNonQueryAsync();
//			}
//		}

//		private DbCommand CreateCommand(string commandText, DbTransaction dbTransaction = null, params object[] parameters)
//		{
//			var dbCommand = _dbConnection.CreateCommand();
//			dbCommand.CommandText = commandText;
//			dbCommand.CommandType = CommandType.Text;

//			System.Diagnostics.Trace.WriteLine("CREATE COMMAND:  " + commandText ?? "NULL");

//			// transaction
//			if (dbTransaction != null)
//			{
//				dbCommand.Transaction = dbTransaction;
//			}

//			// parameters
//			if (parameters != null && parameters.Any())
//			{
//				// Create numbered parameters
//				var dbParameters = parameters.Select((o, index) =>
//				{
//					var dbParameter = dbCommand.CreateParameter();
//					dbParameter.ParameterName = index.ToString();
//					dbParameter.Value = o ?? DBNull.Value;
//					return dbParameter;
//				});

//				foreach (var dbParameter in dbParameters)
//				{
//					dbCommand.Parameters.Add(dbParameter);
//				}
//			}

//			return dbCommand;
//		}

//		public Task GroupInsert(DbTransaction dbTransaction)
//		{
//			throw new NotImplementedException();
//		}
//	}


//	public static class SQLiteSQLGenerator
//	{
//		private static Dictionary<Type, List<PropertyInfo>> _propertyCache = new Dictionary<Type, List<PropertyInfo>>();
//		private static Dictionary<Type, string> _insertStatements = new Dictionary<Type, string>();
//		private static Dictionary<Type, string> _selectStatements = new Dictionary<Type, string>();
//		// private static Dictionary<Type, string> _upsertStatements = new Dictionary<Type, string>();
//		private static Dictionary<Type, string> _updateStatements = new Dictionary<Type, string>();

//		public static string GetInsertStatement<T>(this T type) where T : Type
//		{
//			string statement = null;

//			if (_insertStatements.TryGetValue(type, out statement))
//			{
//				return statement;
//			}

//			var properties = GetProperties(type)
//				.Select(p => p.Name).ToList();

//			statement = "INSERT INTO [" + type.Name + "] ([" + string.Join("], [", properties) + "]) VALUES (@" + string.Join(", @", Enumerable.Range(0, properties.Count)) + ")";

//			_insertStatements[type] = statement;

//			return statement;
//		}

//		public static string GetSelectStatement<T>(this T type) where T : Type
//		{
//			string statement = null;

//			if (_selectStatements.TryGetValue(type, out statement))
//			{
//				return statement;
//			}

//			var properties = GetProperties(type)
//				.Select(p => p.Name).ToList();

//			statement = "SELECT [" + string.Join("], [", properties) + "] FROM [" + type.Name + "]";

//			_selectStatements[type] = statement;

//			return statement;
//		}

//		public static string GetUpdateStatement<T>(this T type) where T : Type
//		{
//			string statement = null;

//			if (_updateStatements.TryGetValue(type, out statement))
//			{
//				return statement;
//			}

//			var properties = GetProperties(type)
//				.Select(p => p.Name).ToList();


//			statement = "UPDATE [" + type.Name + "] SET ";

//			var idIndex = 0;
//			for (var counter = 0; counter < properties.Count; counter++)
//			{
//				if (properties[counter] == "Id")
//				{
//					idIndex = counter;
//				}
//				else
//				{
//					statement += "[" + properties[counter] + "] = @" + counter.ToString() + (counter == properties.Count - 1 ? "" : ", ");
//				}
//			}

//			statement += " where Id = @" + idIndex.ToString();

//			_updateStatements[type] = statement;

//			return statement;
//		}

//		//public static string GetUpsertStatement<T>(this T type, string pkColumnName) where T : Type
//		//{
//		//	//INSERT INTO Config(name, val)
//		//	//	  VALUES(@0, @1)
//		//	//	  ON CONFLICT(name) DO UPDATE SET
//		//	//		val = @1
//		//	//	  WHERE name = @0;

//		//	string statement = null;

//		//	if (_upsertStatements.TryGetValue(type, out statement))
//		//	{
//		//		return statement;
//		//	}

//		//	var properties = GetProperties(type)
//		//		.Select(p => p.Name).ToList();

//		//	statement = type.GetInsertStatement() +
//		//		"ON CONFLICT ([" + pkColumnName + "] DO UPDATE SET ";

//		//	_upsertStatements[type] = statement;

//		//	return statement;
//		//}

//		public static T MapToEntity<T>(this IDataRecord record) where T : new()
//		{
//			var properties = GetProperties(typeof(T));
//			var entity = new T();

//			for (var counter = 0; counter < properties.Count; counter++)
//			{
//				properties[counter].SetValue(entity, ChangeType(record[counter], properties[counter].PropertyType));
//			}

//			return entity;
//		}

//		public static IDbDataParameter[] MapToInsertParameters<T>(this DbCommand dbCommand, T entity)
//		{
//			var properties = GetProperties(typeof(T));
//			var parameters = new IDbDataParameter[properties.Count];

//			for (var counter = 0; counter < properties.Count; counter++)
//			{
//				var parameter = dbCommand.CreateParameter();
//				parameter.ParameterName = counter.ToString();
//				parameter.Value = properties[counter].GetValue(entity) ?? DBNull.Value;

//				parameters[counter] = parameter;
//			}

//			return parameters;
//		}

//		private static List<PropertyInfo> GetProperties(Type type)
//		{
//			if (_propertyCache.ContainsKey(type))
//			{
//				return _propertyCache[type];
//			}
//			else
//			{
//				var properties = type
//								.GetProperties()
//								.Where(p => p.CanWrite)
//								.Where(p => p.PropertyType.GetInterface(typeof(List<>).FullName) == null) // no lists
//								.Where(p => p.PropertyType.FullName.StartsWith("System.")) // no user defined types
//								.Where(p => !Attribute.IsDefined(p, typeof(XmlIgnoreAttribute))) // ignore the XmlIgnore attributes
//								.OrderBy(p => p.Name)
//								.ToList();

//				_propertyCache.Add(type, properties);

//				return properties;
//			}
//		}

//		private static object ChangeType(object value, Type targetType)
//		{
//			if (value == null || value == DBNull.Value)
//			{
//				return null;
//			}

//			switch (targetType.Name)
//			{
//				case "Guid":
//					return Guid.Parse((string)value);
//				case "DateTime":
//					var dt = DateTime.Parse((string)value).ToUniversalTime(); // convert all date/time values to utc.
//					return dt;
//				default:
//					// because of data types in sqlite, this is not reliable - VARCHAR(32) to Guid failes
//					return Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType);
//			}
//		}
//	}
//}
