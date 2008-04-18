using System;
using System.Collections;
using System.Data;

namespace InforoomOnline.Logging
{
    public class RowCalculatorAttribute : Attribute
    {
        public virtual int GetRowCount(object value)
        {
            if (value == null || !(value is DataSet))
                return 0;
            return ((DataSet) value).Tables[0].Rows.Count;
        }
    }

    public class OfferRowCalculatorAttribute : RowCalculatorAttribute
    {
        public override int GetRowCount(object value)
        {
            if (value == null || !(value is DataSet))
                return base.GetRowCount(value);

            var data = (DataSet) value;
            var hash = new Hashtable();
            foreach (DataRow row in data.Tables[0].Rows)
            {
                var fullCode = Convert.ToUInt32(row["FullCode"]);
                if (!hash.Contains(fullCode))
                    hash.Add(fullCode, null);
            }
            return hash.Count;
        }
    }
}