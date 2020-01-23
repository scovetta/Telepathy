using System.Collections.Generic;
using System.Security.Claims;
using System.ServiceModel;
using System.Text;
using System.Threading;
using IdentityModel;

namespace WcfService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        bool Add(string msg);


        [OperationContract]
        string Echo();

        [OperationContract]
        bool Update(string oldMsg, string newMsg);

        [OperationContract]
        string GetInfo();
    }

    class Service : IService
    {

        public bool Add(string msg)
        {
            var cp = ClaimsPrincipal.Current;
            if (CacheInMemory.dic.ContainsKey(msg))
            {
                return false;
            }
            else
            {
                CacheInMemory.dic.Add(msg, cp.FindFirst(JwtClaimTypes.Subject).Value);
                return true;
            }
        }

        public string Echo()
        {
            var sb = new StringBuilder();

            foreach (var entity in CacheInMemory.dic)
            {
                sb.AppendFormat("{0} :: {1}\n", entity.Key, entity.Value);
            }

            return sb.ToString();
        }

        public bool Update(string oldMsg, string newMsg)
        {
            if (!CacheInMemory.dic.ContainsKey(oldMsg))
                return false;

            string client = ClaimsPrincipal.Current.FindFirst(JwtClaimTypes.Subject).Value;

            if (CacheInMemory.dic[oldMsg].Equals(client))
            {
                CacheInMemory.dic.Remove(oldMsg);
                CacheInMemory.dic.Add(newMsg, client);
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetInfo()
        {
            var sb = new StringBuilder();

            foreach (var claim in ClaimsPrincipal.Current.Claims)
            {
                sb.AppendFormat("{0} :: {1}\n", claim.Type, claim.Value);
            }

            return sb.ToString();
        }
    }
}