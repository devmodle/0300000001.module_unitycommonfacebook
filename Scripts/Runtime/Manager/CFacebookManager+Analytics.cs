using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FACEBOOK_ENABLE && FACEBOOK_ANALYTICS_ENABLE
using Facebook.Unity;

//! 페이스 북 관리자 - 분석
public partial class CFacebookManager : CSingleton<CFacebookManager> {
		#region 함수
	//! 로그를 전송한다
	public void SendLog(string a_oName, float? a_oValue = null) {
		this.SendLog(a_oName, null, a_oValue);
	}

	//! 로그를 전송한다
	public void SendLog(string a_oName, string a_oParam, List<string> a_oDataList, float? a_oValue = null) {
		CAccess.Assert(a_oParam.ExIsValid());

		this.SendLog(a_oName, new Dictionary<string, object>() {
			[a_oParam] = a_oDataList.ExToString(KCDefine.B_TOKEN_CSV_STRING)
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
	#endregion			// 함수	
}
#endif			// #if FACEBOOK_ENABLE && FACEBOOK_ANALYTICS_ENABLE
