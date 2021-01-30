using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using U9Api.CustSV.Base;
using U9Api.CustSV.Model.Response;

namespace U9Api.CustSV.Utils
{
    public static class JsonUtil
    {
        //实例化JavaScriptSerializer类的新实例
        static JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

        public static T GetJsonObject<T>(string jsonRequest)
        {
            LogUtil.WriteLog("CustLog","U9Api","Request",jsonRequest);
            T res = default(T);
            try
            {
                res = javaScriptSerializer.Deserialize<T>(jsonRequest);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }

        public static string GetJsonString(Object o) {
            return javaScriptSerializer.Serialize(o); 
        }

        public static string GetJsonResponse(string custCode, string custMsg, string u9Msg, StringBuilder debugInfo)
        {
            Response commonResponse = new Response();
            commonResponse.custCode = custCode;
            commonResponse.custMsg = custMsg;
            commonResponse.u9Msg = u9Msg;
            commonResponse.custDebugInfo = (debugInfo == null ? "" : debugInfo.ToString());
            string res = javaScriptSerializer.Serialize(commonResponse);
            LogUtil.WriteLog("CustLog", "U9Api", "Response", res);
            return res;
        }
        public static string GetSuccessResponse(string u9Msg)
        {
            return GetJsonResponse(U9Contant.SUCCESS_CODE, U9Contant.CUST_SUCCESS_MSG, u9Msg, null);
        }
        public static string GetSuccessResponse(string u9Msg, StringBuilder debugInfo)
        {
            if (U9Contant.IS_TEST)
            { return GetJsonResponse(U9Contant.SUCCESS_CODE, U9Contant.CUST_SUCCESS_MSG, u9Msg, debugInfo); }
            else
            {
                return GetSuccessResponse(u9Msg);
            }
        }

        public static string GetSuccessResponse(string docNo, string currentStatus,  StringBuilder debugInfo)
        {
            CommonApproveResponse response = new CommonApproveResponse();
            response.DocNo = docNo;
            response.CurrentStatus = currentStatus;
            return GetSuccessResponse(response,debugInfo);
        }
        public static string GetSuccessResponse(CommonApproveResponse response, StringBuilder debugInfo)
        {
            if (U9Contant.IS_TEST)
            { return GetJsonResponse(U9Contant.SUCCESS_CODE, U9Contant.CUST_SUCCESS_MSG, GetJsonString(response), debugInfo); }
            else
            {
                return GetSuccessResponse(GetJsonString(response));
            }
        }
        public static string GetFailResponse(string custMsg, StringBuilder debugInfo = null)
        {
            if (U9Contant.IS_TEST)
                return GetJsonResponse(U9Contant.Fail_CODE, custMsg, null, debugInfo);
            else
                return GetJsonResponse(U9Contant.Fail_CODE, custMsg, null, null);
        }
    }
}
