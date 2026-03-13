const getters = {
	sidebar: state => state.app.sidebar,
	size: state => state.app.size,
	device: state => state.app.device,
	visitedViews: state => state.tagsView.visitedViews,
	// cachedViews: state => state.tagsView.cachedViews,
	token: state => state.user.token,
	avatar: state => state.user.avatar,
	userCode: state => state.user.userCode,
	userName: state => state.user.userName,
	introduction: state => state.user.introduction,
	authorityUser: state => state.user.authorityUser,
	// roles: state => state.user.roles,
	permission_routes: state => state.permission.routes
}
export default getters
