using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace OutfitShop.Helpers
{
    public static class SessionExtensions
    {
        // Lưu object vào Session (chuyển sang JSON)
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Lấy object từ Session (chuyển ngược lại)
        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}