namespace MyDictionary
{
    using MyDictionary.Interfaces;

    public class User : IUser
    {
        public User(string name)
        {
            this.Name = name;
        }

        public User(IUser user)
        {
            this.Name = user.Name;
        }

        public string Name { get; private set; }

        public object Clone()
        {
            return new User(this);
        }
    }
}
