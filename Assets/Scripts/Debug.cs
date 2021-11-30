using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public static class Debug
{
	//[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void Log( object message )
	{
#if !PHEI_RELEASE
        UnityEngine.Debug.Log( message );
#endif
    }

	public static void Log( object message, UnityEngine.Object context )
	{
#if !PHEI_RELEASE
        UnityEngine.Debug.Log( message, context );
#endif
	}

	public static void LogError( object message )
	{
#if !PHEI_RELEASE
		UnityEngine.Debug.LogError( message );
#endif

#if D_USE_WRAPPING_DEBUG_LOG
		PSDK.PSDKManager.LoggingAgent.Message_Error(message.ToString());
#endif
	}

	public static void LogError( object message, UnityEngine.Object context )
	{
#if !PHEI_RELEASE
        UnityEngine.Debug.LogError( message, context );
#endif

#if D_USE_WRAPPING_DEBUG_LOG
		Devsisters.Breadcrumb.Fields fields = new Devsisters.Breadcrumb.Fields();
		fields.Add("Object Name", context.ToString());

		PSDK.PSDKManager.LoggingAgent.Message_Error(message.ToString(), fields);
#endif
	}

	public static void LogWarning( object message )
	{
		UnityEngine.Debug.LogWarning( message.ToString() );
	}

	public static void LogWarning( object message, UnityEngine.Object context )
	{
		UnityEngine.Debug.LogWarning( message.ToString(), context );
	}

	public static void DrawLine( Vector3 start, Vector3 end, Color color = default(Color), float duration = 0.0f, bool depthTest = true )
	{
		UnityEngine.Debug.DrawLine( start, end, color, duration, depthTest );
	}

	public static void DrawRay( Vector3 start, Vector3 dir, Color color = default(Color), float duration = 0.0f, bool depthTest = true )
	{
		UnityEngine.Debug.DrawRay( start, dir, color, duration, depthTest );
	}

	public static void Assert( bool condition )
	{
		UnityEngine.Debug.Assert( condition );

#if D_USE_WRAPPING_DEBUG_LOG
        if (condition)
            return;

		string stackTrace = UnityEngine.StackTraceUtility.ExtractStackTrace();

		Devsisters.Breadcrumb.Fields fields = new Devsisters.Breadcrumb.Fields();
		fields.Add("Stack Trace", stackTrace);

		PSDK.PSDKManager.LoggingAgent.Message_Error("Assert", fields);
#endif

    }

    public static void Assert( bool condition, object context )
	{
		UnityEngine.Debug.Assert(condition, context);

#if D_USE_WRAPPING_DEBUG_LOG
        if (condition)
            return;

        string stackTrace = UnityEngine.StackTraceUtility.ExtractStackTrace();

		Devsisters.Breadcrumb.Fields fields = new Devsisters.Breadcrumb.Fields();
		fields.Add("Stack Trace", stackTrace);

		PSDK.PSDKManager.LoggingAgent.Message_Error("Assert", fields);
#endif
    }

	public static void Assert( bool condition, UnityEngine.Object context )
	{
		UnityEngine.Debug.Assert(condition, context);

#if D_USE_WRAPPING_DEBUG_LOG
        if (condition)
            return;

        string stackTrace = UnityEngine.StackTraceUtility.ExtractStackTrace();

		Devsisters.Breadcrumb.Fields fields = new Devsisters.Breadcrumb.Fields();
		fields.Add("Stack Trace", stackTrace);

		PSDK.PSDKManager.LoggingAgent.Message_Error("Assert", fields);
#endif
	}

	// SDK 에서 사용하는 코드에서 모호성 문제가 생겨서 System.Diagnostics.Debug.Assert 추가  :  SDK 업데이트 되면 삭제 가능
	public static void Assert( bool condition, string message, string details )
	{
		System.Diagnostics.Debug.Assert(condition, message, details);
	}

	public static void LogFormat( string message, params object[] args )
	{
		UnityEngine.Debug.LogFormat( message, args );
	}

	public static void LogWarningFormat(string message, params object[] args)
	{
		UnityEngine.Debug.LogWarningFormat( message, args );
	}

	public static void LogErrorFormat( string message, params object[] args )
	{
		UnityEngine.Debug.LogErrorFormat( message, args );
	}

	public static void AssertFormat( bool condition, string format, params object[] args )
	{
		UnityEngine.Debug.AssertFormat( condition, format, args );
	}

	public static void AssertFormat( bool condition, UnityEngine.Object context, string format, params object[] args )
	{
		UnityEngine.Debug.AssertFormat(condition, context, format, args);
	}

	public static void LogException( Exception exception )
	{
		UnityEngine.Debug.LogException(exception);
	}
	
	public static void LogException( Exception exception, UnityEngine.Object context )
	{
		UnityEngine.Debug.LogException(exception, context);
	}
}
#endif