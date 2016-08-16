using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TrainRouting
{
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

        public bool hasRouteTo(string strDestination)
        {
            if (Routes.ContainsKey(strDestination))
            {
                return true;
            }

            return Routes.Where(route => hasRouteTo(strDestination)).Count() > 0;
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
    }

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
    }

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

        public void findAllRoutes(string strRouteDescription, Func<string, Route, List<Route>, bool> fnTestRoute)
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
                    string strStart = itDestination.Current;
                    if (itDestination.MoveNext())
                    {
                        List<Route> listRoute = new List<Route>();
                        m_mapRoutes[strStart].findAllRoutes(itDestination.Current, listRoute, fnTestRoute);
                    }
                }
            }
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
    }
}
