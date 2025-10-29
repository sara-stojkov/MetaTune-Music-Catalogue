using Core.Services.EmailService;
using Core.Storage;
using DotNetEnv;
using MetaTune.Services;
using PostgreSQLStorage;
using System;
using System.Collections.Generic;

namespace MetaTune
{
    public class Injector
    {
        private readonly Dictionary<Type, object> _implementations;

        private static Injector? _instance = null;

        private Injector()
        {
            _implementations = new Dictionary<Type, object>();

            // Load environment variables
            Core.Utils.Env.Load();

            _implementations[typeof(IEmailService)] = EmailService.FromEnv();

            AddPostgreSQL();
        }

        private void AddPostgreSQL()
        {

            string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                ?? throw new EnvVariableNotFoundException("Environment variable not found", "POSTGRES_CONNECTION_STRING");

            Database db = new Database(connectionString);
            _implementations[typeof(Database)] = db;

            //// Add storage implementations
            _implementations[typeof(IUserStorage)] = new UserStorage(db);
            _implementations[typeof(IGenreStorage)] = new GenreStorage(db);
        }

        private static Injector GetInstance()
        {
            _instance ??= new Injector();
            return _instance;
        }

        public static T CreateInstance<T>()
        {
            Type type = typeof(T);
            Injector instance = GetInstance();
            if (instance._implementations.TryGetValue(type, out object? value))
            {
                return (T)value;
            }

            throw new ArgumentException($"No implementation found for type {type}");
        }
    }
}
