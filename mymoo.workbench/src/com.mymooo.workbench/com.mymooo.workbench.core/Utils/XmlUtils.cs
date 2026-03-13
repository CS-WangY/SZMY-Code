using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace com.mymooo.workbench.core.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class XmlUtils
	{
		/// <summary>
		/// 将一个对象序列化为XML字符串
		/// </summary>
		/// <param name="o">要序列化的对象</param>
		/// <param name="encoding">编码方式</param>
		/// <returns>序列化产生的XML字符串</returns>
		public static string XmlSerialize(object o, Encoding encoding)
		{
			ArgumentNullException.ThrowIfNull(o);

			using MemoryStream stream = new();
			XmlSerializer serializer = new(o.GetType());

			XmlWriterSettings settings = new()
			{
				Indent = true,
				NewLineChars = "\r\n",
				Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding)),
				IndentChars = "  ",
				OmitXmlDeclaration = true
			};

			using (XmlWriter writer = XmlWriter.Create(stream, settings))
			{
				//Create our own namespaces for the output
				XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
				//Add an empty namespace and empty value
				ns.Add("", "");
				serializer.Serialize(writer, o, ns);
				writer.Close();
			}

			stream.Position = 0;
			using StreamReader reader = new(stream, encoding);
			return reader.ReadToEnd();
		}

		/// <summary>
		/// 从XML字符串中反序列化对象
		/// </summary>
		/// <typeparam name="T">结果对象类型</typeparam>
		/// <param name="s">包含对象的XML字符串</param>
		/// <param name="encoding">编码方式</param>
		/// <returns>反序列化得到的对象</returns>
		public static T XmlDeserialize<T>(string s, Encoding encoding)
		{
			if (string.IsNullOrEmpty(s))
			{
				throw new ArgumentNullException(nameof(s));
			}

			ArgumentNullException.ThrowIfNull(encoding);

			XmlSerializer mySerializer = new(typeof(T));
			using MemoryStream ms = new(encoding.GetBytes(s));
			using StreamReader sr = new(ms, encoding);
			return (T)mySerializer.Deserialize(sr);
		}

		/// <summary>
		/// 将一个对象按XML序列化的方式写入到一个文件
		/// </summary>
		/// <param name="o">要序列化的对象</param>
		/// <param name="path">保存文件路径</param>
		/// <param name="encoding">编码方式</param>
		public static void XmlSerializeToFile(object o, string path, Encoding encoding)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException(nameof(path));
			}

			ArgumentNullException.ThrowIfNull(o);

			using FileStream file = new(path, FileMode.Create, FileAccess.Write);
			XmlSerializer serializer = new(o.GetType());

			XmlWriterSettings settings = new()
			{
				Indent = true,
				NewLineChars = "\r\n",
				Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding)),
				IndentChars = "    "
			};

			using XmlWriter writer = XmlWriter.Create(file, settings);
			serializer.Serialize(writer, o);
			writer.Close();
		}

		/// <summary>
		/// 读入一个文件，并按XML的方式反序列化对象。
		/// </summary>
		/// <typeparam name="T">结果对象类型</typeparam>
		/// <param name="path">文件路径</param>
		/// <param name="encoding">编码方式</param>
		/// <returns>反序列化得到的对象</returns>
		public static T XmlDeserializeFromFile<T>(string path, Encoding encoding)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException(nameof(path));
			}
			if (encoding == null)
			{
				throw new ArgumentNullException(nameof(encoding));
			}

			string xml = File.ReadAllText(path, encoding);
			return XmlDeserialize<T>(xml, encoding);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="element"></param>
		/// <returns></returns>
		public static dynamic GetAnonymousType(string xml, XElement element = null)
		{
			// either set the element directly or parse XML from the xml parameter.
			element = string.IsNullOrEmpty(xml) ? element : XDocument.Parse(xml).Root;

			// if there's no element than there's no point to continue
			if (element == null) return null;

			IDictionary<string, dynamic> result = new ExpandoObject();

			// grab any attributes and add as properties
			element.Attributes().AsParallel().ForAll
				 (attribute => result[attribute.Name.LocalName] = attribute.Value);

			// check if there are any child elements.
			if (!element.HasElements)
			{
				// check if the current element has some value and add it as a property
				if (!string.IsNullOrWhiteSpace(element.Value))
					result[element.Name.LocalName] = element.Value;
				return result;
			}

			// Check if the child elements are part of a collection (array). If they are not then
			// they are either a property of complex type or a property with simple type
			var isCollection = (element.Elements().Count() > 1
									&& element.Elements().All(e => e.Name.LocalName.ToLower()
									   == element.Elements().First().Name.LocalName.ToLower())

									// the pluralizer is needed in a scenario where you have 
									// 1 child item and you still want to treat it as an array.
									// If this is not important than you can remove the last part 
									// of the if clause which should speed up this method considerably.
									|| element.Name.LocalName.ToLower() ==
									   new Pluralize.NET.Core.Pluralizer().Pluralize
									   (element.Elements().First().Name.LocalName).ToLower());

			var values = new ConcurrentBag<dynamic>();

			// check each child element
			element.Elements().ToList().AsParallel().ForAll(i =>
			{
				// if it's part of a collection then add the collection items to a temp variable 
				if (isCollection) values.Add(GetAnonymousType(null, i));
				else
					// if it's not a collection, but it has child elements
					// then it's either a complex property or a simple property
					if (i.HasElements)
					// create a property with the current child elements name 
					// and process its properties
					result[i.Name.LocalName] = GetAnonymousType(null, i);
				else
					// create a property and just add the value
					result[i.Name.LocalName] = i.Value;
			});

			// for collection items we want skip creating a property with the child item names, 
			// but directly add the child properties to the
			if (!values.IsEmpty)
			{
				result[element.Name.LocalName] = values;
			}

			// return the properties of the processed element
			return result;
		}
	}
}
