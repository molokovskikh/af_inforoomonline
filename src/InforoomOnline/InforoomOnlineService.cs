﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceModel;
using Common.Models;
using Common.Models.Repositories;
using Common.MySql;
using Common.Service;
using Common.Tools;
using log4net;
using MySql.Data.MySqlClient;
using NHibernate;
using MySqlHelper = Common.MySql.MySqlHelper;
using With = Common.MySql.With;

namespace InforoomOnline
{
	public class BaseService
	{
		protected ISession Session;

		public User User
		{
			get { return ServiceContext.User; }
		}

		protected IDisposable GetPrices(MySqlConnection connection)
		{
			return StorageProcedures.GetPrices(connection, User.Id);
		}

		protected IDisposable GetOffers(MySqlConnection connection)
		{
			return StorageProcedures.GetOffers(connection, User.Id);
		}

		protected IDisposable GetActivePrices(MySqlConnection connection)
		{
			return StorageProcedures.GetActivePrices(connection, User.Id);
		}
	}

	public class InforoomOnlineService : BaseService, IInforoomOnlineService
	{
		private ILog log = LogManager.GetLogger(typeof(InforoomOnlineService));

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
			var settings = Session.Load<OrderRules>(User.Client.Id);
			return With.Connection(c => {
				var helper = new MySqlHelper(c);
				var columnNameMapping = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
					{ "offerid", "offers.Id" },
					{ "pricecode", "offers.PriceCode" },
					{ "fullcode", "p.CatalogId" },
					{ "name", "s.synonym" },
					{ "crname", "sfc.synonym" },
					{ "code", "c.Code" },
					{ "codecr", "c.CodeCr" },
					{ "unit", "c.Unit" },
					{ "volume", "c.Volume" },
					{ "quantity", "c.Quantity" },
					{ "note", "c.Note" },
					{ "period", "c.Period" },
					{ "doc", "c.Doc" },
					{ "junk", "c.Junk" },
					{ "cost", "offers.Cost" },
					{ "SupplierId", "ap.FirmCode" },
				};

				ValidateFieldNames(columnNameMapping, rangeField);
				ValidateFieldNames(columnNameMapping, sortField);
				ValidateSortDirection(sortOrder);

				var groupedValues = GroupValues(rangeField, rangeValue);

				if (rangeField != null
					&& (rangeValue == null
						|| rangeField.Length != rangeValue.Length))
					throw new Exception("Количество полей для фильтрации не совпадает с количеством значение по которым производится фильтрация");

				using (GetOffers(c)) {
					var builder = SqlBuilder
						.WithCommandText(@"
SELECT	offers.Id as OfferId,
	ap.FirmCode SupplierId,
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
	c.OrderCost MinOrderSum,
	c.MinOrderCount,
	max(cc.Cost) RegistryCost,
	c.VitallyImportant,
	offers.Cost,
	cl.Code as ClientCatalogId
FROM core as offers
JOIN farm.core0 as c on c.id = offers.id
	join farm.Synonym s on c.synonymcode = s.synonymcode
	join usersettings.ActivePrices ap on ap.PriceCode = c.PriceCode
	left join farm.SynonymFirmCr sfc on sfc.SynonymFirmCrCode = c.synonymfirmcrcode
	join Catalogs.Products p on p.Id = c.ProductId
	left join farm.Core0 rp on rp.PriceCode = 4863 and rp.ProductId = c.ProductId and rp.CodeFirmCr <=> c.CodeFirmCr
	left join farm.CoreCosts cc on cc.Core_id = rp.id and cc.PC_CostCode = 8317
	left join farm.Core0 cl on cl.ProductId = c.ProductId and cl.CodeFirmCr <=> c.CodeFirmCr and cl.PriceCode = ?mapPriceId");

					foreach (var pair in groupedValues)
						builder.AddInCriteria(columnNameMapping[pair.Key], pair.Value);

					return builder
						.Append("group by c.Id")
						.AddOrderMultiColumn(sortField, sortOrder)
						.Limit(limit, selStart)
						.ToCommand(helper)
						.AddParameter("mapPriceId", settings.CatalogMapPriceId)
						.Fill();
				}
			});
		}

