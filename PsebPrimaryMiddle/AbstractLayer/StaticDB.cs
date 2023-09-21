using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Data;
using System.ComponentModel;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Text;
using System.Net;
using System.IO;
using System.Net.NetworkInformation;
using ClosedXML.Excel;
using System.Globalization;

namespace PsebJunior.AbstractLayer
{
    public static class StaticDB
    {
        public static bool DateInFormat(string dateValue)
        {
            DateTime dt;
            string[] formats = { "dd/MM/yyyy" };
            if (!DateTime.TryParseExact(dateValue, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out dt))
            {
                //your condition fail code goes here
                return false;
            }
            else
            {
                //success code
                return true;
            }

        }
        public static string GenerateFileName(string context)
        {
            //return context + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N");
            return context + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }


        public static string GetMACAddress(out string ipAdd)
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String sMacAddress = string.Empty;
            ipAdd = "";
            foreach (NetworkInterface adapter in nics)
            {
                if (sMacAddress == String.Empty)// only return MAC Address from first card  
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();  
                    IPAddress ipa = properties.UnicastAddresses[1].Address;
                    ipAdd = ipa.ToString();
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            }
            
            return sMacAddress;
        }

        public static string GetFullIPAddress()
        {

            string OutputFullIPAddress;

            string hostName = System.Net.Dns.GetHostName(); // Retrive the Name of HOST
            string localIP = System.Net.Dns.GetHostByName(hostName).AddressList[0].ToString();

            string externalIP;
            try
            {
                externalIP = (new System.Net.WebClient()).DownloadString("https://checkip.dyndns.org/");
                externalIP = externalIP.Split(':')[1].Replace("</body></html>", "");
            }
            catch (Exception ex)
            {
                externalIP = "ExIP: " + ex.Message;
            }

            string RemoteHOST;
            try
            {
               RemoteHOST = Dns.GetHostEntry(HttpContext.Current.Request.ServerVariables["REMOTE_HOST"]).HostName;
            }
            catch (Exception ex)
            {
                try
                {                    
                    RemoteHOST = hostName;
                }
                catch (Exception)
                {
                    RemoteHOST = "HOST: " + ex.Message;
                }
               
            }

             // Remote IP Address (useful for getting user's IP from public site; run locally it just returns localhost)  
            string remoteIpAddress;
            try
            {
                
                remoteIpAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(remoteIpAddress))
                {
                    remoteIpAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
            }
            catch (Exception ex)
            {
                remoteIpAddress = "RmIP: " + ex.Message;
            }

          

            string ipAdd = "";
            string macAddress = GetMACAddress(out ipAdd);
            //OutputFullIPAddress = "HostNM:" + RemoteHOST + ",LocalIP:" + localIP + ",PublicIP:" + externalIP 
            //                   + ",RemoteIp:" + remoteIpAddress+",MAC:" + macAddress + ",IPv4:" + ipAdd ;

            OutputFullIPAddress = "HostNM:" + RemoteHOST + ",LocalIP:" + localIP + ",PublicIP:" + externalIP
                             + ",RemoteIp:" + remoteIpAddress + ",MAC:" + macAddress + ",IPv4:" + ipAdd;
            return OutputFullIPAddress;
        }


        public static string DataTableToNewtonsoftJSON(DataTable dt)
        {
            string JSONresult;
            JSONresult = JsonConvert.SerializeObject(dt);
            return JSONresult;
        }

