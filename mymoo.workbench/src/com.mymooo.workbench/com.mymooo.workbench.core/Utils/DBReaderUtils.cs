using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public static class DBReaderUtils
    {
        #region 扩展函数

        /// <summary>
        /// 转换从数据库读取出来的数据到指定的类型
        /// </summary>
        /// <typeparam name="T">希望转换的数据类型</typeparam>
        /// <param name="value">希望转换的原始数据。</param>
        /// <param name="convertFunc">允许指定一个转换器函数，如果未指定，系统将试图寻找默认的转换器。</param>
        /// <returns>转换后的值。</returns>
        public static T ConvertTo<T>(object value, Func<object, T> convertFunc)
        {
            return ConvertTo<T>(value, convertFunc, default(T));
        }

        /// <summary>
        /// 转换从数据库读取出来的数据到指定的类型
        /// </summary>
        /// <typeparam name="T">希望转换的数据类型</typeparam>
        /// <param name="value">希望转换的原始数据。</param>
        /// <param name="convertFunc">允许指定一个转换器函数，如果未指定，系统将试图寻找默认的转换器。</param>
        /// <param name="defaultValue">如果原始数据是DbNull或null时，应该返回的值。</param>
        /// <returns>转换后的值。</returns>
        public static T ConvertTo<T>(object value, Func<object, T> convertFunc, T defaultValue)
        {
            //DbNull 时返回默认值，例如：T = int32，那么返回0，如果T = int32?，那么返回null.
            if (Convert.IsDBNull(value) || value == null)
            {
                return defaultValue;
            }
            else
            {
                //如果类型已经正确，不转换
                if (value is T)
                {
                    return (T)value;
                }
                else
                {
                    //如果未提供转换器，尝试使用默认的转换器
                    if (convertFunc == null)
                    {
                        convertFunc = ConvertFuncHelper<T>.Default;//使用泛型静态的特点，不再使用字典。
                    }

                    if (convertFunc != null)
                    {
                        return convertFunc(value);
                    }
                    throw new ArgumentException(string.Format("数据{0}不能转换成{1}，请尝试提供转换器参数convertFunc或修正数据", value, typeof(T)));

                }
            }
        }

        /// <summary>
        /// 创建转换器字典
        /// </summary>
        /// <returns></returns>
        private static Dictionary<Type, object> CreateDefaultConvertFunc()
        {
            Dictionary<Type, object> funcs = new Dictionary<Type, object>();
            AddConvertFunc<bool>(funcs, ToBoolean);
            AddConvertFunc<byte>(funcs, Convert.ToByte);
            AddConvertFunc<char>(funcs, Convert.ToChar);
            AddConvertFunc<DateTime>(funcs, Convert.ToDateTime);
            AddConvertFunc<decimal>(funcs, Convert.ToDecimal);
            AddConvertFunc<double>(funcs, Convert.ToDouble);
            AddConvertFunc<Int16>(funcs, Convert.ToInt16);
            AddConvertFunc<Int32>(funcs, Convert.ToInt32);
            AddConvertFunc<Int64>(funcs, Convert.ToInt64);
            AddConvertFunc<SByte>(funcs, Convert.ToSByte);
            AddConvertFunc<Single>(funcs, Convert.ToSingle);
            AddConvertFunc<string>(funcs, Convert.ToString);
            AddConvertFunc<UInt16>(funcs, Convert.ToUInt16);
            AddConvertFunc<UInt32>(funcs, Convert.ToUInt32);
            AddConvertFunc<UInt64>(funcs, Convert.ToUInt64);

            AddConvertFunc<Guid>(funcs, ToGuid); //扩展

            return funcs;
        }

        /// <summary>
        /// 转换对象的值到Guid
        /// </summary>
        /// <param name="value">要转换的值，支持byte、string转换到guid</param>
        /// <returns>如果是DbNull将仍然返回DbNull，否则将尝试转换。</returns>
        private static Guid ToGuid(object value)
        {
            byte[] t1 = value as byte[];
            if (t1 != null)
            {
                return new Guid(t1);
            }
            else
            {
                string t2 = value as string;
                if (t2 != null)
                {
                    return new Guid(t2);
                }
            }
            throw new InvalidCastException(string.Format("无法将{0}转换为GUID类型", value));

        }

        /// <summary>
        /// 主要是处理 0,1 这种表示形式。
        /// </summary>
        /// <param name="value">需要处理对象</param>
        /// <returns>处理结果</returns>
        private static bool ToBoolean(object value)
        {
            if (value is string)
            {
                string str = (string)value;
                if ((str == string.Empty) || (str == "0"))
                {
                    return false;
                }
                else if ((str == "1") || (str == "-1"))
                {
                    return true;
                }
            }
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// 将指定类型转换器压入字典中
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="funcs">转换器</param>
        /// <param name="func">转换器集合</param>
        private static void AddConvertFunc<T>(Dictionary<Type, object> funcs, Func<object, T> func)
        {
            funcs.Add(typeof(T), func);
        }

        /// <summary>
        /// 获取指定类型的转换器
        /// </summary>
        /// <typeparam name="T">指定数据类型<</typeparam>
        /// <returns>转换器</returns>
        private static Func<object, T> GetDefaultConvertFunc<T>()
        {
            Dictionary<Type, object> dict = CreateDefaultConvertFunc();
            object temp;
            if (dict.TryGetValue(typeof(T), out temp))
            {
                return (Func<object, T>)temp;
            }
            return null;
        }

        private static class ConvertFuncHelper<T>
        {
            static ConvertFuncHelper()
            {
                //虽然每次构建字典有些浪费，但是一旦程序执行过一次，性能损失就忽略不计了。
                Default = GetDefaultConvertFunc<T>();
            }

            public static readonly Func<object, T> Default;
        }

        /// <summary>
        /// IDataReader的扩展读取方法，可以指定数据类型，如果读取的字段是DbNull，将返回此类型的缺省值
        /// </summary>
        /// <typeparam name="T">此字段的数据类型</typeparam>
        /// <param name="dr">读取器</param>
        /// <param name="fieldName">字段的名称</param>
        /// <returns>最后的结果，如果读取的字段是DbNull，将返回此类型的缺省值</returns>
        public static T GetValue<T>(this IDataRecord dr, string fieldName)
        {
            return ConvertTo<T>(dr[fieldName], null);
        }

        /// <summary>
        /// IDataReader的扩展读取方法，可以指定数据类型，如果读取的字段是DbNull，将返回此类型的缺省值
        /// </summary>
        /// <typeparam name="T">此字段的数据类型</typeparam>
        /// <param name="dr">读取器</param>
        /// <param name="fieldName">字段的名称</param>
        /// <param name="convertFunc">数据的强制转换函数</param>
        /// <returns>最后的结果，如果读取的字段是DbNull，将返回此类型的缺省值</returns>
        public static T GetValue<T>(this IDataRecord dr, string fieldName, Func<object, T> convertFunc)
        {
            return ConvertTo<T>(dr[fieldName], convertFunc);
        }

        /// <summary>
        /// IDataReader的扩展读取方法，可以指定数据类型，如果读取的字段是DbNull，将返回此类型的缺省值
        /// </summary>
        /// <typeparam name="T">此字段的数据类型</typeparam>
        /// <param name="dr">读取器</param>
        /// <param name="index">字段的所在的索引</param>
        /// <returns>最后的结果，如果读取的字段是DbNull，将返回此类型的缺省值</returns>
        public static T GetValue<T>(this IDataRecord dr, int index)
        {
            return ConvertTo<T>(dr.GetValue(index), null);
        }

        /// <summary>
        /// IDataReader的扩展读取方法，可以指定数据类型，如果读取的字段是DbNull，将返回此类型的缺省值
        /// </summary>
        /// <typeparam name="T">此字段的数据类型</typeparam>
        /// <param name="dr">读取器</param>
        /// <param name="index">字段的所在的索引</param>
        /// <param name="convertFunc">数据的强制转换函数</param>
        /// <returns>最后的结果，如果读取的字段是DbNull，将返回此类型的缺省值</returns>
        public static T GetValue<T>(this IDataRecord dr, int index, Func<object, T> convertFunc)
        {
            return ConvertTo<T>(dr.GetValue(index), convertFunc);
        }

        #endregion
    }
}
