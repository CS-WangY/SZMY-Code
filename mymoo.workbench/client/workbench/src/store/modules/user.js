
import {  logout, getInfo, getAuthorityUserList } from '@/api/user'

import  { resetRouter } from '@/router'
import Cookies from 'js-cookie'

const state = {
	token: '',
	userCode: '',
	userName: '',
	avatar: '',
	introduction: '',
	authorityUser: []
}

const mutations = {
	SET_USERNAME: (state, userName) => {
		state.userName = userName
	},
	SET_USERCODE: (state, userCode) => {
		state.userCode = userCode
	},
	SET_ISADMIN: (state, isAdmin) => {
		state.isAdmin = isAdmin
	},
	SET_AUTHORITYUSER: (state, authorityUser) => {
		state.authorityUser = authorityUser
	}
}

let url=window.location.href

let actions = {
	getInfo({ commit }) {
		return new Promise((resolve, reject) => {
			// Cookies.set(
			// 	'Cookies',
			// 	'dwKTmj0jBDuQf9ML+/v7LNjG3rU/x+RF53TN6hxXigVKkn93cWgAaGWfKKrVsverViYEouLvgIU2nWgKc+7eOa4IMOxfXGVDSbFGfk/3cWHmxZ2t75Snby17lmOcvLplXiebgWDaqtpZT28HeZVERi1ryqwds464kNOjE/jC/jc2u3VEpR6SU9e8JB6x8vfdLvI7XKU5XeUmiKtavLZjsPPeCV9rF14/Wm+3BxSf/OnCmgvJKKm8RZqI6TKY0Uz/JIjslEzJVq7whrqQ45G2JDPEFLZBoCl86M1mhxRGV2R7Sk8u6jD5jEBGXHRS8f/mqTjNw9QwrI/FfrsATLffAQdd2fkyq1x5NWutgcvFCnDakOrfJqEq75bBL39qNjBSUfrKL2DS0SBeVN8eMs5VirYrR4ZZSr1nPlCd4lAg0nkePXPbBSheS3zU1Wbf4PaInTJ7R7j3ltC+ochNzYwIiHS0LfhQZ8e9HAy6WkGH4TYalKW8UunNkWWOLn4/d9Dwv4mowN51OvilYPl55qJYaQKt5tf/8xrqudAucdaXn1H1gwbYMofQtmKnGQ/n2r2+J5KSM9vD/7EFaKbnr2Irnw=='
			// )
			getInfo().then(res => {
				if (res.code === 'success') {
					const { data } = res
					const { code, name, isAdmin } = data

					commit('SET_USERCODE', code)
					commit('SET_USERNAME', name)
					commit('SET_ISADMIN', isAdmin)
					resolve(data)
				} else {
					let strurl='/'
					if(url.indexOf('#')> -1)
					{
						strurl=url.substring(url.indexOf('#')+1)
					}
					//location.href = '/Account/ScanLogin?redirectUri='
					location.href = `/home/login?redirectUri=${strurl}`
				}
			})
		})
	},

	// 获取该客户有权限的用户列表
	getAuthorityUserList({ commit }) {
		return new Promise((resolve, reject) => {
			getAuthorityUserList().then(res => {
				if (res.code === 'success' && res.data) {
					const { data } = res
					commit('SET_AUTHORITYUSER', data)
					resolve(data)
				}
			})
		})
	},

		// user logout
		logout({ commit, state, dispatch }) {
			return new Promise((resolve, reject) => {
				logout(state.token)
					.then(() => {
						commit('SET_TOKEN', '')
						commit('SET_USERCODE', '')
						commit('SET_USERNAME', '')
						// 清空路由表
						dispatch('permission/resetRoutes', {}, { root: true })
						resetRouter()
	
						// reset visited views and cached views
						// to fixed https://github.com/PanJiaChen/vue-element-admin/issues/2485
						dispatch('tagsView/delAllViews', null, { root: true })
	
						resolve()
					})
					.catch(error => {
						reject(error)
					})
			})
		},
}

export default {
	namespaced: true,
	state,
	mutations,
	actions
}
