using Newtonsoft.Json;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using System.Diagnostics;
using System.Drawing;
using Point = GeoJSON.Net.Geometry.Point;

/**
 * An issue I encountered while making the PrepaCH app when searching for Range Dates is that not all ranges / dates have a location
 * Many directly have GPS coordinates attached, however some just have the commune (there are others that just have a name, which for now is not supported)
 * If I wanted to be able to search by closest range, I would have to find the missing data
 * While I originally wanted to do GeoCoding with https://geocode.maps.co/, it has rtate limiting and honestly doing API requests per entry wasn't going to be ideal
 * 
 * So instead, I got date from https://www.swisstopo.admin.ch/en/geodata/official-geographic-directories/directory-towns-cities.html
 * For performance, the stand location will just be the commune center point, as many polygon checks would be computationally expensive for effectively the same result
 * However, the file with just points doesn't have the NPA of the commune, only the file with the commune polygon region has the NPA attached
 * 
 * The purpose of this script is hence to create a GeoJson file to be read by PrepaCH, which is a FeatureCollection of Points
 * Each Points with have the NPZ property, which is the commune number
 * So the point of this script is to read the Polygon (plz) and Point (osnamepos) files for communes, and add the missing data to the Point file
 * We want to use Points for the communes instead of the Polygon 
 *     A: due to file size (~1.5MB vs 80+MB)
 *     B: speed of calculation when searching for range dates (in the app)
 * 
 * Input files are GeoJson as well, as reading Shape files is a massive pain honestly. 
 * I can't find a good lib to do it and I honestly don't want to spend the time to do it myself atm.
 * I originally followed https://www.statsilk.com/maps/convert-esri-shapefile-map-geojson-format, however it was easier to use QGIS once I discovered that I
 *     also had to reproject the CH data files to  WGS 84 (EPSG 4326) from the swiss CH1903+ / LV95 (EPSG 2056)
 */
// See https://aka.ms/new-console-template for more information

FeatureCollection? ReadGeoJson(string path)
{
    string str = File.ReadAllText(path);
    var fc = JsonConvert.DeserializeObject<FeatureCollection>(str);

    return fc;
}

double DistanceBetweenPoints(Point a, Point b)
{
    return Math.Sqrt((Math.Pow(a.Coordinates.Latitude - b.Coordinates.Latitude, 2) - Math.Pow(a.Coordinates.Longitude - b.Coordinates.Longitude, 2)));
}

bool PointInPolygon(Point point, MultiPolygon polygon)
{
    //Could use bounding boxes for added efficiency, but they aren't there by default, so would have to build them...
    // IGNORE!
    /*if (polygon.BoundingBoxes.Length > 0)
    {
        
    }*/
    //I am assuming the polygon is a convex hull

    double angleSum = 0;

    /*for (int i = 0; i < polygon.Coordinates.Count - 1; i++)
    {
        angleSum += Math.Acos(Math.);
    }*/
    //Don't need to add angle for first / last, as they should be the same (GeoJSON standard)

    double x = point.Coordinates.Latitude;
    double y = point.Coordinates.Longitude;

    //Console.WriteLine(polygon.Coordinates);
    //Console.WriteLine(polygon.Coordinates.Count);

    //Console.WriteLine(mpoly.Coordinates.First().Coordinates.First().Coordinates.First().Latitude);
    //For now... https://gist.github.com/udoliess/fbcdc2e419a061f7ff3f644d5da638d7 ... too tired to work it out myself
    //I much prefer https://stackoverflow.com/a/4243079/13781067 but whatever for now
    IEnumerable<double> px = polygon.Coordinates[0].Coordinates.First().Coordinates.Select(o => o.Latitude);
    IEnumerable<double> py = polygon.Coordinates[0].Coordinates.First().Coordinates.Select(o => o.Longitude);

    var p = px.Zip(py, (x_, y_) => new { x = x_, y = y_ });
    int res = p.Zip(p.Skip(1).Concat(p), (a, b) =>
    {
        if (a.y == y && b.y == y)
            return (a.x <= x && x <= b.x) || (b.x <= x && x <= a.x) ? 0 : 1;
        return a.y <= b.y ?
            y <= a.y || b.y < y ? 1 : Math.Sign((a.x - x) * (b.y - y) - (a.y - y) * (b.x - x)) :
            y <= b.y || a.y < y ? 1 : Math.Sign((b.x - x) * (a.y - y) - (b.y - y) * (a.x - x));
    }).Aggregate(-1, (r, v) => r * v);
    return res >= 0;
}

