import request from '@/utils/request'

// 菜单管理-获取菜单
export function getMenu(params) {
	return request({
		url: '/SystemManage/GetMenuList',
		method: 'get',
		params
	})
}

// 菜单管理-获取菜单(分页)
export function getMenuByFilter(data) {
	return request({
		url: '/SystemManage/GetMenuListByFilter',
		method: 'post',
		data
	})
}

// 菜单管理-获取系统
export function getAppList(params) {
	return request({
		url: '/SystemManage/GetAppList',
		method: 'get',
		params
	})
}

// 菜单管理-根据系统获取菜单
export function getMenuByAppId(params) {
	return request({
		url: '/SystemManage/GetMenuByAppId',
		method: 'get',
		params
	})
}

// 菜单管理-新增菜单
export function addMenu(data) {
	return request({
		url: '/SystemManage/InsertMenu',
		method: 'post',
		data
	})
}

// 菜单管理-编辑菜单
export function editMenu(data) {
	return request({
		url: '/SystemManage/UpdateMenu',
		method: 'post',
		data
	})
}

// 菜单管理-删除菜单
export function delMenu(params) {
	return request({
		url: '/SystemManage/DeleteMenu',
		method: 'get',
		params
	})
}

// 菜单管理-删除菜单引用
export function delMenuQuote(params) {
	return request({
		url: '/SystemManage/DeleteMenuQuote',
		method: 'get',
		params
	})
}

// 菜单管理-发布菜单
export function publishMenu(params) {
	return request({
		url: '/SystemManage/PublishMenu',
		method: 'get',
		params
	})
}

// 菜单管理-下架菜单
export function offMenu(params) {
	return request({
		url: '/SystemManage/OffMenu',
		method: 'get',
		params
	})
}

// 角色管理-获取角色
export function getRoleList(params) {
	return request({
		url: '/SystemManage/GetRoleList',
		method: 'get',
		params
	})
}

// 角色管理-获取角色
export function getRoleListByFilter(data) {
	return request({
		url: '/SystemManage/GetRoleListByFilter',
		method: 'post',
		data
	})
}

// 角色管理-新增角色
export function addRole(data) {
	return request({
		url: '/SystemManage/InsertRole',
		method: 'post',
		data
	})
}

// 角色管理-编辑角色
export function editRole(data) {
	return request({
		url: '/SystemManage/UpdateRole',
		method: 'post',
		data
	})
}

// 角色管理-删除角色
export function delRole(params) {
	return request({
		url: '/SystemManage/DeleteRole',
		method: 'get',
		params
	})
}

// 角色管理-删除角色引用
export function delRoleQuoted(params) {
	return request({
		url: '/SystemManage/DeleteRoleQuoted',
		method: 'get',
		params
	})
}

// 角色管理-给角色设置管理员
export function setRoleAdmin(params) {
	return request({
		url: '/SystemManage/SetRoleAdmin',
		method: 'get',
		params
	})
}

// 角色管理-禁用角色
export function forbiddenRole(params) {
	return request({
		url: '/SystemManage/ForbiddenRole',
		method: 'get',
		params
	})
}

// 角色管理-启用角色
export function enableRole(params) {
	return request({
		url: '/SystemManage/EnableRole',
		method: 'get',
		params
	})
}

// 角色菜单-获取角色菜单列表
export function getRolesMenu(params) {
	return request({
		url: '/SystemManage/GetRolesMenu',
		method: 'get',
		params
	})
}

// 角色菜单-给角色分配菜单
export function assignMenu(data) {
	return request({
		url: '/SystemManage/AssignMenu',
		method: 'post',
		data
	})
}

// 角色菜单-角色取消分配菜单
export function cancelMenu(data) {
	return request({
		url: '/SystemManage/CancelMenu',
		method: 'post',
		data
	})
}

export function GetWeiXinWorkDepartmentDetials(params) {
	return request({
		url: '/SystemManage/GetDepartment',
		method: 'get',
		params
	})
}

// 用户管理-获取部门成员详情
export function getDepartmentUserList(data) {
	return request({
		url: '/SystemManage/GetUsersByDepatMent',
		method: 'post',
		data
	})
}

export function getDeptByUser(data){
	return request({
		url:'/SystemManage/GetDeptByUser',
		method:'post',
		data
	})
}

// 用户管理-通过用户code获取角色列表
export function getUserRoles(params) {
	return request({
		url: '/SystemManage/GetUserRoles',
		method: 'get',
		params
	})
}

// 用户管理-给用户分配角色
export function assignRole(data) {
	return request({
		url: '/SystemManage/AssignRole',
		method: 'post',
		data
	})
}

