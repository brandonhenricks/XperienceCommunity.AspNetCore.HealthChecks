﻿using System.Collections.ObjectModel;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.SiteProvider;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.AspNetCore.HealthChecks.Extensions;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class EmailHealthCheck : BaseKenticoHealthCheck<EmailInfo>, IHealthCheck
    {
        private readonly IEmailInfoProvider _emailInfoProvider;

        public EmailHealthCheck(IEmailInfoProvider emailInfoProvider)
        {
            _emailInfoProvider = emailInfoProvider ?? throw new ArgumentNullException(nameof(emailInfoProvider));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (!CMSApplication.ApplicationInitialized.HasValue)
            {
                return HealthCheckResult.Healthy("Application is not Initialized.");
            }

            var currentTimePlusTwoHours = DateTime.Now.AddHours(2);

            var data = await GetDataForTypeAsync(cancellationToken);

            var filtered = data.Where(email =>
                    email.EmailLastSendAttempt < currentTimePlusTwoHours &&
                    email.EmailStatus == EmailStatusEnum.Waiting)
                .ToList();

            if (filtered.Count > 0)
            {
                return HealthCheckResult.Degraded("Email Items are not being sent.", data: GetErrorData(filtered));
            }

            return HealthCheckResult.Healthy("Email Items Appear to be Healthy.");
        }

        protected override IEnumerable<EmailInfo> GetDataForType()
        {
            throw new NotImplementedException();
        }

        protected override async Task<List<EmailInfo>> GetDataForTypeAsync(
            CancellationToken cancellationToken = default)
        {
            using (new CMSConnectionScope(true))
            {
                var query = _emailInfoProvider.Get()
                    .WhereEquals(nameof(EmailInfo.EmailStatus), EmailStatusEnum.Waiting)
                    .OnSite(SiteContext.CurrentSiteID);

                return await query.ToListAsync(cancellationToken: cancellationToken);
            }
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<EmailInfo> objects)
        {
            var dictionary = objects.ToDictionary<EmailInfo, string, object>(email => email.EmailID.ToString(), email => $"{email.EmailStatus} - {email.EmailLastSendAttempt}");

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
