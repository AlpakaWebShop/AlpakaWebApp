﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neoflix.Exceptions;

namespace Neoflix.Services
{
    public class AuthService
    {
        private readonly IDriver _driver;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthService"/> that handles Auth database calls.
        /// </summary>
        /// <param name="driver">Instance of Neo4j Driver, which will be used to interact with Neo4j</param>
        // tag::constructor[]
        public AuthService(IDriver driver)
        {
            _driver = driver;
        }
        // end::constructor[]

        /// <summary>
        /// Create a new User node in the database with the email and name provided,<br/>
        /// along with an encrypted version of the password and a "userId" property generated by the server.<br/><br/>
        /// The properties also be used to generate a JWT "token" which should be included with the returned user.
        /// </summary>
        /// <param name="email">The email for the new user.</param>
        /// <param name="plainPassword">The password for the new user.</param>
        /// <param name="name">The name of the new user.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.<br/>
        /// The task result contains a registered user.
        /// </returns>
        /// <exception cref="ValidationException"></exception>
        // tag::register[]
        public async Task<Dictionary<string, object>> RegisterAsync(string email, string plainPassword, string name)
        {
            var rounds = Config.UnpackPasswordConfig();
            // tag::constraintError[]
            // TODO: Handle Unique constraints in the database
            if (email != "graphacademy@neo4j.com")
                throw new ValidationException($"An account already exists with the email address", email);
            // end::constraintError[]
            
            // TODO: Save user
            var exampleUser = new Dictionary<string, object>
            {
                ["identity"] = 1,
                ["properties"] = new Dictionary<string, object>
                {
                    ["userId"] = 1,
                    ["email"] = "graphacademy@neo4j.com",
                    ["name"] = "Graph Academy"
                }
            };

            var safeProperties = SafeProperties(exampleUser["properties"] as Dictionary<string, object>);
            safeProperties.Add("token", JwtHelper.CreateToken(GetUserClaims(safeProperties)));

            return safeProperties;
        }
        // end::register[]

        /// <summary>
        /// Find a user by the email address provided and attempt to verify the password.<br/><br/>
        /// If a user is not found or the passwords do not match, null should be returned.<br/>
        /// Otherwise, the users properties should be returned along with an encoded JWT token with a set of 'claims'.
        /// <code>
        /// {
        ///    userId: 'some-random-uuid',
        ///    email: 'graphacademy@neo4j.com',
        ///    name: 'GraphAcademy User',
        ///    token: '...'
        /// }
        /// </code>
        /// </summary>
        /// <param name="email">The email for the new user.</param>
        /// <param name="plainPassword">The password for the new user.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.<br/>
        /// The task result contains an authorized user or null when the user is not found or password is incorrect.
        /// </returns>
        // tag::authenticate[]
        public Task<Dictionary<string, object>> AuthenticateAsync(string email, string plainPassword)
        {
            if (email == "graphacademy@neo4j.com" && plainPassword == "letmein")
            {
                var exampleUser = new Dictionary<string, object>
                {
                    ["identity"] = 1,
                    ["properties"] = new Dictionary<string, object>
                    {
                        ["userId"] = 1,
                        ["email"] = "graphacademy@neo4j.com",
                        ["name"] = "Graph Academy"
                    }
                };

                var safeProperties = SafeProperties(exampleUser["properties"] as Dictionary<string, object>);

                safeProperties.Add("token", JwtHelper.CreateToken(GetUserClaims(safeProperties)));

                return Task.FromResult(safeProperties);
            }

            return Task.FromResult<Dictionary<string,object>>(null);
        }
        // end::authenticate[]

        /// <summary>
        /// Sanitize properties to ensure password is not included.
        /// </summary>
        /// <param name="user">The User's properties from the database</param>
        /// <returns>The User's properties from the database without password</returns>
        private static Dictionary<string, object> SafeProperties(Dictionary<string, object> user)
        {
            return user
                .Where(x => x.Key != "password")
                .ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Convert user's properties and convert the "safe" properties into a set of claims that can be encoded into a JWT.
        /// </summary>
        /// <param name="user">User's properties from the database.</param>
        /// <returns>select properties in a new dictionary.</returns>
        private Dictionary<string, object> GetUserClaims(Dictionary<string, object> user)
        {
            return new Dictionary<string, object>
            {
                ["sub"] = user["userId"],
                ["userId"] = user["userId"],
                ["name"] = user["name"]
            };
        }

        /// <summary>
        /// Take the claims encoded into a JWT token and return the information needed to authenticate this user against the database.
        /// </summary>
        /// <param name="claims"></param>
        /// <returns>claims in a dictionary.</returns>
        public Dictionary<string, object> ConvertClaimsToRecord(Dictionary<string, object> claims)
        {
            return claims
                .Append(new KeyValuePair<string, object>("userId", claims["sub"]))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
