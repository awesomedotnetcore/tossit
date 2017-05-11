#if !net451
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Tossit.Core;
using Tossit.WorkQueue.Worker;

namespace Tossit.WorkQueue
{
    /// <summary>
    /// Tossit.WorkQueue.ApplicationBuilderExtensions
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Register workers when the application starts.
        /// </summary>
        /// <remarks>You can do that by IWorkerRegistrar after the application started.</remarks>
        /// <param name="app">IApplicationBuilder</param>
        /// <exception cref="ArgumentNullException">Throws when app is null.</exception>
        public static void UseTossitWorker(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var services = app.ApplicationServices;

            // Get required services.
            var workerRegistrar = services.GetRequiredService<IWorkerRegistrar>();
            var reflectionHelper = services.GetRequiredService<IReflectionHelper>();

            if (workerRegistrar == null || reflectionHelper == null)
            {
                throw new InvalidOperationException("Unable to find the required services. AddTossitWorker() should be called.");
            }

            // Get consumers.
            var consumers = services.GetServices<IConsumer>();
            if (consumers != null)
            {
                // Get workers.
                var workers = reflectionHelper.FilterObjectsByInterface(consumers, typeof(IWorker<>));

                // Register workers.
                foreach (var worker in workers)
                {
                    // Maybe, register all of them on servicecollectionextension and than register here?
                    reflectionHelper.InvokeGenericMethod(
                        nameof(workerRegistrar.Register), 
                        workerRegistrar, 
                        worker, 
                        typeof(IWorker<>));
                }
            }            
        }
    }
}
#endif