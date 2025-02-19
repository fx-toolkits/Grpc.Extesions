﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Grpc.Extension.Common
{
    internal static class JsonSerialization
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        /// <summary>
        /// 使用json序列化为字符串
        /// </summary>
        /// <param name="dateTimeFormat">默认null,即使用json.net默认的序列化机制，如："\/Date(1439335800000+0800)\/"</param>
        /// <returns></returns>
        public static string ToJson(this object input, string dateTimeFormat = "yyyy-MM-dd HH:mm:ss", bool ignoreNullValue = true, bool isIndented = false)
        {
            settings.NullValueHandling = ignoreNullValue ? NullValueHandling.Ignore : NullValueHandling.Include;

            if (!string.IsNullOrWhiteSpace(dateTimeFormat))
            {
                var jsonConverter = new List<JsonConverter>()
                {
                    // 如： "yyyy-MM-dd HH:mm:ss"
                    new Newtonsoft.Json.Converters.IsoDateTimeConverter() { DateTimeFormat = dateTimeFormat } 
                };

                settings.Converters = jsonConverter;
            }

            var format = isIndented ? Formatting.Indented : Formatting.None;
            var json = JsonConvert.SerializeObject(input, format, settings);

            return json;
        }

        /// <summary>
        /// 从序列化字符串里反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="dateTimeFormat">默认null,即使用json.net默认的序列化机制</param>
        /// <returns></returns>
        public static T TryFromJson<T>(this string input, string dateTimeFormat = "yyyy-MM-dd HH:mm:ss", bool ignoreNullValue = true)
        {
            try
            {
                return input.FromJson<T>(dateTimeFormat, ignoreNullValue);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 从序列化字符串里反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="dateTimeFormat">默认null,即使用json.net默认的序列化机制</param>
        /// <returns></returns>
        public static T FromJson<T>(this string input, string dateTimeFormat = "yyyy-MM-dd HH:mm:ss", bool ignoreNullValue = true)
        {
            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
            settings.NullValueHandling = ignoreNullValue ? NullValueHandling.Ignore : NullValueHandling.Include;

            if (!string.IsNullOrWhiteSpace(dateTimeFormat))
            {
                var jsonConverter = new List<JsonConverter>()
                {
                    // 如： "yyyy-MM-dd HH:mm:ss"
                    new Newtonsoft.Json.Converters.IsoDateTimeConverter(){ DateTimeFormat = dateTimeFormat }
                };
                settings.Converters = jsonConverter;
            }

            return JsonConvert.DeserializeObject<T>(input, settings);
        }

        /// <summary>
        /// 从序列化字符串里反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="dateTimeFormat">默认null,即使用json.net默认的序列化机制</param>
        /// <returns></returns>
        public static object FromJson(this string input, Type type, string dateTimeFormat = "yyyy-MM-dd HH:mm:ss", bool ignoreNullValue = true)
        {
            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
            if (ignoreNullValue)
            {
                settings.NullValueHandling = NullValueHandling.Ignore;
            }

            if (!string.IsNullOrWhiteSpace(dateTimeFormat))
            {
                var jsonConverter = new List<JsonConverter>()
                {
                    // 如： "yyyy-MM-dd HH:mm:ss"
                    new Newtonsoft.Json.Converters.IsoDateTimeConverter(){ DateTimeFormat = dateTimeFormat }
                };
                settings.Converters = jsonConverter;
            }

            return JsonConvert.DeserializeObject(input, type, settings);
        }
    }
}
