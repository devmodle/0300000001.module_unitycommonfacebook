using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if FACEBOOK_MODULE_ENABLE
using Facebook.Unity;

/** 페이스 북 관리자 */
public partial class CFacebookManager : CSingleton<CFacebookManager> {
	/** 콜백 */
	public enum ECallback {
		NONE = -1,
		INIT,
		[HideInInspector] MAX_VAL
	}

	/** 페이스 북 콜백 */
	private enum EFacebookCallback {
		NONE = -1,
		LOGIN,
		CHANGE_VIEW_STATE,
		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams {
		public Dictionary<ECallback, System.Action<CFacebookManager, bool>> m_oCallbackDict;
	}

	#region 변수
	private STParams m_stParams;
	private Dictionary<EFacebookCallback, System.Action<CFacebookManager, bool>> m_oCallbackDict = new Dictionary<EFacebookCallback, System.Action<CFacebookManager, bool>>();
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
				
				return stExpirationTime.ExGetDeltaTimePerDays(System.DateTime.Now).ExIsGreate(KCDefine.B_VAL_0_FLT);
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
	/** 초기화 */
	public virtual void Init(STParams a_stParams) {
		CFunc.ShowLog("CFacebookManager.Init", KCDefine.B_LOG_COLOR_PLUGIN);

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		// 초기화 되었을 경우
		if(this.IsInit) {
			a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, true);
		} else {
			m_stParams = a_stParams;
			FB.Init(this.OnInit, this.OnChangeViewState);
		}
#else
		a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, false);
#endif			// #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
	}
	
	/** 로그인을 처리한다 */
	public void Login(List<string> a_oPermissionList, System.Action<CFacebookManager, bool> a_oCallback, System.Action<CFacebookManager, bool> a_oChangeViewStateCallback = null) {
		CFunc.ShowLog($"CFacebookManager.Login: {a_oPermissionList}", KCDefine.B_LOG_COLOR_PLUGIN);
		CAccess.Assert(a_oPermissionList.ExIsValid());		

#if UNITY_IOS || UNITY_ANDROID
		// 로그인 되었을 경우
		if(!this.IsInit || this.IsLogin) {
			CFunc.Invoke(ref a_oCallback, this, this.IsLogin);
		} else {
			m_oCallbackDict.ExReplaceVal(EFacebookCallback.LOGIN, a_oCallback);
			m_oCallbackDict.ExReplaceVal(EFacebookCallback.CHANGE_VIEW_STATE, a_oChangeViewStateCallback);

			FB.LogInWithReadPermissions(a_oPermissionList, this.OnLogin);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}

	/** 로그아웃을 처리한다 */
	public void Logout(System.Action<CFacebookManager> a_oCallback) {
		CFunc.ShowLog("CFacebookManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 로그인 되었을 경우
		if(this.IsInit && this.IsLogin) {
			FB.LogOut();
		}
#endif			// #if UNITY_IOS || UNITY_ANDROID

		CFunc.Invoke(ref a_oCallback, this);
	}
	#endregion			// 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID
	// 초기화 되었을 경우
	private void OnInit() {
		CFunc.ShowLog($"CFacebookManager.OnInit: {this.IsInit}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FACEBOOK_M_INIT_CALLBACK, () => {
			FB.Mobile.SetAutoLogAppEventsEnabled(false);

#if ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD
			FB.Mobile.SetAdvertiserTrackingEnabled(true);
			FB.Mobile.SetAdvertiserIDCollectionEnabled(true);
#else
			FB.Mobile.SetAdvertiserTrackingEnabled(false);
			FB.Mobile.SetAdvertiserIDCollectionEnabled(false);
#endif			// #if ANALYTICS_TEST_ENABLE || STORE_DIST_BUILD

			FB.ActivateApp();
			m_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, this.IsInit);
		});
	}

	/** 로그인 되었을 경우 */
	private void OnLogin(ILoginResult a_oResult) {
		CFunc.ShowLog($"CFacebookManager.OnLogin: {this.IsLogin}, {a_oResult}", KCDefine.B_LOG_COLOR_PLUGIN);
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_FACEBOOK_M_LOGIN_CALLBACK, () => m_oCallbackDict.GetValueOrDefault(EFacebookCallback.LOGIN)?.Invoke(this, this.IsLogin));
	}

	/** 뷰 상태가 변경 되었을 경우 */
	private void OnChangeViewState(bool a_bIsShow) {
		CFunc.ShowLog($"CFacebookManager.OnChangeViewState: {a_bIsShow}", KCDefine.B_LOG_COLOR_PLUGIN);
		CScheduleManager.Inst.AddCallback(a_bIsShow ? KCDefine.U_KEY_FACEBOOK_M_VIEW_STATE_SHOW_CALLBACK : KCDefine.U_KEY_FACEBOOK_M_VIEW_STATE_CLOSE_CALLBACK, () => m_oCallbackDict.GetValueOrDefault(EFacebookCallback.CHANGE_VIEW_STATE)?.Invoke(this, a_bIsShow));
	}
#endif			// #if UNITY_IOS || UNITY_ANDROID
	#endregion			// 조건부 함수
}
#endif			// #if FACEBOOK_MODULE_ENABLE
