﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BamChatBot.Models
{
    public class User
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Error { get; set; }

        internal User GetUser()
        {
            var snConfig = new Config();
            var result = snConfig.GetApiResult("getUser");
            var user = JsonConvert.DeserializeObject<User>(result.Content);
          /*  var user = new User();
            //get user first name  
            var url = "https://bayviewdev.service-now.com/api/baam/bam_chat_bot/getUser";
            var userName = GetLoginUserName();
            var urlParameters = "?user_name="+ userName;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            //get credentials
           
            //add basic authorization
            snConfig.AddAuthorization(client);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;

                var result = response.Content.ReadAsStringAsync().Result;
                //get rid of {"result":} wrapper from response
                result = snConfig.ClearResponse(result);
                user = JsonConvert.DeserializeObject<User>(result);
            
            //Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
            client.Dispose();*/
            return user;
        }

        internal string GetLoginUserName()
        {
            var name = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            var slashIndex = name.IndexOf("\\");
            var userName = slashIndex > -1
                ? name.Substring(slashIndex + 1)
                : name.Substring(0, name.IndexOf("@"));
            return userName;
        }
    }
}