using System.Collections.ObjectModel;
using CMS.DataEngine;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class EmailSettingsHealthCheck : BaseKenticoHealthCheck<SettingsKeyInfo>, IHealthCheck
    {
        private const string EmailDefaultDomain = "localhost.local";

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var status = HealthCheckResult.Healthy("E-mail Configured Correctly");

            try
            {
                var settingKeys = await GetDataForTypeAsync(cancellationToken);

                if (settingKeys.Count == 0)
                {
                    return status;
                }

                var filteredKeys = settingKeys.Where(key =>
                    key.KeyValue.Contains(EmailDefaultDomain, StringComparison.OrdinalIgnoreCase)).ToList();

                if (filteredKeys.Count > 0)
                {
                    status = GetHealthCheckResult(context, "E-mail Configuration contains Errors.", GetErrorData(filteredKeys));
                }

                return status;
            }
            catch (Exception ex)
            {
                status = HandleException(ex);
                return status;
            }
        }

        protected override IEnumerable<SettingsKeyInfo> GetDataForType()
        {
            throw new NotImplementedException();
        }

        protected override async Task<List<SettingsKeyInfo>> GetDataForTypeAsync(
            CancellationToken cancellationToken = default)
        {
            using (new CMSConnectionScope(true))
            {
                var query = await SettingsKeyInfoProvider
                    .GetSettingsKeys()
                    .WhereContains(nameof(SettingsKeyInfo.KeyName), "email")
                    .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

                return query.ToList();
            }
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<SettingsKeyInfo> objects)
        {
            var dictionary = objects.ToDictionary<SettingsKeyInfo, string, object>(setting => setting.KeyName.ToString(), webFarmTask => "Not Properly Configured.");

            return new ReadOnlyDictionary<string, object>(dictionary);
        }

    }
}
