using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BamChatBot.Cards
{
	public class CardActionCustom: CardAction
	{
		//
		// Summary:
		//     Initializes a new instance of the CardAction class.
		public CardActionCustom() { }
		//
		// Summary:
		//     Initializes a new instance of the CardAction class.
		//
		// Parameters:
		//   type:
		//     The type of action implemented by this button. Possible values include: 'openUrl',
		//     'imBack', 'postBack', 'playAudio', 'playVideo', 'showImage', 'downloadFile',
		//     'signin', 'call', 'payment', 'messageBack', 'openApp'
		//
		//   title:
		//     Text description which appears on the button
		//
		//   image:
		//     Image URL which will appear on the button, next to text label
		//
		//   text:
		//     Text for this action
		//
		//   displayText:
		//     (Optional) text to display in the chat feed if the button is clicked
		//
		//   value:
		//     Supplementary parameter for action. Content of this property depends on the ActionType
		//
		//   channelData:
		//     Channel-specific data associated with this action
		public CardActionCustom(string type = null, string title = null, string image = null, string text = null, string displayText = null, object value = null, object channelData = null)
		{
			if(type == "openUrl")
			{

			}
		}

		//
		// Summary:
		//     Gets or sets the type of action implemented by this button. Possible values include:
		//     'openUrl', 'imBack', 'postBack', 'playAudio', 'playVideo', 'showImage', 'downloadFile',
		//     'signin', 'call', 'payment', 'messageBack'
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }
		//
		// Summary:
		//     Gets or sets text description which appears on the button
		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }
		//
		// Summary:
		//     Gets or sets image URL which will appear on the button, next to text label
		[JsonProperty(PropertyName = "image")]
		public string Image { get; set; }
		//
		// Summary:
		//     Gets or sets text for this action
		[JsonProperty(PropertyName = "text")]
		public string Text { get; set; }
		//
		// Summary:
		//     Gets or sets (Optional) text to display in the chat feed if the button is clicked
		[JsonProperty(PropertyName = "displayText")]
		public string DisplayText { get; set; }
		//
		// Summary:
		//     Gets or sets supplementary parameter for action. Content of this property depends
		//     on the ActionType
		[JsonProperty(PropertyName = "value")]
		public object Value { get; set; }
		//
		// Summary:
		//     Gets or sets channel-specific data associated with this action
		[JsonProperty(PropertyName = "channelData")]
		public object ChannelData { get; set; }

		
	
}
}
