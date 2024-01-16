using Microsoft.Maui.Devices.Sensors;
using Newtonsoft.Json;
using System.Diagnostics;

namespace PrepaCH
{
    public static class Extensions
    {
        public static bool IsEmpty<T>(this ICollection<T> collection)
        {
            return collection.Count == 0;
        }

        public static bool IsNotEmpty<T>(this ICollection<T> collection)
        {
            return collection.Count > 0;
        }

        public static string ToVVAEsportCHParameter(this DateTime dateTime)
        {
            return $"{dateTime.Year}{dateTime.Month:00}{dateTime.Day:00}";
        }

        public static string ToSATAdminParameter(this DateTime dateTime)
        {
            return $"{dateTime.Year}-{dateTime.Month:00}-{dateTime.Day:00}T00:00:00.000Z";
        }

        public static double CalculateDistance2D(this Location from, Location to, DistanceUnits units = default)
        {
            Debug.WriteLine(from);
            Debug.WriteLine(to);
            Debug.WriteLine(units);
            return Location.CalculateDistance(from, to, units);
        }
    }

    public static class Utils
    {
        /**
         * Creates a set of specified length
         */
        public static HashSet<int> RandomSet(int count, int min, int max)
        {
            Random r = new Random();
            HashSet<int> result = new HashSet<int>();

            while (result.Count < count)
            {
                result.Add(r.Next(min, max));
            }

            return result;
        }

        public class GeoCodeJsonElement
        {
            public string lat;
            public string lon;
        }

        public static async Task<Location> LocationFromAdress(int npa, string commune)
        {
            HttpClient _client = new HttpClient();
            Uri uri = new Uri(string.Format(
                "https://geocode.maps.co/search?q={0}%20{1}", npa, commune
            ));

            HttpResponseMessage response = await _client.GetAsync(uri);
            string content = await response.Content.ReadAsStringAsync();
            List<GeoCodeJsonElement> res = JsonConvert.DeserializeObject<List<GeoCodeJsonElement>>(content);

            //Response should be a JSON array. get the lat/lon of the first element...
            return new Location(double.Parse(res[0].lat), double.Parse(res[0].lon));
        }

#nullable enable
        public static async Task<Location?> GetCachedLocation()
#nullable disable
        {
            try
            {
                return await Geolocation.Default.GetLastKnownLocationAsync();
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            catch (Exception ex)
            {
                // Unable to get location
            }

            return null;
        }
    }

    class Duration//OR DateTimeRange
    {
        public DateTime From;
        public DateTime To;

        public Duration(DateTime From, DateTime To)
        {
            this.From = From;
            this.To = To;
        }
    }
}
