import Vue from 'vue'
import Router from 'vue-router'
import Layout from '@/layout'

Vue.use(Router)

/**
 * constantRoutes
 * a base page that does not have permission requirements
 * all roles can be accessed
 */
export const constantRoutes = [
	// {
	// 	path: "/login",
	// 	component: () => import("@/views/login/index"),
	// 	hidden: true,
	// },
	{
		path: '/',
		component: Layout,
		redirect: '/dashboard',
		children: [
			{
				path: 'dashboard',
				component: () => import('@/views/dashboard/index'),
				name: 'dashboard',
				meta: { title: '欢迎使用', icon: 'dashboard', affix: true }
			}
		]
	},
	// {
	// 	path: '/systemManage',
	// 	component: Layout,
	// 	alwaysShow: true,
	// 	meta: { title: '系统管理', icon: 'user' },
	// 	children: [
	// 		{
	// 			path: 'usermanage',
	// 			component: () => import('@/views/systemManage/usermanage/index'),
	// 			name: 'usermanage',
	// 			sort: 10,
	// 			meta: {
	// 				title: '用户管理',
	// 				icon: ''
	// 			}
	// 		},
	// 		{
	// 			path: 'rolemanage',
	// 			component: () => import('@/views/systemManage/rolemanage/index'),
	// 			name: 'rolemanage',
	// 			sort: 10,
	// 			meta: {
	// 				title: '角色管理',
	// 				icon: ''
	// 			}
	// 		},
	// 		{
	// 			path: 'menumanage',
	// 			component: () => import('@/views/systemManage/menumanage/index'),
	// 			name: 'menumanage',
	// 			sort: 10,
	// 			meta: {
	// 				title: '菜单管理',
	// 				icon: ''
	// 			}
	// 		},
	// 		{
	// 			path: 'menuitemmanage',
	// 			component: () => import('@/views/systemManage/menuitemmanage/index'),
	// 			name: 'menuitemmanage',
	// 			sort: 10,
	// 			meta: {
	// 				title: '功能管理',
	// 				icon: ''
	// 			}
	// 		}
	// 	]
	// }
	
]

const createRouter = () =>
	new Router({
		scrollBehavior: () => ({ y: 0 }),
		routes: constantRoutes
	})

const router = createRouter()

export function resetRouter() {
	const newRouter = createRouter()
	router.matcher = newRouter.matcher // reset router
}

//解决Redirected from “xxx“ to “xxx“ via a navigation guard报错
const originalPush = Router.prototype.push
Router.prototype.push = function push(location, onResolve, onReject) {
	if (onResolve || onReject) return originalPush.call(this, location, onResolve, onReject)
	return originalPush.call(this, location).catch(err => err)
}

export default router

export const test = 2