		public DataSet GetPriceList(string[] firmName)
		{
			return With.Connection(c => {
				var helper = new MySqlHelper(c);
				using (GetPrices(c)) {
					return SqlBuilder
						.WithCommandText(@"
select	p.FirmCode SupplierId,
		p.PriceCode as PriceCode,
		p.PriceName as PriceName,
		p.PriceDate as PriceDate,
		rd.ContactInfo,
		rd.OperativeInfo,
		0 as PublicUpCost,
		p.DisabledByClient,
		s.Name as FirmShortName,
		s.FullName as FirmFullName,
		rd.SupportPhone as RegionPhone,
		(select c.contactText
		from contacts.contact_groups cg
		  join contacts.contacts c on cg.Id = c.ContactOwnerId
		where s.ContactGroupOwnerId = cg.ContactGroupOwnerId
			  and cg.Type = 0
			  and c.Type = 1
		limit 1) as Phone,
		'' as Address
from prices p
	join Customers.Suppliers s on p.firmcode = s.Id
		join usersettings.regionaldata rd on rd.firmcode = s.Id and rd.regioncode = p.regioncode")
						.AddInCriteria("s.Name", firmName)
						.AddOrder("s.Name")
						.ToCommand(helper)
						.Fill();
				}
			});
		}

		public DataSet GetSupplierInfoById(int firmId)
		{
			return With.Connection(c => {
				var helper = new MySqlHelper(c);
				using (GetPrices(c))
				{
					return SqlBuilder
						.WithCommandText(@"
select	p.FirmCode SupplierId,
		p.PriceCode as PriceCode,
		p.PriceName as PriceName,
		p.PriceDate as PriceDate,
		rd.ContactInfo,
		rd.OperativeInfo,
		0 as PublicUpCost,
		p.DisabledByClient,
		s.Name as FirmShortName,
		s.FullName as FirmFullName,
		rd.SupportPhone as RegionPhone,
		(select c.contactText
		from contacts.contact_groups cg
		  join contacts.contacts c on cg.Id = c.ContactOwnerId
		where s.ContactGroupOwnerId = cg.ContactGroupOwnerId
			  and cg.Type = 0
			  and c.Type = 1
		limit 1) as Phone,
		s.Address as Address
from prices p
	join Customers.Suppliers s on p.firmcode = s.Id
		join usersettings.regionaldata rd on rd.firmcode = s.Id and rd.regioncode = p.regioncode")
						.AddInCriteria("p.FirmCode", new int[] {firmId})
						.ToCommand(helper)
						.Fill();
				}
			});
		}

		public DataSet GetMinReqSettings()
		{
			return With.Connection(c => {
				var helper = new MySqlHelper(c);
				using (GetPrices(c)) {
					return SqlBuilder
						.WithCommandText(@"
select a.Address,
	p.PriceCode,
	p.RegionCode,
	ai.ControlMinReq,
	if(ai.MinReq > 0, ai.MinReq, p.MinReq) as MinReq
from (Usersettings.Prices p, Customers.Users u)
join Customers.Addresses a on a.ClientId = u.ClientId
join Customers.Intersection i on i.PriceId = p.PriceCode
	and i.RegionId = p.RegionCode
	and i.ClientId = u.ClientId
	and a.LegalEntityId = i.LegalEntityId
join Customers.AddressIntersection ai on ai.IntersectionId = i.Id and a.Id = ai.AddressId")
						.AddCriteria("u.Id = ?UserId")
						.AddOrder("a.Address")
						.ToCommand(helper)
						.AddParameter("userId", ServiceContext.User.Id)
						.Fill();
				}
			});
		}

		public DataSet GetWaybills(DateTime begin, DateTime end)
		{
			return With.Connection(c => {
				return c.FillDataSet(@"
select l.RowId as Id,
	l.LogTime,
	l.FirmCode as SupplierId,
	l.AddressId,
	l.DocumentType as DocType,
	l.DocumentSize as DocSize,
	l.FileName
from Logs.Document_Logs l
	join Customers.Addresses a on a.Id = l.AddressId
		join Customers.UserAddresses ua on a.Id = ua.AddressId
			join Customers.Users u on ua.UserId = u.Id and a.ClientId = u.ClientId
where l.LogTime >= ?begin and l.LogTime <= ?end
	and l.IsFake = 0
	and a.Enabled = 1
	and u.Id = ?userId", new { userId = User.Id, begin, end });
			});
		}

		public Stream GetWaybill(uint id)
		{
			var data = With.Connection(c => c
				.Read("select DocumentType, AddressId from Logs.Document_Logs where RowId = ?id", new { id })
				.Select(r => new { docType = (DocType)Convert.ToUInt32(r["DocumentType"]), addressId = Convert.ToUInt32(r["addressId"]) })
				.FirstOrDefault());
			if (data == null) {
				log.WarnFormat("Документ с номером {0} не найден", id);
				throw new FaultException(String.Format("Документ с номером {0} не найден", id));
			}
			var docPath = ConfigurationManager.AppSettings["DocPath"];

			var addresses = User.AvaliableAddresses.Where(a => a.Enabled && a.Client == User.Client).Select(a => a.Id);
			if (!addresses.Contains(data.addressId)) {
				log.WarnFormat("Попытка получить накладную {0} адреса доставки {1} доступа к которому у него нет", id, data.addressId);
				throw new FaultException("Доступ запрещен");
			}

			string path = Path.Combine(docPath, data.addressId.ToString(), data.docType.ToString());
			var file = Directory.GetFiles(path, String.Format("{0}_*", id)).FirstOrDefault();
			if (String.IsNullOrEmpty(file)) {
				log.WarnFormat("На найден файл накладной {0} по пути {1}\\{0}_*", id, path);
				throw new FaultException("Файл не найден, файлы накладных хранятся за последнии 3 месяца.");
			}
			return File.OpenRead(file);
		}

		public DataSet GetAddresses()
		{
			return With.Connection(c => {
				return c.FillDataSet(@"
select a.Id, a.Address as Name
from Customers.Users u
	join Customers.UserAddresses ua on u.Id = ua.UserId
		join Customers.Addresses a on ua.AddressId = a.Id and a.ClientId = u.ClientId
where a.Enabled = 1 and u.Id = ?userId", new { userId = User.Id });
			});
		}

		public DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart)
		{
			DataSet result = null;
			With.Connection(c => {
				var helper = new MySqlHelper(c);
				SqlBuilder builder;
				using (GetActivePrices(c)) {
					if (offerOnly) {
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
					else {
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
						.Fill();
				}
			});
			return result;
		}

		public DataSet PostOrder(long[] offerId, int[] quantity, string[] message, uint addressId)
		{
			var toResult = new DataSet();
			toResult.Tables.Add();
			toResult.Tables[0].Columns.Add("OfferId", typeof(long));
			toResult.Tables[0].Columns.Add("Posted", typeof(bool));
			var orders = new List<Order>();

			using (new UnitOfWork()) {
				var session = UnitOfWork.Current.CurrentSession;

				var user = ServiceContext.User;
				var address = session.Load<Address>(addressId);
				var orderRules = _orderRulesRepository.Get(ServiceContext.Client.FirmCode);
				orderRules.CheckAddressAccessibility = false;
				orderRules.Strict = true;

				var offers = _offerRepository.GetByIds(user, offerId.Select(v => (ulong)v));

				for (var i = 0; i < offerId.Length; i++) {
					var id = (ulong)offerId[i];
					var row = toResult.Tables[0].NewRow();
					toResult.Tables[0].Rows.Add(row);
					row["OfferId"] = offerId[i];
					var offer = offers.FirstOrDefault(o => o.Id.CoreId == id);
					if (offer == null) {
						row["Posted"] = false;
						continue;
					}
					row["Posted"] = true;
					var order = orders.FirstOrDefault(o => o.PriceList.PriceCode == offer.PriceList.Id.Price.PriceCode);
					if (order == null) {
						order = new Order(offer.PriceList, user, address, orderRules);
						order.ClientAddition = message[i];
						orders.Add(order);
					}

					order.AddOrderItem(offer, (uint)quantity[i]);
				}
			}
			Common.Models.With.Transaction(() => { orders.Each(_orderRepository.Save); });

			return toResult;
		}

		private static Dictionary<string, List<string>> GroupValues(IEnumerable<string> fields, string[] values)
		{
			var result = new Dictionary<string, List<string>>();
			var i = 0;
			if (fields != null) {
				foreach (var field in fields) {
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
				if (!mapping.ContainsKey(fieldName))
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