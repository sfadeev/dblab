using System;
using System.Linq;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Mapping;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace linq2db.tests
{
	public class TableInheritanceTests
	{
		private IConfigurationRoot _configuration;
		private LinqToDbConnectionOptions _connectionOptions;

		[SetUp]
		public void Setup()
		{
			_configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddUserSecrets(typeof(TableInheritanceTests).Assembly)
				.Build();

			_connectionOptions = new LinqToDbConnectionOptionsBuilder()
				.UsePostgreSQL(_configuration.GetConnectionString("Default"))
				.Build();

			var fm = MappingSchema.Default.GetFluentMappingBuilder();

			fm.Entity<DbClassifier>().HasTableName("classifier")
				.Inheritance(x => x.Type, null, typeof(DbClassifier), true)
				.Inheritance(x => x.Type, "company", typeof(DbCompany))
				.Inheritance(x => x.Type, "user", typeof(DbUser))
				.Property(x => x.Uid).HasColumnName("uid").HasDataType(DataType.Guid).IsPrimaryKey()
				.Property(x => x.Type).HasColumnName("type").HasDataType(DataType.VarChar).IsDiscriminator()
				.Property(x => x.Name).HasColumnName("name").HasDataType(DataType.VarChar);

			fm.Entity<DbCompany>().HasTableName("company")
				.Property(x => x.FullName).HasColumnName("full_name").HasDataType(DataType.VarChar)
				.Property(x => x.Vatin).HasColumnName("vatin").HasDataType(DataType.VarChar);

			fm.Entity<DbUser>().HasTableName("user")
				.Property(x => x.FirstName).HasColumnName("first_name").HasDataType(DataType.VarChar)
				.Property(x => x.LastName).HasColumnName("last_name").HasDataType(DataType.VarChar);
		}

		/// <summary>
		/// https://github.com/linq2db/linq2db/issues/848#issuecomment-328469353
		///
		/// Sorry, but for now we do support only Single Table Inheritance.
		/// </summary>
		[Test]
		public void Test1()
		{
			using (var db = new LinqToDB.Data.DataConnection(_connectionOptions))
			{
				var query1 = db.GetTable<DbClassifier>()
					.Where(x => x.Uid == Guid.Empty);

				Console.WriteLine(query1.ToString());

				/* --  PostgreSQL.9.5 PostgreSQL
DECLARE @Empty Uuid -- Guid
SET     @Empty = '00000000-0000-0000-0000-000000000000'

SELECT
	x.uid,
	x.type,
	x.name,
	x.vatin,
	x.full_name,
	x.last_name,
	x.first_name
FROM
	classifier x
WHERE
	x.uid = :Empty
				 */

				var query2 = db.GetTable<DbCompany>()
					.Where(x => x.Uid == Guid.Empty);

				Console.WriteLine(query2.ToString());

				/* --  PostgreSQL.9.5 PostgreSQL
DECLARE @Empty Uuid -- Guid
SET     @Empty = '00000000-0000-0000-0000-000000000000'

SELECT
	x.name,
	x.type,
	x.uid,
	x.vatin,
	x.full_name
FROM
	classifier x
WHERE
	x.type = 'company' AND x.uid = :Empty
				 */

				var query3 = db.GetTable<DbUser>()
					.Where(x => x.Uid == Guid.Empty);

				Console.WriteLine(query3.ToString());
			}
		}
	}

	public class DbClassifier
	{
		public Guid Uid { get; set; }

		public string Type { get; set; }

		public string Name { get; set; }
	}

	public class DbCompany : DbClassifier
	{
		public string FullName { get; set; }
		
		public string Vatin { get; set; }
	}

	public class DbUser : DbClassifier
	{
		public string FirstName { get; set; }
		
		public string LastName { get; set; }
	}
}
