//
//  Bu class ile xmle insert update ve delete işlemlerini yapabilirsiniz.
//  Müslüm ÖZTÜRK 14.01.2013
//
//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using XmlProvider.Model;

namespace XmlProvider
{
    public static class XmlPoco
    {
        // xml folders path web.config key name
        private const string XmlPathConfigKey = "xmlFolderPath";

        /// <summary>
        /// Get all record dataset from xml file
        /// </summary>
        private static DataSet GetAllToDataSet<T>() where T : class
        {
            var ds = new DataSet();
            ds.Clear();
            var itemType = typeof(T);
            ds.ReadXml(GetXmlPath<T>(itemType.Name), XmlReadMode.ReadSchema);
            return ds;
        }

        /// <summary>
        /// <para>Get xml file path using modelName</para>
        /// <para>This method get xml folder directory from web.config</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modelName"></param>
        /// <returns></returns>
        private static string GetXmlPath<T>(string modelName) where T : class
        {
            var xmlFolderPath = ConfigurationManager.AppSettings[XmlPathConfigKey];
            var xmlName = string.Format("{0}.xml", modelName);

            var path = xmlFolderPath + xmlName;
            var fi = new FileInfo(path);
            if (!fi.Exists)
            {
                CreateXmlFile(path, typeof(T));
            }
            return path;
        }

        /// <summary>
        /// Get all record from xml file and bind records to generic list which types model Class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        public static List<T> GetAllList<T>() where T : class
        {
            return GetAllToDataSet<T>().ToFrList<T>();
        }

        /// <summary>
        /// Get only one record to datarow from xml file by using table id
        /// </summary>
        public static DataRow Select<T>(long id, string primaryColumnName = "Id") where T : class
        {
            var ds = GetAllToDataSet<T>();
            if (DataTableExists(ds))
            {
                var dv = ds.Tables[0].DefaultView;
                dv.RowFilter = string.Format("{0}='{1}'", primaryColumnName, id);
                dv.Sort = primaryColumnName;
                DataRow dr = null;
                if (dv.Count > 0)
                {
                    dr = dv[0].Row;
                }
                dv.RowFilter = "";
                dv.Dispose();
                ds.Dispose();
                return dr;
            }
            return null;
        }

