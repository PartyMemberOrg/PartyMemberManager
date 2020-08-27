using PartyMemberManager.Dal;
using PartyMemberManager.Dal.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PartyMemberManager.LoggerProviders
{
    public class LoggerDatabaseProvider : ILoggerProvider
    {
        IServiceScope _serviceScope = null;
        public LoggerDatabaseProvider(IServiceProvider serviceProvider)
        {
            _serviceScope = serviceProvider.CreateScope();

        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(categoryName, _serviceScope);
        }

        public void Dispose()
        {
        }

        public class Logger : ILogger
        {
            private readonly string _categoryName;
            private readonly IServiceScope _serviceScope;
            private readonly PMContext _context;

            public Logger(string categoryName, IServiceScope serviceScope)
            {
                _serviceScope = serviceScope;
                _categoryName = categoryName;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Critical || logLevel == LogLevel.Error || logLevel == LogLevel.Warning)
                    RecordMsg(logLevel, eventId, state, exception, formatter);
            }

            private void RecordMsg<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                using (PMContext _context = new PMContext(_serviceScope.ServiceProvider.GetRequiredService<DbContextOptions<PMContext>>()))
                {
                    _context.Logs.Add(new Log
                    {
                        Id = Guid.NewGuid(),
                        CreateTime = DateTime.Now,
                        Ordinal = 0,
                        OperatorId = null,
                        LogLevel = logLevel.ToString(),
                        CategoryName = _categoryName,
                        Message = formatter(state, exception),
                        User = "System"
                    });
                    _context.SaveChanges();
                }
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return new NoopDisposable();
            }

            private class NoopDisposable : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}