// 用户管理-给用户分配助理
export function assignAssistant(data) {
	return request({
		url: '/SystemManage/AssignAssistant',
		method: 'post',
		data
	})
}

// 用户管理-通过用户code获取助理列表/获取负责员工列表
export function getUserAssistant(params) {
	return request({
		url: '/SystemManage/GetUserAssistant',
		method: 'get',
		params
	})
}

// 用户管理-通过用户code获取岗位列表
export function getUserPosition(params) {
	return request({
		url: '/SystemManage/GetUserPosition',
		method: 'get',
		params
	})
}

// 用户管理-给用户分派岗位
export function assignPostion(data) {
	return request({
		url: '/SystemManage/AssignPostion',
		method: 'post',
		data
	})
}

// 用户管理-给用户删除助理
export function deleteAssistant(params) {
	return request({
		url: '/SystemManage/DeleteAssistant',
		method: 'get',
		params
	})
}

// 功能管理-获取按钮配置
export function getMenuItemList(params) {
	return request({
		url: '/SystemManage/GetMenuItemList',
		method: 'get',
		params
	})
}

// 功能管理-获取按钮配置(分页)
export function getMenuItemListByFilter(data) {
	return request({
		url: '/SystemManage/GetMenuItemListByFilter',
		method: 'post',
		data
	})
}

// 功能管理-根据系统获取按钮配置
export function getMenuItemByAppId(params) {
	return request({
		url: '/SystemManage/GetMenuItemByAppId',
		method: 'get',
		params
	})
}

// 功能管理-新增功能按钮
export function addMenuItem(data) {
	return request({
		url: '/SystemManage/InsertMenuItem',
		method: 'post',
		data
	})
}

// 功能管理-编辑功能按钮
export function editMenuItem(data) {
	return request({
		url: '/SystemManage/UpdateMenuItem',
		method: 'post',
		data
	})
}

// 功能管理-删除功能按钮
export function delMenuItem(params) {
	return request({
		url: '/SystemManage/DeleteMenuItem',
		method: 'get',
		params
	})
}

// 功能管理-删除功能按钮引用
export function delMenuItemQuote(params) {
	return request({
		url: '/SystemManage/DeleteMenuItemQuote',
		method: 'get',
		params
	})
}

// 功能管理-根据菜单id获取按钮配置
export function getBottonByMenuId(params) {
	return request({
		url: '/SystemManage/GetBottonByMenuId',
		method: 'get',
		params
	})
}

// 功能管理-获取角色按钮列表
export function getRolesMenuItem(params) {
	return request({
		url: '/SystemManage/GetRolesMenuItem',
		method: 'get',
		params
	})
}

// 功能管理-根据appId获取按钮配置
export function getBottonByAppId(params) {
	return request({
		url: '/SystemManage/GetBottonByAppId',
		method: 'get',
		params
	})
}

// 功能管理-根据roleId获取按钮配置
export function getRolesMenuItemByRoleId(params) {
	return request({
		url: '/SystemManage/GetRolesMenuItemByRoleId',
		method: 'get',
		params
	})
}

// 功能管理-给角色分配按钮权限
export function assignBottonSave(data) {
	return request({
		url: '/SystemManage/AssignBottonSave',
		method: 'post',
		data
	})
}

// 功能管理-控制权限
export function enableMenuItem(params) {
	return request({
		url: '/SystemManage/EnableMenuItem',
		method: 'get',
		params
	})
}

// 功能管理-禁用按钮
export function forbiddenMenuItem(params) {
	return request({
		url: '/SystemManage/ForbiddenMenuItem',
		method: 'get',
		params
	})
}

// 系统参数-获取系统参数列表
export function getSystemParam(params) {
	return request({
		url: '/SystemManage/GetSystemParam',
		method: 'get',
		params
	})
}

// 系统参数-添加系统参数
export function addSystemParam(data) {
	return request({
		url: '/SystemManage/InsertSystemParam',
		method: 'post',
		data
	})
}

// 系统参数-修改系统参数
export function editSystemParam(data) {
	return request({
		url: '/SystemManage/UpdateSystemParam',
		method: 'post',
		data
	})
}

// 系统参数-删除系统参数
export function delSystemParam(params) {
	return request({
		url: '/SystemManage/DeleteSystemParam',
		method: 'get',
		params
	})
}

// 部门管理-获取职能属性列表
export function getFunctionAttrLsit(params) {
	return request({
		url: '/DepartmentManage/GetFunctionAttrLsit',
		method: 'get',
		params
	})
}

// 部门管理-修改部门职能属性
export function updateDepartmentFunctionAttr(params) {
	return request({
		url: '/DepartmentManage/UpdateDepartmentFunctionAttr',
		method: 'get',
		params
	})
}

