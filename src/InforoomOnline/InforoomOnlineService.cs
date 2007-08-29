using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using Common.MySql;
using log4net;

namespace InforoomOnline
{
	public class InforoomOnlineService : IInforoomOnlineService
	{
		private readonly ILog _log = LogManager.GetLogger(typeof (InforoomOnlineService));

		public DataSet GetOffers(string[] rangeField, string[] rangeValue, bool newEar, string[] sortField, string[] sortOrder, int limit, int selStart)
		{
			try
			{
				DataSet result = null;
				With.Transaction(
					delegate(MySqlHelper helper)
						{
							Dictionary<string, string> columnNameMapping = new Dictionary<string, string>();
							columnNameMapping.Add("offers.Id", "OfferId");
							columnNameMapping.Add("offers.PriceCode", "PriceCode");
							columnNameMapping.Add("offers.FullCode", "FullCode");
							columnNameMapping.Add("s.synonym", "Name");
							columnNameMapping.Add("c.CodeCr", "Code");
							columnNameMapping.Add("c.Code", "CodeCr");
							columnNameMapping.Add("c.Unit", "Unit");
							columnNameMapping.Add("c.Volume", "Volume");
							columnNameMapping.Add("c.Quantity", "Quantity");
							columnNameMapping.Add("c.Note", "Note");
							columnNameMapping.Add("c.Period", "Period");
							columnNameMapping.Add("c.Doc", "Doc");
							columnNameMapping.Add("c.Junk", "Junk");
							columnNameMapping.Add("offers.Cost", "Cost");

							ValidateFieldNames(columnNameMapping, rangeField);
							ValidateFieldNames(columnNameMapping, sortField);
							ValidateSortDirection(sortOrder);

							Dictionary<string, List<string>> groupedValues = GroupValues(rangeField, rangeValue);

							if (rangeField.Length != rangeValue.Length)
								throw new Exception(
									"Количество полей для фильтрации не совпадает с количеством значение по которым производится фильтрация");

							helper
								.Command("Usersettings.GetOffers", CommandType.StoredProcedure)
								.AddParameter("?ClientCodeParam", GetClientCode(helper))
								.AddParameter("?FreshOnly", false)
								.Execute();

							StringBuilder commandText =
								new StringBuilder(
									@"
SELECT	offers.Id as OfferId,
		offers.PriceCode,
		offers.FullCode,
		c.Code,
		c.CodeCr,
		s.synonym as Name,
		c.Unit,
		c.Volume,
		c.Quantity,
		c.Note,
		c.Period,
		c.Doc,
		length(c.Junk) > 0 as Junk,
		offers.Cost 
FROM core as offers
    JOIN farm.core0 as c on c.id = offers.id
		JOIN farm.synonym s on c.synonymcode = s.synonymcode");

							SqlBuilder builder = SqlBuilder
								.ForCommandTest(commandText);

							foreach (KeyValuePair<string, List<string>> pair in groupedValues)
								builder.AddCriteria(Utils.StringArrayToQuery(pair.Value, columnNameMapping[pair.Key]));

							builder
								.AddOrder(sortField, sortOrder)
								.Limit(limit, selStart);

							result = helper.Fill(commandText.ToString());
						});
				return result;
			}
			catch(Exception ex)
			{
				_log.Error("", ex);
				return null;
			}
		}

		public DataSet GetPriceList(string[] firmName)
		{
			return null;
		}

		public DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart)
		{
			return null;
		}

		public DataSet PostOrder(long[] offerId, int[] quantity, string[] message)
		{
			return null;
		}

		private static Dictionary<string, List<string>> GroupValues(IEnumerable<string> fields, string[] values)
		{
			Dictionary<string, List<string >> result = new Dictionary<string, List<string>>();
			int i = 0;
			foreach (string field in fields)
			{
				if (!result.ContainsKey(field))
					result.Add(field, new List<string>());
				result[field].Add(values[i]);
				i++;
			}
			return result;
		}

		private static void ValidateFieldNames(IDictionary<string, string> mapping, 
											   IEnumerable<string> fieldsToValidate)
		{
			foreach (string fieldName in fieldsToValidate)
				if (!mapping.ContainsKey(fieldName))
					throw new Exception(String.Format("Не найдено сопоставление для поля {0}", fieldName));
		}

		private static void ValidateSortDirection(IEnumerable<string> sortDirections)
		{
			foreach (string sortDirection in sortDirections)
				if (String.Compare(sortDirection, "asc", true) != 0 
					&& String.Compare(sortDirection, "desc", true) != 0)
					throw new Exception(String.Format("Не верный порядок сортировки {0}", sortDirection));
		}

		private static uint GetClientCode(MySqlHelper helper)
		{
			string userName = ServiceSecurityContext.Current.PrimaryIdentity.Name.Replace(@"analit\", "");

			return helper.Command(@"
SELECT clientcode 
FROM osuseraccessright 
WHERE OSUserName = ?UserName")
					.AddParameter("?UserName", userName)
					.ExecuteScalar<uint>();
		}
	}
}
