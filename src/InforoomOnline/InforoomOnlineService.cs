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
				log4net.Config.XmlConfigurator.Configure();
				DataSet result = null;
				With.Transaction(
					delegate(MySqlHelper helper)
						{
							Dictionary<string, string> columnNameMapping = new Dictionary<string, string>();
							columnNameMapping.Add("offers.Id", "offerid");
							columnNameMapping.Add("offers.PriceCode", "pricecode");
							columnNameMapping.Add("offers.FullCode", "fullcode");
							columnNameMapping.Add("s.synonym", "name");
							columnNameMapping.Add("c.CodeCr", "code");
							columnNameMapping.Add("c.Code", "codecr");
							columnNameMapping.Add("c.Unit", "unit");
							columnNameMapping.Add("c.Volume", "volume");
							columnNameMapping.Add("c.Quantity", "quantity");
							columnNameMapping.Add("c.Note", "note");
							columnNameMapping.Add("c.Period", "period");
							columnNameMapping.Add("c.Doc", "doc");
							columnNameMapping.Add("c.Junk", "junk");
							columnNameMapping.Add("offers.Cost", "cost");

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

							SqlBuilder builder = SqlBuilder
								.ForCommandTest(@"
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

							foreach (KeyValuePair<string, List<string>> pair in groupedValues)
								builder.AddCriteria(Utils.StringArrayToQuery(pair.Value, columnNameMapping[pair.Key]));

							builder
								.AddOrderMultiColumn(sortField, sortOrder)
								.Limit(limit, selStart);

							result = helper.Fill(builder.GetSql());
						});
				return result;
			}
			catch(Exception ex)
			{
				_log.Error("", ex);
				throw;
			}
		}

		public DataSet GetPriceList(string[] firmName)
		{
			DataSet result = null;
			With.Transaction(
				delegate(MySqlHelper helper)
					{
						SqlBuilder builder = SqlBuilder
							.ForCommandTest(@"
call GetPrices(?ClientCode);

select	p.PriceCode as PriceCode,
		p.PriceName as PriceName,
		p.PriceDate as PriceDate,
		rd.ContactInfo,
		rd.OperativeInfo,
		p.PublicUpCost as PublicUpCost,
  		p.DisabledByClient,
		cd.ShortName as FirmShortName,
		cd.FullName as FirmFullName,
		rd.SupportPhone as RegionPhone,
		(select c.contactText
        from contacts.contact_groups cg
          join contacts.contacts c on cg.Id = c.ContactOwnerId
        where cd.ContactGroupOwnerId = cg.ContactGroupOwnerId
              and cg.Type = 0
              and c.Type = 1
        limit 1) as Phone,
		cd.Adress as Address
from prices p
	join usersettings.clientsdata cd on p.firmcode = cd.firmcode
		join usersettings.regionaldata rd on rd.firmcode = cd.firmcode and rd.regioncode = p.regioncode")
							.AddCriteria(Utils.StringArrayToQuery(firmName, "cd.ShortName"))
							.AddOrder("cd.ShortName", null);

						result = helper.Fill(builder.GetSql());
					});
			return result;
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
				if (!result.ContainsKey(field.ToLower()))
					result.Add(field.ToLower(), new List<string>());
				result[field].Add(values[i]);
				i++;
			}
			return result;
		}

		private static void ValidateFieldNames(IDictionary<string, string> mapping, 
											   IEnumerable<string> fieldsToValidate)
		{
			foreach (string fieldName in fieldsToValidate)
				if (!mapping.ContainsKey(fieldName.ToLower()))
					throw new Exception(String.Format("Не найдено сопоставление для поля {0}", fieldName));
		}

		private static void ValidateSortDirection(IEnumerable<string> sortDirections)
		{
			foreach (string sortDirection in sortDirections)
				if (String.Compare(sortDirection, "asc", true) != 0 
					&& String.Compare(sortDirection, "desc", true) != 0)
					throw new Exception(String.Format("Не верный порядок сортировки {0}", sortDirection));
		}

		private uint GetClientCode(MySqlHelper helper)
		{
			string userName = ServiceSecurityContext.Current.PrimaryIdentity.Name.Replace(@"ANALIT\", "");
			
			_log.Error(userName);

			return helper.Command(@"
SELECT clientcode 
FROM osuseraccessright 
WHERE OSUserName = ?UserName")
					.AddParameter("?UserName", userName)
					.ExecuteScalar<uint>();
		}
	}
}
