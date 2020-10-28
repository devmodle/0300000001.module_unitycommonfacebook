using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FACEBOOK_MODULE_ENABLE
using Facebook.Unity;

//! 페이스 북 관리자
public partial class CFacebookManager : CSingleton<CFacebookManager> {
	#region 변수
	private System.Action<CFacebookManager, bool> m_oInitCallback = null;
	private System.Action<CFacebookManager, bool> m_oLoginCallback = null;
	private System.Action<CFacebookManager, bool> m_oChangeViewStateCallback = null;
	#endregion			// 변수

	#region 프로퍼티
	public bool IsInit {
		get {
#if UNITY_IOS || UNITY_ANDROID
			return !CAccess.IsMobile() ? false : FB.IsInitialized;
#else
			return false;
#endif			// #if UNITY_IOS || UNITY_ANDROID
		}	
	}

	public bool IsLogin {
		get {
#if UNITY_IOS || UNITY_ANDROID
			// 초기화 되었을 경우
			if(this.IsInit) {
				var oToken = Facebook.Unity.AccessToken.CurrentAccessToken;
				var stExpirationTime = (oToken != null) ? oToken.ExpirationTime : System.DateTime.Now;

				double dblDeltaTime = stExpirationTime.ExGetDeltaTimePerDays(System.DateTime.Now);
				return dblDeltaTime.ExIsGreate(KCDefine.B_VALUE_FLOAT_0);
			}

			return false;
#else
			return false;
#endif			// #if UNITY_IOS || UNITY_ANDROID
		}
	}

	public string UserID {
		get {
#if UNITY_IOS || UNITY_ANDROID
			return this.IsLogin ? Facebook.Unity.AccessToken.CurrentAccessToken.UserId : string.Empty;
#else
			return string.Empty;
#endif			// #if UNITY_IOS || UNITY_ANDROID
		}
	}

	public string AccessToken {
		get {
#if UNITY_IOS || UNITY_ANDROID
			return this.IsLogin ? Facebook.Unity.AccessToken.CurrentAccessToken.TokenString : string.Empty;
#else
			return string.Empty;
#endif			// #if UNITY_IOS || UNITY_ANDROID
		}
	}
	#endregion			// 프로퍼티

	#region 함수
	//! 초기화
	public virtual void Init(System.Action<CFacebookManager, bool> a_oCallback) {
		CFunc.ShowLog("CFacebookManager.Init", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화 되었을 경우
		if(this.IsInit) {
			a_oCallback?.Invoke(this, true);
		} else {
			m_oInitCallback = a_oCallback;
			FB.Init(this.OnInit, this.OnChangeViewState);
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}
	
	//! 로그인을 처리한다
	public void Login(List<string> a_oPermissionList, 
		System.Action<CFacebookManager, bool> a_oCallback, System.Action<CFacebookManager, bool> a_oChangeViewStateCallback = null) 
	{
		CFunc.ShowLog("CFacebookManager.Login: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oPermissionList);

#if UNITY_IOS || UNITY_ANDROID
		// 로그인 되었을 경우
		if(!this.IsInit || this.IsLogin) {
			a_oCallback?.Invoke(this, this.IsLogin);
		} else {
			m_oLoginCallback = a_oCallback;
			m_oChangeViewStateCallback = a_oChangeViewStateCallback;

			FB.LogInWithReadPermissions(a_oPermissionList, this.OnLogin);
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}

	//! 로그아웃을 처리한다
	public void Logout(System.Action<CFacebookManager> a_oLogoutCallback) {
		CFunc.ShowLog("CFacebookManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			FB.LogOut();
		}
#endif			// #if UNITY_IOS || UNITY_ANDROID

		a_oLogoutCallback?.Invoke(this);
	}
	#endregion			// 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID
	//! 초기화 되었을 경우
	private void OnInit() {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_FACEBOOK_M_INIT_CALLBACK, () => {
			CFunc.ShowLog("CFacebookManager.OnInit: {0}", KCDefine.B_LOG_COLOR_PLUGIN, this.IsInit);

#if FACEBOOK_ANALYTICS_ENABLE && (ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD))
			FB.Mobile.SetAutoLogAppEventsEnabled(true);
			FB.Mobile.SetAdvertiserTrackingEnabled(true);
			FB.Mobile.SetAdvertiserIDCollectionEnabled(true);
#else
			FB.Mobile.SetAutoLogAppEventsEnabled(false);
			FB.Mobile.SetAdvertiserTrackingEnabled(false);
			FB.Mobile.SetAdvertiserIDCollectionEnabled(false);
#endif			// #if FACEBOOK_ANALYTICS_ENABLE && (ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD))

			FB.ActivateApp();
			m_oInitCallback?.Invoke(this, this.IsInit);
		});
	}

	//! 로그인 되었을 경우
	private void OnLogin(ILoginResult a_oResult) {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_FACEBOOK_M_LOGIN_CALLBACK, () => {
			CFunc.ShowLog("CFacebookManager.OnLogin: {0}, {1}", 
				KCDefine.B_LOG_COLOR_PLUGIN, this.IsLogin, a_oResult);

			m_oLoginCallback?.Invoke(this, this.IsLogin);
		});
	}

	//! 뷰 상태가 변경 되었을 경우
	private void OnChangeViewState(bool a_bIsShow) {
		string oKey = a_bIsShow ? 
			KCDefine.U_KEY_FACEBOOK_M_VIEW_STATE_SHOW_CALLBACK : KCDefine.U_KEY_FACEBOOK_M_VIEW_STATE_CLOSE_CALLBACK;

		CScheduleManager.Instance.AddCallback(oKey, () => {
			CFunc.ShowLog("CFacebookManager.OnChangeViewState: {0}", 
				KCDefine.B_LOG_COLOR_PLUGIN, a_bIsShow);

			m_oChangeViewStateCallback?.Invoke(this, a_bIsShow);
		});
	}
#endif			// #if UNITY_IOS || UNITY_ANDROID
	#endregion			// 조건부 함수
}
#endif			// #if FACEBOOK_MODULE_ENABLE
