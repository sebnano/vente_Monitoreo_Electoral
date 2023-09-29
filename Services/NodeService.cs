using System;
using Refit;

namespace ElectoralMonitoring
{
    public class NodeService : MobileBaseApiService
    {
        readonly AuthService _authService;
        readonly INodeApi _nodeApi;
        public NodeService(INodeApi nodeApi, AnalyticsService analyticsService, AuthService authService) : base(analyticsService)
        {
            _authService = authService;
            _nodeApi = nodeApi;
        }

        public async Task<NodeResponse<MinuteAttributes, MinuteRelationships>?> GetMinutes(CancellationToken cancellationToken)
        {
            var result = await AttemptAndRetry_Mobile(_nodeApi.GetMinutes, cancellationToken);

            return result;
        }

        public async Task<List<Minute>?> GetMinutesByUser(CancellationToken cancellationToken)
        {
            var result = await AttemptAndRetry_Mobile(async () => {

                return await _nodeApi.GetMinutesByUser(_authService.IdUser).ConfigureAwait(false);

            }, cancellationToken);

            return result;
        }

        public async Task<List<VotingCenter>?> GetVotingCenters(CancellationToken cancellationToken)
        {
            var result = await AttemptAndRetry_Mobile(async () => {

                return await _nodeApi.GetVotingCenters(_authService.IdUser).ConfigureAwait(false);

            }, cancellationToken);

            return result;
        }

        public async Task<FormResponse?> GetMinutesForm(CancellationToken cancellationToken)
        {
            var result = await AttemptAndRetry_Mobile(async () => {

                return await _nodeApi.GetMinutesForm().ConfigureAwait(false);

            }, cancellationToken);

            return result;
        }

        public async Task<ServerResponse?> UploadMinute(string fileName, FileStream file, CancellationToken cancellationToken)
        {
            var result = await AttemptAndRetry_Mobile(async () => {
                
                return await _nodeApi.UploadFile($"file; filename=\"{fileName}\"", new StreamPart(file, fileName)).ConfigureAwait(false);

            }, cancellationToken);

            return result;
        }

        public async Task<Dictionary<string,List<Node>>?> CreateNode(object values, CancellationToken cancellationToken)
        {
            var result = await AttemptAndRetry_Mobile(async () => {

                return await _nodeApi.CreateNode(values).ConfigureAwait(false);

            }, cancellationToken);

            return result;
        }

        public override Task LogOut()
        {
            return _authService.LogOut();
        }
    }
}

