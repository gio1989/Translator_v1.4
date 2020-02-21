using Newtonsoft.Json;

namespace Translator.Classes
{
    public class LoginHelperObject
    {
        [JsonProperty(PropertyName = "savePasswordEnabled")]
        public string SavePasswordEnabled { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }
    }
}
