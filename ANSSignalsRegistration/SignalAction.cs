using System;
using Newtonsoft.Json;

namespace Adamant.NotificationService.SignalsRegistration
{
    public enum SignalAction { add, remove }

    internal class SignalActionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = (string)reader.Value;

            try
            {
                return Enum.Parse(typeof(SignalAction), enumString, true);
            }
            catch
            {
                return SignalAction.add;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var action = (SignalAction)value;

            switch (action)
            {
                case SignalAction.add:
                    writer.WriteValue("add");
                    break;

                case SignalAction.remove:
                    writer.WriteValue("remove");
                    break;
            }
        }
    }
}
