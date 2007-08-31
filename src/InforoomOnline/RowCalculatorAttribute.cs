using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InforoomOnline
{
	public class RowCalculatorAttribute : Attribute
	{
		public virtual int GetRowCount(DataSet data)
		{
			return data.Tables[0].Rows.Count;
		}
	}

	public class OfferRowCalculatorAttribute : RowCalculatorAttribute
	{
		public override int GetRowCount(DataSet data)
		{
			Hashtable hash = new Hashtable();
			foreach (DataRow row in data.Tables[0].Rows)
			{
				uint fullCode = Convert.ToUInt32(row["FullCode"]);
			    if (!hash.Contains(fullCode))
					hash.Add(fullCode, null);
			}
			return hash.Count;
		}
	}
}
