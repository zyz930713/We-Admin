using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Wenba.Controllers
{
    public class WeiXinApiController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetOpenId(string code)
        {
            string content = "";
            string appId = ConfigurationManager.AppSettings["AppId"];
            string appSecret = ConfigurationManager.AppSettings["AppSecret"];

            if (!String.IsNullOrEmpty(code))
            {
                
                string url = "https://api.weixin.qq.com/sns/jscode2session?appid=" + appId + "&secret=" + appSecret + "&js_code=" + code + "&grant_type=authorization_code";
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "GET";
                using(WebResponse wr = req.GetResponse())
                {
                    Stream responseStream = wr.GetResponseStream();
                    using(StreamReader sr = new StreamReader(responseStream))
                    {
                        content = sr.ReadToEnd();
                    }
                }
            }
            else
            {
                return NotFound();
            }
            return Ok(content);
        }
    }
}
