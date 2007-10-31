using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using InforoomOnline.Logging;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace InforoomOnline.Tests
{
	[TestFixture]
	public class RowCalculatorAttributeFixture
	{
		[Test]
		public void GetRowCount()
		{
			DataSet data = new DataSet();
			data.Tables.Add();
			data.Tables[0].Rows.Add(data.Tables[0].NewRow());
			data.Tables[0].Rows.Add(data.Tables[0].NewRow());
			data.Tables[0].Rows.Add(data.Tables[0].NewRow());
			RowCalculatorAttribute calculator = new RowCalculatorAttribute();
			Assert.That(calculator.GetRowCount(data), Is.EqualTo(3));
		}

		[Test]
		public void GetOffersRowCountTest()
		{
			DataSet data = new DataSet();
			data.Tables.Add();
			data.Tables[0].Columns.Add("FullCode", typeof (UInt32));
			data.Tables[0].Rows.Add(data.Tables[0].NewRow());
			data.Tables[0].Rows.Add(data.Tables[0].NewRow());
			data.Tables[0].Rows.Add(data.Tables[0].NewRow());
			data.Tables[0].Rows[0]["FullCode"] = 2;
			data.Tables[0].Rows[1]["FullCode"] = 1;
			data.Tables[0].Rows[2]["FullCode"] = 2;

			OfferRowCalculatorAttribute calculator = new OfferRowCalculatorAttribute();
			Assert.That(calculator.GetRowCount(data), Is.EqualTo(2));
		}
	}
}

