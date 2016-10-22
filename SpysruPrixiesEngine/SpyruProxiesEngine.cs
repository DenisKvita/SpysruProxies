using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using HtmlAgilityPack;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace SpysruProxiesEngineNs
{
    public class SpysruProxiesEngine
    {
        public event EventHandler<ProxyLookupEventArgs> RaiseProxyLookup;
        public event EventHandler<ProxyLookupEventArgs> RaiseProxyLookupStateChange;
        public event EventHandler RaiseProxyLookupEnd;

        public SpysruProxiesEngine()
        {
        }

        public void lookup(string protocol)
        {
            Task<ProxyLookupEventArgs[]> tsk = new Task<ProxyLookupEventArgs[]>(() =>
            {
                HtmlWeb w = new HtmlWeb();
                HtmlDocument wdoc = w.Load("http://spys.ru/proxies/");
                HtmlNodeCollection nv = wdoc.DocumentNode.SelectNodes("/html/body/table[2]/tr[4]/td/table/tr[starts-with(@onmouseover, 'this.')]");////div[2]");
                HtmlNode port = wdoc.DocumentNode.SelectSingleNode("/html/body/table[2]/tr[4]/td/table/tr[8]/td[2]");//font[2]/script");
                HtmlNode scpt = wdoc.DocumentNode.SelectSingleNode("/html/body/script");
                ProxyPortDecoder dec = new ProxyPortDecoder();
                dec.init(scpt.InnerHtml.Split(new char[] { ';' }).Where(s => s.Contains('^')).ToArray());

                List<ProxyLookupEventArgs> res = new List<ProxyLookupEventArgs>();
                int i = 1;
                foreach (HtmlNode hn in nv)
                {
                    HtmlNode hostN = hn.SelectSingleNode("./td[1]/font[2]");
                    HtmlNode portN = hn.SelectSingleNode("./td[1]/font[2]/script");
                    HtmlNode protocolN = hn.SelectSingleNode("./td[2]");
                    res.Add(new ProxyLookupEventArgs(i
                          , protocolN.InnerText.Split(new char[] { ' ' }, StringSplitOptions.None)[0]
                          , hostN.InnerText.Split(new string[] { "document." }, StringSplitOptions.None)[0]
                          , dec.decode(portN.InnerText)));
                    i++;
                }
                return res.ToArray();
            });
            tsk.ContinueWith((t) =>
            {
                ProxyLookupEventArgs[] proxies = (ProxyLookupEventArgs[])t.Result;
                Task[] tsks = new Task[proxies.Count()];
                for (int i = 0; i < proxies.Count(); i++)
                {
                    tsks[i] = Task.Factory.StartNew<ProxyLookupEventArgs>((pl) =>
                    {
                        ProxyLookupEventArgs prx = (ProxyLookupEventArgs)pl;
                        prx.State = ProxyLookupEventArgs.CheckState.Checking;
                        OnRaiseProxyLookupStateChange(prx);
                        try
                        {
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.google.com");
                            WebProxy myproxy = new WebProxy(prx.Host, Int32.Parse(prx.Port));
                            myproxy.BypassProxyOnLocal = false;
                            request.Proxy = myproxy;
                            request.Method = "GET";
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                            prx.State = (response.StatusCode == HttpStatusCode.OK ? ProxyLookupEventArgs.CheckState.Online : ProxyLookupEventArgs.CheckState.Offline);
                        }
                        catch (Exception e)
                        {
                            prx.State = ProxyLookupEventArgs.CheckState.Error;
                            prx.Details = e.Message;
                            OnRaiseProxyLookupStateChange(prx);
                        }
                        OnRaiseProxyLookupStateChange(prx);
                        return prx;
                    }, proxies[i]);
                    OnRaiseProxyLookup(proxies[i]);

                }
                Task tc = Task.Factory.ContinueWhenAll(tsks, (ts) =>
                {
                    OnRaiseProxyLookupEnd();
                });
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            tsk.Start();
        }

        protected virtual void OnRaiseProxyLookup(ProxyLookupEventArgs e)
        {
            EventHandler<ProxyLookupEventArgs> handler = RaiseProxyLookup;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnRaiseProxyLookupEnd()
        {
            EventHandler handler = RaiseProxyLookupEnd;
            if (handler != null) handler(this, new EventArgs());
        }

        protected virtual void OnRaiseProxyLookupStateChange(ProxyLookupEventArgs e)
        {
            EventHandler<ProxyLookupEventArgs> handler = RaiseProxyLookupStateChange;
            if (handler != null) handler(this, e);
        }

    }
}
