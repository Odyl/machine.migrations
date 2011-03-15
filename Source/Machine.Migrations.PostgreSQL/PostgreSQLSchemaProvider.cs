using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Machine.Migrations.Builders;
using Machine.Migrations.DatabaseProviders;
using Machine.Migrations.SchemaProviders;

namespace Machine.Migrations.PostgreSQL
{
    public class PostgreSQLSchemaProvider : ISchemaProvider
    {
        readonly IDatabaseProvider _databaseProvider;

        protected IDatabaseProvider DatabaseProvider
        {
            get { return _databaseProvider; }
        }

        public PostgreSQLSchemaProvider(IDatabaseProvider databaseProvider)
        {
            _databaseProvider = databaseProvider;
        }

        public void AddTable(string table, ICollection<Column> columns)
        {
            if (columns.Count == 0)
            {
                throw new ArgumentException("columns");
            }
            using (Machine.Core.LoggingUtilities.Log4NetNdc.Push("AddTable"))
            {
                var sb = new StringBuilder();
                sb.Append("create table ").Append(table).Append(" (");
                var first = true;
                foreach (var column in columns)
                {
                    if (!first) sb.Append(",");
                    sb.AppendLine().Append(ColumnToCreateTableSql(column));
                    first = false;
                }

                foreach (var column in columns)
                {
                    var sql = ColumnToConstraintsSql(table, column);
                    if (sql != null)
                    {
                        sb.Append(",").AppendLine().Append(sql);
                    }
                }

                sb.AppendLine().Append(")");
                _databaseProvider.ExecuteNonQuery(sb.ToString());
            }
        }

        public void DropTable(string table)
        {
            _databaseProvider.ExecuteNonQuery("drop table {0}", table);
        }

        public virtual bool HasTable(string table)
        {
            using (Machine.Core.LoggingUtilities.Log4NetNdc.Push("HasTable({0})", table))
            {
                return _databaseProvider.ExecuteScalar<Int32>("select count(*) from information_schema.tables where table_name = '{0}'", table) > 0;
            }
        }

        public void AddColumn(string table, string column, Type type)
        {
            AddColumn(table, column, type, -1, false, false);
        }

        public void AddColumn(string table, string column, Type type, short size, bool isPrimaryKey, bool allowNull)
        {
            _databaseProvider.ExecuteNonQuery("alter table \"{0}\" add {1}", table, ColumnToCreateTableSql(new Column(column, type, size, isPrimaryKey, allowNull)));
        }

        public void AddColumn(string table, string column, Type type, bool allowNull)
        {
            AddColumn(table, column, type, 0, false, allowNull);
        }

        public void AddColumn(string table, string column, Type type, short size, bool allowNull)
        {
            AddColumn(table, column, type, size, false, allowNull);
        }

        public void RemoveColumn(string table, string column)
        {
            _databaseProvider.ExecuteNonQuery("alter table \"{0}\" drop column \"{1}\"", table, column);
        }

        public void RenameTable(string table, string newName)
        {
            _databaseProvider.ExecuteNonQuery("alter table \"{0}\" rename to \"{1}\"", table, newName);
        }

        public void RenameColumn(string table, string column, string newName)
        {
            _databaseProvider.ExecuteNonQuery("alter table \"{0}\" rename \"{1}\" to \"{2}\"", table, column, newName);
        }

        public void AddSchema(string schemaName)
        {
            _databaseProvider.ExecuteNonQuery("create schema \"{0}\"", schemaName);
        }

        public void RemoveSchema(string schemaName)
        {
            _databaseProvider.ExecuteNonQuery("drop schema \"{0}\"", schemaName);
        }

        public bool HasSchema(string schemaName)
        {
            using (Machine.Core.LoggingUtilities.Log4NetNdc.Push("HasSchema({0})", schemaName))
            {
                return _databaseProvider.ExecuteScalar<Int32>("select count(*) from information_schema.schemata where table_schema = '{0}'", schemaName) > 0;
            }
        }

        public virtual bool HasColumn(string table, string column)
        {
            using (Machine.Core.LoggingUtilities.Log4NetNdc.Push("HasColumn({0}.{1})", table, column))
            {
                return _databaseProvider.ExecuteScalar<Int32>("select count(*) from information_schema.columns where table_name = '{0}' and column_name = '{1}'", table, column) > 0;
            }
        }

