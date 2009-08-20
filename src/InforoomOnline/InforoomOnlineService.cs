using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Common.Models;
using Common.Models.Repositories;
using Common.MySql;
using Common.Service;
using Common.Tools;
using With=Common.MySql.With;

namespace InforoomOnline
{
	public class InforoomOnlineService : IInforoomOnlineService
	{
		private IOfferRepository _offerRepository;
		private IRepository<Order> _orderRepository;
		private IRepository<OrderRules> _orderRulesRepository;

		public InforoomOnlineService(IRepository<Order> orderRepository, IRepository<OrderRules> orderRulesRepository, IOfferRepository offerRepository)
		{
			_orderRepository = orderRepository;
			_orderRulesRepository = orderRulesRepository;
			_offerRepository = offerRepository;
		}

		public DataSet GetOffers(string[] rangeField,
								 string[] rangeValue,
								 bool newEar,
								 string[] sortField,
								 string[] sortOrder,
								 int limit,
								 int selStart)
		{
			DataSet result = null;
			uint clientCode = ServiceContext.Client.FirmCode;
			With.Transaction(helper => {
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

					using (StorageProcedures.GetOffers(helper.Connection, clientCode))
					{
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
		c.RequestRatio,
		c.OrderCost as MinOrderSum,
		c.MinOrderCount,
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
			var clientCode = ServiceContext.Client.FirmCode;
			With.Transaction(helper => {
				using (StorageProcedures.GetPrices(helper.Connection, clientCode))
				{
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
						.AddParameter("?ClientCode", clientCode)
						.Fill();
				}
			});
			return result;
		}

		public DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart)
		{
			DataSet result = null;
			var clientCode = ServiceContext.Client.FirmCode;
			With.Transaction(helper => {
				SqlBuilder builder;
				using (StorageProcedures.GetActivePrices(helper.Connection, clientCode))
				{
					if (offerOnly)
					{
						builder =
							SqlBuilder.WithCommandText(
								@"
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
						builder =
							SqlBuilder.WithCommandText(
								@"
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
						.AddParameter("?ClientCode", clientCode)
						.Fill();
				}
			});
			return result;
		}

		public DataSet PostOrder(long[] offerId, int[] quantity, string[] message)
		{
			var toResult = new DataSet();
			toResult.Tables.Add();
			toResult.Tables[0].Columns.Add("OfferId", typeof(long));
			toResult.Tables[0].Columns.Add("Posted", typeof(bool));

			var client = ServiceContext.Client;
			var orderRules = _orderRulesRepository.Get(client.FirmCode);

			var offers = _offerRepository.GetByIds(client, offerId.Select(v => (ulong)v));
			var orders = new List<Order>();
			for(var i = 0; i < offerId.Length; i++)
			{
				var id = (ulong)offerId[i];
				var row = toResult.Tables[0].NewRow();
				toResult.Tables[0].Rows.Add(row);
				row["OfferId"] = offerId[i];
				var offer = offers.FirstOrDefault(o => o.Id == id);
				if (offer == null)
				{
					row["Posted"] = false;
					continue;
				}
				row["Posted"] = true;
				var order = orders.FirstOrDefault(o => o.PriceList.PriceCode == offer.PriceList.PriceCode);
				if (order == null)
				{
					order = new Order(true, offer.PriceList, client, orderRules);
					order.ClientAddition = message[i];
					orders.Add(order);
				}

				order.AddOrderItem(offer, (uint) quantity[i]);
			}
			Common.Models.With.Transaction(() => {
				orders.Each(_orderRepository.Save);
			});

			return toResult;
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
	}
}