// 岗位管理-获取岗位列表
export function getPositionList(params) {
	return request({
		url: '/PositionManage/GetPositionList',
		method: 'get',
		params
	})
}

// 岗位管理-新增岗位
export function addPosition(data) {
	return request({
		url: '/PositionManage/InsertPosition',
		method: 'post',
		data
	})
}

// 岗位管理-编辑岗位
export function editPosition(data) {
	return request({
		url: '/PositionManage/UpdatePosition',
		method: 'post',
		data
	})
}

// 岗位管理-禁用岗位
export function forbiddenPosition(params) {
	return request({
		url: '/PositionManage/ForbiddenPosition',
		method: 'get',
		params
	})
}

// 岗位管理-启用岗位
export function enablePosition(params) {
	return request({
		url: '/PositionManage/EnablePosition',
		method: 'get',
		params
	})
}

// 获取微信职位列表
export function getWeiXinPositionList(params) {
	return request({
		url: '/SystemManage/GetWeiXinPositionList',
		method: 'get',
		params
	})
}

// 获取审批模板列表
export function getApprovalTemplateList(params) {
	return request({
		url: '/WeiXinWork/GetApprovalTemplateList',
		method: 'get',
		params
	})
}

// 获取审批流配置列表
export function getAuditFlowConfigList(params) {
	return request({
		url: '/WeiXinWork/GetAuditFlowConfigList',
		method: 'get',
		params
	})
}

// 新增审批流配置列表
export function addAuditFlowConfigList(data) {
	return request({
		url: '/WeiXinWork/AddAuditFlowConfigList',
		method: 'post',
		data
	})
}

// 新增审批模板
export function addApprovalTemplate(data) {
	return request({
		url: '/WeiXinWork/AddApprovalTemplate',
		method: 'post',
		data
	})
}

// 新增审批配置
export function addApprovalConfig(data) {
	return request({
		url: '/WeiXinWork/AddApprovalConfig',
		method: 'post',
		data
	})
}

// 更新字段
export function updateTemplateFieldNumber(data) {
	return request({
		url: '/WeiXinWork/UpdateTemplateFieldNumber',
		method: 'post',
		data
	})
}

//获取结算方式
export function getPaymentMethod(){
	return request({
		url: '/SystemManage/GetPaymentMethod',
		method: 'get',
	})
} 

//获取所有用户
export function getUserList(params){
	return request({
		url: '/SystemManage/GetUserList',
		method: 'get',
		params
	})
}  

//获取审批流字段
export function getTemplateField(params){
	return request({
		url: '/WeiXinWork/GetTemplateField',
		method: 'get',
		params
	})
}

//获取字段
export function getField(params){
	return request({
		url: '/WeiXinWork/GetField',
		method: 'get',
		params
	})
}

//删除审批配置
export function delApprovalConfig(params){
	return request({
		url: '/WeiXinWork/DelApprovalConfig',
		method: 'get',
		params
	})
}

//获取微信回调消息
export function getWxCallbackMessage(data){
	return request({
		url: '/WeiXinWork/GetWxCallbackMessage',
		method: 'post',
		data
	})
}

//加载全部缓存
export function reloadAllCache(params){
	return request({
		url: '/WeiXinWork/reloadAllCache',
		method: 'get',
		params
	})
}
//加载缓存
export function reloadCache(data){
	return request({
		url: '/WeiXinWork/reloadCache',
		method: 'post',
		data
	})
}
//获取环境变量
export function getEnvCodes(params){
	return request({
		url: '/WeiXinWork/GetEnvCodes',
		method: 'get',
		params
	})
}
// 
export function getEnvIsProduction(params){
	return request({
		url: '/WeiXinWork/getEnvIsProduction',
		method: 'get',
		params
	})
}

//获取审批详情
export function getSpDetail(params){
	return request({
		url: '/WeiXinWork/GetSpDetail',
		method: 'get',
		params
	})
}

//审核执行
export function execute(params){
	return request({
		url: '/WeiXinWork/Execute',
		method: 'get',
		params
	})
}

export function repairSp(params){
	return request({
		url: '/WeiXinWork/repairSp',
		method: 'get',
		params
	})
}

export function getMobileById(params)
{
	return request({
		url: '/SystemManage/GetMobileById',
		method: 'get',
		params
	})
}

export function queryMoblieTimes(data)
{
	return request({
		url: '/SystemManage/QueryMoblieTimes',
		method: 'post',
		data
	})
}

export function queryMobileHistory(data)
{
	return request({
		url: '/SystemManage/QueryMobileHistory',
		method: 'post',
		data
	})
}