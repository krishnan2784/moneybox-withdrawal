using System;

namespace Moneybox.App.Tests.Builders
{
    public class UserBuilder
    {
        Guid id = Guid.NewGuid();
        string name = "";
        string email = "";

        public static implicit operator User(UserBuilder instance)
        {
            return instance.Build();
        }

        public User Build()
        {
            return new User(id, name, email);
        }

        public UserBuilder WithId(Guid id)
        {
            this.id = id;
            return this;
        }

        public UserBuilder WithName(string name)
        {
            this.name = name;
            return this;
        }

        public UserBuilder WithEmail(string email)
        {
            this.email = email;
            return this;
        }
    }
}
