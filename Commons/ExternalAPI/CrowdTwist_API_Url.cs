using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ROG.Commons.ExternalAPI
{
    public class CrowdTwist_API_Url
    {
#if DEBUG
        //public string apikey = "ZxxgiWXTLTIERPL538fqaQ1uB49xMYFD";
        //public const string baseUrl = "https://api108.crowdtwist.com";
        //public const string CTUrl = "https://asus-sandbox.crowdtwist.com";
        public string apikey = "gtkWjDs5tQ8SqqhbQo22eRDI7YcLh16y";
        public const string baseUrl = "https://api107.crowdtwist.com";
        public const string CTUrl = "https://asus.crowdtwist.com";
#endif
#if !DEBUG
        public string apikey = "gtkWjDs5tQ8SqqhbQo22eRDI7YcLh16y";
        public const string baseUrl = "https://api107.crowdtwist.com";
        public const string CTUrl = "https://asus.crowdtwist.com";
#endif

        private string leaderboard_url = baseUrl + "/v2/leaderboard?api_key=[apikey]&start=[start]&finish=[finish]&period=[period]";
        private string rewards_url = baseUrl + "/v2/rewards?api_key=[apikey]";
        private string events_url = baseUrl + "/v2/activities/extended?api_key=[apikey]";
        private string userProfile_url = baseUrl + "/v2/users/[user_id]/?api_key=[apikey]&id_type=[id_type]";
        private string userActivities_url = baseUrl + "/v2.1/users/[user_id]/activity_history/extended?api_key=[apikey]&id_type=[id_type]";
        private string redemptionHistory_url = baseUrl + "/v2/users/[user_id]/redemption_history?api_key=[apikey]&id_type=[id_type]";
        private string userActivityCredit_url = baseUrl + "/v2/users/[user_id]/activities?api_key=[apikey]&id_type=[id_type]";
        private string userSignIn_url = CTUrl + "/http/v2/auth-sign-in?api_sig=[api_sig]";
        private string userSignOut_url = CTUrl + "/http/v2/auth-sign-out?redirect=[redirect]&api_sig=[api_sig]";

        public CrowdTwist_API_Url()
        {
            leaderboard_url = this.leaderboard_url.Replace("[apikey]", this.apikey);
            rewards_url = this.rewards_url.Replace("[apikey]", this.apikey);
            events_url = this.events_url.Replace("[apikey]", this.apikey);
            userProfile_url = this.userProfile_url.Replace("[apikey]", this.apikey);
            userActivities_url = this.userActivities_url.Replace("[apikey]", this.apikey);
            redemptionHistory_url = this.redemptionHistory_url.Replace("[apikey]", this.apikey);
            userActivityCredit_url = this.userActivityCredit_url.Replace("[apikey]", this.apikey);
        }
        
        public string GetLeaderBoard_Url(string start = "10", string finish = "20", string period = "all-time")
        {
            var result_url = this.leaderboard_url.Replace("[start]", start)
                                                                    .Replace("[finish]", finish)
                                                                    .Replace("[period]", period);
            return result_url;
        }
        public string GetRewards_Url(string category = "", string for_points = "", string featured = "", string redeemable_by = "", string for_displa = "")
        {
            var result_url = rewards_url;
            string active_only = "1";
            if (!string.IsNullOrWhiteSpace(category)) result_url += "&category=" + category;
            if (!string.IsNullOrWhiteSpace(active_only)) result_url += "&active_only=" + active_only;
            if (!string.IsNullOrWhiteSpace(featured)) result_url += "&featured=" + featured;
            if (!string.IsNullOrWhiteSpace(redeemable_by)) result_url += "&redeemable_by=" + redeemable_by;
            if (!string.IsNullOrWhiteSpace(for_displa)) result_url += "&for_displa=" + for_displa;
            return result_url;
        }
        public string GetEvents_Url()
        {
            return this.events_url;
        }
        public string GetUserProfile_Url(string id, string id_type)
        {
            var result_url = userProfile_url;
            result_url = result_url.Replace("[user_id]", id)
                                            .Replace("[id_type]", id_type);
            return result_url;
        }
        public string GetActivityCredit_url(string cT_ID)
        {
            var result_url = userActivityCredit_url;
            result_url = result_url.Replace("[user_id]", cT_ID)
                                             .Replace("[id_type]", "id");
            return result_url;
        }
      
        public string GetUserActivities_Url(string cT_ID, string date_Start, string date_End)
        {
            var result_url = userActivities_url;
            result_url = result_url.Replace("[user_id]", cT_ID)
                                                .Replace("[id_type]", "id");

            if (!string.IsNullOrWhiteSpace(date_Start) && !string.IsNullOrWhiteSpace(date_End))
            {
                result_url += "&date_start=" + date_Start;
                result_url += "&date_end=" + date_End;
            }
            return result_url;
        }
        public string GetRedemptionHistory_Url(string cT_ID)
        {
            var result_url = redemptionHistory_url;
            result_url = result_url.Replace("[user_id]", cT_ID)
                                             .Replace("[id_type]", "id");
            return result_url;
        }
        public string GetuserSignIn_Url(string api_sig)
        {
            var result_url = userSignIn_url;
            result_url = result_url.Replace("[api_sig]", api_sig);
            return result_url;
        }
        public string GetuserSignOut_Url(string api_sig,string redirect)
        {
            var result_url = userSignOut_url;
            result_url = result_url.Replace("[api_sig]", api_sig)
                                            .Replace("[redirect]", redirect);
            return result_url;
        }
    }
}
