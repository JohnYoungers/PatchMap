using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Data
{
    public class LogFormatter : DatabaseLogFormatter
    {
        public LogFormatter(ExampleContext context, Action<string> writeAction)
            : base(context, writeAction)
        {
        }

        public override void LogResult<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
            StringBuilder log = new StringBuilder(Environment.NewLine);
            WriteCommand(log, command);

            if (interceptionContext.Exception != null)
            {
                log.AppendLine("--Failed: " + interceptionContext.Exception.Message + Environment.NewLine);
            }
            else if (interceptionContext.TaskStatus.HasFlag(TaskStatus.Canceled))
            {
                log.AppendLine("--Cancelled after  " + GetStopwatch(interceptionContext).ElapsedMilliseconds + " milliseconds");
            }
            else
            {
                log.AppendLine("--Completed after " + GetStopwatch(interceptionContext).ElapsedMilliseconds + " milliseconds");
            }

            Write(log.ToString());
        }

        public void WriteCommand(StringBuilder log, DbCommand command)
        {
            if (command.Parameters != null && command.Parameters.Count > 0)
            {
                foreach (var parameter in command.Parameters.OfType<DbParameter>())
                {
                    log.Append("DECLARE ")
                       .Append((parameter.ParameterName[0] == '@') ? parameter.ParameterName : "@" + parameter.ParameterName)
                       .Append(" ")
                       .Append(parameter.DbType)
                       .Append(" = ")
                       .Append((parameter.Value == null || parameter.Value == DBNull.Value) ? "null" : "'" + parameter.Value + "'")
                       .Append(Environment.NewLine);
                }
            }

            log.AppendLine(command.CommandText);
        }

        public override void LogCommand<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext) { }
        public override void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext) { }
        public override void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext) { }
    }
}
