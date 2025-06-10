using System.Drawing;
using StarCitizenTracker.Interfaces;

namespace StarCitizenTracker.Services
{
    public class LocationIdConverter : IIDConverter
    {
        public string ConvertLocationID(string locationID)
        {
            switch(locationID)
            {
                //------------------------ STANTON ------------------------
                case "85446772": return "Seraphim Station"; //verified

                case "354726507":   return "Orison"; //verified

                case "3725966025":  return "Grim Hex"; //Verified

                case "3839298489":  return "Port Tressler"; //Verified

                case "2065796676":  return "Microtech"; //Verified

                case "1409305106":  return "Everus Harbor"; //Verified

                case "335738819":   return "Lorville"; //Verified

                case "29423974":    return "Baijini Point"; //Verified

                case "392070389":   return "Area18"; //Verified

                case "580138847":   return "Pyro Gateway Station"; //Verified

                //------------------------ PYRO ------------------------

                case "4022511652":  return "Stanton Gateway Station"; //Verified

                case "2703988875":  return "Ruin Station"; //Verified

                case "1720346272":  return "Patch City Station"; //Verified

                case "3515131989":  return "Checkmate Station"; //Verified

                //------------------------ UNKNOWN ------------------------

                case "1311943844":  return "NOT SURE, SHARED WITH SERAPHIM"; //


                //------------------------ DEFAULT ------------------------
                // Just print ID if not listed
                default:            return locationID;
            }
        }
    }
}