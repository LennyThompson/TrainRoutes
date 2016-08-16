using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace TrainRouting
{
    public class RouteBuilder
    {
        private static Regex ROUTE_REGEX = new Regex("(\\s*(.)(.)(\\d+),?)");
        public static TrainRouter buildRoute(TextReader readRoutes, List<string> listErrorReporter)
        {
            string strRoute;
            TrainRouter trainRouter = new TrainRouter();
            while ((strRoute = readRoutes.ReadLine()) != null)
            {
                Match matchRoute = ROUTE_REGEX.Match(strRoute);
                while (matchRoute.Success)
                {
                    try
                    {
                        trainRouter.addRoute(new Destination(matchRoute.Groups[2].ToString()), new Destination(matchRoute.Groups[3].ToString()), Int32.Parse(matchRoute.Groups[4].ToString()));
                    }
                    catch(FormatException exc)
                    {
                        listErrorReporter.Add("Format of route " + strRoute + " is incorrect, " + exc.Message);
                    }
                    catch(ArgumentException )
                    {
                        listErrorReporter.Add("A route of " + strRoute + " already exists");
                    }
                    catch (Exception excDegenerateRoute)
                    {
                        listErrorReporter.Add(excDegenerateRoute.Message);
                    }
                    matchRoute = matchRoute.NextMatch();
                }
                if (Enumerable.Count(trainRouter.Destinations) == 0)
                {
                    listErrorReporter.Add("Unable to match " + strRoute + " as a route");
                }
            }
            return trainRouter;
        }
    }
}
