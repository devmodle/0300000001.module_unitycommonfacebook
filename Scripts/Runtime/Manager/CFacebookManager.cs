using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FACEBOOK_ENABLE
using Facebook.Unity;

//! 페이스 북 관리자
public class CFacebookManager : CSingleton<CFacebookManager> {
	#region 변수
	private System.Action<CFacebookManager, bool> m_oInitCallback = null;
	private System.Action<CFacebookManager, bool> m_oLoginCallback = null;
	#endregion			// 변수

	#region 프로퍼티
	public bool IsInit => !CAccess.IsMobilePlatform() ? false : FB.IsInitialized;
	public string UserID => this.IsLogin ? Facebook.Unity.AccessToken.CurrentAccessToken.UserId : string.Empty;
	public string AccessToken => this.IsLogin ? Facebook.Unity.AccessToken.CurrentAccessToken.TokenString : string.Empty;

	public bool IsLogin {
		get {
			if(this.IsInit) {
				var oToken = Facebook.Unity.AccessToken.CurrentAccessToken;
				return oToken != null && oToken.ExpirationTime.ExGetDeltaTimePerDays(System.DateTime.Now).ExIsGreate(0.0f);
			}

			return false;
		}
	}
	#endregion			// 프로퍼티

	#region 함수
	//! 초기화
	public virtual void Init(System.Action<CFacebookManager, bool> a_oCallback) {
		CFunc.ShowLog("CFacebookManager.Init", KCDefine.B_LOG_COLOR_PLUGIN);

		if(this.IsInit || !CAccess.IsMobilePlatform()) {
			a_oCallback?.Invoke(this, this.IsInit);
		} else {
			m_oInitCallback = a_oCallback;
			FB.Init(this.OnInit, this.OnChangeViewState);
		}
	}

	//! 초기화 되었을 경우
	public void OnInit() {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_FACEBOOK_M_INIT_CALLBACK, () => {
			CFunc.ShowLog("CFacebookManager.OnInit: {0}", KCDefine.B_LOG_COLOR_PLUGIN, this.IsInit);

#if FACEBOOK_ANALYTICS_ENABLE
#if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
			FB.Mobile.SetAutoLogAppEventsEnabled(true);
			FB.Mobile.SetAdvertiserIDCollectionEnabled(true);
#else
			FB.Mobile.SetAutoLogAppEventsEnabled(false);
			FB.Mobile.SetAdvertiserIDCollectionEnabled(false);
#endif			// #if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
#endif			// #if FACEBOOK_ANALYTICS_ENABLE

			FB.ActivateApp();
			m_oInitCallback?.Invoke(this, this.IsInit);
		});
	}

	//! 로그인 되었을 경우
	public void OnLogin(ILoginResult a_oResult) {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_FACEBOOK_M_LOGIN_CALLBACK, () => {
			CFunc.ShowLog("CFacebookManager.OnLogin: {0}, {1}", KCDefine.B_LOG_COLOR_PLUGIN, this.IsLogin, a_oResult);
			CActivityIndicatorManager.Instance.StopActivityIndicator();

			m_oLoginCallback?.Invoke(this, this.IsLogin);
			m_oLoginCallback = null;
		});
	}

	//! 뷰 상태가 변경 되었을 경우
	public void OnChangeViewState(bool a_bIsShow) {
		string oKey = a_bIsShow ? KCDefine.U_KEY_FACEBOOK_M_VIEW_STATE_SHOW_CALLBACK
			: KCDefine.U_KEY_FACEBOOK_M_VIEW_STATE_CLOSE_CALLBACK;

		CScheduleManager.Instance.AddCallback(oKey, () => {
			CFunc.ShowLog("CFacebookManager.OnChangeViewState: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_bIsShow);

			if(a_bIsShow) {
				CActivityIndicatorManager.Instance.StopActivityIndicator();

				m_oLoginCallback?.Invoke(this, this.IsLogin);
				m_oLoginCallback = null;
			} else {
				CActivityIndicatorManager.Instance.StartActivityIndicator(true);
			}
		});
	}

	//! 로그인을 처리한다
	public void Login(List<string> a_oPermissionList, System.Action<CFacebookManager, bool> a_oCallback) {
		CFunc.ShowLog("CFacebookManager.Login: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oPermissionList);

		if(!this.IsInit || this.IsLogin) {
			a_oCallback?.Invoke(this, this.IsLogin);
		} else {
			m_oLoginCallback = a_oCallback;
			FB.LogInWithReadPermissions(a_oPermissionList, this.OnLogin);
		}
	}

	//! 로그아웃을 처리한다
	public void Logout(System.Action<CFacebookManager> a_oLogoutCallback) {
		CFunc.ShowLog("CFacebookManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

		if(this.IsInit) {
			FB.LogOut();
		}

		a_oLogoutCallback?.Invoke(this);
	}
	#endregion			// 함수

	#region 조건부 함수
#if FACEBOOK_ANALYTICS_ENABLE
	//! 로그를 전송한다
	public void SendLog(string a_oName, float? a_oValue = null) {
		this.SendLog(a_oName, null, a_oValue);
	}

	//! 로그를 전송한다
	public void SendLog(string a_oName, string a_oParam, List<string> a_oDataList, float? a_oValue = null) {
		CAccess.Assert(a_oParam.ExIsValid());

		this.SendLog(a_oName, new Dictionary<string, object>() {
			[a_oParam] = a_oDataList.ExToString(KCDefine.U_TOKEN_FACEBOOK_ANALYTICS_LOG_DATA)
		}, a_oValue);
	}

	//! 로그를 전송한다
	public void SendLog(string a_oName, Dictionary<string, object> a_oDataList, float? a_oValue = null) {
		CAccess.Assert(a_oName.ExIsValid());
		CFunc.ShowLog("CFacebookManager.SendLog: {0}, {1}", KCDefine.B_LOG_COLOR_PLUGIN, a_oName, a_oDataList);

#if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
		if(this.IsInit) {
			var oDataList = a_oDataList ?? new Dictionary<string, object>();

#if MSG_PACK_ENABLE
			oDataList.ExAddValue(KCDefine.U_LOG_KEY_DEVICE_ID, CAppInfoStorage.Instance.AppInfo.DeviceID);

#if AUTO_LOG_PARAM_ENABLE
			oDataList.ExAddValue(KCDefine.U_LOG_KEY_PLATFORM, CAppInfoStorage.Instance.PlatformName);
			oDataList.ExAddValue(KCDefine.U_LOG_KEY_USER_TYPE, CUserInfoStorage.Instance.UserInfo.UserType.ToString());
			
			oDataList.ExAddValue(KCDefine.U_LOG_KEY_LOG_TIME, System.DateTime.UtcNow.ExToLongString());
			oDataList.ExAddValue(KCDefine.U_LOG_KEY_INSTALL_TIME, CAppInfoStorage.Instance.AppInfo.UTCInstallTime.ExToLongString());
#endif			// #if AUTO_LOG_PARAM_ENABLE
#endif			// #if MSG_PACK_ENABLE

			FB.LogAppEvent(a_oName, a_oValue, oDataList);
		}
#endif			// #if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
	}
#endif			// #if FACEBOOK_ANALYTICS_ENABLE
	#endregion			// 조건부 함수
}
#endif			// #if FACEBOOK_ENABLE
