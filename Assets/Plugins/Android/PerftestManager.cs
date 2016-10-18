using UnityEngine;
using System;
using System.Collections;

public class PerftestManager : MonoBehaviour
{
    #if UNITY_ANDROID
	static AndroidJavaClass unityPlayerClass;
	static AndroidJavaObject perftestClass;
	
	static public void J(string appKey, string userId, string gameId, string gameVersion) // Login
	{
		try 
        {
			if(unityPlayerClass == null)
				unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			if(perftestClass == null)
				perftestClass = new AndroidJavaClass("com.nhnent.perftest.perftestAPI");

			if ( (unityPlayerClass != null) && (perftestClass != null) ){
				perftestClass.CallStatic ("j", appKey, userId, gameId, gameVersion, unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"));
			}
		}
		catch(Exception e)
		{
			UnityEngine.Debug.Log(e.ToString());
		}
	}

    static public void J(string userId, string gameId, string gameVersion) // Login
	{
		try 
        {
			if(unityPlayerClass == null)
				unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			if(perftestClass == null)
				perftestClass = new AndroidJavaClass("com.nhnent.perftest.perftestAPI");

			if ( (unityPlayerClass != null) && (perftestClass != null) ){
				perftestClass.CallStatic ("j", userId, gameId, gameVersion, unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"));
			}
		}
		catch(Exception e)
		{
			UnityEngine.Debug.Log(e.ToString());
		}
	}

	static public void C() // detect
	{
		try
		{
			if(perftestClass == null)
				perftestClass = new AndroidJavaClass("com.nhnent.perftest.perftestAPI");
			
			if (perftestClass != null)
				perftestClass.CallStatic("c");
		}
		catch(Exception e)
		{
			UnityEngine.Debug.Log(e.ToString());
		}
	}
	
	static public void O(string objectName, string functionName)
	{
		try
		{
			if(unityPlayerClass == null)
				unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			if(perftestClass == null)
				perftestClass = new AndroidJavaClass("com.nhnent.perftest.perftestAPI");

			if ( (unityPlayerClass != null) && (perftestClass != null) ){
				perftestClass.CallStatic("o", objectName, functionName, true);
			}
		}
		catch(Exception e)
		{
			UnityEngine.Debug.Log(e.ToString());
		}
	}

	#else
	static public void J(string appKey, string userId, string gameId, string gameVersion){}
    static public void J(string userId, string gameId, string gameVersion){}
	static public void C(){}
	static public void O(string objectName, string functionName){}
	#endif
}
