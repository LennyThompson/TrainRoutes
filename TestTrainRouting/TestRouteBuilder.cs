using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

using TrainRouting;
using System.Collections.Generic;

namespace TestTrainRouting
{
    [TestClass]
    public class TestRouteBuilder
    {
        [TestMethod]
        public void TestReadSimpleInput()
        {
            string strInputRoute = "AB5";
            TextReader readRoutes = new StringReader(strInputRoute);

            List<string> listErrors = new List<string>();
            TrainRouter trainRouter = RouteBuilder.buildRoute(readRoutes, listErrors);

            Assert.IsTrue(listErrors.Count == 0);
            Assert.AreEqual(2, Enumerable.Count(trainRouter.Destinations));
        }

        [TestMethod]
        public void TestExtendedInput()
        {
            string strInputRoute = "AB5, BC4";
            TextReader readRoutes = new StringReader(strInputRoute);

            List<string> listErrors = new List<string>();
            TrainRouter trainRouter = RouteBuilder.buildRoute(readRoutes, listErrors);

            Assert.IsTrue(listErrors.Count == 0);
            Assert.AreEqual(3, Enumerable.Count(trainRouter.Destinations));
        }

        [TestMethod]
        public void TestAllInput()
        {
            string strInputRoute = "AB5, BC4, CD8, DC8, DE6, AD5, CE2, EB3, AE7";
            TextReader readRoutes = new StringReader(strInputRoute);

            List<string> listErrors = new List<string>();
            TrainRouter trainRouter = RouteBuilder.buildRoute(readRoutes, listErrors);

            Assert.IsTrue(listErrors.Count == 0);
            Assert.AreEqual(5, Enumerable.Count(trainRouter.Destinations));
        }

        [TestMethod]
        public void TestIllegalInput()
        {
            string strInputRoute = "AB5, BC4, CD8, AB10, DE6, AD5, CC2, EB3, AE7";
            TextReader readRoutes = new StringReader(strInputRoute);

            List<string> listErrors = new List<string>();
            TrainRouter trainRouter = RouteBuilder.buildRoute(readRoutes, listErrors);

            Assert.IsTrue(listErrors.Count == 2);
            Assert.AreEqual(5, Enumerable.Count(trainRouter.Destinations));
        }

        [TestMethod]
        public void TestFindRoute()
        {
            string strInputRoute = "AB5, BC4, CD8, DC8, DE6, AD5, CE2, EB3, AE7";
            TextReader readRoutes = new StringReader(strInputRoute);

            List<string> listErrors = new List<string>();
            TrainRouter trainRouter = RouteBuilder.buildRoute(readRoutes, listErrors);

            Assert.AreEqual(5, Enumerable.Count(trainRouter.Destinations));
            Assert.IsTrue(trainRouter.findRoute("A-B-C"));
            Assert.IsFalse(trainRouter.findRoute("A-E-A"));
        }

        [TestMethod]
        public void TestMeasureRoute()
        {
            string strInputRoute = "AB5, BC4, CD8, DC8, DE6, AD5, CE2, EB3, AE7";
            TextReader readRoutes = new StringReader(strInputRoute);

            List<string> listErrors = new List<string>();
            TrainRouter trainRouter = RouteBuilder.buildRoute(readRoutes, listErrors);

            Assert.AreEqual(5, Enumerable.Count(trainRouter.Destinations));
            Assert.AreEqual(9, trainRouter.measureRoute("A-B-C"));
            Assert.AreEqual(5, trainRouter.measureRoute("A-D"));
            Assert.AreEqual(13, trainRouter.measureRoute("A-D-C"));
            Assert.AreEqual(22, trainRouter.measureRoute("A-E-B-C-D"));
            Assert.AreEqual(-1, trainRouter.measureRoute("A-E-D"));
        }

