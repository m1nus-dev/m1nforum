using M1nforum.Web.Services.Entities;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace M1nforum.Web.Services.Repositories
{
    public class XmlRepository<T> : Repository<T> where T : IEntity
    {
        private readonly string _path;

        public XmlRepository()
            : this("./App_Data/{0}.xml")
        {
        }

        public XmlRepository(string path)
        {
            _path = path;
        }

        protected override List<T> Load()
        {
            var fileName = GetFileName();

            if (File.Exists(fileName))
            {
                var xmlSerializer = new XmlSerializer(typeof(List<T>));
                using (var streamReader = new StreamReader(fileName))
                {
                    return (List<T>)xmlSerializer.Deserialize(streamReader);
                }
            }
            else
            {
                return new List<T>();
            }
        }

        protected override void Save(List<T> entities)
        {
            if (entities != null)
            {
                var fileName = GetFileName();

                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                var xmlSerializer = new XmlSerializer(typeof(List<T>));
                using (var streamWriter = new StreamWriter(fileName))
                {
                    xmlSerializer.Serialize(streamWriter, entities);
                }
            }
        }

        private string GetFileName()
        {
            var fileName = string.Format(_path, typeof(T).FullName);

            if (fileName.StartsWith("~"))
            {
                // dev - 2011-12-16 - Changed to use assembly location.
                // fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName.Replace("~", ""));
            }

            return fileName;
        }
    }
}
