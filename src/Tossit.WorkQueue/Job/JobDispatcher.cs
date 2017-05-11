﻿using System;
using Tossit.Core;
#if !net451
using Microsoft.Extensions.Logging;
#endif

namespace Tossit.WorkQueue.Job
{
    /// <summary>
    /// Job dispatcher implementation.
    /// </summary>
    public class JobDispatcher : IJobDispatcher
    {
        /// <summary>
        /// JobNameValidator field.
        /// </summary>
        private readonly IJobNameValidator _jobNameValidator;
        /// <summary>
        /// MessageQueue field.
        /// </summary>
        private readonly IMessageQueue _messageQueue;
        /// <summary>
        /// JsonConverter field.
        /// </summary>
        private readonly IJsonConverter _jsonConverter;
#if !net451
        /// <summary>
        /// Logger field.
        /// </summary>
        private readonly ILogger<JobDispatcher> _logger;
#endif

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="jobNameValidator">IJobNameValidator</param>
        /// <param name="messageQueue">IMessageQueue</param>
        /// <param name="jsonConverter">IJsonConverter</param>
#if !net451
        /// <param name="logger">ILogger{JobDispatcher}</param>
#endif
        public JobDispatcher(IJobNameValidator jobNameValidator,
            IMessageQueue messageQueue,
            IJsonConverter jsonConverter
#if !net451
            ,ILogger<JobDispatcher> logger
#endif            
            )
        {
            _jobNameValidator = jobNameValidator;
            _messageQueue = messageQueue;
            _jsonConverter = jsonConverter;
#if !net451
            _logger = logger;
#endif
        }

        /// <summary>
        /// Dispatch job as sync.
        /// </summary>
        /// <typeparam name="TData">Type of data to send to worker.</typeparam>
        /// <param name="job">Job instance to dispatch.</param>
        /// <returns>If job dispatched successfully, returns true, otherwise returns false.</returns>
        /// <exception cref="Exception">Throws when job could not be dispatched.</exception>>
        public bool Dispatch<TData>(IJob<TData> job) where TData : class
        {
            this.ValidateJob(job);

            try
            {
                // Send.
                var result = _messageQueue.Send(job.Name, _jsonConverter.Serialize(job.Data));

                // Log, if could not be dispatched.
                if (!result)
                {
                    var message = $"Job {job.GetType().FullName} could not be dispatched.";
#if !net451
                    _logger.LogError(message);
#endif
                    return false;
                }
            }
            catch (Exception ex)
            {
#if !net451
                _logger.LogError(
                    new EventId(), 
                    ex, 
                    $"Job {job.GetType().FullName} dispatching failed. {ex.Message}");
#endif
                throw ex;
            }

            return true;
        }

        /// <summary>
        /// Validate given job.
        /// </summary>
        /// <typeparam name="TData">Type of job data.</typeparam>
        /// <param name="job">Instance of job.</param>
        /// <exception cref="ArgumentNullException">Throws when job or job data is null.</exception>
        private void ValidateJob<TData>(IJob<TData> job) where TData : class
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            if (job.Data == null)
            {
                throw new ArgumentNullException(nameof(job.Data));
            }

            _jobNameValidator.Validate(job.Name);
        }
    }
}
