using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.VNPay
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> requestData = new SortedList<string, string>();

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                requestData.Add(key, value);
            }
        }

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var query = new StringBuilder();
            foreach (var kv in requestData)
            {
                query.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }

            string signData = query.ToString().TrimEnd('&');
            string vnp_SecureHash = HmacSHA512(hashSecret, signData);
            return baseUrl + "?" + signData + "&vnp_SecureHash=" + vnp_SecureHash;
        }

        public static string HmacSHA512(string key, string inputData)
        {
            var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputData));
            var hex = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            return hex;
        }
    }
}
