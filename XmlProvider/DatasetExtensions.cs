using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace XmlProvider
{
    public static class DataSetExtensions
    {
        public static List<T> ToFrList<T>(this DataSet ds) where T : class
        {
            return (from DataRow row in ds.Tables[0].Rows select GetRow<T>(row)).ToList();
        }

        private static T GetRow<T>(DataRow dr)
        {
            var gelenTip = typeof(T);
            // tip olarak verilen entity clasının bir instancesini oluşturuyoruz.
            var entityClass = Activator.CreateInstance<T>();

            foreach (var property in gelenTip.GetProperties())
            {
                var fieldName = property.Name;
                // data yoksa bu colonda o propertyi setlemiyoruz.
                if (Convert.IsDBNull(dr[fieldName]))
                    continue;

                //property tipine göre convert işlemi yapılıp setleme yapılıyor
                var propType = property.PropertyType;

                if (propType == typeof(Int16) || propType == typeof(Int16?))
                {
                    short deger;
                    short.TryParse(dr[fieldName].ToString(), out deger);
                    property.SetValue(entityClass, deger, null);
                }

                if (propType == typeof(Int32) || propType == typeof(Int32?))
                {
                    int deger;
                    int.TryParse(dr[fieldName].ToString(), out deger);
                    property.SetValue(entityClass, deger, null);
                }

                if (propType == typeof(Int64) || propType == typeof(Int64?))
                {
                    long deger;
                    long.TryParse(dr[fieldName].ToString(), out deger);
                    property.SetValue(entityClass, deger, null);
                }

                if (propType == typeof(String))
                {
                    property.SetValue(entityClass, dr[fieldName].ToString(), null);
                }

                if (propType == typeof(DateTime) || propType == typeof(DateTime?))
                {
                    DateTime deger;
                    DateTime.TryParse(dr[fieldName].ToString(), out deger);
                    property.SetValue(entityClass, deger, null);
                }

                if (propType == typeof(Boolean) || propType == typeof(Boolean?))
                {
                    bool deger;
                    bool.TryParse(dr[fieldName].ToString(), out deger);
                    property.SetValue(entityClass, deger, null);
                }

                if (propType == typeof(decimal) || propType == typeof(decimal?))
                {
                    decimal deger;
                    decimal.TryParse(dr[fieldName].ToString(), out deger);
                    property.SetValue(entityClass, deger, null);
                }

                if (propType == typeof(double) || propType == typeof(double?))
                {
                    double deger;
                    double.TryParse(dr[fieldName].ToString(), out deger);
                    property.SetValue(entityClass, deger, null);
                }

                if (propType == typeof(float) || propType == typeof(float?))
                {
                    float deger;
                    float.TryParse(dr[fieldName].ToString(), out deger);
                    property.SetValue(entityClass, deger, null);
                }
            }
            return entityClass;
        }
    }
}