        /// <summary>
        /// Check dataset and is Dataset has table 
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        private static bool DataTableExists(DataSet ds)
        {
            if (ds != null && ds.Tables.Count > 0)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Get only one record to model class from xml file by lamda expression
        /// </summary>
        public static T Select<T>(Func<T, bool> function) where T : class
        {
            var list = GetAllList<T>();
            return list.Where(function).FirstOrDefault();
        }

        /// <summary>
        /// insert new item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="primaryColumnName"></param>
        /// <returns></returns>
        public static bool Insert<T>(T item, string primaryColumnName = "Id") where T : class
        {
            CheckOrCreateXml(item);
            var ds = GetAllToDataSet<T>();
            if (DataTableExists(ds))
            {
                var dv = ds.Tables[0].DefaultView;
                var dr = dv.Table.NewRow();

                var itemType = typeof(T);

                //tüm propertyleri dolanıp değerleri setliyoruz
                foreach (var property in itemType.GetProperties())
                {
                    dr[property.Name] = property.GetValue(item, null);
                }

                if (itemType != typeof(XmlMaxIds))
                {
                    long maxId = CheckThisTableMaxId(itemType.Name);
                    dr[primaryColumnName] = (maxId + 1).ToString(CultureInfo.InvariantCulture);
                    IncreaseMaxId(itemType.Name);
                }

                dv.Table.Rows.Add(dr);
                Save<T>(ds);
                dv.Dispose();
                ds.Dispose();
                return true;
            }
            return false;
        }


        /// <summary>
        /// Add one to table's maxId
        /// </summary>
        /// <param name="modelName"></param>
        private static void IncreaseMaxId(string modelName)
        {
            var maxIds = GetAllList<XmlMaxIds>();

            // Eğer maxId tablosunda bu tabloya ait değer var ise onu çekelim
            if (maxIds != null && maxIds.Count(x => x.TableName == modelName) > 0)
            {
                var item = maxIds.FirstOrDefault(x => x.TableName == modelName);
                if (item != null)
                {
                    item.MaxId += 1;
                    Update(item, "TableName");
                }
            }
        }

        //Identity tablosundan belirtilen modele ait max Id değeri alınır
        private static long CheckThisTableMaxId(string modelName)
        {
            //id xml i var ise seçilen xmlin en son Id sini çekelim
            var xmlFolderPath = ConfigurationManager.AppSettings[XmlPathConfigKey];

            // xml Max Id tablosunun xml dosya ismini alıyorum
            var type = typeof(XmlMaxIds);
            var xmlName = string.Format("{0}.xml", type.Name);


            var path = xmlFolderPath + xmlName;
            var fi = new FileInfo(path);
            if (!fi.Exists)
            {
                CreateXmlIdFile(path);
            }

            var maxIds = GetAllList<XmlMaxIds>();

            // Eğer maxId tablosunda bu tabloya ait değer var ise onu çekelim
            if (maxIds != null && maxIds.Count(x => x.TableName == modelName) > 0)
            {
                return maxIds.First(x => x.TableName == modelName).MaxId;
            }

            //Eğer maxId tablosunda bu tabloya ait kayıt yok ise ilk kaydı ekleyelim ve geriye 0 değerini döndürelim
            var maxId = new XmlMaxIds
                            {
                                MaxId = 0,
                                TableName = modelName
                            };

            Insert(maxId, "TableName");
            return maxId.MaxId;
        }

        private static void CreateXmlIdFile(string xmlPath)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version='1.0' standalone='yes' ?>");//Note: Xml oluştuştururken ilk karakter boşluk olamaz
            sb.AppendFormat("<XmlMaxIds>");
            sb.AppendFormat("        <xs:schema id='XmlMaxIds' xmlns='' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>");
            sb.AppendFormat("    <xs:element name='XmlMaxIds' msdata:IsDataSet='true' msdata:UseCurrentLocale='true'>");
            sb.Append("      <xs:complexType>");
            sb.Append("        <xs:choice minOccurs='0' maxOccurs='unbounded'>");
            sb.Append("          <xs:element name='Table'>");
            sb.Append("            <xs:complexType>");
            sb.Append("              <xs:sequence>");

            sb.AppendFormat("           <xs:element name='TableName' type='xs:string' minOccurs='0' />");
            sb.AppendFormat("           <xs:element name='MaxId' type='xs:string' minOccurs='0' />");

            sb.Append("              </xs:sequence>");
            sb.Append("            </xs:complexType>");
            sb.Append("          </xs:element>");
            sb.Append("        </xs:choice>");
            sb.Append("      </xs:complexType>");
            sb.Append("    </xs:element>");
            sb.Append("  </xs:schema>");
            sb.AppendFormat("</XmlMaxIds>");

            CreateFile(xmlPath, sb.ToString());
        }

        /// <summary>
        /// Xmlin yolunu kontrol eder eğer xml yok ise oluşturur
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        private static void CheckOrCreateXml<T>(T item) where T : class
        {
            string xmlPath = GetXmlPath<T>(typeof(T).Name);
            var fi = new FileInfo(xmlPath);
            if (!fi.Exists)
            {
                CreateXmlFile(xmlPath, item.GetType());
            }
        }

        private static void CreateXmlFile(string xmlPath, Type itemType)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version='1.0' standalone='yes' ?>"); // Note: Xml oluştuştururken ilk karakter boşluk olamaz
            sb.AppendFormat("<{0}>", itemType.Name);
            sb.AppendFormat("        <xs:schema id='{0}' xmlns='' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>", itemType.Name);
            sb.AppendFormat("    <xs:element name='{0}' msdata:IsDataSet='true' msdata:UseCurrentLocale='true'>", itemType.Name);
            sb.Append("      <xs:complexType>");
            sb.Append("        <xs:choice minOccurs='0' maxOccurs='unbounded'>");
            sb.Append("          <xs:element name='Table'>");
            sb.Append("            <xs:complexType>");
            sb.Append("              <xs:sequence>");

