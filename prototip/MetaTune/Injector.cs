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

            string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                ?? throw new EnvVariableNotFoundException("Environment variable not found", "POSTGRES_CONNECTION_STRING");

            Database db = new Database(connectionString);
            _implementations.Add(typeof(Database), db);

            //// Add storage implementations
            _implementations.Add(typeof(IUserStorage), new UserStorage(db));
            //_implementations.Add(typeof(IStudentStorage), new StudentStorage(db));
            //_implementations.Add(typeof(IDirectorStorage), new DirectorStorage(db));
            //_implementations.Add(typeof(IProfessorStorage), new ProfessorStorage(db));
            //_implementations.Add(typeof(IEnrollmentStorage), new EnrollmentStorage(db));

            //_implementations.Add(typeof(IStudentViewStorage), new StudentViewStorage(db));

            //_implementations.Add(typeof(ICourseStorage), new CourseStorage(db));

            //// Search storage for professors
            //_implementations.Add(typeof(IProfessorSearchStorage), new ProfessorSearchStorage(db));

            //// Search storage for exams
            //_implementations.Add(typeof(IExamSearchStorage), new ExamSearchStorage(db));

            //// Genre storage
            //_implementations.Add(typeof(ILanguageStorage), new LanguageStorage(db));

            //// Exam storage 
            //_implementations.Add(typeof(IExamStorage), new ExamStorage(db));

            ////Penalty storage
            //_implementations.Add(typeof(IPenaltyStorage), new PenaltyStorage(db));

            //_implementations.Add(typeof(ITakingStorage), new TakingStorage(db));

            //// Mail Service
            //_implementations.Add(typeof(IEmailService), EmailService.FromEnv());

            //// Report A Storage
            //_implementations.Add(typeof(IReportAStorage), new ReportAStorage(db));

            //// Report B Storage
            //_implementations.Add(typeof(IReportBStorage), new ReportBStorage(db));

            //// Report C Storage
            //_implementations.Add(typeof(IReportCStorage), new ReportCStorage(db));

            //// Report D Storage
            //_implementations.Add(typeof(IReportDStorage), new ReportDStorage(db));


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
