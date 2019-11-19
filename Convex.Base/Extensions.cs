#region

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace Convex.Base
{
    public static class Extensions
    {
        /// <summary>
        ///     Obtain HTTP response from a GET request
        /// </summary>
        /// <returns>GET response</returns>
        public static async Task<string> HttpGet(this string instance)
        {
            using HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(instance)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(instance);
            string message = string.Empty;

            if (response.IsSuccessStatusCode)
            {
                message = await response.Content.ReadAsStringAsync();
            }

            return message;
        }

        /// <summary>
        ///     Splits a string into separate parts
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="maxLength">max length of individual strings to split</param>
        public static IEnumerable<string> LengthSplit(this string instance, int maxLength)
        {
            for (int i = 0; i < instance.Length; i += maxLength)
            {
                yield return instance.Substring(i, Math.Min(maxLength, instance.Length - i));
            }
        }

        /// <summary>
        ///     Removes run-on spaces in a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DeepTrim(this string str)
        {
            StringBuilder trimmedString = new StringBuilder();
            bool isSpace = false;

            foreach (char character in str)
            {
                if (character.Equals(' '))
                {
                    // if the previous character was also a space, ignore char
                    if (isSpace)
                    {
                        continue;
                    }

                    isSpace = true;
                }
                else
                {
                    isSpace = false;
                }

                trimmedString.Append(character);
            }

            return trimmedString.ToString();
        }
    }
}