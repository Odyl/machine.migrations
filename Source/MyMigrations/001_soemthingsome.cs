using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Migrations;


class soemthingsome : SimpleMigration
{
    public override void Up()
    {
        Schema.AddTable("users", new Column[]
                                         {
                                             new Column("Id", typeof (Int32), 4, true, false),
                                             new Column("Name", typeof (string), 64, false, false),
                                             new Column("Email", typeof (string), 64, false, false),
                                             new Column("Login", typeof (string), 64, false, false),
                                             new Column("Password", typeof (string), 64, false, false),
                                         });
    }

    public override void Down()
    {
        Schema.DropTable("users");
    }

}
