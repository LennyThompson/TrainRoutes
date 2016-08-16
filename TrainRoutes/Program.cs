using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainRouting;

using System.IO;

namespace TrainRoutes
{
    class Program
    {
        static void Main(string[] args)
        {
            string strInputRoute = "AB5, BC4, CD8, DC8, DE6, AD5, CE2, EB3, AE7";
            TextReader readRoutes = new StringReader(strInputRoute);

            List<string> listErrors = new List<string>();
            TrainRouter trainRouter = RouteBuilder.buildRoute(readRoutes, listErrors);

            // Get the route distances for questions 1 - 5

            int nQuestion = 0;
            string[] listRoutes = new string[] { "A-B-C", "A-D", "A-D-C", "A-E-B-C-D", "A-E-D" };
            IEnumerable<int> listLengths = listRoutes.Select(route => trainRouter.measureRoute(route));
            foreach(int nLength in listLengths)
            {
                writeQuestionResponse(nLength, ++nQuestion);
            }

            // Question 6

            List<List<Route>> listAllRoutes = new List<List<Route>>();
            trainRouter.findAllRoutes
                (
                    "C-C",
                    (finalDest, route, listRoute) =>
                    {
                        if (listRoute.Count > 3)
                        {
                            return false;
                        }
                        listRoute.Add(route);
                        if (route.To.Name.CompareTo(finalDest) == 0)
                        {
                            listAllRoutes.Add(listRoute);
                        }
                        return true;
                    }
                );
            List<List<Route>> listShortRoutes = listAllRoutes.Where(route => route.Count <= 3).ToList();
            writeQuestionResponse(listShortRoutes.Count, ++nQuestion);

            // Question 7

            int nMaxStops = 4;
            listAllRoutes.Clear();
            trainRouter.findAllRoutes
                (
                    "A-C",
                    (finalDest, route, listRoute) =>
                    {
                        listRoute.Add(route);
                        if (listRoute.Count > nMaxStops)
                        {
                            return false;
                        }
                        if (route.To.Name.CompareTo(finalDest) == 0)
                        {
                            listAllRoutes.Add(listRoute);
                        }
                        return true;
                    }
                );
            List<List<Route>> listSelectRoutes = listAllRoutes.Distinct().Where(route => route.Count == nMaxStops).ToList();
            writeQuestionResponse(listSelectRoutes.Count, ++nQuestion);

            // Question 8

            int nShortestRoute = Int32.MaxValue;
            trainRouter.findAllRoutes
                (
                    "A-C",
                    (finalDest, route, listRoute) =>
                    {
                        if (listRoute.Where(routePart => routePart.To.Name.CompareTo(route.To.Name) == 0).Count() > 0)
                        {
                            return false;
                        }
                        listRoute.Add(route);
                        int nRouteLength = listRoute.Select(routePart => routePart.Distance).Sum();
                        if (nRouteLength >= nShortestRoute)
                        {
                            return false;
                        }
                        if (route.To.Name.CompareTo(finalDest) == 0)
                        {
                            if (nShortestRoute > nRouteLength)
                            {
                                nShortestRoute = nRouteLength;
                                return false;
                            }
                        }
                        return true;
                    }
                );
            writeQuestionResponse(nShortestRoute, ++nQuestion);

            // Question 9

            nShortestRoute = Int32.MaxValue;
            trainRouter.findAllRoutes
                (
                    "B-B",
                    (finalDest, route, listRoute) =>
                    {
                        if (listRoute.Where(routePart => routePart.To.Name.CompareTo(route.To.Name) == 0).Count() > 0)
                        {
                            return false;
                        }
                        listRoute.Add(route);
                        int nRouteLength = listRoute.Select(routePart => routePart.Distance).Sum();
                        if (nRouteLength >= nShortestRoute)
                        {
                            return false;
                        }
                        if (route.To.Name.CompareTo(finalDest) == 0)
                        {
                            if (nShortestRoute > nRouteLength)
                            {
                                nShortestRoute = nRouteLength;
                                return false;
                            }
                        }
                        return true;
                    }
                );
            writeQuestionResponse(nShortestRoute, ++nQuestion);

            // Question 10

            int nRouteCount = 0;
            trainRouter.findAllRoutes
                (
                    "C-C", 
                    (finalDest, route, listRoute) =>
                    {
                        bool bIsSameRoute = route.To.Name.CompareTo(finalDest) == 0;
                        listRoute.Add(route);
                        int nRouteLength = listRoute.Select(routePart => routePart.Distance).Sum();
                        if (nRouteLength >= 30)
                        {
                            return false;
                        }
                        if (bIsSameRoute)
                        {
                            ++nRouteCount;
                        }
                        return true;
                    }
                );
            writeQuestionResponse(nRouteCount, ++nQuestion);

            Console.Write("Type any character: ");
            Console.ReadKey();
        }

        private static void writeQuestionResponse(int nAnswer, int nQuestionNo)
        {
            if (nAnswer > 0)
            {
                Console.WriteLine("Output#" + nQuestionNo + ": " + nAnswer);
            }
            else
            {
                Console.WriteLine("Output#" + nQuestionNo + ": NO SUCH ROUTE");
            }
        }
    }
}
