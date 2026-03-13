import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import ElementUI from 'element-ui'
import 'element-ui/lib/theme-chalk/index.css'
import '@/styles/index.scss' // global css
import './permission' // permission control
import './icons'
import 'default-passive-events' // 解决浏览器警告 [Violation] Added non-passive event listener to a scroll-blocking ‘mousewheel’ event. Consider marking event handler as ‘passive’ to make the page more responsive。
import dayjs from 'dayjs'
import * as signalR from '@microsoft/signalr'

Vue.prototype.dayjs = dayjs //可以全局使用dayjs
Vue.use(ElementUI)
Vue.config.productionTip = false
Vue.prototype.signalR = signalR;

new Vue({
	router,
	store,
	render: h => h(App)
}).$mount('#app')
