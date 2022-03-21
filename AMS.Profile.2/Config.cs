using System.IO;
using System.Xml;

namespace AMS.Profile
{
    public class Config : XmlBased
    {
        private string m_groupName = "profile";
        private const string SECTION_TYPE = "System.Configuration.NameValueSectionHandler, System, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, Custom=null";

        public Config()
        {
        }

        public Config(string fileName)
          : base(fileName)
        {
        }

        public Config(Config config)
          : base(config)
          => m_groupName = config.m_groupName;

        public override string DefaultName => DefaultNameWithoutExtension + ".config";

        public override object Clone() => new Config(this);

        public string GroupName
        {
            get => m_groupName;
            set
            {
                VerifyNotReadOnly();
                if (m_groupName == value || !RaiseChangeEvent(true, ProfileChangeType.Other, null, nameof(GroupName), value))
                    return;
                m_groupName = value;
                if (m_groupName != null)
                {
                    m_groupName = m_groupName.Replace(' ', '_');
                    if (m_groupName.IndexOf(':') >= 0)
                        throw new XmlException("GroupName may not contain a namespace prefix.");
                }
                RaiseChangeEvent(false, ProfileChangeType.Other, null, nameof(GroupName), value);
            }
        }

        private bool HasGroupName => m_groupName != null && m_groupName != "";

        private string GroupNameSlash => HasGroupName ? m_groupName + "/" : "";

        private bool IsAppSettings(string section) => !HasGroupName && section != null && section == "appSettings";

