using Prometheus;

namespace Minitwit.Infrastructure.Middleware;

public class CustomMeters
{
    private static readonly Counter ApiRequestsCounter = Metrics.CreateCounter(
        "minitwit_api_requests_total",
        "Number of API requests"
    );

    public static void IncrementApiRequestsCounter()
    {
        ApiRequestsCounter.Inc();
    }
    
    private static readonly Counter ApiRequestsErrorCounter = Metrics.CreateCounter(
        "minitwit_api_requests_error_total",
        "Number of API requests with error"
    );
    
    public static void IncrementApiRequestsErrorCounter()
    {
        ApiRequestsErrorCounter.Inc();
    }
    
    private static readonly Counter ApiRequestsSuccessCounter = Metrics.CreateCounter(
        "minitwit_api_requests_success_total",
        "Number of API requests with success"
    );
    
    public static void IncrementApiRequestsSuccessCounter()
    {
        ApiRequestsSuccessCounter.Inc();
    }

    private static readonly Counter RegisterApiRequestCounter = Metrics.CreateCounter(
        "minitwit_register_user_total",
        "Number of registered users"
    );
    
    public static void IncrementRegisterUserCounter()
    {
        RegisterApiRequestCounter.Inc();
    }
    
    private static readonly Counter LoginApiRequestCounter = Metrics.CreateCounter(
        "minitwit_login_user_total",
        "Number of logged in users"
    );
    
    public static void IncrementLoginUserCounter()
    {
        LoginApiRequestCounter.Inc();
    }
    
    private static readonly Counter FollowApiRequestCounter = Metrics.CreateCounter(
        "minitwit_follow_user_total",
        "Number of followed users"
    );
    
    public static void IncrementFollowUserCounter()
    {
        FollowApiRequestCounter.Inc();
    }
    
    private static readonly Counter UnfollowApiRequestCounter = Metrics.CreateCounter(
        "minitwit_unfollow_user_total",
        "Number of unfollowed users"
    );
    
    public static void IncrementUnfollowUserCounter()
    {
        UnfollowApiRequestCounter.Inc();
    }
    
    private static readonly Counter PostApiRequestCounter = Metrics.CreateCounter(
        "minitwit_post_message_total",
        "Number of posted messages"
    );
    
    public static void IncrementPostMessageCounter()
    {
        PostApiRequestCounter.Inc();
    }
    
    private static readonly Counter PostMsgApiRequestCounter = Metrics.CreateCounter(
        "minitwit_get_messages_total",
        "Number of retrieved messages"
    );
    
    public static void IncrementGetMsgCounter()
    {
        PostMsgApiRequestCounter.Inc();
    }


}