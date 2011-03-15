using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Migrations;


    class Anothermigration : SimpleMigration
    {
        public override void Up()
        {
            Schema.AddColumn("users", "rofl", typeof (string), 500, false, false);
        }

        public override void Down()
        {
            Schema.RemoveColumn("users", "rofl");
        }
    }

