import { constantRoutes } from '@/router'
import { getMenuList } from '@/api/user'

// 递归修改返回的路由component路径
export function filterRoutes(routes, level = 0) {
	const res = []
	routes.forEach(route => {
		const tmp = { ...route }
		if (!tmp.children) {
			let aa = tmp['component']
			tmp['component'] = () => import(`@/views${aa}`)
		} else {
			let bb = tmp['component']
			if (level === 0) {
				tmp['component'] = () => import(`@/layout${bb}`)
			} else {
				if (tmp.name !== 'platformAdmin' || tmp.name !== 'crm' || tmp.name !== 'report') {
					tmp['component'] = () => import(`@/views${bb}`)
				}
			}
			level++
			tmp.children = filterRoutes(tmp.children, level)
			level--
		}
		res.push(tmp)
	})
	return res
}

const state = {
	routes: []
}

//store 会比router先执行,设置延时保证routes有值
setTimeout(() => {
	state.routes = constantRoutes
}, 1000)

const mutations = {
	SET_ROUTES: (state, routes) => {
		state.routes = constantRoutes.concat(routes)
	},
	RESET_ROUTES: state => {
		state.routes = []
	}
}

const actions = {
	generateRoutes({ commit }) {
		return new Promise(resolve => {
			let accessedRoutes
			getMenuList().then(res => {
				if (res.code == 'success') {
					if (res.data && res.data.length > 0) {
						res.data.forEach(item => {
							item.alwaysShow = true
						})
					}

					// res.data.push(
					// 	res.data.splice(
					// 		res.data.findIndex(item => item.meta.title === '系统管理'),
					// 		1
					// 	)[0]
					// )
					accessedRoutes = filterRoutes(res.data)
					setTimeout(() => {
						commit('SET_ROUTES', accessedRoutes)
						resolve(accessedRoutes)
					}, 1000)
				}
			})
		})
	},
	// 清空路由表
	resetRoutes({ commit }) {
		return new Promise(resolve => {
			commit('RESET_ROUTES')
			resolve()
		})
	}
}

export default {
	namespaced: true,
	state,
	mutations,
	actions
}
