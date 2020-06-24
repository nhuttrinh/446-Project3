using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Assignment5
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        string fLocation = Path.Combine(HttpRuntime.AppDomainAppPath, @"App_Data\XMLAssignment5.xml");
        XNamespace nameSpace = "http://example.com/Messages";
        public void sendMsg(string senderID, string receiverID, string msg)
        {
            
            XDocument xmlDocMsgs = new XDocument();

            try
            {
                xmlDocMsgs = XDocument.Load(fLocation);
                XElement xmlElement = new XElement(nameSpace + "Message",
                new XElement(nameSpace + "SenderID", senderID),
                new XElement(nameSpace + "ReceiverID", receiverID),
                new XElement(nameSpace + "TimeStamp", System.DateTime.Now.ToString()),
                new XElement(nameSpace + "MessageContents", msg),
                new XElement(nameSpace + "Read",false));
                xmlDocMsgs.Element(nameSpace + "Messages").Add(xmlElement);
                xmlDocMsgs.Save(fLocation);


            }
            catch (XmlException ex)
            {
                if ((!(ex.Message.ToLower().Contains("root") && ex.Message.ToLower().Contains("element") && ex.Message.ToLower().Contains("not") && ex.Message.ToLower().Contains("found"))))
                {
                    xmlDocMsgs = new XDocument(
                    new XDeclaration("1.0", "UTF-8", "yes"),
                    new XComment("Project3Assignment5"),
                    new XElement(nameSpace + "Messages",
                    new XElement(nameSpace + "Message",
                    new XElement(nameSpace + "SenderID", senderID),
                    new XElement(nameSpace + "ReceiverID", receiverID),
                    new XElement(nameSpace + "TimeStamp", System.DateTime.Now.ToString()),
                    new XElement(nameSpace + "MessageContents", msg),
                    new XElement(nameSpace + "Read", "false"))));
                    xmlDocMsgs.Save(fLocation);
                }
            }
        }

        public string[] receiveMsg(string receiverID, bool purge)
        {
            string[] returnMsg;
            XDocument xmlDocMsgs = XDocument.Load(fLocation);

            IEnumerable<XElement> queryElementItems =
            from item in xmlDocMsgs.Root.Descendants(nameSpace + "Message")
            where item.Element(nameSpace + "ReceiverID").Value == receiverID
            where item.Element(nameSpace + "Read").Value == "false"
            orderby (DateTime)item.Element(nameSpace + "TimeStamp") descending
            select item;
            returnMsg = new string[queryElementItems.Count() * 3];
            int iter = 0;

            foreach (XElement item in queryElementItems)
            {
                returnMsg[iter++] = item.Element(nameSpace + "SenderID").Value;
                returnMsg[iter++] = item.Element(nameSpace + "TimeStamp").Value;
                returnMsg[iter++] = item.Element(nameSpace + "MessageContents").Value;
            }

            checkMessage(receiverID);

            if (purge)
            {
                deleteMsg(receiverID);
            }

            return returnMsg;
        }

        public void deleteMsg(string receiverID)
        {
            XDocument xmlDocMsgs = XDocument.Load(fLocation);

            IEnumerable<XElement> queryElementItems =
            from item in xmlDocMsgs.Root.Descendants(nameSpace + "Message")
            where item.Element(nameSpace + "ReceiverID").Value == receiverID
            orderby (DateTime)item.Element(nameSpace + "TimeStamp") descending
            select item;
            foreach (XElement item in queryElementItems)
            {
                item.Remove();
            }
                xmlDocMsgs.Save(fLocation);
        }

        public void checkMessage(string receiverID)
        {
            XDocument xmlDocMsgs = XDocument.Load(fLocation);
            foreach (XElement item in xmlDocMsgs.Element(nameSpace + "Messages").Elements(nameSpace + "Message"))
            {

                if (item.Element(nameSpace + "ReceiverID").Value == receiverID)
                {
                    item.Element(nameSpace + "Read").Value = "true";
                }

                xmlDocMsgs.Save(fLocation);
            }
        }
    }
}
