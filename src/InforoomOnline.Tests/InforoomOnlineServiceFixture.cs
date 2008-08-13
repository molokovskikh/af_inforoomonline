using System;
using System.Configuration;
using System.Data;
using System.Reflection;
using System.ServiceModel;
using Castle.Windsor;
using Common.Models;
using Common.Models.Repositories;
using MySql.Data.MySqlClient;
using NHibernate.Expression;
using NHibernate.Mapping.Attributes;
using NUnit.Framework;
using InforoomOnline.Tests.ForTesting;
using NUnit.Framework.SyntaxHelpers;
using Order=Common.Models.Order;

namespace InforoomOnline.Tests
{
    [TestFixture]
    public class InforoomOnlineServiceFixture
    {
    	private InforoomOnlineService _service;
    	private string[] Empty;

		public T[] Array<T>(params T[] items)
		{
			return items;
		}

    	[SetUp]
		public void Setup()
		{
			_service = new InforoomOnlineService();
    		ServiceContext.GetUserName = () => "kvasov";

			var container = new WindsorContainer();
			container.AddComponent("RepositoryInterceptor", typeof(RepositoryInterceptor));
			container.AddComponent("OfferRepository", typeof(IOfferRepository), typeof(OfferRepository));
			container.AddComponent("Repository", typeof(IRepository<>), typeof(Repository<>));
			var holder = new SessionFactoryHolder();
			holder
				.Configuration
				.Configure()
				.AddInputStream(HbmSerializer.Default.Serialize(Assembly.Load("Common.Models")));
			holder.BuildSessionFactory();
			container.Kernel.AddComponentInstance<ISessionFactoryHolder>(holder);
			IoC.Initialize(container);
		}

    	[Test]
        public void GetNameFromCatalog()
        {
    		LogDataSet(_service.GetNamesFromCatalog(new string[0], new string[0], false, 100, 0));
            LogDataSet(_service.GetNamesFromCatalog(new[] {"*Тест*"}, new string[0], false, 100, 0));
            LogDataSet(_service.GetNamesFromCatalog(new string[0], new[] {"*Тест*"}, false, 100, 0));
            LogDataSet(_service.GetNamesFromCatalog(new string[0], new string[0], true, 100, 0));
            LogDataSet(_service.GetNamesFromCatalog(new[] {"*Тест*"}, new string[0], true, 100, 0));
            LogDataSet(_service.GetNamesFromCatalog(new string[0], new[] {"*Тест*"}, true, 100, 0));
        }

        [Test]
        public void GetOffers()
        {
            var data = _service.GetOffers(null, null, false,
                                             new string[0], new string[0], 100, 0);
            LogDataSet(data);
        }

        [Test]
        public void GetPriceList()
        {
            LogDataSet(_service.GetPriceList(new string[0]));
            LogDataSet(_service.GetPriceList(new[] {"%а%"}));
        }

        [Test]
        public void PostOrder()
        {
        	var offerRepository = IoC.Resolve<IOfferRepository>();
			var orderRepository = IoC.Resolve<IRepository<Order>>();

        	var begin = DateTime.Now;
			var data = _service.GetOffers(Array("name"), Array("*папа*"), false, Empty, Empty, 100, 0);
			Assert.That(data.Tables[0].Rows.Count, Is.GreaterThan(0), "не нашли предложений");
        	var row = data.Tables[0].Rows[0];
        	var coreId = Convert.ToInt64(row["OfferId"]);

			using (var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["Main"].ConnectionString))
			{
				connection.Open();
				var command = new MySqlCommand(@"
update farm.core0
set OrderCost = 10.5,
	MinOrderCount = 50,
	RequestRatio = 10
where id = ?coreid;

delete from orders.ordershead
where clientcode = 2575 and writetime > curdate();", connection);
				command.Parameters.AddWithValue("?CoreId", coreId);
				command.ExecuteNonQuery();
			}

        	var result = _service.PostOrder(Array(coreId),
        	                                Array(1),
        	                                Array("это тестовый заказ"));

			Assert.That(result.Tables[0].Rows[0]["OfferId"], Is.EqualTo(coreId));
			Assert.That(result.Tables[0].Rows[0]["Posted"], Is.EqualTo(true));

        	var offer = offerRepository.GetById(new Client {FirmCode = 2575}, (ulong)coreId);
        	var order = orderRepository.FindOne(Expression.Eq("ClientCode", 2575u)
        	                                    && Expression.Gt("WriteTime", begin));
			Assert.That(offer.MinOrderCount, Is.EqualTo(50));
			Assert.That(order.OrderItems[0].CoreId, Is.EqualTo(offer.Id));
			Assert.That(order.OrderItems[0].OrderCost, Is.EqualTo(offer.OrderCost));
			Assert.That(order.OrderItems[0].MinOrderCount, Is.EqualTo(offer.MinOrderCount));
			Assert.That(order.OrderItems[0].RequestRatio, Is.EqualTo(offer.RequestRatio));
        }

		[Test]
		public void All_methods_must_be_marked_with_fault_contract_attribute()
		{
			foreach(var method in typeof(IInforoomOnlineService).GetMethods())
			{
				var finded = false;
				foreach (var attribute in method.GetCustomAttributes(typeof(FaultContractAttribute), true))
				{
					finded = ((FaultContractAttribute) attribute).DetailType == typeof (ApplicationException);
					if (finded)
						break;
				}

				Assert.That(finded, String.Format("Метод {0} не содержит атрибута FaultContract с типом NotHavePermissionException", method.Name));
			}
		}

    	private static void LogDataSet(DataSet dataSet)
        {
            foreach (DataTable dataTable in dataSet.Tables)
            {
                Console.WriteLine("<table>");
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    Console.WriteLine("\t<row>");
                    Console.Write("\t");
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        Console.Write(dataRow[column] + " ");
                    }
                    Console.WriteLine();
                    Console.WriteLine("\t</row>");
                }
                Console.WriteLine("</table>");
            }
        }
    }
}
