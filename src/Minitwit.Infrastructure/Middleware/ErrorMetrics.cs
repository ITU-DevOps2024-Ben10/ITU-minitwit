using System.Diagnostics.Metrics;
using Prometheus;

namespace Minitwit.Infrastructure.Middleware;

public class ErrorMetrics
{
    private static readonly Counter GetLatestError = Metrics.CreateCounter(
        "minitwit_get_latest_error_total",
        "Number of errors in getting the latest messages"
    );

    public static void IncrementGetLatestError()
    {
        GetLatestError.Inc();
    }
    
    private static readonly Counter PostRegisterUserError = Metrics.CreateCounter(
        "minitwit_post_register_user_error_total",
        "Number of errors in registering a user"
    );
    
    public static void IncrementPostRegisterUserError()
    {
        PostRegisterUserError.Inc();
    }
    
    private static readonly Counter GetMsgsError = Metrics.CreateCounter(
        "minitwit_get_msgs_error_total",
        "Number of errors in getting messages"
    );
    
    public static void IncrementGetMsgsError()
    {
        GetMsgsError.Inc();
    }
    
    private static readonly Counter GetMsgsForUserError = Metrics.CreateCounter(
        "minitwit_get_msgs_for_user_error_total",
        "Number of errors in getting messages for a user"
    );
    
    public static void IncrementGetMsgsForUserError()
    {
        GetMsgsForUserError.Inc();
    }
    
    private static readonly Counter PostMsgsForUserError = Metrics.CreateCounter(
        "minitwit_post_msgs_for_user_error_total",
        "Number of errors in posting messages for a user"
    );
    
    public static void IncrementPostMsgsForUserError()
    {
        PostMsgsForUserError.Inc();
    }
    
    private static readonly Counter GetFollowsForUserError = Metrics.CreateCounter(
        "minitwit_get_followers_for_user_error_total",
        "Number of errors in getting followers for a user"
    );
    
    public static void IncrementGetFollowersForUserError()
    {
        GetFollowsForUserError.Inc();
    }
    
    private static readonly Counter PostFollowsForUserError = Metrics.CreateCounter(
        "minitwit_post_followers_for_user_error_total",
        "Number of errors in posting followers for a user"
    );
    
    public static void IncrementPostFollowsForUserError()
    {
        PostFollowsForUserError.Inc();
    }
}