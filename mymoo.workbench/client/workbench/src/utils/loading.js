import { Loading } from 'element-ui'

let loadingCount = 0
let loading = null

const startLoading = () => {
	loading = Loading.service({
		lock: false,
		text: '拼命加载中...', //可以自定义文字
		spinner: 'el-icon-loading', //自定义加载图标类名
		background: 'rgba(0, 0, 0, 0.7)' //遮罩层背景色
	})
}

const endLoading = () => {
	loading.close()
}

export const openLoading = () => {
	if (loadingCount === 0) {
		startLoading()
	}
	loadingCount += 1
}

export const closeLoading = () => {
	if (loadingCount <= 0) {
		return
	}
	loadingCount -= 1
	if (loadingCount === 0) {
		endLoading()
	}
}