        protected override void VerifyAndAdjustSection(ref string section)
        {
            base.VerifyAndAdjustSection(ref section);
            if (section.IndexOf(' ') < 0)
                return;
            section = section.Replace(' ', '_');
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
                bool hasGroupName = HasGroupName;
                bool flag = IsAppSettings(section);
                if ((m_buffer == null || m_buffer.IsEmpty) && !File.Exists(Name))
                {
                    XmlTextWriter writer = m_buffer != null ? new XmlTextWriter(new MemoryStream(), Encoding) : new XmlTextWriter(Name, Encoding);
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteStartElement("configuration");
                    if (!flag)
                    {
                        writer.WriteStartElement("configSections");
                        if (hasGroupName)
                        {
                            writer.WriteStartElement("sectionGroup");
                            writer.WriteAttributeString("name", null, m_groupName);
                        }
                        writer.WriteStartElement(nameof(section));
                        writer.WriteAttributeString("name", null, section);
                        writer.WriteAttributeString("type", null, "System.Configuration.NameValueSectionHandler, System, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, Custom=null");
                        writer.WriteEndElement();
                        if (hasGroupName)
                            writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    if (hasGroupName)
                        writer.WriteStartElement(m_groupName);
                    writer.WriteStartElement(section);
                    writer.WriteStartElement("add");
                    writer.WriteAttributeString("key", null, entry);
                    writer.WriteAttributeString(nameof(value), null, value.ToString());
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    if (hasGroupName)
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
                    if (!flag)
                    {
                        XmlNode xmlNode1 = documentElement.SelectSingleNode("configSections") ?? documentElement.AppendChild(xmlDocument.CreateElement("configSections"));
                        XmlNode xmlNode2 = xmlNode1;
                        if (hasGroupName)
                        {
                            xmlNode2 = xmlNode1.SelectSingleNode("sectionGroup[@name=\"" + m_groupName + "\"]");
                            if (xmlNode2 == null)
                            {
                                XmlElement element = xmlDocument.CreateElement("sectionGroup");
                                XmlAttribute attribute = xmlDocument.CreateAttribute("name");
                                attribute.Value = m_groupName;
                                element.Attributes.Append(attribute);
                                xmlNode2 = xmlNode1.AppendChild(element);
                            }
                        }
                        XmlNode xmlNode3 = xmlNode2.SelectSingleNode("section[@name=\"" + section + "\"]");
                        if (xmlNode3 == null)
                        {
                            XmlElement element = xmlDocument.CreateElement(nameof(section));
                            XmlAttribute attribute = xmlDocument.CreateAttribute("name");
                            attribute.Value = section;
                            element.Attributes.Append(attribute);
                            xmlNode3 = xmlNode2.AppendChild(element);
                        }
                        XmlAttribute attribute1 = xmlDocument.CreateAttribute("type");
                        attribute1.Value = "System.Configuration.NameValueSectionHandler, System, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, Custom=null";
                        xmlNode3.Attributes.Append(attribute1);
                    }
                    XmlNode xmlNode4 = documentElement;
                    if (hasGroupName)
                        xmlNode4 = documentElement.SelectSingleNode(m_groupName) ?? documentElement.AppendChild(xmlDocument.CreateElement(m_groupName));
                    XmlNode xmlNode5 = xmlNode4.SelectSingleNode(section) ?? xmlNode4.AppendChild(xmlDocument.CreateElement(section));
                    XmlNode xmlNode6 = xmlNode5.SelectSingleNode("add[@key=\"" + entry + "\"]");
                    if (xmlNode6 == null)
                    {
                        XmlElement element = xmlDocument.CreateElement("add");
                        XmlAttribute attribute = xmlDocument.CreateAttribute("key");
                        attribute.Value = entry;
                        element.Attributes.Append(attribute);
                        xmlNode6 = xmlNode5.AppendChild(element);
                    }
                    XmlAttribute attribute2 = xmlDocument.CreateAttribute(nameof(value));
                    attribute2.Value = value.ToString();
                    xmlNode6.Attributes.Append(attribute2);
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
                return GetXmlDocument().DocumentElement.SelectSingleNode(GroupNameSlash + section + "/add[@key=\"" + entry + "\"]").Attributes["value"].Value;
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
            XmlNode oldChild = xmlDocument.DocumentElement.SelectSingleNode(GroupNameSlash + section + "/add[@key=\"" + entry + "\"]");
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
            XmlNode oldChild1 = documentElement.SelectSingleNode(GroupNameSlash + section);
            if (oldChild1 == null || !RaiseChangeEvent(true, ProfileChangeType.RemoveSection, section, null, null))
                return;
            oldChild1.ParentNode.RemoveChild(oldChild1);
            if (!IsAppSettings(section))
            {
                XmlNode oldChild2 = documentElement.SelectSingleNode("configSections/" + (HasGroupName ? "sectionGroup[@name=\"" + m_groupName + "\"]" : "") + "/section[@name=\"" + section + "\"]");
                oldChild2?.ParentNode.RemoveChild(oldChild2);
            }
            Save(xmlDocument);
            RaiseChangeEvent(false, ProfileChangeType.RemoveSection, section, null, null);
        }

        public override string[] GetEntryNames(string section)
        {
            if (!HasSection(section))
                return null;
            VerifyAndAdjustSection(ref section);
            XmlNodeList xmlNodeList = GetXmlDocument().DocumentElement.SelectNodes(GroupNameSlash + section + "/add[@key]");
            if (xmlNodeList == null)
                return null;
            string[] strArray = new string[xmlNodeList.Count];
            int num = 0;
            foreach (XmlNode xmlNode in xmlNodeList)
                strArray[num++] = xmlNode.Attributes["key"].Value;
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
            XmlNode xmlNode1 = HasGroupName ? documentElement.SelectSingleNode(m_groupName) : documentElement;
            if (xmlNode1 == null)
                return null;
            XmlNodeList childNodes = xmlNode1.ChildNodes;
            if (childNodes == null)
                return null;
            string[] strArray = new string[childNodes.Count];
            int num = 0;
            foreach (XmlNode xmlNode2 in childNodes)
                strArray[num++] = xmlNode2.Name;
            return strArray;
        }
    }
}
