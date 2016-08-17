using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TrainRouting
{
    // A destination on the network - has a name and set of routes to destinations reachable from this destination
    public class Destination
    {
        public Destination(string strName)
        {
            Name = strName;
            Routes = new Dictionary<string, Route>();
        }

        public string Name
        {
            get;
            set;
        }

        public Dictionary<string, Route> Routes
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            if (obj is Destination)
            {
                return this.Name.CompareTo(((Destination)obj).Name) == 0;
            }
            return false;
        }

        public void addRoute(Destination destTo, int nDistance)
        {
            Routes.Add(destTo.Name, new Route(destTo, nDistance));
        }

        public bool hasRoute(IEnumerator<string> itDestination)
        {
            // Are we there yet?
            if(itDestination.MoveNext())
            {
                if (Routes.ContainsKey(itDestination.Current))
                {
                    return Routes[itDestination.Current].To.hasRoute(itDestination);
                }
                // Cant find a way

                return false;
            }
            // Weve arrived...
            return true;
        }

        private static int DEPTH_LIMIT = 50;
        public bool hasRouteTo(Destination destTo, int nDepth)
        {
            if (Routes.ContainsKey(destTo.Name))
            {
                return true;
            }

            return Routes.Where(route => nDepth < DEPTH_LIMIT && route.Value.To.hasRouteTo(destTo, nDepth + 1)).Count() > 0;
        }

        internal int measureRoute(IEnumerator<string> itDestination, int nTotalDistance)
        {
            // Are we there yet?
            if (itDestination.MoveNext())
            {
                if (Routes.ContainsKey(itDestination.Current))
                {
                    return Routes[itDestination.Current].To.measureRoute(itDestination, nTotalDistance + Routes[itDestination.Current].Distance);
                }
                // Cant find a way

                return -1;
            }
            // Weve arrived...
            return nTotalDistance;
        }

        internal void findAllRoutes(string strDestination, List<Route> listRouteToHere, Func<string, Route, List<Route>, bool> fnTestRoute)
        {
            foreach(KeyValuePair<string, Route> keyPair in Routes)
            {
                List<Route> listClone = listRouteToHere.Select(route => route).ToList();
                if (fnTestRoute(strDestination, keyPair.Value, listClone))
                {
                    keyPair.Value.To.findAllRoutes(strDestination, listClone, fnTestRoute);
                }
            }
        }

        internal IEnumerable<List<Route>> findAllRoutesAlt(Destination destTo, Func<List<Route>, bool> routePredicate)
        {
            return Routes.Select(keyPair => keyPair.Value)
                .SelectMany(route => route.buildRouteTo(destTo, new List<Route>(), routePredicate));
        }
    }

    // Route - simply a distance to a destination
    public class Route
    {
        public Route(Destination destTo, int nDistance)
        {
            To = destTo;
            Distance = nDistance;
        }

        public Destination To
        {
            get;
            set;
        }

        public int Distance
        {
            get;
            set;
        }

        private static int DEPTH_LIMIT = 50;
        public bool hasRouteTo(Destination destTo, int nDepth)
        {
            if (To == destTo)
            {
                return true;
            }

            return To.Routes.Select(keyPair => keyPair.Value).Where(route => nDepth < DEPTH_LIMIT && route.hasRouteTo(destTo, nDepth + 1)).Count() > 0;
        }

        public List<List<Route>> buildRouteTo(Destination destTo, List<Route> listRouteToHere, Func<List<Route>, bool> routePredicate)
        {
            List<List<Route>> listRoutes = new List<List<Route>>();
            listRouteToHere.Add(this);
            if (To == destTo)
            {
                listRoutes.Add(listRouteToHere);
                if(!routePredicate(listRouteToHere))
                {
                    return listRoutes;
                }
            }

            if (hasRouteTo(destTo, listRouteToHere.Count))
            {
                listRoutes.AddRange(To.Routes.Select(keyPair => keyPair.Value)
                .Where(route => listRouteToHere.Count < DEPTH_LIMIT && route.hasRouteTo(destTo, listRouteToHere.Count))
                .SelectMany
                (
                    route =>
                        route.buildRouteTo(destTo, listRouteToHere.Select(routeToHere => routeToHere).ToList(), routePredicate)
                )
                .ToList());
            }
            return listRoutes;
        }
    }

    // Container for Destination and Route objects, and searching routes.
    public class TrainRouter
    {
        public TrainRouter()
        {
            m_mapRoutes = new Dictionary<string, Destination>();
        }

        public void addRoute(Destination destFrom, Destination destTo, int nDistance)
        {
            if(destFrom.Name.CompareTo(destTo.Name) == 0)
            {
                throw new Exception("Degenerate route not permitted: source " + destFrom.Name + " destination " + destTo.Name + " distance: " + nDistance);
            }
            if(!m_mapRoutes.ContainsKey(destFrom.Name))
            {
                m_mapRoutes.Add(destFrom.Name, destFrom);
            }
            if (!m_mapRoutes.ContainsKey(destTo.Name))
            {
                m_mapRoutes.Add(destTo.Name, destTo);
            }
            m_mapRoutes[destFrom.Name].addRoute(m_mapRoutes[destTo.Name], nDistance);
        }

        public IEnumerable<Destination> Destinations
        {
            get
            {
                return from keyPair in m_mapRoutes select keyPair.Value;
            }
        }

        // Find a specific route in the form of "X-Y-Z"
        public bool findRoute(string strRouteDescription)
        {
            IEnumerable<string> listDestinations = destinationsFromString(strRouteDescription);
            if(Enumerable.Count(listDestinations) > 0)
            {
                IEnumerator<string> itDestination = listDestinations.GetEnumerator();
                
                if 
                (
                    itDestination.MoveNext()
                    &&
                    m_mapRoutes.ContainsKey(itDestination.Current)
                )
                {
                    return m_mapRoutes[itDestination.Current].hasRoute(itDestination);
                }
            }
            return false;
        }

        // Measure a specific route
        public int measureRoute(string strRouteDescription)
        {
            IEnumerable<string> listDestinations = destinationsFromString(strRouteDescription);
            if (Enumerable.Count(listDestinations) > 0)
            {
                IEnumerator<string> itDestination = listDestinations.GetEnumerator();

                if
                (
                    itDestination.MoveNext()
                    &&
                    m_mapRoutes.ContainsKey(itDestination.Current)
                )
                {
                    return m_mapRoutes[itDestination.Current].measureRoute(itDestination, 0);
                }
            }
            return -1;
        }

        // Find all routes that satisfy the test function - essentially a visitor

        public void findAllRoutes(string strRouteDescription, Func<string, Route, List<Route>, bool> fnTestRoute)
        {
            IEnumerable<string> listDestinations = destinationsFromString(strRouteDescription);
            if (Enumerable.Count(listDestinations) > 0)
            {
                IEnumerator<string> itDestination = listDestinations.GetEnumerator();

                if
                (
                    itDestination.MoveNext()
                    &&
                    m_mapRoutes.ContainsKey(itDestination.Current)
                )
                {
                    string strStart = itDestination.Current;
                    if (itDestination.MoveNext())
                    {
                        List<Route> listRoute = new List<Route>();
                        m_mapRoutes[strStart].findAllRoutes(itDestination.Current, listRoute, fnTestRoute);
                    }
                }
            }
        }

        // Find all routes that staisfy the predicate - simpler implementation than findAllRoutes

        public IEnumerable<List<Route>> findAllRoutesAlt(string strRouteDescription, Func<List<Route>, bool> routePredicate)
        {
            List<List<Route>> listAllRoutes = new List<List<Route>>();
            IEnumerable<string> listDestinations = destinationsFromString(strRouteDescription);
            if (Enumerable.Count(listDestinations) > 0)
            {
                IEnumerator<string> itDestination = listDestinations.GetEnumerator();

                if
                (
                    itDestination.MoveNext()
                    &&
                    m_mapRoutes.ContainsKey(itDestination.Current)
                )
                {
                    Destination destStart = m_mapRoutes[itDestination.Current];
                    if 
                    (
                        itDestination.MoveNext()
                        &&
                        m_mapRoutes.ContainsKey(itDestination.Current)
                    )
                    {
                        return destStart.findAllRoutesAlt(m_mapRoutes[itDestination.Current], routePredicate);
                    }
                }
            }
            return listAllRoutes;
        }


        private static IEnumerable<string> destinationsFromString(string strRouteDescription)
        {
            Regex regexDestinationSearch = new Regex("((.)-?)");
            Match matchDestinations = regexDestinationSearch.Match(strRouteDescription);
            List<string> listDestinations = new List<string>();
            while(matchDestinations.Success)
            {
                listDestinations.Add(matchDestinations.Groups[2].ToString());
                matchDestinations = matchDestinations.NextMatch();
            }
            return listDestinations.ToArray();
        }

        private Dictionary<string, Destination> m_mapRoutes;

        public bool hasRouteTo(string strRouteDescription)
        {
            IEnumerable<string> listDestinations = destinationsFromString(strRouteDescription);
            if (Enumerable.Count(listDestinations) > 0)
            {
                IEnumerator<string> itDestination = listDestinations.GetEnumerator();

                if
                (
                    itDestination.MoveNext()
                    &&
                    m_mapRoutes.ContainsKey(itDestination.Current)
                )
                {
                    Destination destStart = m_mapRoutes[itDestination.Current];
                    if
                    (
                        itDestination.MoveNext()
                        &&
                        m_mapRoutes.ContainsKey(itDestination.Current)
                    )
                    {
                        return destStart.hasRouteTo(m_mapRoutes[itDestination.Current], 1);
                    }
                }
            }
            return false;
        }
    }
}
