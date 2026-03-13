'use strict'
const path = require('path')
const defaultSettings = require('./src/settings.js')
// const CompressionPlugin = require('compression-webpack-plugin')

function resolve(dir) {
	return path.join(__dirname, dir)
}

const name = defaultSettings.title || 'workbench' // page title

module.exports = {
	publicPath: process.env.NODE_ENV === 'production' ? '/html5/' : '/',
	outputDir: 'dist',
	assetsDir: 'static',
	lintOnSave: false,
	productionSourceMap: false,
	devServer: {
		open: true,
		overlay: {
			warnings: false,
			errors: true
		},
		proxy: {
			'/': {
				target: 'http://localhost:1205/', //'http://moyifeng.devwork.mymooo.com:1205/'
				changeOrigin: true,
				onProxyReq: function(proxyReq, req, res, options) {

					// console.log(req.headers['content-type'])
					if (req.body && req.headers['content-type'] && req.headers['content-type'].indexOf('multipart/form-data') < 0) {
						let bodyData = JSON.stringify(req.body)
						// incase if content-type is application/x-www-form-urlencoded -> we need to change to application/json
						proxyReq.setHeader('Content-Type', 'application/json;charset=UTF-8')
						proxyReq.setHeader('Content-Length', Buffer.byteLength(bodyData))
						// stream the content
						proxyReq.write(bodyData)
					}
				}
			}
		}
	},
	configureWebpack: {
		devtool: process.env.NODE_ENV === 'development' ? 'eval-source-map' : undefined,
		// provide the app's title in webpack's name field, so that
		// it can be accessed in index.html to inject the correct title.
		name: name,
		resolve: {
			alias: {
				'@': resolve('src')
			}
		},
		// plugins: [
		// 	new CompressionPlugin({
		// 		algorithm: 'gzip', // 使用gzip压缩
		// 		test: /\.js$|\.html$|\.css$/, // 匹配文件名
		// 		threshold: 10240, // 对超过10k的数据压缩
		// 		deleteOriginalAssets: false // 是否删除未压缩的源文件，谨慎设置，如果希望提供非gzip的资源，可不设置或者设置为false（比如删除打包后的gz后还可以加载到原始资源文件）
		// 	})
		// ]
	},
	chainWebpack(config) {
		// it can improve the speed of the first screen, it is recommended to turn on preload
		config.plugin('preload').tap(() => [
			{
				rel: 'preload',
				// to ignore runtime.js
				// https://github.com/vuejs/vue-cli/blob/dev/packages/@vue/cli-service/lib/config/app.js#L171
				fileBlacklist: [/\.map$/, /hot-update\.js$/, /runtime\..*\.js$/],
				include: 'initial'
			}
		])

		// when there are many pages, it will cause too many meaningless requests
		config.plugins.delete('prefetch')

		// set svg-sprite-loader
		config.module
			.rule('svg')
			.exclude.add(resolve('src/icons'))
			.end()
		config.module
			.rule('icons')
			.test(/\.svg$/)
			.include.add(resolve('src/icons'))
			.end()
			.use('svg-sprite-loader')
			.loader('svg-sprite-loader')
			.options({
				symbolId: 'icon-[name]'
			})
			.end()
		// when there are many pages, it will cause too many meaningless requests
		config.plugins.delete('prefetch')

		config.when(process.env.NODE_ENV !== 'development', config => {
			config
				.plugin('ScriptExtHtmlWebpackPlugin')
				.after('html')
				.use('script-ext-html-webpack-plugin', [
					{
						// `runtime` must same as runtimeChunk name. default is `runtime`
						inline: /runtime\..*\.js$/
					}
				])
				.end()

			config.optimization.splitChunks({
				chunks: 'all',
				cacheGroups: {
					libs: {
						name: 'chunk-libs',
						test: /[\\/]node_modules[\\/]/,
						priority: 10,
						chunks: 'initial' // only package third parties that are initially dependent
					},
					elementUI: {
						name: 'chunk-elementUI', // split elementUI into a single package
						priority: 20, // the weight needs to be larger than libs and app or it will be packaged into libs or app
						test: /[\\/]node_modules[\\/]_?element-ui(.*)/ // in order to adapt to cnpm
					},
					commons: {
						name: 'chunk-commons',
						test: resolve('src/components'), // can customize your rules
						minChunks: 3, //  minimum common number
						priority: 5,
						reuseExistingChunk: true
					}
				}
			})
			config.optimization.runtimeChunk('single')
		})
	}
}
