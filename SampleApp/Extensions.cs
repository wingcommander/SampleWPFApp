namespace SampleApp.Extensions
{
    using System;    
    using System.ComponentModel;
    using System.Data;
    using System.Data.SqlClient;

    public static class TypeExtensions
    {
       
        public static SqlDbType GetSqlDbType(this Type theType)
        {
            SqlParameter parameter = new SqlParameter();
            TypeConverter converter = TypeDescriptor.GetConverter(parameter.DbType);

            try
            {
                Object convertFrom = converter.ConvertFrom(theType.Name);
                if (convertFrom != null)
                {
                    parameter.DbType = (DbType)convertFrom;
                }
            }
            catch
            {
               
            }

            return parameter.SqlDbType;
        }
    }
}