using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel.Activation;
using Castle.Core;
using Common.MySql;
using Common.Service;
using Common.Service.Interceptors;
using MySqlHelper=Common.MySql.MySqlHelper;

namespace InforoomOnline
{
	[
		Interceptor(typeof(ErrorLoggingInterceptor)),
		ResultLoging("InforoomOnlineService"),
		ReqiredPermission("IOL"),
		UpdateLastAccessTime("IOLTime"),
		Interceptor(typeof(MonitorExecutingTimeInterceptor)),
		AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
	]
	public class InforoomOnlineService : IInforoomOnlineService
	{
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
                    var columnNameMapping = new Dictionary<string, string>
                                                    {
                                                        {"offerid", "offers.Id"},
                                                        {"pricecode", "offers.PriceCode"},
                                                        {"fullcode", "p.CatalogId"},
                                                        {"name", "s.synonym"},
                                                        {"crname", "sfc.synonym"},
                                                        {"code", "c.Code"},
                                                        {"codecr", "c.CodeCr"},
                                                        {"unit", "c.Unit"},
                                                        {"volume", "c.Volume"},
                                                        {"quantity", "c.Quantity"},
                                                        {"note", "c.Note"},
                                                        {"period", "c.Period"},
                                                        {"doc", "c.Doc"},
                                                        {"junk", "c.Junk"},
                                                        {"cost", "offers.Cost"}
                                                    };

                    ValidateFieldNames(columnNameMapping, rangeField);
                    ValidateFieldNames(columnNameMapping, sortField);
                    ValidateSortDirection(sortOrder);

                    var groupedValues = GroupValues(rangeField, rangeValue);

                    if (rangeField != null
                        && (rangeValue == null
                            || rangeField.Length != rangeValue.Length))
                        throw new Exception("Количество полей для фильтрации не совпадает с количеством значение по которым производится фильтрация");

