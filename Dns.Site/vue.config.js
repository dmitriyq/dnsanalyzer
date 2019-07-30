// vue.config.js
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
module.exports = {
    outputDir: 'wwwroot',
	chainWebpack: config => {
		if (config.plugins.has('extract-css')) {
			const extractCSSPlugin = config.plugin('extract-css')
			extractCSSPlugin && extractCSSPlugin.tap(() => [{
				filename: 'css/app.css',
				chunkFilename: 'css/chunk-vendors.css'
			}])
		}
	},
	configureWebpack: {
		//plugins: [
		//	new BundleAnalyzerPlugin()
		//],
		output: {
			filename: 'js/app.js',
			chunkFilename: 'js/chunk-vendors.js'
		},
	},
};