        public virtual bool IsColumnOfType(string table, string column, string type)
        {
            using (Machine.Core.LoggingUtilities.Log4NetNdc.Push("IsColumnOfType({0}.{1}.{2})", table, column, type))
            {
                return _databaseProvider.ExecuteScalar<Int32>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}' AND DATA_TYPE = '{2}'", table, column, type) > 0;
            }
        }

        public void ChangeColumn(string table, string column, Type type, short size, bool allowNull)
        {
            _databaseProvider.ExecuteNonQuery("ALTER TABLE {0} ALTER COLUMN {1}", table, ColumnToCreateTableSql(new Column(column, type, size, false, allowNull)));
        }

        public virtual string[] Columns(string table)
        {
            using (var reader = _databaseProvider.ExecuteReader("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", table))
            {
                return GetColumnAsArray(reader, 0);
            }
        }

        public virtual string[] Tables()
        {
            using (var reader = _databaseProvider.ExecuteReader("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"))
            {
                return GetColumnAsArray(reader, 0);
            }
        }

        public void AddForeignKeyConstraint(string table, string name, string column, string foreignTable, string foreignColumn)
        {
            _databaseProvider.ExecuteNonQuery(
              "ALTER TABLE {0} ADD CONSTRAINT \"{1}\" FOREIGN KEY (\"{2}\") REFERENCES {3} (\"{4}\")", table, name, column,
              foreignTable, foreignColumn);
        }

        public void AddUniqueConstraint(string table, string name, params string[] columns)
        {
            if (columns.Length == 0)
                throw new ArgumentException("AddUniqueConstraint requires at least one column name", "columns");

            var colList = "";
            foreach (string column in columns)
            {
                if (colList.Length != 0)
                    colList += ", ";
                colList += "\"" + column + "\" ASC";
            }

            _databaseProvider.ExecuteNonQuery(
              "ALTER TABLE {0} ADD CONSTRAINT \"{1}\" UNIQUE ({2})", table, name, colList);
        }

        public void DropConstraint(string table, string name)
        {
            _databaseProvider.ExecuteNonQuery("ALTER TABLE {0} DROP CONSTRAINT \"{1}\"", table, name);
        }

        public static string[] GetColumnAsArray(IDataReader reader, int columnIndex)
        {
            var values = new List<string>();
            while (reader.Read())
            {
                values.Add(reader.GetString(columnIndex));
            }
            return values.ToArray();
        }

        string ColumnToCreateTableSql(Column column)
        {
            if(column.IsIdentity)
            {
                throw new NotSupportedException("PostgreSQL does not have identity columns");
            }

            return String.Format("\"{0}\" {1} {2}",
                                 column.Name,
                                 ToMsSqlType(column.ColumnType, column.Size),
                                 column.AllowNull ? "" : "NOT NULL");
        }

        public virtual string ColumnToConstraintsSql(string tableName, Column column)
        {
            if (column.IsPrimaryKey)
            {
                return String.Format("CONSTRAINT PK_{0}_{1} PRIMARY KEY (\"{1}\")", SchemaUtils.Normalize(tableName), SchemaUtils.Normalize(column.Name));
            }

            if (column.IsUnique)
            {
                return String.Format("CONSTRAINT UK_{0}_{1} UNIQUE (\"{1}\" ASC)", SchemaUtils.Normalize(tableName), SchemaUtils.Normalize(column.Name));
            }

            return null;
        }

        public virtual string ToMsSqlType(ColumnType type, int size)
        {
            switch (type)
            {
                case ColumnType.Int16:
                    return "smallint";
                case ColumnType.Int32:
                    return "integer";
                case ColumnType.Long:
                    return "bigint";
                case ColumnType.Money:
                    return "money";
                case ColumnType.NVarChar:
                    if (size == 0)
                    {
                        return "varchar(255)";
                    }
                    return String.Format("varchar({0})", size);
                case ColumnType.Real:
                    return "real";
                case ColumnType.Text:
                    return "text";
                case ColumnType.Binary:
                    return "bytea";
                case ColumnType.Bool:
                    return "bool";
                case ColumnType.Char:
                    return "char(1)";
                case ColumnType.DateTime:
                    return "timestamp";
                case ColumnType.Decimal:
                    return "decimal";
                case ColumnType.Image:
                    return "bytea";
                case ColumnType.Guid:
                    return "uuid";
            }

            throw new ArgumentException("type");
        }
    }
}
