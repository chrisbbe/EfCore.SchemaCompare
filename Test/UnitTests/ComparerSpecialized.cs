// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.SpecialisedEntities;
using EfSchemaCompare;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class ComparerSpecialized
    {
        private readonly string _connectionString;
        private readonly DbContextOptions<SpecializedDbContext> _options;
        private readonly ITestOutputHelper _output;

        public ComparerSpecialized(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<SpecializedDbContext>();

            using (var context = new SpecializedDbContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public void CompareSpecializedDbContext()
        {
            //SETUP
            using (var context = new SpecializedDbContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY

                hasErrors.ShouldBeTrue(comparer.GetAllErrors);
                comparer.GetAllErrors.ShouldEqual("DIFFERENT: BookDetail->Property 'Price', nullability. Expected = NOT NULL, found = NULL");
            }
        }

        [Fact]
        public void CompareOwnedWithKeyDbContext()
        {
            //SETUP
            var options = this.CreateUniqueMethodOptions<OwnedWithKeyDbContext>();
            using (var context = new OwnedWithKeyDbContext(options))
            {
                context.Database.EnsureCreated();
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }
    }
}
