using System;
using System.Collections.Generic;

using Machine.Core;

using NUnit.Framework;

namespace Machine.Migrations.Services.Impl
{
  [TestFixture]
  public class MigrationFactoryChooserTests : StandardFixture<MigrationFactoryChooser>
  {
    private CSharpMigrationFactory _cSharpMigrationFactory;
    private BooMigrationFactory _booMigrationFactory;
    private IConfiguration _configuration;

    [Test]
    public void ChooseFactory_IsCSharp_ReturnsFactory()
    {
      Assert.AreEqual(_cSharpMigrationFactory, _target.ChooseFactory(new MigrationReference(1, "Migration", "001_migration.cs")));
    }

    [Test]
    public void ChooseFactory_IsBoo_ReturnsFactory()
    {
      Assert.AreEqual(_booMigrationFactory, _target.ChooseFactory(new MigrationReference(1, "Migration", "001_migration.boo")));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ChooseFactory_IsNotCSharpOrBoo_Throws()
    {
      _target.ChooseFactory(new MigrationReference(1, "Migration", "001_migration.vb"));
    }

    public override MigrationFactoryChooser Create()
    {
      _configuration = _mocks.DynamicMock<IConfiguration>();
      _cSharpMigrationFactory = new CSharpMigrationFactory(_configuration);
      _booMigrationFactory = new BooMigrationFactory(_configuration);
      return new MigrationFactoryChooser(_cSharpMigrationFactory, _booMigrationFactory);
    }
  }
}