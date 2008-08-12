using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace Common.MySql
{
	public class SqlBuilder
	{
		private readonly StringBuilder _builder;
		private bool _isAdd = false;
		private readonly List<MySqlParameter> _parameters = new List<MySqlParameter>();

		protected SqlBuilder(StringBuilder commandText)
		{
			_builder = commandText;
		}

		public string GetSql()
		{
			return _builder.ToString();
		}

		public static SqlBuilder WithCommandText(string commandText)
		{
			return new SqlBuilder(new StringBuilder(commandText));
		}

		public SqlBuilder AddCriteria(string criteriaString)
		{
			if (String.IsNullOrEmpty(criteriaString))
				return this;
			if (_isAdd)
			{
				_builder.Append(" and ");
			}
			else
			{
				_builder.AppendLine();
				_builder.Append("where ");
			}
			_builder.Append(criteriaString);
			_isAdd = true;
			return this;
		}

		public SqlBuilder AddInCriteria(string fieldName, IEnumerable items)
		{
			return AddCriteria(Utils.StringArrayToQuery(items, fieldName, _parameters));
		}

		public SqlBuilder AddCriteria(string criteriaString, bool addOrNot)
		{
			if (addOrNot)
				AddCriteria(criteriaString);
			return this;
		}

		public SqlBuilder Limit(int offset, int count)
		{
			if (offset > 0)
			{
				_builder.AppendLine();
				_builder.Append("limit " + offset);
				if (count > 0)
					_builder.Append(", " + count);
			}
			_builder.Append(";");
			return this;
		}

		public SqlBuilder AddOrder(string fieldName, string sortDirection)
		{
			if (fieldName != null)
			{
				if (sortDirection != null)
					AddOrderMultiColumn(new string[] {fieldName}, new string[] {sortDirection});
				else
					AddOrderMultiColumn(new string[] {fieldName}, null);
			}
			return this;
		}

		public SqlBuilder AddOrder(string fieldName)
		{
			AddOrderMultiColumn(new string[] { fieldName }, null);
			return this;
		}

		public SqlBuilder AddOrderMultiColumn(string[] fieldNames, string[] sortDirections)
		{
			if ((fieldNames != null) && (fieldNames.Length > 0))
			{
				_builder.AppendLine();
				_builder.Append("ORDER BY ");
				for (int i = 0; i < fieldNames.Length; i++)
				{
					if ((sortDirections != null) && (i < sortDirections.Length))
						_builder.Append(fieldNames[i] + " " + sortDirections[i]);
					else
						_builder.Append(fieldNames[i]);

					if (i < fieldNames.Length - 1)
						_builder.Append(", ");
				}
			}
			return this;
		}

		public CommandHelper ToCommand(IMySqlHelper helper)
		{
			return helper.Command(GetSql())
				.AddParameters(_parameters);
		}
	}
}
