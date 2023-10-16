using AndroidX.Emoji2.Text.FlatBuffer;
using MonkeyCache;
using MonkeyCache.FileStore;
using Org.W3c.Dom;

namespace ElectoralMonitoring
{
    public class NodeService : MobileBaseApiService
    {
        readonly AuthService _authService;
        readonly INodeApi _nodeApi;
        double expireSeconds = 14400;
        public NodeService(INodeApi nodeApi, AnalyticsService analyticsService, AuthService authService) : base(analyticsService)
        {
            _authService = authService;
            _nodeApi = nodeApi;
        }

        T? GetFromBarrel<T>(string key)
        {
            AnalyticsService.Track("tryget_from_cache", "key", key);

            T? value = default(T);

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet && Barrel.Current.Exists(key))
            {
                value = Barrel.Current.Get<T>(key);
                AnalyticsService.Track("get_from_cache_success", "key", key);
            }
            else if (Barrel.Current.Exists(key) && !Barrel.Current.IsExpired(key))
            {
                value = Barrel.Current.Get<T>(key);
                AnalyticsService.Track("get_from_cache_success", "key", key);
            }

            return value;
        }

        public async Task<NodeResponse<MinuteAttributes, MinuteRelationships>?> GetMinutes(CancellationToken cancellationToken, bool forceRefresh = false)
        {

            return await AttemptAndRetry_Mobile(async() =>
            {

                var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                NodeResponse<MinuteAttributes, MinuteRelationships>? listValue = refresh ? null : GetFromBarrel<NodeResponse<MinuteAttributes, MinuteRelationships>>(nameof(GetMinutes));

                if (listValue is not null) return listValue;

                listValue = await _nodeApi.GetMinutes();
                Barrel.Current.Add(nameof(GetMinutes), listValue, TimeSpan.FromMinutes(expireSeconds));

                return listValue;
            }, cancellationToken);
        }

        public async Task<NodeResponse<VotingCentersAttrs, NodeRelationships>?> GetAllVotingCentersByCode(string ccv, CancellationToken cancellationToken, bool forceRefresh = false)
        {
            var result = await AttemptAndRetry_Mobile(async () =>
            {

                var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                NodeResponse<VotingCentersAttrs, NodeRelationships>? listValue = refresh ? null :
                GetFromBarrel<NodeResponse<VotingCentersAttrs, NodeRelationships>>($"{nameof(GetAllVotingCentersByCode)}/{ccv}");

                if (listValue is not null) return listValue;

                listValue = await _nodeApi.GetVotingCentersByCode(ccv);
                Barrel.Current.Add($"{nameof(GetAllVotingCentersByCode)}/{ccv}", listValue, TimeSpan.FromMinutes(expireSeconds));

                return listValue;
            }, cancellationToken);

            return result;
        }

        public async Task<List<Minute>?> GetMinutesByUser(CancellationToken cancellationToken, bool forceRefresh = true)
        {
            var result = await AttemptAndRetry_Mobile(async () =>
            {

                var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                List<Minute>? listValue = refresh ? null :
                GetFromBarrel<List<Minute>>(nameof(GetMinutesByUser));

                if (listValue is not null) return listValue;

                listValue = await _nodeApi.GetMinutesByUser(_authService.IdUser);
                Barrel.Current.Add(nameof(GetMinutesByUser), listValue, TimeSpan.FromMinutes(expireSeconds));

                return listValue;
            }, cancellationToken);

            return result;
        }

        public async Task<List<Minute>?> GetMinutesByCcvAndTable(string ccv, string table, CancellationToken cancellationToken, bool forceRefresh = true)
        {
            var result = await AttemptAndRetry_Mobile(async () =>
            {
                var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                List<Minute>? listValue = refresh ? null :
                GetFromBarrel<List<Minute>>($"{nameof(GetMinutesByCcvAndTable)}/{ccv}/{table}");

                if (listValue is not null) return listValue;

                listValue = await _nodeApi.GetMinutesByCcvAndTable(ccv, table);
                Barrel.Current.Add($"{nameof(GetMinutesByCcvAndTable)}/{ccv}/{table}", listValue, TimeSpan.FromMinutes(expireSeconds));

                return listValue;
            }, cancellationToken);

            return result;
        }

        public async Task<List<VotingCenter>?> GetVotingCenters(CancellationToken cancellationToken, bool forceRefresh = false)
        {
            var result = await AttemptAndRetry_Mobile(async () => {
                var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                List<VotingCenter>? listValue = refresh ? null :
                GetFromBarrel<List<VotingCenter>>(nameof(GetVotingCenters));

                if (listValue is not null) return listValue;

                listValue = await _nodeApi.GetVotingCenters(_authService.IdUser);
                Barrel.Current.Add(nameof(GetVotingCenters), listValue, TimeSpan.FromMinutes(expireSeconds));

                return listValue;

            }, cancellationToken);

            return result;
        }

