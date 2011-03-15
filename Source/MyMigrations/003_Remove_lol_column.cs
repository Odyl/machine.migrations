using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Migrations;


    class Remove_lol_column : SimpleMigration
    {
        public override void Up()
        {
            Schema.RemoveColumn("users", "rofl");            
        }

        public override void Down()
        {
            Schema.AddColumn("users", "rofl", typeof(string), 500, false, true);
        }
    }

