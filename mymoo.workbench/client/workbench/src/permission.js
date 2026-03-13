import router from './router'
import store from './store'
import { Message } from 'element-ui'
import NProgress from 'nprogress' // progress bar
import 'nprogress/nprogress.css' // progress bar style
import getPageTitle from '@/utils/get-page-title'
NProgress.configure({ showSpinner: false }) // NProgress Configuration

router.beforeEach(async (to, from, next) => {
	// start progress bar
	NProgress.start()
	// set page title
	document.title = getPageTitle(to.meta.title)
	//next()
	if (to.path === '/login') {
		// if is logged in, redirect to the home page
		next({ path: '/' })
		NProgress.done() // hack: https://github.com/PanJiaChen/vue-element-admin/pull/2939
	} else {
		// determine whether the user has obtained his permission roles through getInfo
		const hasRoutes = store.getters.permission_routes && store.getters.permission_routes.length > 0
		if (hasRoutes) {
			next()
		} else {
			try {
				// get user info
				// note: roles must be a object array! such as: ['admin'] or ,['developer','editor']
				await store.dispatch('user/getInfo')
				// 获取该客户有权限用户列表
				await store.dispatch('user/getAuthorityUserList')

				// generate accessible routes map based on roles
				const accessRoutes = await store.dispatch('permission/generateRoutes')

				// dynamically add accessible routes
				router.addRoutes(accessRoutes)

				// hack method to ensure that addRoutes is complete
				// set the replace: true, so the navigation will not leave a history record
				next({ ...to, replace: true })
			} catch (error) {
				// remove token and go to login page to re-login
				await store.dispatch('user/resetToken')
				Message.error({ message: error || 'Has Error' })
				next()
				//location.href = `/Account/ScanLogin?redirectUri=${to.path}`
				location.href = `/home/login?redirectUri=/`

				NProgress.done()
			}
		}
	}
})

router.afterEach(() => {
	// finish progress bar
	NProgress.done()
})
