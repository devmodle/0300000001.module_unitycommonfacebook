using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FACEBOOK_MODULE_ENABLE && FACEBOOK_ANALYTICS_ENABLE
using Facebook.Unity;

#if PURCHASE_MODULE_ENABLE
using UnityEngine.Purchasing;
#endif			// #if PURCHASE_MODULE_ENABLE

//! 페이스 북 관리자 - 분석
public partial class CFacebookManager : CSingleton<CFacebookManager> {
		#region 함수
	//! 로그를 전송한다
	public void SendLog(string a_oName, float? a_oValue = null) {
		this.SendLog(a_oName, null, a_oValue);
	}

	//! 로그를 전송한다
	public void SendLog(string a_oName, 
		string a_oParam, List<string> a_oDataList, float? a_oValue = null) 
	{
		this.SendLog(a_oName, new Dictionary<string, object>() {
			[a_oParam] = a_oDataList.ExToString(KCDefine.B_TOKEN_CSV_STRING)
		}, a_oValue);
	}

	//! 로그를 전송한다
	public void SendLog(string a_oName, 
		Dictionary<string, object> a_oDataList, float? a_oValue = null) 
	{
		CFunc.ShowLog("CFacebookManager.SendLog: {0}, {1}", 
			KCDefine.B_LOG_COLOR_PLUGIN, a_oName, a_oDataList);

#if UNITY_IOS || UNITY_ANDROID
#if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
		// 초기화 되었을 경우
		if(this.IsInit) {
			var oDataList = a_oDataList ?? new Dictionary<string, object>();

#if MSG_PACK_ENABLE
			oDataList.ExAddValue(KCDefine.U_LOG_KEY_DEVICE_ID, 
				CCommonAppInfoStorage.Instance.AppInfo.DeviceID);

#if AUTO_LOG_PARAMS_ENABLE
			oDataList.ExAddValue(KCDefine.U_LOG_KEY_PLATFORM, 
				CCommonAppInfoStorage.Instance.PlatformName);

			oDataList.ExAddValue(KCDefine.U_LOG_KEY_USER_TYPE, 
				CCommonUserInfoStorage.Instance.UserInfo.UserType.ToString());
			
			oDataList.ExAddValue(KCDefine.U_LOG_KEY_LOG_TIME, 
				System.DateTime.UtcNow.ExToLongString());

			oDataList.ExAddValue(KCDefine.U_LOG_KEY_INSTALL_TIME, 
				CCommonAppInfoStorage.Instance.AppInfo.UTCInstallTime.ExToLongString());
#endif			// #if AUTO_LOG_PARAMS_ENABLE
#endif			// #if MSG_PACK_ENABLE

			FB.LogAppEvent(a_oName, a_oValue, oDataList);
		}
#endif			// #if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}
	#endregion			// 함수

	#region 조건부 함수
#if PURCHASE_MODULE_ENABLE
	//! 결제 로그를 전송한다
	public void SendPurchaseLog(Product a_oProduct, Dictionary<string, object> a_oDataList) {
		CFunc.ShowLog("CFacebookManager.SendPurchaseLog: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oProduct);
		
#if UNITY_IOS || UNITY_ANDROID
#if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
		// 초기화 되었을 경우
		if(this.IsInit) {
			FB.LogPurchase(a_oProduct.metadata.localizedPrice, 
				a_oProduct.metadata.isoCurrencyCode, a_oDataList);
		}
#endif			// #if ANALYTICS_TEST_ENABLE || (ADHOC_BUILD || STORE_BUILD)
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}
#endif			// #if PURCHASE_MODULE_ENABLE
	#endregion			// 조건부 함수
}
#endif			// #if FACEBOOK_MODULE_ENABLE && FACEBOOK_ANALYTICS_ENABLE
