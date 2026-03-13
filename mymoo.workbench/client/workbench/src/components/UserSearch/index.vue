<template>
	<el-select v-model="userValue" clearable filterable remote :placeholder="placeholder" :remote-method="remoteMethod" :loading="loading"  ref="select" @hook:mounted="cancalReadOnly"  @visible-change="cancalReadOnly"    @clear="clear">
		<el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value">
			<span style="float: left">{{ item.label }}</span>
			<span style="float: right; color: #8492a6; font-size: 13px">{{ item.mymoooCompany.toLowerCase() === 'dgweixinwork' ? '东莞蚂蚁' : item.mymoooCompany.toLowerCase() === 'weixinwork' ? '深圳蚂蚁' : '' }}</span> 
		</el-option>
	</el-select>
</template>

<script>
import { getUserInfo } from '@/api/user'

export default {
	name: 'UserSearch',
	props: {
		user: {
			type: Number,
			default: null
		},
		placeholder: {
			type: String,
			default: ''
		},
		isAssistant: {
			type: Boolean,
			default: false
		},
		isMymo:{
			type:Boolean,
			default:false
		}
	},
	data() {
		return {
			loading: false,
			options: []
		}
	},
	computed: {
		userValue: {
			get() {
				return this.user
			},
			set(val) {
				this.$emit('update:user', val)
			}
		}
	},
	methods: {
		cancalReadOnly(onOff) {
      this.$nextTick(() => {
        if (!onOff) {
                const { select } = this.$refs;
                const input = select.$el.querySelector(".el-input__inner");
                input.removeAttribute("readonly");
                }
      })
	},
		// 获取用户列表
		remoteMethod(queryString) {
			this.loading = true
			getUserInfo({ code: queryString, isAssistant: this.isAssistant,isMymo:this.isMymo }).then(res => {
				if (res.code === 'success' && res.data) {
					this.options = res.data.map(item => {
						return { value: item.userId, label: item.name, code: item.code, mymoooCompany: item.mymoooCompany }
					})
					let obj = {}
					this.options = this.options.reduce(function(prev, cur) {
						obj[cur.value] ? '' : (obj[cur.value] = true && prev.push(cur))
						return prev
					}, [])
				}
				this.loading = false
			})
		},
		clear() {
			this.userValue = null
		}
		
	}
}
</script>
