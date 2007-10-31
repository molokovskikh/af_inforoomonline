using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel.Activation;
using Castle.Core;
using Common.Models;
using Common.Models.Repositories;
using Common.MySql;
using InforoomOnline.Logging;
using MySql.Data.MySqlClient;
using MySqlHelper=Common.MySql.MySqlHelper;

namespace InforoomOnline
{
	[Interceptor(typeof(ErrorLoggingInterceptor))]
	[Interceptor(typeof(ResultLogingInterceptor))]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class InforoomOnlineService : IInforoomOnlineService
	{
		[OfferRowCalculator]
		public DataSet GetOffers(string[] rangeField,
		                         string[] rangeValue,
		                         bool newEar,
		                         string[] sortField,
		                         string[] sortOrder,
		                         int limit,
		                         int selStart)
		{
			DataSet result = null;
			With.Transaction(
				delegate(MySqlHelper helper)
					{
						Dictionary<string, string> columnNameMapping = new Dictionary<string, string>();
                        columnNameMapping.Add("offerid", "offers.Id");
                        columnNameMapping.Add("pricecode", "offers.PriceCode");
						columnNameMapping.Add("fullcode", "p.CatalogId");
                        columnNameMapping.Add("name", "s.synonym");
						columnNameMapping.Add("crname", "sfc.synonym");
						columnNameMapping.Add("code", "c.Code");
						columnNameMapping.Add("codecr", "c.CodeCr");
                        columnNameMapping.Add("unit", "c.Unit");
                        columnNameMapping.Add("volume", "c.Volume");
                        columnNameMapping.Add("quantity", "c.Quantity");
                        columnNameMapping.Add("note", "c.Note");
                        columnNameMapping.Add("period", "c.Period");
                        columnNameMapping.Add("doc", "c.Doc");
                        columnNameMapping.Add("junk", "c.Junk");
                        columnNameMapping.Add("cost", "offers.Cost");

						ValidateFieldNames(columnNameMapping, rangeField);
						ValidateFieldNames(columnNameMapping, sortField);
						ValidateSortDirection(sortOrder);

						Dictionary<string, List<string>> groupedValues = GroupValues(rangeField, rangeValue);

						if (rangeField != null
							&& (rangeValue == null
								|| rangeField.Length != rangeValue.Length))
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
		p.CatalogId as FullCode,
		c.Code,
		c.CodeCr,
		s.synonym as Name,
		sfc.synonym as CrName,
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
		JOIN farm.synonym s on c.synonymcode = s.synonymcode
		JOIN farm.synonymfirmcr sfc on sfc.SynonymFirmCrCode = c.synonymfirmcrcode
		JOIN Catalogs.Products p on p.Id = c.ProductId");

						foreach (KeyValuePair<string, List<string>> pair in groupedValues)
							builder.AddCriteria(Utils.StringArrayToQuery(pair.Value, columnNameMapping[pair.Key]));

						builder
							.AddOrderMultiColumn(sortField, sortOrder)
							.Limit(limit, selStart);

						result = helper
									.Command(builder.GetSql())
									.Fill();
					});
			return result;
		}

		[RowCalculator]
		public DataSet GetPriceList(string[] firmName)
		{
			DataSet result = null;
			With.Transaction(
				delegate(MySqlHelper helper)
					{
						helper
							.Command("call GetPrices(?ClientCode);")
							.AddParameter("?ClientCode", GetClientCode(helper))
							.Execute();

						SqlBuilder builder = SqlBuilder
							.ForCommandTest(@"
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
							.AddOrder("cd.ShortName");

						result = helper
									.Command(builder.GetSql())
										.AddParameter("?ClientCode", GetClientCode(helper))
										.Fill();
					});
			return result;
		}

		[RowCalculator]
		public DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart)
		{
			DataSet result = null;
			With.Transaction(
				delegate (MySqlHelper helper)
					{
						SqlBuilder builder;
						if (offerOnly)
						{
							helper
								.Command("CALL GetOffers(?ClientCode, 0);")
								.AddParameter("?ClientCode", GetClientCode(helper))
								.Execute();

							builder = SqlBuilder.ForCommandTest(@"
SELECT	distinct c.id as FullCode, 
		cn.name, 
		cf.form
FROM Catalogs.Catalog c
	JOIN Catalogs.CatalogNames cn on cn.id = c.nameid
	JOIN Catalogs.CatalogForms cf on cf.id = c.formid
	JOIN Catalogs.Products p on p.CatalogId = c.Id
		LEFT JOIN Catalogs.ProductProperties pp on pp.ProductId = p.Id
	JOIN Farm.Core0 c0 on c0.ProductId = p.Id
		JOIN core offers on offers.Id = c0.Id");
						}
						else
						{
							builder =
								SqlBuilder.ForCommandTest(@"
SELECT	c.id as FullCode,  
		cn.name, 
		cf.form
FROM Catalogs.Catalog c 
	JOIN Catalogs.CatalogNames cn on cn.id = c.nameid
	JOIN Catalogs.CatalogForms cf on cf.id = c.formid
	JOIN Catalogs.Products p on p.CatalogId = c.Id
		LEFT JOIN Catalogs.ProductProperties pp on pp.ProductId = p.Id");
						}

						builder
							.AddCriteria(Utils.StringArrayToQuery(name, "cn.Name"))
							.AddCriteria(Utils.StringArrayToQuery(name, "cf.Form"))
							.AddCriteria("pp.ProductId is null")
							.AddCriteria("p.Hidden = 0")
							.Limit(limit, selStart);

						result = helper
									.Command(builder.GetSql())
										.AddParameter("?ClientCode", GetClientCode(helper))
										.Fill();
					});
			return result;
		}

		[RowCalculator]
		public DataSet PostOrder(long[] offerId, int[] quantity, string[] message)
		{
			DataSet result = null;
			With.Transaction(
				delegate(MySqlHelper helper)
					{
						Dictionary<long, bool> offersStatus = new Dictionary<long, bool>();

						foreach (long id in offerId)
							offersStatus.Add(id, false);

						int Index;
						DataSet dsRes;
						DataTable dtPricesRes;

						DataSet dsPost;
						DataTable dtSummaryOrder;
						DataTable dtOrderHead;


						string CoreIDString = "(";
						foreach (long ID in offerId)
						{
							if ((CoreIDString.Length > 1) && (ID > 0))
							{
								CoreIDString += ", ";
							}
							if (ID > 0)
							{
								CoreIDString += ID.ToString();
							}
						}
						CoreIDString += ")";

						dsPost = new DataSet();

						helper.Command("CALL GetActivePrices(?ClientCode);")
							.AddParameter("?ClientCode", GetClientCode(helper))
							.Execute();


						string commandText = @"
SELECT  cd.FirmCode as ClientCode,
		cd.RegionCode,
		ap.PriceCode,
		ap.PriceDate,
		c.Id,
        c.ProductId,
        c.CodeFirmCr,
        c.SynonymCode,
        c.SynonymFirmCrCode,
        c.Code,
		c.CodeCr,
		0 Quantity,
        length(c.Junk) > 0 Junk,
		length(c.Await) > 0 Await,
		c.BaseCost,
		round(if(if(ap.costtype=0, corecosts.cost, c.basecost) * ap.UpCost < c.minboundcost, c.minboundcost, if(ap.costtype=0, corecosts.cost, c.basecost) * ap.UpCost),2) as Cost
FROM    (farm.formrules fr,
        usersettings.clientsdata,
        (farm.core0 c, ActivePrices ap)
          LEFT JOIN farm.corecosts
            ON corecosts.Core_Id     = c.id
              AND corecosts.PC_CostCode = ap.CostCode),
		UserSettings.ClientsData cd
WHERE c.firmcode                          = if(ap.costtype=0, ap.PriceCode, ap.CostCode)
    AND fr.firmcode                         = ap.pricecode
    AND clientsdata.firmcode                = ap.firmcode
	AND if(ap.costtype = 0, corecosts.cost is not null, c.basecost is not null)
	AND cd.FirmCode							= ?ClientCode
	AND c.ID in " +
							CoreIDString;
						helper.Command(commandText)
							.AddParameter("?ClientCode", GetClientCode(helper))
							.Fill(dsPost, "SummaryOrder");

						dtSummaryOrder = dsPost.Tables["SummaryOrder"];

						foreach (DataRow row in dtSummaryOrder.Rows)
						{
							long toOrderOfferId = Convert.ToInt64(row["Id"]);
							offersStatus[toOrderOfferId] = true;
						}

						DataRow[] drs;
						dtSummaryOrder.Columns.Add(new DataColumn("Message", typeof (string)));
						for (int i = 0; i < offerId.Length; i++)
						{
							drs = dtSummaryOrder.Select("Id = " + offerId[i]);
							if (drs.Length > 0)
							{
								drs[0]["Quantity"] = quantity[i];
								if ((message != null) && (message.Length > i))
									drs[0]["Message"] = message[i];
							}
						}

						dtOrderHead = dtSummaryOrder.DefaultView.ToTable(true, "ClientCode", "RegionCode", "PriceCode", "PriceDate");
						dtOrderHead.Columns.Add(new DataColumn("OrderID", typeof (long)));

						DataRow[] drOrderList;
						foreach (DataRow drOH in dtOrderHead.Rows)
						{
							drOrderList = dtSummaryOrder.Select("PriceCode = " + drOH["PriceCode"]);
							if (drOrderList.Length > 0)
							{
								drOH["OrderID"] =
									helper.Command(@"
insert into orders.ordershead (WriteTime, ClientCode, PriceCode, RegionCode, PriceDate, RowCount, ClientAddition, Processed)
values(now(), ?ClientCode, ?PriceCode, ?RegionCode, ?PriceDate, ?RowCount, ?ClientAddition, 0);
select LAST_INSERT_ID();")
										.AddParameter("?ClientCode", drOH["ClientCode"])
										.AddParameter("?PriceCode", drOH["PriceCode"])
										.AddParameter("?RegionCode", drOH["RegionCode"])
										.AddParameter("?PriceDate", drOH["PriceDate"], MySqlDbType.Datetime)
										.AddParameter("?RowCount", drOrderList.Length)
										.AddParameter("?ClientAddition", drOrderList[0]["Message"])
										.ExecuteScalar<object>();


								foreach (DataRow drOL in drOrderList)
								{
									helper.Command(@"
insert into orders.orderslist (OrderID, ProductId, CodeFirmCr, SynonymCode, SynonymFirmCrCode, Code, CodeCr, Quantity, Junk, Await, Cost) values (?OrderID, ?ProductId, ?CodeFirmCr, ?SynonymCode, ?SynonymFirmCrCode, ?Code, ?CodeCr, ?Quantity, ?Junk, ?Await, ?Cost);")
										.AddParameter("?OrderID", drOH["OrderID"])
										.AddParameter("?ProductId", drOL["ProductId"])
										.AddParameter("?CodeFirmCr", drOL["CodeFirmCr"])
										.AddParameter("?SynonymCode", drOL["SynonymCode"])
										.AddParameter("?SynonymFirmCrCode", drOL["SynonymFirmCrCode"])
										.AddParameter("?Code", drOL["Code"])
										.AddParameter("?CodeCr", drOL["CodeCr"])
										.AddParameter("?Junk", drOL["Junk"])
										.AddParameter("?Await", drOL["Await"])
										.AddParameter("?Cost", drOL["Cost"])
										.AddParameter("?Quantity", drOL["Quantity"])
										.Execute();
								}
							}
						}

						DataSet toResult = new DataSet();
						toResult.Tables.Add();
						toResult.Tables[0].Columns.Add("OfferId", typeof(long));
						toResult.Tables[0].Columns.Add("Posted", typeof(bool));
						foreach (KeyValuePair<long, bool> offer in offersStatus)
						{
							DataRow row = toResult.Tables[0].NewRow();
							row["OfferId"] = offer.Key;
							row["Posted"] = offer.Value;
							toResult.Tables[0].Rows.Add(row);
						}
						result = toResult;
					});

			return result;
		}

		private static Dictionary<string, List<string>> GroupValues(IEnumerable<string> fields, string[] values)
		{
				Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
				int i = 0;
				if (fields != null)
				{
					foreach (string field in fields)
					{
						if (!result.ContainsKey(field.ToLower()))
							result.Add(field.ToLower(), new List<string>());
                        result[field.ToLower()].Add(values[i]);
						i++;
					}
				}
			return result;
		}

		private static void ValidateFieldNames(IDictionary<string, string> mapping,
		                                       IEnumerable<string> fieldsToValidate)
		{
			if (fieldsToValidate != null)
			{
				foreach (string fieldName in fieldsToValidate)
					if (!mapping.ContainsKey(fieldName.ToLower()))
						throw new Exception(String.Format("Не найдено сопоставление для поля {0}", fieldName));
			}
		}

		private static void ValidateSortDirection(IEnumerable<string> sortDirections)
		{
			if (sortDirections != null)
			{
				foreach (string sortDirection in sortDirections)
					if (String.Compare(sortDirection, "asc", true) != 0
					    && String.Compare(sortDirection, "desc", true) != 0)
						throw new Exception(String.Format("Не верный порядок сортировки {0}", sortDirection));
			}
		}

		private static uint GetClientCode(MySqlHelper helper)
		{
#if !DEBUG
			string userName = ServiceSecurityContext.Current.PrimaryIdentity.Name.Replace(@"ANALIT\", "");

			return helper.Command(@"
SELECT clientcode 
FROM osuseraccessright 
WHERE OSUserName = ?UserName")
				.AddParameter("?UserName", userName)
				.ExecuteScalar<uint>();
#else
			return 2359;
#endif
		}
	}
}