                    using (CleanUp.AfterGetOffers(helper))
                    {
                        helper
                            .Command("Usersettings.GetOffers", CommandType.StoredProcedure)
                            .AddParameter("?ClientCodeParam", GetClientCode(helper))
                            .AddParameter("?FreshOnly", false)
                            .Execute();

                        var builder = SqlBuilder
                            .WithCommandText(@"
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
		c.Junk,
		offers.Cost 
FROM core as offers
    JOIN farm.core0 as c on c.id = offers.id
		JOIN farm.synonym s on c.synonymcode = s.synonymcode
		JOIN farm.synonymfirmcr sfc on sfc.SynonymFirmCrCode = c.synonymfirmcrcode
		JOIN Catalogs.Products p on p.Id = c.ProductId");

                        foreach (var pair in groupedValues)
                            builder.AddInCriteria(columnNameMapping[pair.Key], pair.Value);

                        result = builder
                            .AddOrderMultiColumn(sortField, sortOrder)
                            .Limit(limit, selStart)
                            .ToCommand(helper)
                            .Fill();
                    }
                });
			return result;
		}

		public DataSet GetPriceList(string[] firmName)
		{
			DataSet result = null;
			With.Transaction(
				delegate(MySqlHelper helper)
					{
						using (CleanUp.AfterGetPrices(helper))
						{
							helper
								.Command("call GetPrices(?ClientCode);")
								.AddParameter("?ClientCode", GetClientCode(helper))
								.Execute();

							result = SqlBuilder
								.WithCommandText(@"
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
								.AddInCriteria("cd.ShortName", firmName)
								.AddOrder("cd.ShortName")
								.ToCommand(helper)
								.AddParameter("?ClientCode", GetClientCode(helper))
								.Fill();
						}
					});
			return result;
		}

		public DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart)
		{
			DataSet result = null;
			With.Transaction(
				delegate (MySqlHelper helper)
					{
						SqlBuilder builder;
						using (CleanUp.AfterGetActivePrices(helper))
						{
							if (offerOnly)
							{
								helper
									.Command("CALL GetActivePrices(?ClientCode);")
									.AddParameter("?ClientCode", GetClientCode(helper))
									.Execute();

								builder = SqlBuilder.WithCommandText(@"
SELECT	distinct c.id as FullCode, 
		cn.name, 
		cf.form
FROM Catalogs.Catalog c
	JOIN Catalogs.CatalogNames cn on cn.id = c.nameid
	JOIN Catalogs.CatalogForms cf on cf.id = c.formid
	JOIN Catalogs.Products p on p.CatalogId = c.Id
	JOIN Farm.Core0 c0 on c0.ProductId = p.Id
		JOIN activeprices ap on ap.PriceCode = c0.PriceCode");
							}
							else
							{
								builder = SqlBuilder.WithCommandText(@"
SELECT	c.id as FullCode,  
		cn.name, 
		cf.form
FROM Catalogs.Catalog c 
	JOIN Catalogs.CatalogNames cn on cn.id = c.nameid
	JOIN Catalogs.CatalogForms cf on cf.id = c.formid");
							}

							result = builder
								.AddInCriteria("cn.Name", name)
								.AddInCriteria("cf.Form", form)
								.AddCriteria("c.Hidden = 0")
								.Limit(limit, selStart)
								.ToCommand(helper)
								.AddParameter("?ClientCode", GetClientCode(helper))
								.Fill();
						}
					});
			return result;
		}

		public DataSet PostOrder(long[] offerId, int[] quantity, string[] message)
		{
			DataSet result = null;
			With.Transaction(
				delegate(MySqlHelper helper)
					{
						var offersStatus = new Dictionary<long, bool>();

						foreach (var id in offerId)
							offersStatus.Add(id, false);

					    var CoreIDString = "(";
					    var index = 0;
						foreach (var ID in offerId)
						{
                            if (index >= quantity.Length || quantity[index] < 1)
                                continue;

							if ((CoreIDString.Length > 1) && (ID > 0))
								CoreIDString += ", ";

							if (ID > 0)
								CoreIDString += ID.ToString();

						    index++;
						}
						CoreIDString += ")";


						var clientCode = GetClientCode(helper);
						var dsPost = new DataSet();
						var submit = helper
										.Command("select SubmitOrders from usersettings.RetClientsSet where clientcode = ?ClientCode")
										.AddParameter("?ClientCode", clientCode)
										.ExecuteScalar<bool>();

						using (CleanUp.AfterGetActivePrices(helper))
						{
							helper
								.Command("CALL GetActivePrices(?ClientCode);")
								.AddParameter("?ClientCode", clientCode)
								.Execute();


							var commandText = @"
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
        c.Junk,
		c.Await,
		c.OrderCost,
		c.MinOrderCount,
		c.RequestRatio,
		if(if(round(cc.Cost*ap.UpCost,2)<MinBoundCost, MinBoundCost, round(cc.Cost*ap.UpCost,2))>MaxBoundCost,MaxBoundCost, if(round(cc.Cost*ap.UpCost,2)<MinBoundCost, MinBoundCost, round(cc.Cost*ap.UpCost,2))) as Cost
FROM (farm.core0 c, usersettings.clientsdata cd)
  JOIN ActivePrices ap on c.PriceCode = ap.PriceCode
    JOIN farm.CoreCosts cc on cc.Core_Id = c.Id and cc.PC_CostCode = ap.CostCode
WHERE   cd.FirmCode	= ?ClientCode
	    AND c.ID in " + CoreIDString;

							helper.Command(commandText)
								.AddParameter("?ClientCode", GetClientCode(helper))
								.Fill(dsPost, "SummaryOrder");
						}

						var dtSummaryOrder = dsPost.Tables["SummaryOrder"];

						foreach (DataRow row in dtSummaryOrder.Rows)
						{
							var toOrderOfferId = Convert.ToInt64(row["Id"]);
							offersStatus[toOrderOfferId] = true;
						}

						dtSummaryOrder.Columns.Add(new DataColumn("Message", typeof (string)));
						for (var i = 0; i < offerId.Length; i++)
						{
							var drs = dtSummaryOrder.Select("Id = " + offerId[i]);

						    if (drs.Length <= 0) 
                                continue;

						    drs[0]["Quantity"] = quantity[i];
						    if (message != null && message.Length > i)
						        drs[0]["Message"] = message[i];
						}

						var dtOrderHead = dtSummaryOrder.DefaultView.ToTable(true, "ClientCode", "RegionCode", "PriceCode", "PriceDate");
						dtOrderHead.Columns.Add(new DataColumn("OrderID", typeof (long)));

						DataRow[] drOrderList;
						foreach (DataRow drOH in dtOrderHead.Rows)
						{
							drOrderList = dtSummaryOrder.Select("PriceCode = " + drOH["PriceCode"]);

						    if (drOrderList.Length <= 0) 
                                continue;

						    drOH["OrderID"] =
						        helper.Command(@"
insert into orders.ordershead (WriteTime, ClientCode, PriceCode, RegionCode, PriceDate, RowCount, ClientAddition, Processed, Submited, SubmitDate)
values(now(), ?ClientCode, ?PriceCode, ?RegionCode, ?PriceDate, ?RowCount, ?ClientAddition, 0, ?Submited, ?SubmitDate);


select LAST_INSERT_ID();")
						            .AddParameter("?ClientCode", drOH["ClientCode"])
						            .AddParameter("?PriceCode", drOH["PriceCode"])
						            .AddParameter("?RegionCode", drOH["RegionCode"])
						            .AddParameter("?PriceDate", drOH["PriceDate"])
						            .AddParameter("?RowCount", drOrderList.Length)
						            .AddParameter("?ClientAddition", drOrderList[0]["Message"])
									.AddParameter("?Submited", submit ? 0 : 1)
									.AddParameter("?SubmitDate", submit ? null : new DateTime?(DateTime.Now))
						            .ExecuteScalar();


						    foreach (var drOL in drOrderList)
						    {
						        helper.Command(@"
insert into orders.orderslist (OrderID, ProductId, CodeFirmCr, SynonymCode, SynonymFirmCrCode, Code, CodeCr, Quantity, Junk, Await, Cost, CoreId, MinOrderCount, RequestRatio, OrderCost) 
values (?OrderID, ?ProductId, ?CodeFirmCr, ?SynonymCode, ?SynonymFirmCrCode, ?Code, ?CodeCr, ?Quantity, ?Junk, ?Await, ?Cost, ?CoreId, ?MinOrderCount, ?RequestRatio, ?OrderCost);")
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
									.AddParameter("?CoreId", drOL["Id"])
									.AddParameter("?MinOrderCount", drOL["MinOrderCount"])
									.AddParameter("?OrderCost", drOL["OrderCost"])
									.AddParameter("?RequestRatio", drOL["RequestRatio"])
						            .Execute();
						    }
						}

						var toResult = new DataSet();
						toResult.Tables.Add();
						toResult.Tables[0].Columns.Add("OfferId", typeof(long));
						toResult.Tables[0].Columns.Add("Posted", typeof(bool));
						foreach (var offer in offersStatus)
						{
							var row = toResult.Tables[0].NewRow();
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
		    var result = new Dictionary<string, List<string>>();
		    var i = 0;
		    if (fields != null)
		    {
		        foreach (var field in fields)
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
			if (fieldsToValidate == null)
                return;

		    foreach (var fieldName in fieldsToValidate)
		        if (!mapping.ContainsKey(fieldName.ToLower()))
		            throw new Exception(String.Format("Не найдено сопоставление для поля {0}", fieldName));
		}

		private static void ValidateSortDirection(IEnumerable<string> sortDirections)
		{
			if (sortDirections == null)
                return;

		    foreach (var sortDirection in sortDirections)
		        if (String.Compare(sortDirection, "asc", true) != 0
		            && String.Compare(sortDirection, "desc", true) != 0)
		            throw new Exception(String.Format("Не верный порядок сортировки {0}", sortDirection));
		}

		private static uint GetClientCode(MySqlHelper helper)
		{
			var userName = ServiceContext.GetUserName().Replace(@"ANALIT\", "");

			return helper.Command(@"
SELECT clientcode 
FROM osuseraccessright 
WHERE OSUserName = ?UserName")
				.AddParameter("?UserName", userName)
				.ExecuteScalar<uint>();
		}
	}
}