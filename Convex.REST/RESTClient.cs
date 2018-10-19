using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Convex.REST {
    public class RESTClient {
        public RESTClient(string endPoint, RequestMethod method) {
            Method = method;
        }

        public RequestMethod Method { get; set; }

        public async Task<string> Request(RequestMethod method, string endPoint) {
            string responseValue = string.Empty;

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(endPoint);
            request.Method = method.ToString();

            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse()) {
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                using (StreamReader reader = GetStreamReader(response)) {
                    while (!reader.EndOfStream)
                        responseValue = await reader.ReadToEndAsync();
                }
            }

            return responseValue;
        }

        private static StreamReader GetStreamReader(WebResponse response) {
            Stream stream = response.GetResponseStream();

            return stream == null ? null : new StreamReader(stream);
        }
    }
}