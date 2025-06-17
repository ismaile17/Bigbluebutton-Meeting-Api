using System.IO;
using System.Xml.Serialization;

namespace BigBlueButtonAPI.Common
{
    public class XmlHelper
    {
        public static T FromXml<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }
}