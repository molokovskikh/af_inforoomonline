using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace Common.MySql
{
	public interface IMySqlHelper
	{
		ICommandHelper Command(string commandText);
		ICommandHelper Command(string commandText, CommandType commandType);
	}

	public interface ICommandHelper
	{
		CommandHelper AddParameters(IEnumerable<MySqlParameter> parameters);
		CommandHelper AddParameter(string name, object value);
		void Execute();
		T ExecuteScalar<T>();
	}

	public class MySqlHelper : IMySqlHelper
	{
		private readonly MySqlConnection _connection;
		private readonly MySqlTransaction _transaction;

		public MySqlHelper(MySqlConnection connection, MySqlTransaction transaction)
		{
			_connection = connection;
			_transaction = transaction;
		}

		public ICommandHelper Command(string commandText)
		{
			return Command(commandText, CommandType.Text);
		}

		public ICommandHelper Command(string commandText, CommandType commandType)
		{
			MySqlCommand command = new MySqlCommand(commandText, _connection, _transaction);
			command.CommandType = commandType;
			return new CommandHelper(command);
		}
	}

	public class CommandHelper : ICommandHelper
	{
		private readonly MySqlCommand _command;

		public CommandHelper(MySqlCommand command)
		{
			_command = command;
		}

		public CommandHelper AddParameter(string name, object value)
		{
			_command.Parameters.AddWithValue(name, value);
			return this;
		}

		public CommandHelper AddParameter(string name, object value, MySqlDbType dbType)
		{
			_command.Parameters.Add(name, dbType);
			_command.Parameters[name].Value = value;
			return this;
		}

		public void Execute()
		{
			_command.ExecuteNonQuery();
		}

		public T ExecuteScalar<T>()
		{
			var result = _command.ExecuteScalar();
			return (T)TypeDescriptor.GetConverter(result.GetType()).ConvertTo(result, typeof(T));
		}

		public object ExecuteScalar()
		{
			return _command.ExecuteScalar();
		}

		public DataSet Fill()
		{
			DataSet data = new DataSet();
			MySqlDataAdapter dataAdapter = new MySqlDataAdapter(_command);
			dataAdapter.Fill(data);
			return data;
		}

		public DataSet Fill(DataSet data, string tableName)
		{
			MySqlDataAdapter dataAdapter = new MySqlDataAdapter(_command);
			dataAdapter.Fill(data, tableName);
			return data;
		}

		public CommandHelper AddParameters(IEnumerable<MySqlParameter> parameters)
		{
			foreach (MySqlParameter parameter in parameters)
				_command.Parameters.Add(parameter);
			return this;
		}
	}


}