        [TestMethod]
        public void TestFindRoutesCC()
        {
            string strInputRoute = "AB5, BC4, CD8, DC8, DE6, AD5, CE2, EB3, AE7";
            TextReader readRoutes = new StringReader(strInputRoute);

            List<string> listErrors = new List<string>();
            TrainRouter trainRouter = RouteBuilder.buildRoute(readRoutes, listErrors);

            List<List<Route>> listAllRoutes = new List<List<Route>>();
            Func<string, Route, List<Route>, bool> fnTestRoute = (finalDest, route, listRoute) =>
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
            };
            trainRouter.findAllRoutes("C-C", fnTestRoute);
            Assert.IsTrue(listAllRoutes.Count >= 3);
            List<List<Route>> listShortRoutes = listAllRoutes.Where(route => route.Count <= 3).ToList();
            Assert.AreEqual(2, listShortRoutes.Count);
        }

        [TestMethod]
        public void TestFindRoutesAC()
        {
            string strInputRoute = "AB5, BC4, CD8, DC8, DE6, AD5, CE2, EB3, AE7";
            TextReader readRoutes = new StringReader(strInputRoute);

            List<string> listErrors = new List<string>();
            TrainRouter trainRouter = RouteBuilder.buildRoute(readRoutes, listErrors);
            List<List<Route>> listAllRoutes = new List<List<Route>>();
            int nMaxStops = 4;
            Func<string, Route, List<Route>, bool> fnTestRoute = (finalDest, route, listRoute) => 
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
                            };
            trainRouter.findAllRoutes("A-C", fnTestRoute);
            Assert.IsTrue(listAllRoutes.Count > 0);
            List<List<Route>> listSelectRoutes = listAllRoutes.Distinct().Where(route => route.Count == 4).ToList();
            Assert.AreEqual(3, listSelectRoutes.Count);

            nMaxStops = 10;
            listAllRoutes.Clear();
            trainRouter.findAllRoutes("B-B", fnTestRoute);
            Assert.IsTrue(listAllRoutes.Count > 0);
            listSelectRoutes = listAllRoutes.Distinct().Where(route => route.Count == 4).ToList();
            Assert.AreEqual(1, listSelectRoutes.Count);
        }

        [TestMethod]
        public void TestFindShortestRouteAC()
        {
            string strInputRoute = "AB5, BC4, CD8, DC8, DE6, AD5, CE2, EB3, AE7";
            TextReader readRoutes = new StringReader(strInputRoute);

            List<string> listErrors = new List<string>();
            TrainRouter trainRouter = RouteBuilder.buildRoute(readRoutes, listErrors);
            int nShortestRoute = Int32.MaxValue;
            Func<string, Route, List<Route>, bool> fnTestRoute = (finalDest, route, listRoute) =>
            {
                if(listRoute.Where(routePart => routePart.To.Name.CompareTo(route.To.Name) == 0).Count() > 0)
                {
                    return false;
                }
                listRoute.Add(route);
                int nRouteLength = listRoute.Select(routePart => routePart.Distance).Sum();
                if(nRouteLength >= nShortestRoute)
                {
                    return false;
                }
                if(route.To.Name.CompareTo(finalDest) == 0)
                {
                    if(nShortestRoute > nRouteLength)
                    {
                        nShortestRoute = nRouteLength;
                        return false;
                    }
                }
                return true;
            };
            trainRouter.findAllRoutes("A-C", fnTestRoute);
            Assert.IsTrue(nShortestRoute < Int32.MaxValue);
            Assert.AreEqual(9, nShortestRoute);

            nShortestRoute = Int32.MaxValue;
            trainRouter.findAllRoutes("B-B", fnTestRoute);
            Assert.IsTrue(nShortestRoute < Int32.MaxValue);
            Assert.AreEqual(9, nShortestRoute);
        }

        [TestMethod]
        public void TestFindRouteCountCC()
        {
            string strInputRoute = "AB5, BC4, CD8, DC8, DE6, AD5, CE2, EB3, AE7";
            TextReader readRoutes = new StringReader(strInputRoute);

            List<string> listErrors = new List<string>();
            TrainRouter trainRouter = RouteBuilder.buildRoute(readRoutes, listErrors);
            int nRouteCount = 0;
            Func<string, Route, List<Route>, bool> fnTestRoute = (finalDest, route, listRoute) =>
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
            };
            trainRouter.findAllRoutes("C-C", fnTestRoute);
            Assert.AreEqual(7, nRouteCount);
        }
    }
}
