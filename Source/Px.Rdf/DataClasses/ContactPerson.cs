namespace Px.Dcat.DataClasses
{
    public class ContactPerson
    {
        public string Resource;
        public string Name;
        public string Email;
        public string Phone;

        public override bool Equals(object obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;
            ContactPerson cp = (ContactPerson)obj;
            return Email == cp.Email;
        }

        public override int GetHashCode()
        {
            return Email.GetHashCode();
        }
    }
}
