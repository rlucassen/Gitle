namespace Gitle.Clients.Freckle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Interfaces;
    using Models;
    using ServiceStack.ServiceClient.Web;


    public class EntryClient : IEntryClient
    {
        private readonly JsonServiceClient _client;

        public EntryClient(string freckleApi, string token)
        {
            _client = new JsonServiceClient(freckleApi);
            _client.Headers.Add("X-FreckleToken", token);
            _client.LocalHttpWebRequestFilter = request => request.UserAgent = "auxilium";

        }


        public bool Post(Entry entry)
        {
            try
            {
                _client.Post<bool>("entries", new EntryResult(entry));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        [DataContract]
        protected class EntryResult
        {
            public EntryResult(Entry entry)
            {
                Entry = entry;
            }
            [DataMember(Name = "entry")]
            public Entry Entry { get; set; }
        }


    }
}
