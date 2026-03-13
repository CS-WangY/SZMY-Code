import request from '@/utils/request'

export function getInfo() {
  return request({
    url: '/Home/CurrentUser',
    method: 'get'
  })
}

// 菜单-菜单列表
export function getMenuList() {
  return request({
      url: '/Privilege/GetMenu',
      method: 'get',
      headers: {'appId':'workbench'}
  })
}

// 获取该用户有权限的用户
export function getAuthorityUserList(params) {
  return request({
      url: '/Privilege/GetAuthorityUserList',
      method: 'get',
      params
  })
}

// 模糊查询员工信息
export function getUserInfo(params) {
    return request({
        //url: '/Home/FuzzyQuery',
        url: '/SystemManage/FuzzyQuery',
        method: 'get',
        params
    })
}

// app.UseSignLogin<WorkbenchContext, User>(); 里面统一处理的.退出登录
export function logout() {
	return request({
		url: '/Account/Logout',
		method: 'get'
	})
}
// 同步通讯录
//export function synchronize(params) {
//  return request({
//      url: '/WeiXinWork/Synchronize',
//      method: 'get',
//      params
//  })
//}

export function synchronizeToCapp(params) {
  return request({
      url: '/SystemManage/SynchronizeToCapp',
      method: 'get',
      params
  })
}
export function synchronizeToMes(params) {
  return request({
      url: '/SystemManage/SynchronizeToMes',
      method: 'get',
      params
  })
}

export function reloadCache(params) {
  return request({
      url: '/WeiXinWork/ReloadUserCache',
      method: 'get',
      params
  })
}