using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
//using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

namespace PsebPrimaryMiddle.Helpers
{
    /// <summary>
    /// Collection List Helper methods
    /// </summary>
    public class CollectionHelper
    {
        /// <summary>
        /// Converts a list item default property to comma seperated values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Items"></param>
        /// <returns></returns>
        public static string ConvertToString<T>(IList<T> Items)
        {
            if (Items.Count == 0)
                return string.Empty;

            Type entityType = typeof(T);

            // get the collection of attribbutes of type descriptor
            System.ComponentModel.AttributeCollection attributes =
                TypeDescriptor.GetAttributes(entityType);

            // get the default attribute
            DefaultPropertyAttribute defaultProperty = (DefaultPropertyAttribute)attributes[typeof(DefaultPropertyAttribute)];

            return ConvertTo<T>(Items, defaultProperty.Name);
        }

        /// <summary>
        /// Converts a IList to List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iList"></param>
        /// <returns></returns>
        public static IList<T> ConvertToListOf<T>(IList Items)
        {
            IList<T> result = new List<T>();
            foreach (T value in Items)
                result.Add(value);

            return result;
        }

        /// <summary>
        /// Converts a list item property name value to comma seperated values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Items"></param>
        /// <param name="PropertyName"></param>
        /// <returns></returns>
        public static string ConvertTo<T>(IList<T> Items, string PropertyName)
        {
            StringBuilder builder = new StringBuilder();
            Type entityType = typeof(T);

            // iterate through the property
            foreach (T Item in Items)
            {
                if (string.IsNullOrEmpty(PropertyName))
                    builder.Append(Item.ToString()).Append(",");
                else
                {
                    try
                    {
                        // get the value of the property name passed
                        builder.Append(
                            entityType.GetProperty(PropertyName).GetValue(Item, null).ToString()
                        ).Append(",");
                    }
                    catch
                    {
                        // the user has entered a property name, that does not exists
                        // in this type or the propertyname contains a | for entity type property
                        // and the other index for the property type property
                        if (PropertyName.Contains("|"))
                        {
                            // the passed property value required is for the Property of a class type 
                            string[] PropertyNames = PropertyName.Split(new char[] { '|' });

                            // the property info
                            PropertyInfo destinationPropertyInfo = entityType.GetProperty(PropertyNames[0]);

                            if (destinationPropertyInfo != null)
                            {

                                // get the entity type of the property
                                Type subEntityType = destinationPropertyInfo.PropertyType;

                                // now get the value
                                builder.Append(
                                    subEntityType.GetProperty(PropertyNames[1]).GetValue(
                                    destinationPropertyInfo.GetValue(Item, null), null).ToString()
                                ).Append(",");
                            }
                        }
                    }
                }
            }

            return builder.ToString().TrimEnd(new char[] { ',' });
        }

