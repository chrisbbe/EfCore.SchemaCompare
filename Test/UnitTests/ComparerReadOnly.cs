// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.ReadOnlyTypes.EfCode;
using EfSchemaCompare;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class ComparerReadOnly
    {
        private readonly ITestOutputHelper _output;

        public ComparerReadOnly(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CompareReadOnlyDbContext()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<ReadOnlyDbContext>();
            using var context = new ReadOnlyDbContext(options);
            context.Database.EnsureClean();
            var filepath = TestData.GetFilePath("AddViewToDatabase.sql");
            context.ExecuteScriptFileInTransaction(filepath);
            
            var comparer = new CompareEfSql();

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeTrue();
            var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(
                "NOT CHECKED: Entity 'MappedToQuery', not mapped to database. Expected = <null>, found = MappedToQuery");
        }

        [Fact]
        public void CompareReadOnlyDbContextIgnoreNormalClasses()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<ReadOnlyDbContext>();
            using var context = new ReadOnlyDbContext(options);
            context.Database.EnsureClean();
            var filepath = TestData.GetFilePath("AddViewToDatabase.sql");
            context.ExecuteScriptFileInTransaction(filepath);

            var config = new CompareEfSqlConfig
            {
                TablesToIgnoreCommaDelimited = "NormalClasses"
            };

            var comparer = new CompareEfSql(config);

            //ATTEMPT
            var hasErrors = comparer.CompareEfWithDb(context);

            //VERIFY
            hasErrors.ShouldBeTrue();
            var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
            errors.Count.ShouldEqual(3);
            errors[0].ShouldEqual(
                "NOT CHECKED: Entity 'MappedToQuery', not mapped to database. Expected = <null>, found = MappedToQuery");
            errors[1].ShouldEqual(
                "NOT IN DATABASE: MappedToQuery->Entity 'NormalClass', table name. Expected = NormalClasses");
            errors[2].ShouldEqual(
                "NOT IN DATABASE: MappedToQuery->Entity 'MappedToView', table name. Expected = NormalClasses");
        }

    }
}
