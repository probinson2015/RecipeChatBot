using System;
using System.Linq;
using System.Net;
using unirest_net.http;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace RecipeBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                //int length = (activity.Text ?? string.Empty).Length;

                LuisParser luisAnswer = await Luis.ParseUserInput(activity.Text);
                string userInput = activity.Text;
                string result = "";
                string call = "";
              

                if (luisAnswer.intents.Count() > 0)
                {
                    switch (luisAnswer.intents[0].intent)
                    {
                        case "GetRecipes":
                            call = APICall(userInput);
                            if (call == "[]")
                            {
                               result = "Please enter ingredients and I'll search for recipes that match your ingredients. You can also say things like \"Feed Me\" or \"I\"m hungry\" and I'll fetch the most popular recipes for you.";
                            }
                            else
                            {
                                //System.Web.Script.Serialization.JavaScriptSerializer oSerializer =
                                //new System.Web.Script.Serialization.JavaScriptSerializer();
                                //call = oSerializer.Serialize(call); ;

                                result = call;
                                
                            }
                            break;
                        case "SayGreeting":
                            result = "Hello, welcome to the Food Bot. Tell me the ingredients you have on hand and I will help you make a meal of it!";
                            break;
                        case "GetPopularRecipes":
                            result = GetPopularRecipes(userInput);
                            break;
                        default:
                            break;
                    }
                }

                // return our reply to the user
                Activity reply = activity.CreateReply(result);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private string GetPopularRecipes(string userInput)
        {
            string popularRecipes = "Here are the most popular recipes today: ";
            return popularRecipes;
        }

        public string APICall(string ingredients)
        {
            // These code snippets use an open-source library. http://unirest.io/net
            //var response = Unirest.get("https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/findByIngredients?fillIngredients=false&ingredients=apples%2Cflour%2Csugar&limitLicense=false&number=5&ranking=1")
            //.header("X-Mashape-Key", "9k9FlDWFV8mshco3NWuUsSfLAnu1p1HQqFLjsnlShI8QhwwoRC")
            //.header("Accept", "application/json")
            //.asString();
            //return response.Body;

            string[] items = ingredients.Split(' ');

            string APICallWithIngredients = "https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/findByIngredients?fillIngredients=false&ingredients=";

            for (int i = 0; i < items.Length; i++)
            {
                if (i != items.Length - 1)
                {
                    APICallWithIngredients += items[i];
                    APICallWithIngredients += "%2C";
                }
                else
                {
                    APICallWithIngredients += items[i];
                }
            }

            APICallWithIngredients += "&limitLicense=false&number=5&ranking=1";

            //These code snippets use an open-source library.http://unirest.io/net
            var response = Unirest.get(APICallWithIngredients)
            .header("X-Mashape-Key", "9k9FlDWFV8mshco3NWuUsSfLAnu1p1HQqFLjsnlShI8QhwwoRC")
            .header("Accept", "application/json")
            .asString();

            return response.Body;
       
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
                if (message.Type == "Ping")
                {
                    Activity reply = message.CreateReply();
                    reply.Type = "Ping";
                    return reply;
                }
            }

            return null;
        }
    }
}