        /// <summary>
        /// Converts a IList to a data table with single data Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Items"></param>
        /// <returns></returns>
        public static DataTable ConvertTo<T>(IList<T> Items, bool IsStringOnlyDataType)
        {
            Type entityType = typeof(T);
            DataTable dataTable = CreateTable<T>( IsStringOnlyDataType );

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

        /// <summary>
        /// Converts a IList to a data table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Items"></param>
        /// <returns></returns>
        public static DataTable ConvertTo<T>(IList<T> Items)
        {
            return ConvertTo<T>(Items, false);
        }

        /// <summary>
        /// Creates a datatable with the columns as the properties in the object class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// Converts a data table to a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IList<T> ConvertTo<T>(DataTable dataTable)
        {
            if (dataTable == null)
                return null;

            List<DataRow> Rows = new List<DataRow>();

            foreach (DataRow row in dataTable.Rows)
            {
                Rows.Add(row);
            }

            return ConvertTo<T>(Rows);
        }

        /// <summary>
        /// Converts a list of data row to List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Rows"></param>
        /// <returns></returns>
        public static IList<T> ConvertTo<T>(IList<DataRow> Rows)
        {
            IList<T> Items = null;

            if (Rows != null)
            {
                Items = new List<T>();

                foreach (DataRow row in Rows)
                {
                    T Item = CreateItem<T>(row);
                    Items.Add(Item);
                }
            }

            return Items;
        }

        /// <summary>
        /// Converts a array to list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static IList<T> ConvertTo<T>(T[] Data)
        {
            IList<T> Items = new List<T>();

            if (Data == null)
                return Items;

            if (Data.Length > 0)
            {
                foreach (T Item in Data)
                {
                    Items.Add(Item);
                }
            }

            return Items;
        }

        public static List<T> ConvertToList<T>(T[] Data)
        {
            List<T> Items = new List<T>();

            if (Data == null)
                return Items;

            if (Data.Length > 0)
            {
                foreach (T Item in Data)
                {
                    Items.Add(Item);
                }
            }

            return Items;
        }

        /// <summary>
        /// Create a object of the DataRow
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Row"></param>
        /// <returns></returns>
        public static T CreateItem<T>(DataRow Row)
        {
            T obj = default(T);
            if (Row != null)
            {
                obj = Activator.CreateInstance<T>();

                foreach (DataColumn column in Row.Table.Columns)
                {
                    PropertyInfo property = obj.GetType().GetProperty(column.ColumnName);
                    try
                    {
                        object value = Row[column.ColumnName];
                        property.SetValue(obj, value, null);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return obj;
        }

        /// <summary>
        /// Converts a webservice result data array to the required generic list output
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        //public static List<TOutput> ConvertTo<TInput, TOutput>(TInput[] source)
        //{
        //    TOutput[] Entities = Array.ConvertAll<TInput, TOutput>(source,
        //        new Converter<TInput, TOutput>(TypeFromProxy<TInput, TOutput>));

        //    return new List<TOutput>(Entities);
        //}

        //public static TOutput TypeFromProxy<TInput, TOutput>(TInput source)
        //{
        //    TOutput EntityType = (TOutput)MapObject(source, typeof(TOutput));
        //    return EntityType;
        //}

        //private static object MapObject(object source, Type destinationType)
        //{
        //    if (source == null)
        //        return null;

        //    PropertyInfo[] sourceProperties = source.GetType().GetProperties();
        //    object destination = Activator.CreateInstance(destinationType);

        //    //iterate
        //    foreach (PropertyInfo sourceProperty in sourceProperties)
        //    {
        //        bool processed = false;
        //        object value = sourceProperty.GetValue(source, null);
        //        if (value == null || !sourceProperty.CanWrite)
        //            continue;

        //        PropertyInfo destProperty = destinationType.GetProperty(sourceProperty.Name);

        //        // get the business relation
        //        object[] businessRelations = destProperty.GetCustomAttributes(typeof(BusinessRelationAttribute), false);
        //        if (businessRelations != null && businessRelations.Length > 0)
        //        {
        //            BusinessRelationAttribute businessRelation = (BusinessRelationAttribute)businessRelations[0];

        //            // Only collections are supported for business relations.
        //            IList sourceCollection = value as IList;
        //            // Compound business relation.
        //            if (sourceCollection != null)
        //            {
        //                IList destinationCollection = destProperty.GetValue(destination, null) as IList;
        //                if (destinationCollection != null)
        //                {
        //                    IEnumerator en = sourceCollection.GetEnumerator();
        //                    while (en.MoveNext())
        //                    {
        //                        object mappedChildValue = MapObject(en.Current, businessRelation.EntityType);
        //                        destinationCollection.Add(mappedChildValue);
        //                    }
        //                    processed = true;
        //                }
        //            }
        //            else // direct business realtion
        //            {
        //                destProperty.SetValue(destination, MapObject(value, destProperty.PropertyType), null);
        //                processed = true;
        //            }
        //        }

        //        else if (destProperty.PropertyType.IsArray && destProperty.PropertyType.FullName != "System.Byte[]")
        //        {
        //            #region Set Child Objects

        //            List<object> valueMapping = new List<object>();
        //            if (value is Array)
        //            {
        //                foreach (object arrayValue in ((Array)value))
        //                {
        //                    object mappedChildValue = MapObject(arrayValue, destProperty.PropertyType.GetElementType());
        //                    valueMapping.Add(mappedChildValue);
        //                }
        //            }
        //            else
        //            {
        //                IList sourceCollection = value as IList;
        //                if (sourceCollection != null)
        //                {
        //                    IEnumerator en = sourceCollection.GetEnumerator();
        //                    while (en.MoveNext())
        //                    {
        //                        object mappedChildValue = MapObject(en.Current, destProperty.PropertyType.GetElementType());
        //                        valueMapping.Add(mappedChildValue);
        //                    }
        //                }
        //            }

        //            Array valueMappingArray = Array.CreateInstance(destProperty.PropertyType.GetElementType(),
        //                    valueMapping.Count);
        //            for (int i = 0; i < valueMappingArray.Length; i++)
        //            {
        //                valueMappingArray.SetValue(valueMapping[i], i);
        //            }
        //            destProperty.SetValue(destination, valueMappingArray, null);
        //            processed = true;

        //            #endregion
        //        }
        //        else if (destProperty.PropertyType.IsEnum)
        //        {
        //            // set the enum value
        //            value = Enum.Parse(destProperty.PropertyType, value.ToString(), true);
        //        }

        //        // Use basic data mapping.
        //        if (!processed)
        //        {
        //            destProperty.SetValue(destination, value, null);
        //        }
        //    }

        //    return destination;
        //}

        public static Type GetListType(Type EntityType)
        {
            foreach (Type intType in EntityType.GetInterfaces())
            {
                if (intType.IsGenericType &&
                    intType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    return intType.GetGenericArguments()[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Converts an enum to a dictionary type
        /// </summary>
        /// <typeparam name="T">Enum</typeparam>
        /// <returns></returns>
        public static Dictionary<short, string> ConvertEnum<T>()
        {
            Dictionary<short, string> dataResult = new Dictionary<short, string>();

            // get the enumber record values
            Array enumValues = Enum.GetValues(typeof(T));

            // iterate and add to the dictionary
            foreach (T value in enumValues)
                dataResult.Add(Helper.GetShort((T)value),
                    Helper.ParseCamelToProper(Enum.GetName(typeof(T), value)).Trim());

            return dataResult;
        }
    }
}