using System.Collections.Generic;

namespace Px.Dcat.DataClasses
{
    public class Organization
    {
        public HashSet<(string, string)> Names; // (language, name)
        public string Resource;

        public override bool Equals(object obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;
            Organization other = (Organization)obj;
            return Names.SetEquals(other.Names);
        }

        public override int GetHashCode()
        {
            return Names.GetHashCode();
        }
    }
}
