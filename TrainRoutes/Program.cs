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

            writeQuestionResponse
                (
                    trainRouter.findAllRoutesAlt("C-C", routeList => routeList.Count <= 3)
                    .Where(routeList => routeList.Count <= 3).Count(),
                    ++nQuestion
                );

            // Question 7

            int nMaxStops = 4;
            writeQuestionResponse
                (
                    trainRouter.findAllRoutesAlt("A-C", routeList => routeList.Count <= nMaxStops)
                    .Where(routeList => routeList.Count == nMaxStops).Count(),
                    ++nQuestion
                );

            // Question 8

            int nShortestRoute = Int32.MaxValue;
            Func<List<Route>, bool> fnPredicate = routeList =>
            {
                if (routeList.Select(route => route.Distance).Sum() < nShortestRoute)
                {
                    nShortestRoute = routeList.Select(route => route.Distance).Sum();
                    return true;
                }
                return false;
            };
            trainRouter.findAllRoutesAlt("A-C", fnPredicate)
                            .Distinct()
                            .Count();
            writeQuestionResponse(nShortestRoute, ++nQuestion);

            // Question 9

            nShortestRoute = Int32.MaxValue;
            trainRouter.findAllRoutesAlt("B-B", fnPredicate)
                            .Distinct()
                            .Count();
            writeQuestionResponse(nShortestRoute, ++nQuestion);

            // Question 10

            writeQuestionResponse
                (
                    trainRouter.findAllRoutesAlt("C-C", routeList => routeList.Select(route => route.Distance).Sum() < 30)
                    .Where(routeList => routeList.Select(route => route.Distance).Sum() < 30)
                    .Distinct()
                    .Count(), 
                    ++nQuestion
                );

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
