using System.IO;
using System.Xml;

namespace AMS.Profile
{
    public class Xml : XmlBased
    {
        private string m_rootName = "profile";

        public Xml()
        {
        }

        public Xml(string fileName)
          : base(fileName)
        {
        }

        public Xml(Xml xml)
          : base(xml)
          => m_rootName = xml.m_rootName;

        public override string DefaultName => DefaultNameWithoutExtension + ".xml";

        public override object Clone() => new Xml(this);

        private string GetSectionsPath(string section) => "section[@name=\"" + section + "\"]";

        private string GetEntryPath(string entry) => "entry[@name=\"" + entry + "\"]";

        public string RootName
        {
            get => m_rootName;
            set
            {
                VerifyNotReadOnly();
                if (m_rootName == value.Trim() || !RaiseChangeEvent(true, ProfileChangeType.Other, null, nameof(RootName), (object)value))
                    return;
                m_rootName = value.Trim();
                RaiseChangeEvent(false, ProfileChangeType.Other, null, nameof(RootName), (object)value);
            }
        }

        public override void SetValue(string section, string entry, object value)
        {
            if (value == null)
            {
                RemoveEntry(section, entry);
            }
            else
            {
                VerifyNotReadOnly();
                VerifyName();
                VerifyAndAdjustSection(ref section);
                VerifyAndAdjustEntry(ref entry);
                if (!RaiseChangeEvent(true, ProfileChangeType.SetValue, section, entry, value))
                    return;
                string text = value.ToString();
                if ((m_buffer == null || m_buffer.IsEmpty) && !File.Exists(Name))
                {
                    XmlTextWriter writer = m_buffer != null ? new XmlTextWriter(new MemoryStream(), Encoding) : new XmlTextWriter(Name, Encoding);
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteStartElement(m_rootName);
                    writer.WriteStartElement(nameof(section));
                    writer.WriteAttributeString("name", null, section);
                    writer.WriteStartElement(nameof(entry));
                    writer.WriteAttributeString("name", null, entry);
                    writer.WriteString(text);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    if (m_buffer != null)
                        m_buffer.Load(writer);
                    writer.Close();
                    RaiseChangeEvent(false, ProfileChangeType.SetValue, section, entry, value);
                }
                else
                {
                    XmlDocument xmlDocument = GetXmlDocument();
                    XmlElement documentElement = xmlDocument.DocumentElement;
                    XmlNode xmlNode1 = documentElement.SelectSingleNode(GetSectionsPath(section));
                    if (xmlNode1 == null)
                    {
                        XmlElement element = xmlDocument.CreateElement(nameof(section));
                        XmlAttribute attribute = xmlDocument.CreateAttribute("name");
                        attribute.Value = section;
                        element.Attributes.Append(attribute);
                        xmlNode1 = documentElement.AppendChild(element);
                    }
                    XmlNode xmlNode2 = xmlNode1.SelectSingleNode(GetEntryPath(entry));
                    if (xmlNode2 == null)
                    {
                        XmlElement element = xmlDocument.CreateElement(nameof(entry));
                        XmlAttribute attribute = xmlDocument.CreateAttribute("name");
                        attribute.Value = entry;
                        element.Attributes.Append(attribute);
                        xmlNode2 = xmlNode1.AppendChild(element);
                    }
                    xmlNode2.InnerText = text;
                    Save(xmlDocument);
                    RaiseChangeEvent(false, ProfileChangeType.SetValue, section, entry, value);
                }
            }
        }

        public override object GetValue(string section, string entry)
        {
            VerifyAndAdjustSection(ref section);
            VerifyAndAdjustEntry(ref entry);
            try
            {
                return GetXmlDocument().DocumentElement.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry)).InnerText;
            }
            catch
            {
                return null;
            }
        }

        public override void RemoveEntry(string section, string entry)
        {
            VerifyNotReadOnly();
            VerifyAndAdjustSection(ref section);
            VerifyAndAdjustEntry(ref entry);
            XmlDocument xmlDocument = GetXmlDocument();
            if (xmlDocument == null)
                return;
            XmlNode oldChild = xmlDocument.DocumentElement.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry));
            if (oldChild == null || !RaiseChangeEvent(true, ProfileChangeType.RemoveEntry, section, entry, null))
                return;
            oldChild.ParentNode.RemoveChild(oldChild);
            Save(xmlDocument);
            RaiseChangeEvent(false, ProfileChangeType.RemoveEntry, section, entry, null);
        }

        public override void RemoveSection(string section)
        {
            VerifyNotReadOnly();
            VerifyAndAdjustSection(ref section);
            XmlDocument xmlDocument = GetXmlDocument();
            if (xmlDocument == null)
                return;
            XmlElement documentElement = xmlDocument.DocumentElement;
            if (documentElement == null)
                return;
            XmlNode oldChild = documentElement.SelectSingleNode(GetSectionsPath(section));
            if (oldChild == null || !RaiseChangeEvent(true, ProfileChangeType.RemoveSection, section, null, null))
                return;
            documentElement.RemoveChild(oldChild);
            Save(xmlDocument);
            RaiseChangeEvent(false, ProfileChangeType.RemoveSection, section, null, null);
        }

        public override string[] GetEntryNames(string section)
        {
            if (!HasSection(section))
                return null;
            VerifyAndAdjustSection(ref section);
            XmlNodeList xmlNodeList = GetXmlDocument().DocumentElement.SelectNodes(GetSectionsPath(section) + "/entry[@name]");
            if (xmlNodeList == null)
                return null;
            string[] strArray = new string[xmlNodeList.Count];
            int num = 0;
            foreach (XmlNode xmlNode in xmlNodeList)
                strArray[num++] = xmlNode.Attributes["name"].Value;
            return strArray;
        }

        public override string[] GetSectionNames()
        {
            XmlDocument xmlDocument = GetXmlDocument();
            if (xmlDocument == null)
                return null;
            XmlElement documentElement = xmlDocument.DocumentElement;
            if (documentElement == null)
                return null;
            XmlNodeList xmlNodeList = documentElement.SelectNodes("section[@name]");
            if (xmlNodeList == null)
                return null;
            string[] strArray = new string[xmlNodeList.Count];
            int num = 0;
            foreach (XmlNode xmlNode in xmlNodeList)
                strArray[num++] = xmlNode.Attributes["name"].Value;
            return strArray;
        }
    }
}
