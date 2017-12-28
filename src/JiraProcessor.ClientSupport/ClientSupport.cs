using CommonExtensions;
using Foundation.CustomConfig;
using Jira;
using Jira.Json;
using JiraProcessor;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Wcf.ProxyMonads;
using static Jira.Rest;

namespace JiraProcessor {
  [ClientSupportSettings]
  public class ClientSupport :Foundation.EventLogger<ClientSupport> {
    #region Config
    #region RunTest
    protected override async Task<ExpandoObject> _RunTestAsync(ExpandoObject parameters, params Func<ExpandoObject, ExpandoObject>[] merge) {
      return await TestHostAsync(parameters, async (p, m) => {
        var b = await base._RunTestAsync(parameters, merge);
        var jira = await JiraMonad.RunTestAsync();
        return new ExpandoObject().Merge(GetType().FullName, b.Merge("JIRA", jira));
      }, _LogError, merge);
    }
    #endregion

    class ClientSupportSettings :ConfigSectionAttribute { };
    public static string Project { get { return KeyValue(); } }
    public static string IssueType { get { return KeyValue(); } }

    [JiraSettings]
    public static string RootField { get { return KeyValue(); } }
    [JiraSettings]
    public static string SmsPhoneField { get { return KeyValue(); } }

    #endregion

    #region Static API
    public async static Task<IssueClasses.Issue> CreateTicket(string smsPhone, string account, string summary) {
      SearchResult<IssueClasses.Issue> search = await Commoner.GetOpenTicketsBySmsPhone(Project, IssueType, smsPhone, 1);
      if(search.total > 0)
        return search.issues.FirstOrDefault();
      var fields = new Dictionary<string, object> {
        {SmsPhoneField,smsPhone },
        {RootField,account}
      };
      var newIssue = JiraNewIssue.Create(Project, IssueType, summary, "", null);
      var issue = (await newIssue.ToJiraPost().PostIssueAsync(fields)).Value;
      return issue;
    }
    #endregion
  }
}
