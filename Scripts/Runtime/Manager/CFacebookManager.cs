using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if FACEBOOK_MODULE_ENABLE
using Facebook.Unity;

//! 페이스 북 관리자
public class CFacebookManager : CSingleton<CFacebookManager> {
	//! 콜백 매개 변수
	public struct STCallbackParams {
		public System.Action<CFacebookManager, bool> m_oCallback;
	}

	#region 변수
	private STCallbackParams m_stCallbackParams;

	private System.Action<CFacebookManager, bool> m_oLoginCallback = null;
	private System.Action<CFacebookManager, bool> m_oChangeViewStateCallback = null;
	#endregion			// 변수

	#region 프로퍼티
	public bool IsInit {
		get {
#if UNITY_IOS || UNITY_ANDROID
			return FB.IsInitialized;
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
				return dblDeltaTime.ExIsGreate(KCDefine.B_VAL_0_FLT);
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
	public virtual void Init(STCallbackParams a_stCallbackParams) {
		CFunc.ShowLog("CFacebookManager.Init", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화 되었을 경우
		if(this.IsInit) {
			a_stCallbackParams.m_oCallback?.Invoke(this, true);
		} else {
			m_stCallbackParams = a_stCallbackParams;
			FB.Init(this.OnInit, this.OnChangeViewState);
		}
#else
		a_stCallbackParams.m_oCallback?.Invoke(this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}
	
	//! 로그인을 처리한다
	public void Login(List<string> a_oPermissionList, System.Action<CFacebookManager, bool> a_oCallback, System.Action<CFacebookManager, bool> a_oChangeViewStateCallback = null) {
		CFunc.ShowLog($"CFacebookManager.Login: {a_oPermissionList}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oPermissionList.ExIsValid());		

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
	public void Logout(System.Action<CFacebookManager> a_oCallback) {
		CFunc.ShowLog("CFacebookManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			FB.LogOut();
		}
#endif			// #if UNITY_IOS || UNITY_ANDROID

		a_oCallback?.Invoke(this);
	}
	#endregion			// 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID
	// 초기화 되었을 경우
	private void OnInit() {
		CFunc.ShowLog($"CFacebookManager.OnInit: {this.IsInit}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FACEBOOK_M_INIT_CALLBACK, () => {
			FB.Mobile.SetAutoLogAppEventsEnabled(false);

#if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
			FB.Mobile.SetAdvertiserTrackingEnabled(true);
			FB.Mobile.SetAdvertiserIDCollectionEnabled(true);
#else
			FB.Mobile.SetAdvertiserTrackingEnabled(false);
			FB.Mobile.SetAdvertiserIDCollectionEnabled(false);
#endif			// #if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)

			FB.ActivateApp();
			CFunc.Invoke(ref m_stCallbackParams.m_oCallback, this, this.IsInit);
		});
	}

	//! 로그인 되었을 경우
	private void OnLogin(ILoginResult a_oResult) {
		CFunc.ShowLog($"CFacebookManager.OnLogin: {this.IsLogin}, {a_oResult}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FACEBOOK_M_LOGIN_CALLBACK, () => {
			CFunc.Invoke(ref m_oLoginCallback, this, this.IsLogin);
		});
	}

	//! 뷰 상태가 변경 되었을 경우
	private void OnChangeViewState(bool a_bIsShow) {
		CFunc.ShowLog($"CFacebookManager.OnChangeViewState: {a_bIsShow}", KCDefine.B_LOG_COLOR_PLUGIN);
		string oKey = a_bIsShow ? KCDefine.U_KEY_FACEBOOK_M_VIEW_STATE_SHOW_CALLBACK : KCDefine.U_KEY_FACEBOOK_M_VIEW_STATE_CLOSE_CALLBACK;

		CScheduleManager.Inst.AddCallback(oKey, () => {
			CFunc.Invoke(ref m_oChangeViewStateCallback, this, a_bIsShow);
		});
	}
#endif			// #if UNITY_IOS || UNITY_ANDROID
	#endregion			// 조건부 함수
}
#endif			// #if FACEBOOK_MODULE_ENABLE