FeatureCollection PerformMerge(FeatureCollection polygons, FeatureCollection points)
{
    FeatureCollection result = new();

    Console.WriteLine(string.Join(Environment.NewLine, polygons.Features[0].Properties));

    Console.WriteLine(points.Features[0].Geometry);
    Console.WriteLine(polygons.Features[0].Geometry);

    Console.WriteLine(points.Features.Count);
    Console.WriteLine(polygons.Features.Count);

    var mpoly = polygons.Features[0].Geometry as MultiPolygon;
    Console.WriteLine(mpoly.Coordinates);
    Console.WriteLine(mpoly.Coordinates.Count);
    Console.WriteLine(mpoly.Coordinates.First().Coordinates);
    Console.WriteLine(mpoly.Coordinates.First().Coordinates.Count);
    Console.WriteLine(mpoly.Coordinates.First().Coordinates.ToString());
    Console.WriteLine(mpoly.Coordinates.First().Coordinates.First().Coordinates.First().Latitude);

    //foreach (var point in points.Features)
    for (int i = 0; i < points.Features.Count; i++)
    {
        var point = points.Features[i];

        if (i % ((int)(polygons.Features.Count / 10)) == 0)
        {
            Console.WriteLine($"        {(int)(i / (polygons.Features.Count / 10)) * 10}%");
        }

        //Console.WriteLine(point);
        //Console.WriteLine(point.Type);

        //if (point.Type != GeoJSONObjectType.Point) continue;

        bool alreadyFound = false;

        //foreach (var polygon in polygons.Features)
        for (int h = 0; h < polygons.Features.Count; h++)
        {
            var polygon = polygons.Features[h];
            //Console.WriteLine(polygon.Type);
            //if (polygon.Type != GeoJSONObjectType.Polygon) continue;

            if (PointInPolygon(point.Geometry as Point, polygon.Geometry as MultiPolygon))
            {
                //We have found a match and can continue to the next point after adding it to the results
                Feature ZZ = new Feature(point.Geometry, new Dictionary<string, object>()
                {
                    ["UUID"] = point.Properties["UUID"],
                    ["PLZ"] = polygon.Properties["PLZ"],
                });
                result.Features.Add(ZZ);

                //To make sure this works... iterate over all of them, can optimize with this later once I am sure that this is
                //There seems to be 9 points that intersect multiple communes
                //Which isn't enough of an issue out of 4k+

                if (alreadyFound)
                {
                    Console.WriteLine("Already had intersect thingy");
                }
                alreadyFound = true;
                break;
            }
        }
        if (!alreadyFound)
        {
            Console.WriteLine("No intersections found :(");
        }
        alreadyFound = false;
    }

    return result;
}

void WriteOutput(FeatureCollection result)
{
    //Finally, write results to new file
    string output = "C:\\Users\\sasha\\Downloads\\output.json";
    string resStr = JsonConvert.SerializeObject(result);

    using (StreamWriter outputFile = new(output))
    {
        outputFile.WriteLine(resStr);
    }
}


Stopwatch stopWatch = new Stopwatch();
stopWatch.Start();

//FeatureCollection? osnamepos = ReadGeoJson("C:\\Users\\sasha\\Downloads\\PLZO\\PLZO_OSNAMEPOS.json"); //Has 4026 objects
//FeatureCollection? plz = ReadGeoJson("C:\\Users\\sasha\\Downloads\\PLZO\\PLZO_PLZ.json");   //Has 4126 objects

FeatureCollection? osnp = ReadGeoJson("C:\\Users\\sasha\\Desktop\\osnp.geojson"); //Has 4026 objects
FeatureCollection? plz = ReadGeoJson("C:\\Users\\sasha\\Desktop\\plz.geojson");   //Has 4126 objects

if (osnp == null || plz == null)
{
    throw new Exception("Input files could not be read");
}

FeatureCollection result = PerformMerge(plz, osnp);

result.CRS = osnp.CRS;

WriteOutput(result);

stopWatch.Stop();

TimeSpan ts = stopWatch.Elapsed;

// Format and display the TimeSpan value.
string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
    ts.Hours, ts.Minutes, ts.Seconds,
    ts.Milliseconds / 10);
Console.WriteLine("RunTime " + elapsedTime);