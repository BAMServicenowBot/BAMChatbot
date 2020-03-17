using BamChatBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BamChatBot.Services
{
	public class RPAService
	{
		public string UserName { get; private set; }
		public string Pass { get; private set; }
		public RPAService()
		{
			UserName = "integration_bamchatbot";
			Pass = "dc4e1953dbf1c45897b930cf9d961992";
		}

		internal string ClearResponse(string result)
		{
			//get rid of {"result":} wrapper from response
			result = result.Remove(0, 1);
			result = result.Substring(result.IndexOf(':') + 1);
			result = result.Remove(result.LastIndexOf('}'));
			return result;
		}

		internal void AddAuthorization(HttpClient client)
		{
			//get credentials
			
			var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", this.UserName, this.Pass)));
			//add basic authorization
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
		}

		internal APIResponse GetApiResult(string endPoint, string  userId)
		{
			var user = new User();
			//get user first name  
			var apiPath = GetApiPath();
			var url = apiPath + endPoint;
			var userName = user.GetLoginUserName();
			var urlParameters = "?sys_user=" + userId;

			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(url);
			//get credentials

			//add basic authorization
			AddAuthorization(client);

			// Add an Accept header for JSON format.
			client.DefaultRequestHeaders.Accept.Add(
			new MediaTypeWithQualityHeaderValue("application/json"));

			// List data response.
			HttpResponseMessage response = client.GetAsync(urlParameters).Result;

			var obj = response.Content.ReadAsStringAsync();
			//get rid of {"result":} wrapper from response
			var result = ClearResponse(obj.Result);
			var apiResponse = new APIResponse
			{
				Content = result,
				IsSuccess = response.IsSuccessStatusCode
			};

			return apiResponse;
		}

		private string GetApiPath()
		{
			var apiPath = "https://bayviewdev.service-now.com/api/baam/bam_chat_bot/";
			return apiPath;
		}

		internal APIResponse StartProcess(ProcessModel data)
		{
			var apiResponse = new APIResponse();
			var apiPath = GetApiPath();
			var url = apiPath + "startProcess";
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(url);
			var urlParameters = "?user=" + data.UserId + "&sys_id=" + data.Sys_id;// + "&activity=" + activityId;
			//add basic authorization
			AddAuthorization(client);

			// Add an Accept header for JSON format.
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			try
			{
				HttpResponseMessage response = client.GetAsync(urlParameters).Result;
				var obj = response.Content.ReadAsStringAsync();
				//get rid of {"result":} wrapper from response
				var result = ClearResponse(obj.Result);
				apiResponse = new APIResponse
				{
					Content = result,
					IsSuccess = response.IsSuccessStatusCode
				};
			}
			catch (Exception ex)
			{
				apiResponse.Error = ex.Message;
			}

			return apiResponse;

		}

		internal void UpdateUser(User user)
		{
			var url = "https://bayviewdev.service-now.com/api/now/table/u_chatbot_user_state/" + user.sys_id;
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(url);
			
			AddAuthorization(client);

			// Add an Accept header for JSON format.
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var content = new StringContent(JsonConvert.SerializeObject(new User { u_last_index = user.u_last_index}), Encoding.UTF8, "application/json");
			HttpResponseMessage response = client.PutAsync(url, content).Result;
				var obj = response.Content.ReadAsStringAsync();
				//get rid of {"result":} wrapper from response
				var result = ClearResponse(obj.Result);
				/*apiResponse = new APIResponse
				{
					Content = result,
					IsSuccess = response.IsSuccessStatusCode
				};*/
			
		}

		internal APIResponse ProcessStatus(APIRequest data)
		{
			var apiPath = GetApiPath();
			var url = apiPath + "processStatus";
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(url);
			// var urlParameters = "?user=" + data.UserId + "&sys_id=" + data.Sys_id;
			//add basic authorization
			AddAuthorization(client);
			var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
			var response = client.PostAsync(url, content).Result;
			var obj = response.Content.ReadAsStringAsync();

			//get rid of {"result":} wrapper from response
			var result = ClearResponse(obj.Result);
			var apiResponse = new APIResponse
			{
				Content = result,
				IsSuccess = response.IsSuccessStatusCode
			};

			return apiResponse;

		}

		internal APIResponse StopProcess(string processId)
		{
			var apiResponse = new APIResponse();
			var apiPath = GetApiPath();
			var url = apiPath + "stopProcess";
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(url);
			var urlParameters = "?sys_id=" + processId;
			//add basic authorization
			AddAuthorization(client);
			// Add an Accept header for JSON format.
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			try
			{
				HttpResponseMessage response = client.GetAsync(urlParameters).Result;
				var obj = response.Content.ReadAsStringAsync();

				apiResponse = new APIResponse
				{
					Content = obj.Result,
					IsSuccess = response.IsSuccessStatusCode
				};
				if (!apiResponse.IsSuccess)
				{
					//get rid of {"result":} wrapper from response
					var result = ClearResponse(obj.Result);
					apiResponse = JsonConvert.DeserializeObject<APIResponse>(result);
				}
				
			}
			catch (Exception ex)
			{

				apiResponse.Content = ex.Message;
			}
			return apiResponse;
		}

		internal void SaveUser(User user)
		{
			
		    var url = "https://bayviewdev.service-now.com/api/now/table/u_chatbot_user_state";
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(url);
			// var urlParameters = "?user=" + data.UserId + "&sys_id=" + data.Sys_id;
			//add basic authorization
			AddAuthorization(client);
			var json = JsonConvert.SerializeObject(user);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = client.PostAsync(url, content).Result;
			var obj = response.Content.ReadAsStringAsync();

			//get rid of {"result":} wrapper from response
			/*var result = ClearResponse(obj.Result);
			var apiResponse = new APIResponse
			{
				Content = result,
				IsSuccess = response.IsSuccessStatusCode
			};*/

		}

		internal APIResponse GetUser(string conversationId)
		{
			var user = new User();
			//get user first name  
			var apiPath = GetApiPath();
			var url = "https://bayviewdev.service-now.com/api/now/table/u_chatbot_user_state";
			
			var urlParameters = "?sysparm_query=u_conversation_id%3D" + conversationId;

			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(url);
			//get credentials

			//add basic authorization
			AddAuthorization(client);

			// Add an Accept header for JSON format.
			client.DefaultRequestHeaders.Accept.Add(
			new MediaTypeWithQualityHeaderValue("application/json"));

			// List data response.
			HttpResponseMessage response = client.GetAsync(urlParameters).Result;

			var obj = response.Content.ReadAsStringAsync();
			//get rid of {"result":} wrapper from response
			var result = ClearResponse(obj.Result);
			var apiResponse = new APIResponse
			{
				Content = result,
				IsSuccess = response.IsSuccessStatusCode
			};

			return apiResponse;
		}

			internal TopProcess GetListOfProcess(List<ProcessModel> processes, int lastIdx)
		{
			var tempProcesses = new List<ProcessModel>();
			//test
			/*var _p = processes[processes.Count-1];
			
			for (var i=0; i < 10; i++)
			{
				var pm = new ProcessModel
				{
					Name = _p.Name + i.ToString(),
					Sys_id = _p.Sys_id
				};
				
				processes.Add(pm);
			}*/
			//end test
			//initialize list of Choices
			var choices = new List<Choice>();
			
			

			if (processes.Count > 10)
			{
				var count = 0;
				for(var p=lastIdx; p<processes.Count; p++)
				{
					if (count == 10)
					{
						lastIdx = p;
						break;
					}else if(p== processes.Count - 1)
					{
						lastIdx = p;
					}
					tempProcesses.Add(processes[p]);
					count++;
				}
				

				if (lastIdx != processes.Count - 1)
				{
					tempProcesses.Add(new ProcessModel
					{
						Sys_id = "Load_More",
						Name = "Load More"
					});
				}

			}
			else
			{
				tempProcesses = processes;
			}
		
			foreach (var process in tempProcesses)
			{
				var name = process.Name; 
				if(process.Sys_id != "Load_More")
				{
					//name+= " [" + process.LastRun.State + "]";
				}
				choices.Add(new Choice
				{
					Value = process.Sys_id,
					Action = new CardAction(ActionTypes.PostBack, name, null, process.Name, process.Name, process.Sys_id, null)
				});
			}

			return new TopProcess
			{
				Choices = choices,
				LastIndex = lastIdx
			};
		}

		internal ProcessModel GetSelectedProcess(List<ProcessModel> processes, string value)
		{
			var processSelected = new ProcessModel();
			foreach (var item in processes)
			{
				if (value == item.Sys_id)
				{
					processSelected = item;
				}
			}
			return processSelected;
		}

		internal Choice GetRPASupportOption()
		{
			return new Choice
			{
				Value = "rpaSupport",//RPASupport@bayview.com
				Action = new CardAction(ActionTypes.PostBack, "Contact RPA Support", null, "Contact RPA Support", "openEmail", "RPASupport@bayview.com", null)

			};
		}
		}
}