        #region DataTableToJSON
        public static object DataTableToJSON(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = (Convert.ToString(row[col]));
                }
                list.Add(dict);
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            return serializer.Serialize(list);
        }
        #endregion DataTableToJSON

        public static List<T> DataTableToList<T>(this DataTable table) where T : new()
        {
            List<T> list = new List<T>();
            var typeProperties = typeof(T).GetProperties().Select(propertyInfo => new
            {
                PropertyInfo = propertyInfo,
                Type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType
            }).ToList();

            foreach (var row in table.Rows.Cast<DataRow>())
            {
                T obj = new T();
                foreach (var typeProperty in typeProperties)
                {
                    object value = row[typeProperty.PropertyInfo.Name];
                    object safeValue = value == null || DBNull.Value.Equals(value)
                        ? null
                        : Convert.ChangeType(value, typeProperty.Type);

                    typeProperty.PropertyInfo.SetValue(obj, safeValue, null);
                }
                list.Add(obj);
            }
            return list;
        }


        public static DataTable ConvertTo<T>(IList<T> Items)
        {
            return ConvertTo<T>(Items, false);
        }

        public static DataTable CreateTable<T>(bool IsStringOnlyDataType)
        {
            Type entityType = typeof(T);
            DataTable dataTable = new DataTable(entityType.Name);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);

            foreach (PropertyDescriptor property in properties)
            {
                Type dataColumnType = property.PropertyType;
                if (IsStringOnlyDataType)
                    dataColumnType = typeof(string);

                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    NullableConverter converter = new NullableConverter(property.PropertyType);
                    dataTable.Columns.Add(property.Name, converter.UnderlyingType);
                }
                else
                {
                    dataTable.Columns.Add(property.Name, dataColumnType);
                }
            }

            return dataTable;
        }

    
        public static DataTable ConvertTo<T>(IList<T> Items, bool IsStringOnlyDataType)
        {
            Type entityType = typeof(T);
            DataTable dataTable = CreateTable<T>(IsStringOnlyDataType);

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);

            #region iterate and add rows

            foreach (T item in Items)
            {
                DataRow newRow = dataTable.NewRow();

                foreach (PropertyDescriptor property in properties)
                {
                    newRow[property.Name] = property.GetValue(item);
                }

                dataTable.Rows.Add(newRow);
            }

            #endregion

            return dataTable;
        }

        public static DataTable ConvertListToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prp = props[i];
                table.Columns.Add(prp.Name, prp.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        public static string GetFirmName(string UserNM)
        {            
            switch (UserNM)
            {
                case "CREA": UserNM = "CIPL"; break;
                case "SAI": UserNM = "SAI"; break;
                case "DATA": UserNM = "DATA"; break;
                case "PERF": UserNM = "PERF"; break;
                default: UserNM = "ADMIN"; break;
            }
            return UserNM;
        }

        //    Get All Action and Controller
        //     Start********* Get all ActionName of all Controller by return type;
        public static string GetActionsOfController()
        {
            string actionname= "";
            var asm = Assembly.GetExecutingAssembly();
            var controllerTypes = from d in asm.GetExportedTypes() where typeof(System.Web.Mvc.IController).IsAssignableFrom(d) select d;
            foreach (var val in controllerTypes)
            {
                actionname = get_all_action(val);
            }
            return actionname;
        }   
       public static string get_all_action(Type controllerType)
        {
            string methods = "";
            MethodInfo[] mi = controllerType.GetMethods();
            foreach (MethodInfo m in mi)
            {
                if (m.IsPublic)
                    if (typeof(System.Web.Mvc.JsonResult).IsAssignableFrom(m.ReturnParameter.ParameterType))
                        methods = "'" + m.Name + "' ,'" + controllerType.Name + "'" + Environment.NewLine + methods;
            }
            return methods;
        }

        // 
        public  static int[] StringToIntArray(this string value, char separator)
        {           
            return Array.ConvertAll(value.Split(separator), s => int.Parse(s));
        }

        public static string[] StringToStringArray(this string value, char separator)
        {
            return Array.ConvertAll(value.Split(separator), s => Convert.ToString(s));
        }

        // Check value exists in comma string
        public static bool ContainsValue(this string str, string value)
        {
            return str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                //.Select(x => int.Parse(x.Trim()))
                .Select(x => x.Trim())
                .Contains(value);
        }

        public static string GetCurrentYear(string Year)
        {            
            return Year;
        }

        // Check Array Duplicate Value
        public static bool  CheckArrayDuplicates(string[] array)
        {
            if (array.Count() > 1)
            {
                array = array.ToList().Where(x => x != null).ToArray();
                var duplicates = array
                 .GroupBy(p => p)
                 .Where(g => g.Count() > 1 && g != null)
                 .Select(g => g.Key);

                int dc = duplicates.Count();// Count duplicate records
                return (duplicates.Count() > 0);
            }
            else
            {
                return false;
            }
            
        }

    }
}