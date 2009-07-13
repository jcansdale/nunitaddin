using System;
using System.Collections;
using System.Xml;

namespace NUnit.AddInRunner
{
    public class NUnitConfig
    {
        Info[] infos;

        public NUnitConfig(Info[] libs)
        {
            this.infos = libs;
        }

        public static NUnitConfig Load(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            ArrayList infoList = new ArrayList();
            XmlNodeList infoElements = doc.SelectNodes("/nunit/info");
            foreach (XmlElement infoElement in infoElements)
            {
                XmlAttribute runtimeVersionAttribute = infoElement.Attributes["runtimeVersion"];
                if (runtimeVersionAttribute == null)
                {
                    throw new Exception("Couldn't find 'runtimeVersion' on 'info' element.");
                }
                string runtimeVersion = runtimeVersionAttribute.Value;

                XmlAttribute baseDirAttribute = infoElement.Attributes["baseDir"];
                if (baseDirAttribute == null)
                {
                    throw new Exception("Couldn't find 'baseDir' on 'info' element.");
                }
                string baseDir = baseDirAttribute.Value;

                Version productVersion = null;
                XmlAttribute productVersionAttribute = infoElement.Attributes["productVersion"];
                if (productVersionAttribute != null)
                {
                    productVersion = new Version(productVersionAttribute.Value);
                }

                Info info = new Info(runtimeVersion, baseDir, productVersion);
                infoList.Add(info);
            }

            Info[] libs = (Info[])infoList.ToArray(typeof (Info));
            return new NUnitConfig(libs);
        }

        public Info[] Infos
        {
            get { return infos; }
        }

        public class Info
        {
            readonly string runtimeVersion;
            readonly string baseDir;
            readonly Version productVersion;

            public Info(string runtimeVersion, string baseDir, Version productVersion)
            {
                this.runtimeVersion = runtimeVersion;
                this.baseDir = baseDir;
                this.productVersion = productVersion;
            }

            public string RuntimeVersion
            {
                get
                {
                    return runtimeVersion;
                }
            }

            public string BaseDir
            {
                get
                {
                    return baseDir;
                }
            }

            public Version ProductVersion
            {
                get
                {
                    return productVersion;
                }
            }
        }
    }
}
