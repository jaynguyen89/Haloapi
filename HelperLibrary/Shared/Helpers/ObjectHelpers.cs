using DeepCopy;
using Newtonsoft.Json;
using System.Text;

namespace HelperLibrary.Shared.Helpers;

public static class ObjectHelpers {
    
    public static byte[] EncodeDataAscii(this object data) => Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data));
    
    public static byte[] EncodeDataUtf8(this object data) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));

    public static T? DecodeUtf8<T>(this byte[] data) {
        var decodedData = Encoding.UTF8.GetString(data);
        return typeof(T).IsPrimitive
            ? (T) Convert.ChangeType(decodedData, typeof(T))
            : JsonConvert.DeserializeObject<T>(decodedData);
    }

    public static T SerializedClone<T>(this T any) {
        var serialized = JsonConvert.SerializeObject(any);
        return JsonConvert.DeserializeObject<T>(serialized)!;
    }

    public static T ReflectiveClone<T>(this T any) {
        var type = any!.GetType();
        var properties = type.GetProperties();
        var clone = (T)Activator.CreateInstance(type)!;
        
        foreach (var property in properties) {
            if (!property.CanWrite) continue;
            var value = property.GetValue(any);
            
            if (value != null && value.GetType().IsClass && !value.GetType().FullName!.StartsWith("System."))
                property.SetValue(clone, ReflectiveClone(value));
            else
                property.SetValue(clone, value);
        }
        
        return clone;
    }

    public static T DeepClone<T>(this T any) => DeepCopier.Copy(any);
}