            //tüm propertyleri dolanıp elementlerini oluşturuyoruz
            foreach (PropertyInfo property in itemType.GetProperties())
            {
                sb.AppendFormat("<xs:element name='{0}' type='xs:string' minOccurs='0' />", property.Name);
            }

            sb.Append("              </xs:sequence>");
            sb.Append("            </xs:complexType>");
            sb.Append("          </xs:element>");
            sb.Append("        </xs:choice>");
            sb.Append("      </xs:complexType>");
            sb.Append("    </xs:element>");
            sb.Append("  </xs:schema>");
            sb.AppendFormat("</{0}>", itemType.Name);

            CreateFile(xmlPath, sb.ToString());
        }

        private static void CreateFile(string xmlPath, string xml)
        {
            if (!File.Exists(xmlPath))
            {
                var di = new DirectoryInfo(ConfigurationManager.AppSettings[XmlPathConfigKey]);
                if (!di.Exists)
                {
                    throw new Exception("Klasör Bulunamadı");
                }

                // Dosyayı oluşturma işlemi yapılıyor
                using (FileStream fs = File.Create(xmlPath))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(xml);
                    // xml' e bilgiler yazılıyor
                    fs.Write(info, 0, info.Length);
                }
            }
        }

        public static bool Update<T>(T item, string primaryColumnName = "Id") where T : class
        {
            var ds = GetAllToDataSet<T>();
            if (DataTableExists(ds))
            {
                var dv = ds.Tables[0].DefaultView;
                Type itemType = typeof(T);

                long id;
                long.TryParse(itemType.GetProperty(primaryColumnName).GetValue(item, null).ToString(), out id);

                if (itemType != typeof(XmlMaxIds) && id > 0)
                {
                    dv.RowFilter = string.Format("{0}='{1}'", primaryColumnName, id);
                }
                else if (itemType == typeof(XmlMaxIds))
                {
                    var xmlItem = item as XmlMaxIds;
                    if (xmlItem != null)
                    {
                        dv.RowFilter = string.Format("{0}='{1}'", primaryColumnName, xmlItem.TableName);
                    }
                }

                dv.Sort = primaryColumnName;
                if (dv.Count > 0)
                {
                    DataRow dr = dv[0].Row;

                    //tüm propertyleri dolanıp değerleri setliyoruz
                    foreach (PropertyInfo property in itemType.GetProperties())
                    {
                        dr[property.Name] = property.GetValue(item, null);
                    }

                    Save<T>(ds);
                }
                dv.RowFilter = "";
                dv.Dispose();
                ds.Dispose();
                return true;
            }
            return false;
        }

        public static bool Delete<T>(long id, string primaryColumnName = "Id") where T : class
        {
            var ds = GetAllToDataSet<T>();
            if (DataTableExists(ds))
            {
                var dv = ds.Tables[0].DefaultView;
                dv.RowFilter = string.Format("{0}='{1}'", primaryColumnName, id);
                if (dv.Count > 0)
                {
                    dv.Sort = primaryColumnName;
                    dv.Delete(0);
                    dv.RowFilter = "";
                    Save<T>(ds);
                }
                dv.Dispose();
                ds.Dispose();
                return true;
            }
            return false;
        }

        private static void Save<T>(DataSet ds) where T : class
        {
            var xmlName = GetXmlPath<T>(typeof(T).Name);
            //var fs = new FileStream(xmlName, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            //var writer = new StreamWriter(fs, Encoding.UTF8);
            ds.WriteXml(xmlName, XmlWriteMode.WriteSchema);
        }
    }
}