        public async Task<List<Report>?> GetUserReports(string reportId, CancellationToken cancellationToken, bool forceRefresh = true)
        {
            var result = await AttemptAndRetry_Mobile(async () => {
                var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                List<Report>? listValue = refresh ? null :
                GetFromBarrel<List<Report>>($"{nameof(GetUserReports)}/{reportId}");

                if (listValue is not null) return listValue;

                listValue = await _nodeApi.GetUserReports(_authService.IdUser, reportId);
                Barrel.Current.Add($"{nameof(GetUserReports)}/{reportId}", listValue, TimeSpan.FromMinutes(expireSeconds));

                return listValue;

            }, cancellationToken);

            return result;
        }

        public async Task<FormResponse?> GetMinutesForm(CancellationToken cancellationToken, bool forceRefresh = false)
        {
            var result = await AttemptAndRetry_Mobile(async () => {

                var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                FormResponse? listValue = refresh ? null :
                GetFromBarrel<FormResponse>(nameof(GetMinutesForm));

                if (listValue is not null) return listValue;

                listValue = await _nodeApi.GetMinutesForm();
                Barrel.Current.Add(nameof(GetMinutesForm), listValue, TimeSpan.FromMinutes(expireSeconds));

                return listValue;

            }, cancellationToken);

            return result;
        }

        public async Task<List<FieldForm>?> GetMinutesFormFields(CancellationToken cancellationToken, bool forceRefresh = false)
        {
            var result = await AttemptAndRetry_Mobile(async () => {

                var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                List<FieldForm>? listValue = refresh ? null :
                GetFromBarrel<List<FieldForm>>(nameof(GetMinutesFormFields));

                if (listValue is not null) return listValue;

                listValue = await _nodeApi.GetMinutesFormFields();
                Barrel.Current.Add(nameof(GetMinutesFormFields), listValue, TimeSpan.FromMinutes(expireSeconds));

                return listValue;

            }, cancellationToken);

            return result;
        }

        public async Task<Dictionary<string, List<Node>>?> GetNode(string nodeId, CancellationToken cancellationToken, bool forceRefresh = true)
        {
            var result = await AttemptAndRetry_Mobile(async () => {

                var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                Dictionary<string, List<Node>>? listValue = refresh ? null :
                GetFromBarrel<Dictionary<string, List<Node>>>($"{nameof(GetNode)}/{nodeId}");

                if (listValue is not null) return listValue;

                listValue = await _nodeApi.GetNode(nodeId).ConfigureAwait(false);
                Barrel.Current.Add($"{nameof(GetNode)}/{nodeId}", listValue, TimeSpan.FromMinutes(expireSeconds));

                return listValue;

            }, cancellationToken);

            return result;
        }

        public async Task<List<FieldForm>?> GetReportForm(string contentType, CancellationToken cancellationToken, bool forceRefresh = false)
        {
            var result = await AttemptAndRetry_Mobile(async () => {
                var parts = contentType.Split("/");
                string parent = parts[0];
                string report = parts[1];

                var refresh = forceRefresh && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                List<FieldForm>? listValue = refresh ? null :
                GetFromBarrel<List<FieldForm>>($"{nameof(GetReportForm)}/{report}");

                if (listValue is not null) return listValue;

                listValue = await _nodeApi.GetReportForm(parent, report).ConfigureAwait(false);
                Barrel.Current.Add($"{nameof(GetReportForm)}/{report}", listValue, TimeSpan.FromMinutes(expireSeconds));

                return listValue;

            }, cancellationToken);

            return result;
        }

        #region POST|PATCH

        public async Task<Dictionary<string,List<Node>>?> UploadMinute(string fileName, FileStream file, CancellationToken cancellationToken)
        {
            var result = await AttemptAndRetry_Mobile(async () => {
                
                return await _nodeApi.UploadFile($"file; filename=\"{fileName}\"", file).ConfigureAwait(false);

            }, cancellationToken);

            return result;
        }

        public async Task<Dictionary<string,List<Node>>?> CreateNode(object values, CancellationToken cancellationToken)
        {
            var result = await AttemptAndRetry_Mobile(async () => {
                //todo save offline
                return await _nodeApi.CreateNode(values).ConfigureAwait(false);

            }, cancellationToken);

            return result;
        }

        public async Task<Dictionary<string, List<Node>>?> EditNode(string nodeId, object values, CancellationToken cancellationToken)
        {
            var result = await AttemptAndRetry_Mobile(async () => {
                //todo save offline
                return await _nodeApi.EditNode(nodeId, values).ConfigureAwait(false);

            }, cancellationToken);

            return result;
        }
        #endregion

        public override Task LogOut()
        {
            return _authService.LogOut();
        }
    }
}

