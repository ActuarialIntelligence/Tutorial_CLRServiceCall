using System.Data.SqlTypes;
using System.IO;
using System.Net;
using System.Xml;

public partial class UserDefinedFunctions
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString BPOValidateServiceCLRFunction(string url, string user, string pass, string uacfid, string company, string consultantcode, string type)
    {
        var result = IsValid(url,user,pass,uacfid,company,consultantcode,type);
        return new SqlString(result);
    }

    public static string IsValid(string url, string user, string pass, string uacfid, string company, string consultantcode, string type)
    {
        ServicePointManager
                .ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

        string strings="";
        string result = "";
        try
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + uacfid + "&company=" + company + "&consultantcode=" + consultantcode + "&type=" + type);
            httpWebRequest.ServerCertificateValidationCallback
                += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Method = WebRequestMethods.Http.Get;
            var servicePoint = httpWebRequest.ServicePoint;
            httpWebRequest.Credentials = new NetworkCredential(user, pass);
            HttpWebResponse webResponse = webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader responseReader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()))
            {
                result = responseReader.ReadToEnd();
            }
            strings += webResponse.ResponseUri.ToString() + webResponse.StatusCode + webResponse.StatusDescription;
            strings += ParseAsXMLandRetrieveStringNodeValue(result);
        }
        catch(System.Exception e)
        {
            strings += e.ToString();
        }

        return strings;
    }



    private static string ParseAsXMLandRetrieveStringNodeValue(string result)
    {
        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(result);
        XmlNodeList nodeList = xmldoc.GetElementsByTagName("string");
        string strings = string.Empty;
        foreach (XmlNode node in nodeList)
        {
            strings = node.InnerText;
        }

        return strings;
    }
}
