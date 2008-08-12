using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace Common.MySql
{
	/// <summary>
	/// ����� �������� ��������������� ������� ��� ��������� SQL ��������
	/// </summary>
	public class Utils
	{
		/// <summary>
		/// ��������������� ������ ����� ��� ������������� � �������.
		/// ����� ���������� ������ ������� "*" �� "%".
		/// ������: ������ {"hello", "w*rld"} ��������������� � ������ 
		/// "and ( someField like 'hello'  or  someField like 'w%rld')"
		/// </summary>
		/// <param name="array">��������� ������</param>
		/// <param name="fieldName">��� ���� �� �������� ���������� �����</param>
		/// <returns>����������������� ������</returns>
		public static string StringArrayToQuery(IEnumerable<MySqlParameter> parameters, string fieldName)
		{
			StringBuilder builder = new StringBuilder();
			int index = 0;
			if (parameters != null)
			{
				builder.Append(" (");
				foreach (MySqlParameter item in parameters)
				{
					if (item.Value is string)
					{
						if (item.Value.ToString().Contains("*"))
						{
							item.Value = item.Value.ToString().Replace("*", "%");
							builder.Append(fieldName + " like " + item.ParameterName + "");
						}
						else
							builder.Append(fieldName + " = " + item.ParameterName + "");
					}
					else
						builder.Append(fieldName + " = " + item.ParameterName + "");
					builder.Append(" or ");
					index++;
				}
				if (builder.Length > 4)
					builder.Remove(builder.Length - 4, 4);
				builder.Append(") ");
			}
			if (index > 0)
				return builder.ToString();
			else
				return "";
		}

		public bool IsDeadLockOrSimilarException(Exception ex)
		{
			return false;
		}

		public static string StringArrayToQuery(IEnumerable array, string fieldName, List<MySqlParameter> parameters)
		{
			List<MySqlParameter> newParameters = new List<MySqlParameter>();
			foreach (object item in array)
			{
				string parameterName = "?p" + (parameters.Count + newParameters.Count);
				newParameters.Add(new MySqlParameter(parameterName, item));
			}
			parameters.AddRange(newParameters);
			return StringArrayToQuery(newParameters, fieldName);
		}
	}
}
