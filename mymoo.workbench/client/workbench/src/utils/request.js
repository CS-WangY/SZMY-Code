import axios from 'axios'
import { MessageBox, Message } from 'element-ui'
import store from '@/store'
// import { getToken } from '@/utils/auth'
import { closeLoading } from '@/utils/loading'

// create an axios instance
const service = axios.create({
	baseURL: process.env.VUE_APP_BASE_API, // url = base url + request url
	// withCredentials: true, // send cookies when cross-domain requests
	timeout: 60000 // request timeout
})

// request interceptor
service.interceptors.request.use(
	config => {
		// config.headers['token'] = getToken()
		if (config.method === 'get') {
			config.data = { unused: 0 } // 解决get,请求添加不上Content-Type
		}
		config.headers['Content-Type'] = 'application/json;charset=UTF-8'
		config.headers['appId'] = 'workbench'//每个请求都加上系统标识
		return config
	},
	error => {
		// do something with request error
		console.log(error) // for debug
		return Promise.reject(error)
	}
)

// response interceptor
service.interceptors.response.use(
	/**
	 * If you want to get http information such as headers or status
	 * Please return  response => response
	 */

	/**
	 * Determine the request status by custom code
	 * Here is just an example
	 * You can also judge the status by HTTP Status Code
	 */
	response => {
		const res = response.data
		// if the custom code is not 20000, it is judged as an error.
		if (res.code !== 'success') {
			if (res.code == 'ExistsKeepon') {
				// 客户已备案
				Message({
					message: res.errorMessage,
					type: 'warning',
					duration: 5 * 1000
				})
				return res
			} else if (res.code === 'ToeknInvalid' || res.code === 'ForbidApp' || res.code === 'ToeknFailure') {
				// 是否重新登录
				MessageBox.confirm(res.errorMessage, '重新登录', {
					confirmButtonText: '确定',
					cancelButtonText: '取消',
					type: 'warning'
				}).then(() => {
					// store.dispatch('user/resetToken').then(() => {
					// 	location.reload()
					// })
					//location.href = `/Account/ScanLogin?redirectUri=`
					location.href = `/home/login?redirectUri=/`
				})
				return res
			} else if (res.code === 'titckInvalid' || res.code === 'titckFailure') {
				//单点登录票据失效，跳到登录页面
				Message({
					message: res.errorMessage || 'Error',
					type: 'error',
					duration: 5 * 1000
				})
				return Promise.reject(new Error(res.errorMessage || 'Error'))
			} else if (response.headers['content-disposition']) {
				return response
			} else if (response.config.url.includes('/Attachment')) {
				return res
			} else if (res.code === 'NoLogin') {
				Message({
					message: res.errorMessage || 'Error',
					type: 'error',
					duration: 5 * 1000
				})
				//location.href = `/Account/ScanLogin?redirectUri=`
				location.href = `/home/login?redirectUri=/`
				return res
			} else {
				Message({
					message: res.errorMessage || 'Error',
					type: 'error',
					duration: 5 * 1000
				})
				return res
			}
			// return Promise.reject(new Error(res.errorMessage || "Error"))
		} else {
			return res
		}
	},
	error => {
		if (error.message.includes('timeout')) {
			// 判断请求异常信息中是否含有超时timeout字符串
			Message({
				message: '请求超时，请稍后再试',
				type: 'error',
				duration: 5 * 1000
			})
			closeLoading()
			return Promise.reject(error) // reject这个错误信息
		}
		closeLoading()
		Message({
			message: error.message,
			type: 'error',
			duration: 5 * 1000
		})
		return Promise.reject(error)
	}
)

export default service
